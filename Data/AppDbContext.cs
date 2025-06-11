using Microsoft.EntityFrameworkCore;
using Mimsv2.Models;

namespace Mimsv2.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<LoginModel> tblusers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Optional: Add any custom model configurations here
            base.OnModelCreating(modelBuilder);
        }
    }
}
