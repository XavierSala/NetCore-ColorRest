
using System.ComponentModel.DataAnnotations;

namespace colorsRest.Models 
 {
     public class Color
     {
         public int Id { get; set; }
         
         [Required]
         public string Nom { get; set; }

         [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Must be a hexadecimal value")]

         public string Rgb { get; set; }
     }
 }