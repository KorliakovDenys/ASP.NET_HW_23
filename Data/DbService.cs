using MongoDB.Driver;

namespace ASP.NET_HW_23.Data;

public class DbService : IDbService {
    private readonly IMongoDatabase _mongoDatabase;

    public DbService(IConfiguration configuration) {
        var connectionString = configuration.GetConnectionString("MongoDb");
        if (string.IsNullOrEmpty(connectionString))
            throw new NullReferenceException("Connection string is null or empty");

        var initialDatabase = configuration["InitialDatabase"];
        if (string.IsNullOrEmpty(initialDatabase))
            throw new NullReferenceException("InitialDatabase string is null or empty");

        var mongoClient = new MongoClient(connectionString);
        _mongoDatabase = mongoClient.GetDatabase(initialDatabase);
    }

    public Task<IMongoCollection<T>> GetCollection<T>(string name) {
        return Task.FromResult(_mongoDatabase.GetCollection<T>(name));
    }
}