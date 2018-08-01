using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using colorsRest.Models;

namespace colorsRest.Repository
{

    public interface IColorsRepository
    {
        Color Get(int id);
        IList<Color> Get();
        void Add(Color item);
        Task<int> AddProductAsync(Color item);
        void Update(Color item);
        void Delete(int id);
    }

}