using MongoDB.Driver;
using ServerBlog.Models;

namespace ServerBlog.Data
{
    public class BlogContext
    {
        private readonly IMongoDatabase _database;

        public BlogContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            _database = client.GetDatabase("BlogDb"); // Your DB name
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        public IMongoCollection<Post> Posts => _database.GetCollection<Post>("Posts");

        public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("Comments");
    }
}
