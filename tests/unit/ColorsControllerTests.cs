using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using colorsRest.Controllers;
using colorsRest.Models;
using colorsRest.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;
using FluentAssertions;
using System.Net;

namespace colorsRest.Tests.UnitTests
{
    public class ColorsControllerTests
    {
        ILogger<ColorsController> _mockLogger;

        public ColorsControllerTests()
        {
            _mockLogger = new Mock<ILogger<ColorsController>>().Object;
        }

        private List<Color> GetTestColors()
        {
            var colors = new List<Color>();
            colors.Add(new Color()
            {
                Nom = "vermell",
                Id = 1,
                Rgb = "#FF0000"
            });

            colors.Add(new Color()
            {
                Nom = "verd",
                Id = 1,
                Rgb = "#00FF00"
            });
            return colors;
        }

        [Fact]
        public void GetAll_ReturnsAllColors()
        {
            // Given
            var mockRepo = new Mock<IColorsRepository>();

            mockRepo.Setup(repo => repo.Get()).Returns((GetTestColors()));
            var controller = new ColorsController(mockRepo.Object, _mockLogger);

            // When
            var result = controller.GetAll();

            // Then
            var items = Assert.IsType<JsonResult>(result.Result).Value as List<Color>;
            Assert.Equal(2, items.Count);
            items.Should().HaveCount(2).And.BeEquivalentTo(GetTestColors());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void GetById_ReturnsElementRequested(int element)
        {
            // Given
            var mockRepo = new Mock<IColorsRepository>();
            var expected = GetTestColors()[0];

            mockRepo.Setup(repo => repo.Get(element)).Returns((expected));
            var controller = new ColorsController(mockRepo.Object, _mockLogger);

            // When
            var result = controller.GetById(element);

            // Then
            var okObjectResult = result as OkObjectResult;
            Assert.NotNull(okObjectResult);

            var item = okObjectResult.Value as Color;
            Assert.NotNull(item);

            // Assert.IsType<Color>(item);
            item.Should().Equals(expected);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public void GetById_ReturnsNotFoundInexistentColor(int element)
        {
            // Given
            var mockRepo = new Mock<IColorsRepository>();
            var expected = GetTestColors()[0];
            Color noResult = null;

            mockRepo.Setup(repo => repo.Get(element)).Returns(noResult);
            var controller = new ColorsController(mockRepo.Object, _mockLogger);

            // When
            var result = controller.GetById(element);

            // Then

            var notfound = result as NotFoundObjectResult;
            Assert.NotNull(notfound);

            notfound.Should().BeOfType<NotFoundObjectResult>()
                .Which.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            var content = notfound.Value;
            var message = content.GetType().GetProperty("message").GetValue(content, null) as string;
            Assert.Equal(message, "Not Found");
        }
    }

}
