using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace server.dtos
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password, ErrorMessage = "Please try a more secure password")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}