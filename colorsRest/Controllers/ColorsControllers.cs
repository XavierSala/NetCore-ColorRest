using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using colorsRest.Models;
using colorsRest.Repository;
using colorsRest.Exceptions;

namespace colorsRest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColorsController : Controller
    {
        private readonly IColorsRepository _repository;
        private readonly ILogger<ColorsController> _logger;


        public ColorsController(IColorsRepository repository, ILogger<ColorsController> logger)
        {
            _repository = repository;
            _logger = logger;

        }


        // GET api/colors
        [HttpGet]
        public ActionResult<IList<Color>> GetAll()
        {
            _logger.LogInformation("All items requested");
            return Json(_repository.Get());
        }

        // POST api/colors
        [HttpPost]
        public ActionResult Add([FromBody]Color value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _repository.Add(value);
                return CreatedAtAction(nameof(GetById), new { id = value.Id }, value);
            }
            catch (ColorException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        // GET api/colors/5
        [HttpGet("{id}", Name = "GetColor")]
        public IActionResult GetById(int id)
        {
            var resultat = _repository.Get(id);
            if (resultat == null)
            {
                return NotFound(new { message = "Not Found" });
            }
            return Ok(resultat);
        }

    }
}
