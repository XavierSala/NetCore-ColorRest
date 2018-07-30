using System;
using System.Collections.Generic;
using colorsRest.Models;

namespace colorsRest.Repository
{

    public interface IColorsRepository
    {
        Color Get(int id);
        IList<Color> Get();
        bool Add(Color item);
        void Update(Color item);
        void Delete(int id);
    }

}