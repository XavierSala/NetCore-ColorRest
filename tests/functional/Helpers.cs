using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using colorsRest.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

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

        public static StringContent User2Json(string email, string password)
        {
            var content = JsonConvert.SerializeObject(
                new
                {
                    Email = email,
                    Password = password
                }
            );
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            return stringContent;
        }
        public static StringContent Color2Json(Color colorToAdd)
        {
            var content = JsonConvert.SerializeObject(colorToAdd);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            return stringContent;
        }

        public static async Task<Color> Json2Color(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Color>(json);
            return data;
        }

    }
}
