using MongoDB.Driver;

namespace ASP.NET_HW_23.Data;

public interface IDbService {
    public Task<IMongoCollection<T>> GetCollection<T>(string name);
}