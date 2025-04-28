using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ServerBlog.Models;
using Microsoft.AspNetCore.Authorization;

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
            var posts = await _postsCollection.Aggregate()
                .Lookup(
                    foreignCollectionName: "comments",    // MongoDB "Comments" collection
                    localField: "Id",                     // Post Id (local field in Posts)
                    foreignField: "PostId",               // Comment's reference to PostId
                    @as: "Comments"                       // The new field where comments will be placed
                )
                .Project(post => new Post   // Project BsonDocument to Post model
                {
                    Id = post["_id"].ToString(),  // MongoDB _id field to Post's Id
                    Title = post["Title"].ToString(),
                    Content = post["Content"].ToString(),
                    UserId = post["UserId"].ToString(),
                    // Mapping the Comments array into List<Comment> model
                    Comments = post["Comments"].AsBsonArray.Select(comment => new Comment
                    {
                        Id = comment["_id"].ToString(),
                        Content = comment["content"].ToString(),
                        UserId = comment["userId"].ToString(),
                        PostId = comment["postId"].ToString(),
                        CreatedAt = comment["createdAt"].ToUniversalTime(),  // Convert to DateTime
                        UserName = comment["userName"].ToString(),
                        PostTitle = comment["postTitle"].ToString()
                    }).ToList()  // Convert to List<Comment>
                })
                .ToListAsync();

            return Ok(posts);
        }


        // Get posts with comment count using MongoDB aggregation
        [HttpGet("with-comment-count")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsWithCommentCount()
        {
            var posts = await _postsCollection.Aggregate()
                .Lookup(
                    foreignCollectionName: "comments",  // MongoDB "Comments" collection
                    localField: "Id",                   // Post Id
                    foreignField: "PostId",             // Comment's reference to PostId
                    @as: "Comments"                     // The new field holding comments
                )
                .Group(
                    g => g.Id,  // Group by Post Id
                    group => new
                    {
                        PostId = group.Key,
                        PostTitle = group.First().Title,  // Include post title
                        CommentCount = group.SelectMany(post => post.Comments).Count()  // Count the comments
                    }
                )
                .ToListAsync();

            return Ok(posts);
        }
    }
}
