using global::MongoDB.Driver;

namespace PruebaNetCoreProject.Infrastructure.Persistence.MongoDB.Contexts;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient client, string databaseName)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);
        return _database.GetCollection<TDocument>(collectionName);
    }
}
