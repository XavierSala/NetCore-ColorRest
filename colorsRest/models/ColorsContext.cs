using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace colorsRest.Models
{
    public class ColorsContext : IdentityDbContext
    {
        public ColorsContext() { }
        public ColorsContext(DbContextOptions<ColorsContext> options)
            : base(options)
        {

        }

        public virtual DbSet<Color> Colors { get; set; }
    }
}