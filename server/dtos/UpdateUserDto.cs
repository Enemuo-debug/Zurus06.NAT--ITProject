using System.ComponentModel.DataAnnotations;

namespace server.dtos;

public class UpdateUserDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;
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
}
