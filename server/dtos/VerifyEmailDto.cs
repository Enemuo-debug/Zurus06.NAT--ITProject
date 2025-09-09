using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace server.dtos
{
    public class VerifyEmailDto
    {
        [Required]
        [EmailAddress]
        public required string EmailAddress { get; set; }
    }
}