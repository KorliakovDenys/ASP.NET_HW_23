using ASP.NET_HW_23.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ASP.NET_HW_23.Repository;

public class Repository<T> : IRepository<T> {
    private readonly IMongoCollection<T> _mongoCollection;

    public Repository(IDbService dbService, string collectionName) {
        _mongoCollection = dbService.GetCollection<T>(collectionName).Result;
    }

    public async Task<IEnumerable<T?>> SelectAsync() {
        var cursor = await _mongoCollection.FindAsync(_ => true).ConfigureAwait(false);
        return await cursor.ToListAsync().ConfigureAwait(false);
    }

    public async Task<T?> SelectAsync(ObjectId id) {
        var filter = Builders<T>.Filter.Eq("_id", id);
        var obj = await _mongoCollection.Find(filter).Limit(1).FirstOrDefaultAsync().ConfigureAwait(false);
        return obj;
    }

    public async Task<T?> SelectAsync(string name) {
        var filter = Builders<T>.Filter.Eq("Name", name);
        var obj = await _mongoCollection.Find(filter).Limit(1).FirstOrDefaultAsync().ConfigureAwait(false);
        return obj;
    }

    public async Task InsertAsync(T obj) {
        await _mongoCollection.InsertOneAsync(obj);
    }

    public async Task UpdateAsync(ObjectId id, T obj) {
        var filter = Builders<T>.Filter.Eq("_id", id);
        await _mongoCollection.ReplaceOneAsync(filter, obj).ConfigureAwait(false);
    }

    public async Task DeleteAsync(ObjectId id) {
        var filter = Builders<T>.Filter.Eq("_id", id);
        await _mongoCollection.DeleteOneAsync(filter).ConfigureAwait(false);
    }

    public async Task<bool> AnyAsync(ObjectId id) {
        var filter = Builders<T>.Filter.AnyEq("_id", id);
        var count = await _mongoCollection.CountDocumentsAsync(filter).ConfigureAwait(false);
        return count > 0;
    }
}