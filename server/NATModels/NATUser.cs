using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.tools;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace server.NATModels
{
    public class NATUser : IdentityUser
    {
        [Required]
        [MinLength(30, ErrorMessage = "Your bio should be at least 30 charachters long")]
        [MaxLength(300, ErrorMessage = "Your bio should be not exceed 300 charachters in length")]
        public required string Bio { get; set; }
        public Niche niche { get; set; } = Niche.Network_Design;
        public string CreatedOn { get; set; } = DateTime.Now.ToString("dd MM yyyy");
        public string DisplayName { get; set; } = string.Empty;
    }
}