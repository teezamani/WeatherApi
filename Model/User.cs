using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WeatherApp.Model
{
    public class User : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
    }
}