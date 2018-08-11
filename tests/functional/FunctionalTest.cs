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

namespace colorsRest.Tests.FuncionalTests
{
    public class ColorRestTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

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
            Color data = await Json2Color(response).ConfigureAwait(false);
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
            var response = await _client.PostAsync("/api/colors", Color2Json(colorToAdd));

            // Then
            response.EnsureSuccessStatusCode();

            // Comprovar que retorna l'afegit
            var data = await Json2Color(response).ConfigureAwait(false);
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
            var response = await _client.PostAsync("/api/colors", Color2Json(colorToAdd));

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
            var response = await _client.PostAsync("/api/colors", Color2Json(colorToAdd));

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
            var response = await _client.PostAsync("/api/colors", Color2Json(colorToAdd));

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
            var response = await _client.PostAsync("/api/colors", Color2Json(colorToAdd));

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }



        private static StringContent Color2Json(Color colorToAdd)
        {
            var content = JsonConvert.SerializeObject(colorToAdd);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            return stringContent;
        }

        private static async Task<Color> Json2Color(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Color>(json);
            return data;
        }

    }
}