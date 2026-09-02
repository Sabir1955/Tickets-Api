
using System.ComponentModel.DataAnnotations;

namespace Authentication.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Name must be between 2 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$",
            ErrorMessage = "Name can contain only letters and spaces.")] 
        public string Name { get; set; } = "Example";

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(
     @"^[a-zA-Z0-9._%+-]+@gmail\.com$",
     ErrorMessage = "Email must be a valid Gmail address, for example: example@gmail.com."
 )]
        public string Email { get; set; } = "";

        public string Password { get; set; } = "";
    }
}






