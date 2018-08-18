using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace colorsRest.Controllers
{
    [Route("/api/[controller]/[action]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public UserController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Un usuari existent entra en el sistema i rep un token.
        /// </summary>
        /// <remarks>
        /// Exemple:
        ///
        ///     POST /user/Login
        ///     {
        ///        "Email": "usuari@exemple.com",
        ///        "Password": "Un2Tres!"
        ///     }
        ///
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>El token JWT</returns>
        /// <response code="200">Retorna el token JWT per l'usuari</response>
        /// <response code="400">Hi ha hagut algun problema</response>         
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);
                var token = GenerateJwtToken(model.Email, appUser);
                return Ok(
                    new TokenResult { Token = token }
                ); 
            }

            return BadRequest(new Error { Message = "Invalid Login" });
        }

        /// <summary>
        /// Un usuari es registra en el sistema i rep un token JWT.
        /// </summary>
        /// <remarks>
        /// Exemple:
        ///
        ///     POST /user/Register
        ///     {
        ///        "Email": "usuari@exemple.com",
        ///        "Password": "Un2Tres!"
        ///     }
        ///
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>El token JWT</returns>
        /// <response code="200">Retorna el token JWT per l'usuari</response>
        /// <response code="400">Hi ha hagut algun problema</response>         
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                var token = GenerateJwtToken(model.Email, user);
                return Ok(
                    new TokenResult { Token = token }
                );
            }
            
            return BadRequest(new { message = result.Errors });
        }

        private string GenerateJwtToken(string email, IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}