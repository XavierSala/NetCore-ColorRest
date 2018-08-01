using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using colorsRest.Models;

namespace colorsRest.Repository
{

    public class ColorsRepository : IColorsRepository
    {
        private readonly ColorsContext _context;

        public ColorsRepository(ColorsContext context)
        {
            _context = context;

            if (_context.Colors.Count() != 0)
            {
                return;
            }
            _context.Colors.Add(new Color { Nom = "vermell", Rgb = "#FF0000" });
            _context.Colors.Add(new Color { Nom = "verd", Rgb = "#00FF00" });
            _context.Colors.Add(new Color { Nom = "blau", Rgb = "#0000FF" });
            _context.SaveChanges();
        }

        public void Add(Color item)
        {
            if (item.Id != 0)
            {
                throw new ApplicationException("You can't give an Id");
            }
            
            try
            {
                _context.Colors.Add(item);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }                       
        }

        public async Task<int> AddProductAsync(Color item)
        {
            int rowsAffected = 0;

            _context.Colors.Add(item);
            rowsAffected = await _context.SaveChangesAsync();

            return rowsAffected;
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Color Get(int id) => _context.Colors.Find(id);

        public IList<Color> Get() => _context.Colors.ToList();

        public void Update(Color item)
        {
            throw new NotImplementedException();
        }
    }
}
