namespace ECommerce.Services.Implementations
{
    /// <summary>
    /// Actions decorated with this attribute are excluded from the automatic
    /// database transaction managed by <see cref="DbTransactionFilter"/>.
    /// Use when an action needs manual transaction control — e.g., when an
    /// external API call (Stripe, email, etc.) must happen after a commit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SkipDbTransactionAttribute : Attribute { }
}
