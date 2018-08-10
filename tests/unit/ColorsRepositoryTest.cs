using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using colorsRest.Controllers;
using colorsRest.Models;
using colorsRest.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;
using FluentAssertions;
using System.Net;
using colorsRest.Exceptions;

namespace colorsRest.Tests.UnitTests
{
    public class ColorsRepositoryTests
    {

        private List<Color> getDefaultData()
        {
            return new List<Color>
            {
                new Color
                {
                    Id = 1,
                    Nom = "vermell",
                    Rgb = "#FF0000"
                },
                new Color
                {
                    Id = 2,
                    Nom = "blau",
                    Rgb = "#0000FF"
                }

            };
        }

        private static Mock<DbSet<T>> CreateDbSetMock<T>(IEnumerable<T> elements) where T : class
        {
            var elementsAsQueryable = elements.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();

            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(elementsAsQueryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(elementsAsQueryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(elementsAsQueryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(elementsAsQueryable.GetEnumerator());

            return dbSetMock;
        }


        [Fact]
        public void UnnecessariTestForGetAllColors()
        {
            // Given
            var dades = getDefaultData();
            var colorsMock = CreateDbSetMock(dades);
            var colorsContextMock = new Mock<ColorsContext>();
            colorsContextMock.Setup(x => x.Colors).Returns(colorsMock.Object);

            // When
            var repository = new ColorsRepository(colorsContextMock.Object);
            var actual = repository.Get();

            // Then: Ha de tornar tots els colors
            Assert.Equal(dades.Count, actual.Count());
            // El primer és ...
            Assert.Equal(dades[0].Nom, actual.First().Nom);
        }

        [Fact]
        public void TestIfGetColorByIdWorksOk()
        {
            // Given
            var dades = getDefaultData();
            var id = 0;
            var expected = dades[id];

            var colorsMock = CreateDbSetMock(dades);
            var colorsContextMock = new Mock<ColorsContext>();
            colorsContextMock.Setup(x => x.Colors).Returns(colorsMock.Object);
            colorsContextMock.Setup(x => x.Colors.Find(id)).Returns(dades[id]);

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

            var colorsSetMock = CreateDbSetMock(getDefaultData());
            var colorsContextMock = new Mock<ColorsContext>();
            colorsContextMock.Setup(x => x.Colors).Returns(colorsSetMock.Object);

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