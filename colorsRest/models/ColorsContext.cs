using Microsoft.EntityFrameworkCore;

namespace colorsRest.Models
{
    public class ColorsContext : DbContext
    {
        public ColorsContext(DbContextOptions<ColorsContext> options)
            : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Colors.db");
        }
        
        public DbSet<Color> Colors { get; set; }
    }
}