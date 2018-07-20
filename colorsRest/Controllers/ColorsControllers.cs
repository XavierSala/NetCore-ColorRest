using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using colorsRest.Models;

namespace colorsRest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColorsController : Controller
    {
        private readonly ColorsContext _context;
        private readonly ILogger<ColorsController> _logger;
        

        public ColorsController(ColorsContext context, ILogger<ColorsController> logger)
        {
            _context = context;
            _logger = logger;

            if (_context.Colors.Count() == 0)
            {
                
                _logger.LogWarning("Database Empty");
                _context.Colors.Add(new Color { Nom = "vermell", Rgb = "#FF0000" });
                _context.Colors.Add(new Color { Nom = "verd", Rgb = "#00FF00" });
                _context.Colors.Add(new Color { Nom = "blau", Rgb = "#0000FF" });
                _context.SaveChanges();
            }
        }


        // GET api/colors
        [HttpGet]
        public ActionResult<List<Color>> GetAll()
        {
            _logger.LogInformation("All items requested");
            return _context.Colors.ToList();
        }

        // GET api/colors/5
        [HttpGet("{id}", Name = "GetColor")]
        public ActionResult<Color> GetById(int id)
        {
            var resultat = _context.Colors.Find(id);
            if (resultat == null)
            {
                return NotFound();
            }

            return resultat;
        }

    }
}
