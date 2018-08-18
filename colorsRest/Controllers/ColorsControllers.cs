using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using colorsRest.Models;
using colorsRest.Repository;
using colorsRest.Exceptions;
using Microsoft.AspNetCore.Authorization;

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


        /// <summary>
        /// Obtenir la llista amb tots els colors.
        /// </summary>
        /// <remarks>
        /// Exemple:
        ///
        ///     GET /api/colors
        ///
        /// </remarks>      
        /// <response code="200">Retorna la llista de colors</response>
        [HttpGet]
        public ActionResult<IList<Color>> GetAll()
        {
            _logger.LogInformation("All items requested");
            return Json(_repository.Get());
        }

        /// <summary>
        /// Crea un nou Color. Necessita autenticació
        /// </summary>
        /// <remarks>
        /// Exemple:
        ///
        ///     POST /api/colors
        ///     {
        ///        "Nom": "Vermell",
        ///        "Rgb": "#FF0000"
        ///     }
        ///
        /// </remarks>
        /// <param name="value"></param>
        /// <returns>El nou color creat</returns>
        /// <response code="200">Retorna el Color creat</response>
        /// <response code="400">Hi ha hagut algun problema</response> 
        [Authorize]
        [HttpPost]
        public IActionResult Add([FromBody]Color value)
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
                return BadRequest(new Error { Message = e.Message });
            }
        }

        /// <summary>
        /// Retorna el color que té l'ID especificat.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/colors/1
        ///
        /// </remarks>
        /// <param name="id"></param>
        /// <response code="200">Retorna el color amb l'id especificat</response>
        /// <response code="404">No trobat</response> 
        [HttpGet("{id:int}", Name = "GetColor")]
        public IActionResult GetById(int id)
        {
            var resultat = _repository.Get(id);
            if (resultat == null)
            {
                return NotFound(new Error { Message = "Not Found" });
            }
            return Ok(resultat);
        }

        /// <summary>
        /// Retorna un color a partir del seu nom.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/color/vermell
        ///
        /// </remarks>
        /// <param name="nom"></param>
        /// <response code="200">Retorna el color amb el nom especificat</response>
        /// <response code="404">No trobat</response> 
        [HttpGet("{nom}", Name = "GetColorByName")]
        public async Task<IActionResult> GetByName(string nom)
        {
            var resultat = await _repository.Get(nom);
            if (resultat == null)
            {
                return NotFound(new Error { Message = "Not Found" });
            }
            return Ok(resultat);
        }

        /// <summary>
        /// Esborra el Color que té l'Id especificat. Necessita autenticació
        /// </summary>
        /// <remarks>
        /// Petició d'exemple:
        ///
        ///     DELETE /api/color/1
        ///
        /// </remarks>
        /// <param name="id"></param> 
        /// <response code="200">Retorna el color amb el nom especificat</response>
        /// <response code="401">No s'ha pogut esborrar</response>
        [Authorize]
        [HttpDelete("{id}", Name = "DeleteColor")]
        public async Task<IActionResult> DeleteById(int id)
        {
            return (await _repository.Delete(id))
                        ? (IActionResult)Ok()
                        : NoContent();
        }

    }
}
