namespace PruebaNetCoreProject.Infrastructure.Persistence.MongoDB.Settings;

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDB";

    public string ConnectionString { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = "fintech_db";

    public static class Collections
    {
        public const string BalanceReadModels = "balance_read_models";
        public const string TransferHistory   = "transfer_history";
        public const string AuditLog          = "audit_log";
        public const string DomainEvents      = "domain_events";
    }
}
