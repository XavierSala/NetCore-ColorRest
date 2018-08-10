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
            Assert.Equal(dades[0].Nom, actual.First().Nom);
        }

    }
}