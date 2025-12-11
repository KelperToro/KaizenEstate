using KaizenEstate.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace KaizenEstate.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Apartment> Apartments { get; set; }

        public DbSet<User> Users { get; set; }
    }
}