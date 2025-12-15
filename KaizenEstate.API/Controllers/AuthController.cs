using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KaizenEstate.API.Data;
using KaizenEstate.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KaizenEstate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterModel request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Такой пользователь уже существует");
            }

            string role = "User";
            // Читаем код из appsettings.json
            var adminCode = _configuration["AdminSettings:SecretCode"];

            // Проверяем: если код не пустой и совпадает — даем Админа
            if (!string.IsNullOrEmpty(adminCode) && request.SecretCode == adminCode)
            {
                role = "Admin";
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Role = role,
                PasswordHash = HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginModel request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || user.PasswordHash != HashPassword(request.Password))
            {
                return BadRequest("Неверный логин или пароль");
            }

            string token = CreateToken(user);

            return Ok(new
            {
                Token = token,
                User = user
            });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // БЕРЕМ КЛЮЧ ИЗ КОНФИГА (тот же, что и в Program.cs)
            var jwtKey = _configuration["AppSettings:Token"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                jwtKey = "EtoOchenDlinniySecretniyKeyKotoriyNiktoNeUgadaet_Minimum64SimvolaDlyaSHA512";
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}