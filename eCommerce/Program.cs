using ECommerce.Data;
using ECommerce.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

////builder.Services.AddScoped<DbTransactionFilter>();

// Add services to the container.	
builder.Services.AddControllers(options =>
{
    ////options.Filters.Add<DbTransactionFilter>();
});

builder.Services.AddScoped<IAuthRepository, SQLAuthRepository>();

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
    .AddPolicy("BuyerOnly", policy => policy.RequireRole("Buyer"))
    .AddPolicy("SellerOnly", policy => policy.RequireRole("Seller"));



// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connStringKey = "PgConnString";
var connectionString =
    builder.Configuration.GetConnectionString(connStringKey)
        ?? throw new InvalidOperationException("Connection string"
        + connStringKey + " not found.");

builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Logging.ClearProviders();
builder.Logging.AddConsole();   

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
    Console.WriteLine(db.Database.CanConnect() ? "Database connection successful" : "Database connection failed");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            title = "Internal server error",
            status = 500
        });
    });
});


app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
