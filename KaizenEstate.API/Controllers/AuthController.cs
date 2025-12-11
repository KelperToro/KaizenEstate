using System.Security.Cryptography;
using System.Text;
using KaizenEstate.API.Data;
using KaizenEstate.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KaizenEstate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterModel request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Такой пользователь уже существует");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Role = "User", 
                PasswordHash = HashPassword(request.Password) 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(LoginModel request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }

            if (user.PasswordHash != HashPassword(request.Password))
            {
                return BadRequest("Неверный пароль");
            }

            return Ok(user);
        }

        // (SHA256)
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}