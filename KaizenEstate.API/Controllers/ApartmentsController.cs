using KaizenEstate.API.Data;
using KaizenEstate.API.Services;
using KaizenEstate.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KaizenEstate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IObjectStorageService _fileService;

        public ApartmentsController(ApplicationDbContext context, IObjectStorageService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: api/Apartments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Apartment>>> GetApartments()
        {
            return await _context.Apartments.ToListAsync();
        }

        // GET: api/Apartments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Apartment>> GetApartment(int id)
        {
            var apartment = await _context.Apartments.FindAsync(id);

            if (apartment == null)
            {
                return NotFound();
            }

            return apartment;
        }

        // POST: api/Apartments
        [HttpPost]
        public async Task<ActionResult<Apartment>> PostApartment(
            [FromForm] string title,
            [FromForm] string address,
            [FromForm] string description,
            [FromForm] decimal price,
            [FromForm] int rooms,
            [FromForm] double area,
            IFormFile? image)
        {
            var apartment = new Apartment
            {
                Title = title,
                Address = address,
                Description = description,
                Price = price,
                Rooms = rooms,
                Area = area,
                CreatedAt = DateTime.UtcNow
            };

            if (image != null)
            {
                apartment.ImageUrl = await _fileService.UploadFileAsync(image);
            }

            _context.Apartments.Add(apartment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetApartments), new { id = apartment.Id }, apartment);
        }

        // PUT: api/Apartments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutApartment(int id, [FromForm] string title, [FromForm] string address,
            [FromForm] string description, [FromForm] decimal price, [FromForm] int rooms,
            [FromForm] double area, IFormFile? image)
        {
            var apartment = await _context.Apartments.FindAsync(id);
            if (apartment == null)
            {
                return NotFound();
            }

            // Обновляем текстовые поля
            apartment.Title = title;
            apartment.Address = address;
            apartment.Description = description;
            apartment.Price = price;
            apartment.Rooms = rooms;
            apartment.Area = area;

            // Логика обновления фото
            if (image != null)
            {
                // 1. Если была старая картинка — удаляем её из MinIO
                if (!string.IsNullOrEmpty(apartment.ImageUrl))
                {
                    await _fileService.DeleteFileAsync(apartment.ImageUrl);
                }

                // 2. Загружаем новую
                apartment.ImageUrl = await _fileService.UploadFileAsync(image);
            }

            _context.Entry(apartment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Apartments.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Apartments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApartment(int id)
        {
            var apartment = await _context.Apartments.FindAsync(id);
            if (apartment == null)
            {
                return NotFound();
            }

            // 1. Удаляем файл из MinIO перед удалением записи
            if (!string.IsNullOrEmpty(apartment.ImageUrl))
            {
                await _fileService.DeleteFileAsync(apartment.ImageUrl);
            }

            // 2. Удаляем из БД
            _context.Apartments.Remove(apartment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}