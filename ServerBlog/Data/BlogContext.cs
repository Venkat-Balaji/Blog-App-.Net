using MongoDB.Driver;
using ServerBlog.Models;
using Microsoft.Extensions.Options;

namespace ServerBlog.Data
{
    public class BlogContext
    {
        private readonly IMongoDatabase _database;

        public BlogContext(IOptions<MongoDbSettings> settings, IMongoClient client)
        {
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Post> Posts => _database.GetCollection<Post>("Posts");
        public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("Comments");
    }
}
