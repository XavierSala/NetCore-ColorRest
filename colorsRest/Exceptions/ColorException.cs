using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using colorsRest.Models;
using colorsRest.Repository;

namespace colorsRest.Exceptions
{
    public class ColorException : Exception
    {

        public ColorException()
        {
        }

        public ColorException(string message)
            : base(message)
        {
        }

        public ColorException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}