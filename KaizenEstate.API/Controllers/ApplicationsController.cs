using KaizenEstate.API.Data;
using KaizenEstate.Shared.Models;
using Microsoft.AspNetCore.Authorization; // <--- НУЖНО ДЛЯ ЗАЩИТЫ
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KaizenEstate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. СОЗДАТЬ ЗАЯВКУ (Доступно всем авторизованным)
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<EstateApplication>> CreateApplication(EstateApplication application)
        {
            var exists = await _context.Applications
                .AnyAsync(a => a.UserId == application.UserId && a.ApartmentId == application.ApartmentId);

            if (exists)
            {
                return BadRequest("Вы уже отправили заявку на эту квартиру.");
            }

            application.CreatedAt = DateTime.UtcNow;
            application.Status = "Новая";
            application.User = null;
            application.Apartment = null;

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(application);
        }

        // 2. ЗАЯВКИ ПОЛЬЗОВАТЕЛЯ (Только свои)
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<List<EstateApplication>>> GetUserApplications(int userId)
        {
            return await _context.Applications
                .Include(a => a.Apartment)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // 3. ВСЕ ЗАЯВКИ (ТОЛЬКО ДЛЯ АДМИНА) — НОВЫЙ МЕТОД
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<EstateApplication>>> GetAllApplications()
        {
            return await _context.Applications
                .Include(a => a.Apartment) // Подгружаем квартиру
                .Include(a => a.User)      // Подгружаем, КТО оставил заявку
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}