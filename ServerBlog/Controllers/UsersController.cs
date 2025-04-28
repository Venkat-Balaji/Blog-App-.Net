using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ServerBlog.Data;
using ServerBlog.Models;

namespace ServerBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UsersController(IMongoDatabase database)
        {
            _usersCollection = database.GetCollection<User>("Users");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _usersCollection.Find(_ => true).ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            await _usersCollection.InsertOneAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, User user)
        {
            if (id != user.Id) return BadRequest();
            var result = await _usersCollection.ReplaceOneAsync(u => u.Id == id, user);
            if (result.MatchedCount == 0) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _usersCollection.DeleteOneAsync(u => u.Id == id);
            if (result.DeletedCount == 0) return NotFound();
            return NoContent();
        }
    }
}
