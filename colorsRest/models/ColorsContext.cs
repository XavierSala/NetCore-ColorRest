using Microsoft.EntityFrameworkCore;

namespace colorsRest.Models
{
    public class ColorsContext : DbContext
    {
        public ColorsContext(DbContextOptions<ColorsContext> options)
            : base(options)
        {
            
        }
        
        public DbSet<Color> Colors { get; set; }
    }
}