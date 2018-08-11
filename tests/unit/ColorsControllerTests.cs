using System.Collections.Generic;
using System.Threading.Tasks;
using colorsRest.Controllers;
using colorsRest.Models;
using colorsRest.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using System.Net;
using colorsRest.Exceptions;

namespace colorsRest.Tests.UnitTests
{
    public class ColorsControllerTests
    {
        readonly Mock<IColorsRepository> _mockRepo;
        readonly ColorsController _controller;

        public ColorsControllerTests()
        {
            ILogger<ColorsController> _mockLogger = new Mock<ILogger<ColorsController>>().Object;
            _mockRepo = new Mock<IColorsRepository>();
            _controller = new ColorsController(_mockRepo.Object, _mockLogger);
        }

        [Fact]
        public void TestIfGetAllReturnsAllColors()
        {
            // Given
            var expectedItems = Helper.TestColors;
            var expectedItemsCount = expectedItems.Count;
            _mockRepo.Setup(repo => repo.Get()).Returns((expectedItems));

            // When
            var result = _controller.GetAll();

            // Then
            var items = Assert.IsType<JsonResult>(result.Result).Value as List<Color>;
            Assert.NotNull(items);
            Assert.Equal(expectedItemsCount, items.Count);
            items.Should().HaveCount(expectedItemsCount).And.BeEquivalentTo(expectedItems);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void TestIfGetByIdReturnsElementRequested(int element)
        {
            // Given
            var expected = Helper.TestColors[element];
            _mockRepo.Setup(repo => repo.Get(element)).Returns((expected));

            // When
            var result = _controller.GetById(element);

            // Then
            var okObjectResult = result as OkObjectResult;
            Assert.NotNull(okObjectResult);

            var item = okObjectResult.Value as Color;
            Assert.NotNull(item);

            item.Should().Equals(expected);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public void TestIfGetByIdReturnsNotFoundInexistentColor(int element)
        {
            // Given
            _mockRepo.Setup(repo => repo.Get(element)).Returns((Color)null);

            // When
            var result = _controller.GetById(element);

            // Then
            var notfound = result as NotFoundObjectResult;
            Assert.NotNull(notfound);

            notfound.Should().BeOfType<NotFoundObjectResult>()
                .Which.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            var content = notfound.Value;
            var message = content.GetType().GetProperty("message").GetValue(content, null) as string;
            Assert.Equal("Not Found", message);
        }


        [Fact]
        public void TestIfAddCorrectColorReturnsCreatedResponse()
        {
            // Given
            var expected = Helper.TestColors[0];
            Color colorToAdd = new Color();
            colorToAdd.Nom = expected.Nom;
            colorToAdd.Rgb = expected.Rgb;

            // When
            var result = _controller.Add(colorToAdd);

            // Then
            Assert.IsType<CreatedAtActionResult>(result);

        }

        [Fact]
        public void TestIfAddCorrectColorRetursResponseHasCreatedItem()
        {
            // Given
            var colorToAdd = Helper.TestColors[2];

            // When
            var createdResponse = _controller.Add(colorToAdd) as CreatedAtActionResult;
            Assert.NotNull(createdResponse);
            var item = createdResponse.Value as Color;

            // Then
            Assert.IsType<Color>(item);
            Assert.Equal(colorToAdd.Nom, item.Nom);
            Assert.Equal(colorToAdd.Rgb, item.Rgb);
        }

        [Fact]
        public void TestIfAddInCorrectColorReturnsBadRequest()
        {
            // Given
            Color errorColor = new Color
            {
                Rgb = "#000000"
            };

            _controller.ModelState.AddModelError("Nom", "Required");

            // When
            var badResponse = _controller.Add(errorColor);

            // Then
            Assert.IsType<BadRequestObjectResult>(badResponse);
        }

        [Fact]
        public void TestIfAddColorWithIdReturnsBadRequest()
        {
            // Given
            Color colorToAdd = Helper.TestColors[0];
            _mockRepo.Setup(repo => repo.Add(colorToAdd)).Throws(new ColorException("You can't give an Id"));


            // When
            var badResponse = _controller.Add(colorToAdd);

            // Then
            Assert.IsType<BadRequestObjectResult>(badResponse);
        }

        [Fact]
        public void TestIfAddColorWithIdReturnsBadRequestText()
        {
            // Given
            Color colorToAdd = Helper.TestColors[0];
            var expectedMessage = "You can't give an Id";
            _mockRepo.Setup(repo => repo.Add(colorToAdd)).Throws(new ColorException(expectedMessage));

            // When
            var badResponse = _controller.Add(colorToAdd) as BadRequestObjectResult;

            // Then
            Assert.IsType<BadRequestObjectResult>(badResponse);
            var content = badResponse.Value;
            var message = content.GetType().GetProperty("message").GetValue(content, null) as string;
            Assert.Equal(expectedMessage, message);
        }

        [Fact]
        public async Task TestIfDeleteByWithCorrectIdReturnsOk()
        {
            // Given
            int idToDelete = 1;
            _mockRepo.Setup(repo => repo.Delete(idToDelete)).Returns(Task.FromResult(true));

            // When
            var result = await _controller.DeleteById(idToDelete);

            // Then
            var theresult = result as OkResult;
            Assert.NotNull(theresult);
        }

        [Fact]
        public async Task TestIfDeleteByWithInexistentIdReturnsOk()
        {
            // Given
            int idToDelete = 99999;
            _mockRepo.Setup(repo => repo.Delete(idToDelete)).Returns(Task.FromResult(false));

            // When
            var result = await _controller.DeleteById(idToDelete);

            // Then
            var theresult = result as NoContentResult;
            Assert.NotNull(theresult);
        }
    }

}
