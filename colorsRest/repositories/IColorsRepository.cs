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
        Task<int> AddColorAsync(Color item);
        void Update(Color item);
        Task<bool> Delete(int id);
    }

}