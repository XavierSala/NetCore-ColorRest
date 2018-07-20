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
using colorsRest.Models;
using colorsRest.Controllers;
using Newtonsoft.Json;

namespace colorsRest.Tests.IntegrationTests
{
    public class IndexPageTests : IClassFixture<WebApplicationFactory<colorsRest.Startup>>
    {
        private readonly HttpClient _client;

        public IndexPageTests(
            WebApplicationFactory<colorsRest.Startup> webAppFactory)
        {
            var testWebAppFactory = webAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Create a new service provider.
                    var serviceProvider = new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider();

                    // Add a database context (AppDbContext) using an in-memory 
                    // database for testing.
                    services.AddDbContext<ColorsContext>(options =>
                        {
                            options.UseInMemoryDatabase("InMemoryDbForTests");
                            options.UseInternalServiceProvider(serviceProvider);
                        });

                    // Build the service provider.
                    var sp = services.BuildServiceProvider();

                    // Create a scope to obtain a reference to the database
                    // context (ColorsContext).
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<ColorsContext>();
                        var logger = scopedServices
                            .GetRequiredService<ILogger<IndexPageTests>>();

                        // Ensure the database is created.
                        db.Database.EnsureCreated();

                        try
                        {
                            // Seed the database with test data.
                            Utilities.InitializeDbForTests(db);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"An error occurred seeding the " +
                                "database with colors. Error: {ex.Color}");
                        }
                    }
                });
            });

            // Create an HttpClient to submit requests against the test host.
            _client = testWebAppFactory.CreateDefaultClient();
        }

        public static IEnumerable<object[]> CorrectResults =>
        new List<object[]>
        {
            new object[] { 1, new Color(){Id=1, Nom="vermell", Rgb="#FF0000"} },
            new object[] { 3, new Color(){Id=3, Nom="negre", Rgb="#000000" } },           
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

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Color>(json);
            Assert.Equal(expected.Id, data.Id);
            Assert.Equal(expected.Nom, data.Nom);
            Assert.Equal(expected.Rgb, data.Rgb);
        }

    }
}