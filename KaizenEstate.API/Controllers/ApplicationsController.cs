using KaizenEstate.API.Data;
using KaizenEstate.Shared.Models;
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

        // 1. СОЗДАТЬ ЗАЯВКУ (POST)
        [HttpPost]
        public async Task<ActionResult<EstateApplication>> CreateApplication(EstateApplication application)
        {
            // Задаем начальные значения
            application.CreatedAt = DateTime.UtcNow;
            application.Status = "Новая";

            // Очищаем связи, чтобы EF Core записал только ID, а не пытался создать дубликаты
            application.User = null;
            application.Apartment = null;

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(application);
        }

        // 2. ПОЛУЧИТЬ ЗАЯВКИ ПОЛЬЗОВАТЕЛЯ (GET)
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<EstateApplication>>> GetUserApplications(int userId)
        {
            return await _context.Applications
                .Include(a => a.Apartment) // Подгружаем инфо о квартире
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt) // Новые сверху
                .ToListAsync();
        }
    }
}