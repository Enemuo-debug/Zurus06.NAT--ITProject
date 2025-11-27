using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace server.dtos
{
    public class CreateAccountDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
        [Required]
        [MinLength(30, ErrorMessage = "Your bio should be at least 30 charachters long")]
        [MaxLength(300, ErrorMessage = "Your bio should be not exceed 300 charachters in length")]
        public required string Bio { get; set; }
        [Required]
        [Range(1, 5, MaximumIsExclusive = false, MinimumIsExclusive = false, ErrorMessage = "The nice value must be within the range 1-5")]
        public int Niche { get; set; }
        [Required]
        [MinLength(5, ErrorMessage = "Please choose a longer Display Name")]
        public string DisplayName { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password, ErrorMessage = "Please try a more secure password")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}