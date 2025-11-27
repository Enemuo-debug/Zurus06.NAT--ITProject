using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace server.dtos
{
    public class NewPostDto
    {
        [Required]
        [MaxLength(150, ErrorMessage = "Post Caption cannot exceed 150 characters.")]
        [MinLength(3, ErrorMessage = "Post Caption must be at least 3 character long.")]
        public required string Caption { get; set; }

        [MaxLength(700, ErrorMessage = "Post Caption cannot exceed 650 characters.")]
        public required string Intro { get; set; }
    }
}