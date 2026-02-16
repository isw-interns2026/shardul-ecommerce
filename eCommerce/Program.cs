using ECommerce.Data;
using ECommerce.Repositories.Implementations;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Implementations;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Serilog;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.EntityFrameworkCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ── Fail-fast config validation ──────────────────────────────────
var requiredKeys = new[] { "JWT:SecretKey", "JWT:Issuer", "JWT:Audience", "Stripe:SecretKey", "Stripe:WebhookSecret", "Stripe:SuccessUrl", "Stripe:CancelUrl" };
var missing = requiredKeys.Where(k => string.IsNullOrWhiteSpace(builder.Configuration[k])).ToList();
if (missing.Count > 0)
    throw new InvalidOperationException(
        $"Missing required configuration: {string.Join(", ", missing)}. " +
        $"Set these via user secrets (dotnet user-secrets set \"Key\" \"Value\") or environment variables.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddScoped<DbTransactionFilter>();

// Add services to the container.	
builder.Services.AddControllers(options =>
{
    options.Filters.Add<DbTransactionFilter>();
    options.Filters.Add<FluentValidationFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddScoped<IStockReservationService, StockReservationService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton(new Stripe.StripeClient(builder.Configuration["Stripe:SecretKey"]));
builder.Services.AddScoped<IStripeService, StripeService>();


var connStringKey = "PgConnString";
var connectionString =
    builder.Configuration.GetConnectionString(connStringKey)
        ?? throw new InvalidOperationException("Connection string"
        + connStringKey + " not found.");

builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ECommerceDbContext>());

// --- TickerQ with its own built-in DbContext ---
builder.Services.AddTickerQ(options =>
{
    options.ConfigureScheduler(scheduler =>
    {
        scheduler.MaxConcurrency = 8;
    });

    options.AddOperationalStore(efOptions =>
    {

        efOptions.UseTickerQDbContext<TickerQDbContext>(optionsBuilder =>
        {
            optionsBuilder.UseNpgsql(connectionString,
                cfg =>
                {
                    cfg.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
                    cfg.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), ["40P01"]);
                });
        }, schema: "ticker");

        efOptions.SetDbContextPoolSize(34);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.AddDashboard(dashOpt =>
        {
            dashOpt.SetBasePath("/tickerq-dashboard");
        });
    }
});


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])
            ),
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser().Build());

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("BuyerOnly", policy => policy.RequireRole("Buyer"))
    .AddPolicy("SellerOnly", policy => policy.RequireRole("Seller"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/ecommerce-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation(db.Database.CanConnect() ? "Database connection successful" : "Database connection failed");
}

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseExceptionHandler();

app.UseTickerQ();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
