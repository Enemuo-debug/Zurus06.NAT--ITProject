using System.ComponentModel.DataAnnotations;

namespace server.dtos;

public class newSim
{
    [Required(ErrorMessage = "Diagram name is required")]
    public required string diagramName { get; set; }
}
