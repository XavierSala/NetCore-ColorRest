using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using colorsRest.Exceptions;
using colorsRest.Models;
using Microsoft.EntityFrameworkCore;

namespace colorsRest.Repository
{

    public class ColorsRepository : IColorsRepository
    {
        private readonly ColorsContext _context;

        public ColorsRepository(ColorsContext context)
        {
            _context = context;
        }

        public void Add(Color item)
        {
            if (item.Id != 0)
            {
                throw new ColorException("You can't give an Id");
            }
            try
            {
                _context.Colors.Add(item);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new ColorException(e.Message);
            }
        }

        public async Task<int> AddColorAsync(Color item)
        {
            int rowsAffected = 0;

            _context.Colors.Add(item);
            rowsAffected = await _context.SaveChangesAsync();

            return rowsAffected;
        }

        public async Task<bool> Delete(int id)
        {
            // Search
            Color colorToDelete = await _context.Colors.FindAsync(id);
            if (colorToDelete == null)
            {
                return false;
            }

            // Delete from database and return true/false.
            _context.Colors.Remove(colorToDelete);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Color> Get(string nom)
        {
            return await _context.Colors
                    .Where(b => b.Nom.Equals(nom))
                    .FirstOrDefaultAsync();
        }

        public Color Get(int id) => _context.Colors.Find(id);

        public IList<Color> Get() => _context.Colors.ToList();

        public void Update(Color item)
        {
            throw new NotImplementedException();
        }

    }
}
