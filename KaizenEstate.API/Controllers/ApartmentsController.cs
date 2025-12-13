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

        // DELETE: api/Apartments/5
        // === МЕТОД УДАЛЕНИЯ ===
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApartment(int id)
        {
            var apartment = await _context.Apartments.FindAsync(id);
            if (apartment == null)
            {
                return NotFound();
            }

            // Мы удаляем только запись из БД. 
            // Если картинки нет (старая квартира) — ничего страшного не случится.
            _context.Apartments.Remove(apartment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}