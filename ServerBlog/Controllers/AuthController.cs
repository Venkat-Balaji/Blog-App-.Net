using Microsoft.AspNetCore.Mvc;
using ServerBlog.Data;
using ServerBlog.Models;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ServerBlog.Controllers // <-- Correct Namespace (Models → Controllers)
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly string _jwtSecret;

        public AuthController(BlogContext context, IConfiguration configuration)
        {
            _usersCollection = context.Users;
            _jwtSecret = configuration["Jwt:Secret"];
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Check if user already exists by email
            var existingUser = await _usersCollection
                .Find(u => u.Email == user.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return BadRequest("User already exists.");
            }

            // Optional: Hash password here before saving (recommended for security)

            await _usersCollection.InsertOneAsync(user);
            return Ok("Registration successful.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            var user = await _usersCollection
                .Find(u => u.Email == loginUser.Email && u.Password == loginUser.Password)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Assuming Id is stored as ObjectId or string
                new Claim(ClaimTypes.Name, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
