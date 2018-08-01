using System.Collections.Generic;
using colorsRest.Models;

namespace colorsRest.Tests
{
    public static class Utilities
    {
        public static void InitializeDbForTests(ColorsContext db)
        {
            db.Colors.AddRange(GetTestColors());
            db.SaveChanges();
        }

        public static List<Color> GetTestColors()
        {
            return new List<Color>
            {
                new Color { Nom = "vermell", Rgb="#FF0000" },
                new Color { Nom = "verd", Rgb = "#00FF00" },
                new Color { Nom = "negre", Rgb = "#000000" }
            };
        }
    }
}
