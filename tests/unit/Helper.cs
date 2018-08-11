
using System.Collections.Generic;
using colorsRest.Models;

namespace colorsRest.Tests.UnitTests
{
    public class Helper
    {
        protected Helper()
        {
        }
        public static List<Color> TestColors
        {
            get
            {
                var colors = new List<Color>();
                colors.Add(new Color
                {
                    Nom = "vermell",
                    Id = 1,
                    Rgb = "#FF0000"
                });

                colors.Add(new Color
                {
                    Nom = "verd",
                    Id = 2,
                    Rgb = "#00FF00"
                });

                colors.Add(new Color
                {
                    Nom = "beix",
                    Id = 3,
                    Rgb = "#F2F2DF"
                });

                return colors;
            }
        }
    }
}