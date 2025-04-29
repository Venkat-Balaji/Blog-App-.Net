using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ServerBlog.Models
{
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Title")]
        public string Title { get; set; }

        [BsonElement("Content")]
        public string Content { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; }

        // Optional field to hold comments when returned via aggregation
        public List<Comment> Comments { get; set; } = new();
    }
}