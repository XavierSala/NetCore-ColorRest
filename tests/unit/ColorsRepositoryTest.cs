using System;
using System.Collections.Generic;
using System.Linq;
using colorsRest.Models;
using colorsRest.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;


namespace colorsRest.Tests.UnitTests
{
    public class ColorsRepositoryTests
    {

        private Mock<ColorsContext> colorsContextMock;
        private Mock<DbSet<Color>> colorsSetMock;
        public ColorsRepositoryTests()
        {
            var dades = Helper.TestColors;
            colorsSetMock = Helper.CreateDbSetMock(dades);
            colorsContextMock = new Mock<ColorsContext>();
            colorsContextMock.Setup(x => x.Colors).Returns(colorsSetMock.Object as DbSet<Color>);
        }


        [Fact]
        public void UnnecessariTestForGetAllColors()
        {
            // Given
            var dades = Helper.TestColors;

            // When
            var repository = new ColorsRepository(colorsContextMock.Object);
            var actual = repository.Get();

            // Then: Ha de tornar tots els colors
            Assert.Equal(dades.Count, actual.Count);
            // El primer és ...
            Assert.Equal(dades[0].Nom, actual.First().Nom);
        }

        [Fact]
        public void TestIfGetColorByIdWorksOk()
        {
            // Given
            int id = 1;
            var expected = Helper.TestColors[id];

            colorsContextMock.Setup(x => x.Colors.Find(id)).Returns(expected);

            // When
            var repository = new ColorsRepository(colorsContextMock.Object);
            var actual = repository.Get(id);

            // Then: Ha de tornar tots els colors
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Nom, actual.Nom);
            Assert.Equal(expected.Rgb, actual.Rgb);
        }


        [Fact]
        public void TestIfCreateColorIfItIsCorrect()
        {
            // Given
            var nouColor = new Color
            {
                Id = 0,
                Nom = "Beix",
                Rgb = "#FAFADF"
            };

            // When - Add the color
            var repository = new ColorsRepository(colorsContextMock.Object);
            repository.Add(nouColor);

            Assert.NotNull(colorsContextMock);
            // Then - Verifies the color is added once and saved.
            colorsSetMock.Verify(m => m.Add(It.IsAny<Color>()), Times.Once);
            colorsContextMock.Verify(m => m.SaveChanges(), Times.Exactly(1));
        }

    }
}