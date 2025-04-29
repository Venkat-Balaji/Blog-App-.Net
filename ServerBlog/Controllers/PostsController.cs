using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ServerBlog.Models;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;

namespace ServerBlog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IMongoCollection<Post> _postsCollection;
        private readonly IMongoCollection<User> _usersCollection;

        public PostsController(IMongoDatabase database)
        {
            _postsCollection = database.GetCollection<Post>("Posts");
            _usersCollection = database.GetCollection<User>("Users");
        }

        // Get all posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            var posts = await _postsCollection.Find(_ => true).ToListAsync();
            return Ok(posts);
        }

        // Get a single post by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(string id)
        {
            var post = await _postsCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (post == null)
                return NotFound();
            return Ok(post);
        }

        // Get all posts by a specific user
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsByUser(string userId)
        {
            var userExists = await _usersCollection.Find(u => u.Id == userId).AnyAsync();
            if (!userExists) return NotFound("User not found");

            var posts = await _postsCollection.Find(p => p.UserId == userId).ToListAsync();
            return Ok(posts);
        }

        // Create a new post
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Post>> CreatePost(Post post)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            post.UserId = userId;
            await _postsCollection.InsertOneAsync(post);

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }

        // Update an existing post
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(string id, Post post)
        {
            if (id != post.Id)
                return BadRequest();

            var result = await _postsCollection.ReplaceOneAsync(p => p.Id == id, post);
            if (result.MatchedCount == 0) return NotFound();
            return NoContent();
        }

        // Delete a post
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(string id)
        {
            var result = await _postsCollection.DeleteOneAsync(p => p.Id == id);
            if (result.DeletedCount == 0)
                return NotFound();
            return NoContent();
        }

        // Get posts with comments using MongoDB aggregation
        [HttpGet("with-comments")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsWithComments()
        {
            var bsonDocs = await _postsCollection.Aggregate()
                .Lookup(
                    foreignCollectionName: "comments",
                    localField: "Id",
                    foreignField: "PostId",
                    @as: "Comments"
                )
                .As<BsonDocument>()
                .ToListAsync();

            var posts = bsonDocs.Select(doc =>
            {
                var post = new Post
                {
                    Id = doc.Contains("Id") ? doc["Id"].AsString : doc["_id"].ToString(),
                    Title = doc.GetValue("Title", "").AsString,
                    Content = doc.GetValue("Content", "").AsString,
                    UserId = doc.GetValue("UserId", "").AsString,
                    Comments = new List<Comment>()
                };

                if (doc.Contains("Comments") && doc["Comments"].IsBsonArray)
                {
                    var commentArray = doc["Comments"].AsBsonArray;
                    foreach (var commentBson in commentArray)
                    {
                        var commentDoc = commentBson.AsBsonDocument;

                        var comment = new Comment
                        {
                            Id = commentDoc.Contains("_id") ? commentDoc["_id"].ToString() : "",
                            Content = commentDoc.GetValue("content", "").AsString,
                            UserId = commentDoc.GetValue("userId", "").AsString,
                            PostId = commentDoc.GetValue("postId", "").AsString,
                            UserName = commentDoc.GetValue("userName", "").AsString,
                            PostTitle = commentDoc.GetValue("postTitle", "").AsString,
                            CreatedAt = commentDoc.Contains("createdAt") && commentDoc["createdAt"].IsValidDateTime
                                ? commentDoc["createdAt"].ToUniversalTime()
                                : DateTime.MinValue
                        };

                        post.Comments.Add(comment);
                    }
                }

                return post;
            }).ToList();

            return Ok(posts);
        }








        // Get posts with comment count using MongoDB aggregation
        [HttpGet("with-comment-count")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsWithCommentCount()
        {
            var bsonDocs = await _postsCollection.Aggregate()
                .Lookup(
                    foreignCollectionName: "comments",
                    localField: "Id",
                    foreignField: "PostId",
                    @as: "Comments"
                )
                .As<BsonDocument>()
                .ToListAsync();

            var posts = bsonDocs.Select(doc =>
            {
                // Initialize CommentCount at the time of object creation
                var post = new
                {
                    PostId = doc.Contains("Id") ? doc["Id"].AsString : doc["_id"].ToString(),
                    PostTitle = doc.GetValue("Title", "").AsString,
                    CommentCount = doc.Contains("Comments") && doc["Comments"].IsBsonArray
                        ? doc["Comments"].AsBsonArray.Count
                        : 0
                };

                return post;
            }).ToList();

            return Ok(posts);
        }


        [HttpGet("following/{userId}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsFromFollowedUsers(string userId)
        {
            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return NotFound();

            var followedUserIds = user.Following; // List<string> of followed user IDs

            var filter = Builders<Post>.Filter.In(p => p.UserId, followedUserIds);
            var posts = await _postsCollection.Find(filter).ToListAsync();

            return Ok(posts);
        }



    }
}