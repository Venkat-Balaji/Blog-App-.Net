using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ServerBlog.Data;
using ServerBlog.Models;

namespace ServerBlog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly IMongoCollection<Comment> _commentsCollection;
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Post> _postsCollection;

        public CommentsController(IMongoDatabase database)
        {
            _commentsCollection = database.GetCollection<Comment>("Comments");
            _usersCollection = database.GetCollection<User>("Users");
            _postsCollection = database.GetCollection<Post>("Posts");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
        {
            var comments = await _commentsCollection.Find(_ => true).ToListAsync();
            return Ok(comments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(string id)
        {
            var comment = await _commentsCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (comment == null)
                return NotFound();
            return Ok(comment);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByUser(string userId)
        {
            var userExists = await _usersCollection.Find(u => u.Id == userId).AnyAsync();
            if (!userExists) return NotFound("User not found");

            var comments = await _commentsCollection.Find(c => c.UserId == userId).ToListAsync();
            return Ok(comments);
        }

        [HttpPost]
        public async Task<ActionResult<Comment>> CreateComment(Comment comment)
        {
            var userExists = await _usersCollection.Find(u => u.Id == comment.UserId).AnyAsync();
            var postExists = await _postsCollection.Find(p => p.Id == comment.PostId).AnyAsync();

            if (!userExists || !postExists)
                return BadRequest("Invalid UserId or PostId");

            await _commentsCollection.InsertOneAsync(comment);

            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(string id, Comment comment)
        {
            if (id != comment.Id)
                return BadRequest();

            var result = await _commentsCollection.ReplaceOneAsync(c => c.Id == id, comment);
            if (result.MatchedCount == 0) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(string id)
        {
            var result = await _commentsCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
                return NotFound();

            return NoContent();
        }

    }
}
