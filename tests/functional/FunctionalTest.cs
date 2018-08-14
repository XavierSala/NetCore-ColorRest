using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Newtonsoft.Json;
using System.Text;
using FluentAssertions;

using colorsRest.Models;
using System.Net.Http.Headers;

namespace colorsRest.Tests.FuncionalTests
{
    public class ColorRestTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string DefaultUsername = "me@localhost";
        private const string DefaultUserPassword = "Ies2010!";

        private readonly HttpClient _client;
        private readonly string token;

        public ColorRestTests(
            WebApplicationFactory<Startup> webAppFactory)
        {
            var testWebAppFactory = webAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Crear una nova BDD amb base de dades i serveis (que no calen)
                    var serviceProvider = new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider();

                    // Crear una base de dades només per fer les proves
                    services.AddDbContext<ColorsContext>(options =>
                        {
                            options.UseInMemoryDatabase("InMemoryDbForTests");
                            options.UseInternalServiceProvider(serviceProvider);
                        });

                    // No cal però l'hi deixo com a referència
                    var sp = services.BuildServiceProvider();

                    // context (ColorsContext).
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<ColorsContext>();
                        var logger = scopedServices
                            .GetRequiredService<ILogger<ColorRestTests>>();

                        // Comprovar que s'ha creat la base de dades
                        db.Database.EnsureCreated();

                        try
                        {
                            // Entrar les dades d'exemple
                            Utilities.InitializeDbForTests(db);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred seeding the " +
                                "database with colors. Error: {ex.Color}");
                        }
                    }
                });
            });

            // Crear el client per enviar les peticions al servidor
            _client = testWebAppFactory.CreateDefaultClient();
            token = RegisterUser(DefaultUsername, DefaultUserPassword);
        }

        public class Token
        {
            public string token { get; set; }
        }

        private string RegisterUser(string email, string password)
        {
            var response = _client.PostAsync("/api/user/Register",
                                             Utilities.User2Json(
                                             email,
                                             password));
            var content = response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Token>(content.Result);
            return result.token;
        }



        /// Comprovar que no es pot tornar a crear el mateix usuari dos cops
        [Fact]
        public async Task NoEsPotCrearElMateixUsuariDosCops()
        {
            // Given: Default user defined

            // When
            var response = await _client.PostAsync("/api/user/Register",
                                             Utilities.User2Json(
                                             DefaultUsername,
                                             DefaultUserPassword));

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// Comprovar que es no es pot crear un nou usuari sense una contrasenya
        /// que tingui números, símbols, majúscules i minúscules o si és de menys
        /// de 6 dígits
        [Theory]
        [InlineData("userko1", "LesNenesMaquesAlDemati")]
        [InlineData("userko2", "")]
        [InlineData("userko3", "patatesfregides")]
        [InlineData("userko4", "X1ab!")]
        [InlineData("userko5@localhost", "Les4NenesMaques")]
        public async Task NoEsPotCrearUnNouUsuariAlSistemaAmbMalaContrasenya(string username, string password)
        {
            // Given

            // When
            var response = await _client.PostAsync("/api/user/Register",
                                             Utilities.User2Json(
                                             username,
                                             password));

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// Comprovar que es pot crear un nou usuari amb contrasenya correcta
        /// Amb els caràcters correctes i 6 dígits
        [Theory]
        [InlineData("userok@localhost", "Les4NenesMaques!")]
        [InlineData("userok2@localhost", "Sis6s!")]
        public async Task EsPotCrearUnNouUsuariAlSistema(string username, string password)
        {
            // Given

            // When
            var response = await _client.PostAsync("/api/user/Register",
                                             Utilities.User2Json(
                                             username,
                                             password));

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        /// Un usuari que no existeix no pot fer login ...
        [Fact]
        public async Task NoEsPotFerLoginSiNoEsUnUsuariExistent()
        {
            // Given
            var username = "vader";
            var password = "ImThe4ce!";

            // When
            var response = await _client.PostAsync("/api/user/Login",
                                             Utilities.User2Json(
                                             username,
                                             password)
            );

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Contain("Invalid Login");
        }

        [Fact]
        public async Task SiRegistresUnNouUsuariPotsFerLogin()
        {
            // Given un usuari que es registra
            var username = "vader@localhost";
            var password = "ImThe4ce!";

            var response = await _client.PostAsync("/api/user/Register",
                                             Utilities.User2Json(
                                             username,
                                             password)
            );
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // When fa Login
            response = await _client.PostAsync("/api/user/Login",
                                             Utilities.User2Json(
                                             username,
                                             password)
            );

            // Then Hauria de rebre el token
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Contain("token");
        }

        /// Comprovar que amb dades correctes el resultat es torna bé
        ///
        public static IEnumerable<object[]> CorrectResults =>
        new List<object[]>
        {
            new object[] { 1, new Color {Id=1, Nom="vermell", Rgb="#FF0000"} },
            new object[] { 2, new Color {Id=2, Nom="verd", Rgb="#00FF00" } },
            new object[] { 3, new Color {Id=3, Nom="negre", Rgb="#000000" } },
        };

        [Theory]
        [MemberData(nameof(CorrectResults))]
        public async Task GetAnCorrectColorFromService(int id, Color expected)
        {
            // Act
            var response = await _client.GetAsync("/api/colors/" + id);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
            Color data = await Utilities.Json2Color(response).ConfigureAwait(false);
            Assert.Equal(expected.Id, data.Id);
            Assert.Equal(expected.Nom, data.Nom);
            Assert.Equal(expected.Rgb, data.Rgb);
        }



        /// Comprovar que dóna 404 quan s'intenta anar a recuperar
        /// dades que **no existeixen**
        public static IEnumerable<object[]> FailedResults =>
        new List<object[]>
        {
            new object[] { 0 },
            new object[] { 99 },
            new object[] { -1 },
        };

        [Theory]
        [MemberData(nameof(FailedResults))]
        public async Task GetAnInexistentColorFromService(int id)
        {
            // Act
            var response = await _client.GetAsync("/api/colors/" + id);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Contain("Not Found");
        }

        /// Comprovar que els elements s'afegeixen bé
        public static IEnumerable<object[]> NewCorrectElements =>
        new List<object[]>
        {
            new object[] {"blanc", "#FFFFFF" },
            new object[] {"blau", "#0000FF" }
        };


        [Fact]
        public async Task AddElementShouldFailWithNonAuthenticatedUsers()
        {
            // Given
            var colorToAdd = new Color
            {
                Nom = "correcte",
                Rgb = "#CACA00"
            };

            // When
            var response = await _client.PostAsync("/api/colors", Utilities.Color2Json(colorToAdd));

            // Then
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        }

        [Fact]
        public async Task AddElementShouldFailWithInvalidToken()
        {
            // Given
            var colorToAdd = new Color
            {
                Nom = "correcte",
                Rgb = "#CACA00"
            };

            // When
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "ErrareHumanumEst");

            var response = await _client.PostAsync("/api/colors", Utilities.Color2Json(colorToAdd));

            // Then
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        }



        [Theory]
        [MemberData(nameof(NewCorrectElements))]
        public async Task AddElementsShouldWorkIfDataIsCorrect(string nom, string codi)
        {
            // Given
            var colorToAdd = new Color
            {
                Nom = nom,
                Rgb = codi
            };

            // When
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PostAsync("/api/colors", Utilities.Color2Json(colorToAdd));

            // Then
            response.EnsureSuccessStatusCode();

            // Comprovar que retorna l'afegit
            var data = await Utilities.Json2Color(response).ConfigureAwait(false);
            Assert.NotEqual(0, data.Id);
            Assert.Equal(nom, data.Nom);
            Assert.Equal(codi, data.Rgb);
        }


        /// Comprovar que els elements no s'afegeixen quan no hi ha nom
        [Fact]
        public async Task AddElementsShouldFailWithoutNom()
        {
            // Given
            var colorToAdd = new Color
            {
                Rgb = "#FF00FF"
            };

            // When
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PostAsync("/api/colors", Utilities.Color2Json(colorToAdd));

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Contain("The Nom field is required");
        }

        /// Comprovar que els elements no s'afegeixen quan no hi ha nom i el
        /// Rgb està mal format.
        [Theory]
        [InlineData("#FF")]
        [InlineData("FFFFFF")]
        [InlineData("#")]
        [InlineData("#FFFFFF0")]
        [InlineData("#XXXXXX")]
        public async Task AddElementsShouldFailWithoutCorrectRgbCode(string data)
        {
            // Given
            var colorToAdd = new Color
            {
                Rgb = data
            };

            // When
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PostAsync("/api/colors", Utilities.Color2Json(colorToAdd));

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Contain(Color.getRGBError());
        }

        /// Comprovar que els elements no s'afegeixen quan no hi ha res i que donen error
        /// em tots dos camps
        [Fact]
        public async Task AddElementsNoData()
        {
            // Given
            var colorToAdd = new Color();

            // When
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PostAsync("/api/colors", Utilities.Color2Json(colorToAdd));

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Contain("The Nom field is required")
                               .And.Contain("The Rgb field is required");
        }




        /// Comprovar que afegir elements inventant-se un Id dóna error
        public static IEnumerable<object[]> NewDuplicatedElements =>
        new List<object[]>
        {
            new object[] {new Color { Id=1, Nom="fail", Rgb="#CACACA"} },
            new object[] {new Color { Id=25, Nom="alsoFail", Rgb="#BACABA"} },
        };

        [Theory]
        [MemberData(nameof(NewDuplicatedElements))]
        public async Task AddElementsShouldNotPermitIdEspcification(Color colorToAdd)
        {
            // Given

            // When
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PostAsync("/api/colors", Utilities.Color2Json(colorToAdd));

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

    }
}