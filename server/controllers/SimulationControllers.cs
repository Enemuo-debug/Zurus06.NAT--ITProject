using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.dtos;
using server.Interfaces;
using server.NATModels;

namespace server.controllers;
[ApiController]
[Route("diagrams")]
public class SimulationControllers: ControllerBase  
{
    private readonly ISimulation simRepo;
    private readonly IPosts postsRepo;
    public SimulationControllers(ISimulation _simRepo, IPosts _postsRepo)
    {
        simRepo = _simRepo;
        postsRepo = _postsRepo;
    }

    [Authorize]
    [HttpPost("new")]
    public async Task<IActionResult> CreateNewSimulation([FromBody] newSim createSimulationDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var ownerId = await postsRepo.GetLoggedInUser(User);
        HTTPResponseStructure response;
        if (ownerId == null)
        {
            response = new HTTPResponseStructure(false, "User not found");
            return Unauthorized(response);
        }
        var result = await simRepo.CreateNewSimulation(createSimulationDto.diagramName, ownerId.Id);
        if (!result)
        {
            response = new HTTPResponseStructure(false, "Could not create simulation");
            return BadRequest(response);
        }
        response = new HTTPResponseStructure(true, "Simulation created successfully");
        return Ok(response);
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetSimulationsByOwner()
    {
        var ownerId = await postsRepo.GetLoggedInUser(User);
        HTTPResponseStructure response;
        if (ownerId == null)
        {
            response = new HTTPResponseStructure(false, "User not found");
            return Unauthorized(response);
        }
        var simulations = await simRepo.GetSimulationsByOwnerId(ownerId.Id);
        response = new HTTPResponseStructure(true, "Simulations fetched successfully", simulations);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("{simulationId}")]
    public async Task<IActionResult> GetSimulationById([FromRoute] int simulationId)
    {
        var user = await postsRepo.GetLoggedInUser(User);
        HTTPResponseStructure response;
        if (user == null)
        {
            response = new HTTPResponseStructure(false, "User not found");
            return Unauthorized(response);
        }

        var simulation = await simRepo.GetSimulationById(simulationId);
        if (simulation == null)
        {
            response = new HTTPResponseStructure(false, "Simulation not found");
            return NotFound(response);
        }

        if (simulation.OwnerId != user.Id)
        {
            return Forbid();
        }

        response = new HTTPResponseStructure(true, "Simulation fetched successfully", simulation);
        return Ok(response);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSimulation(
        [FromRoute] int id,
        [FromBody] UpdateSimulationDTO dto)
    {
        var user = await postsRepo.GetLoggedInUser(User);
        if (user == null)
        {
            Console.WriteLine("User is null in UpdateSimulation");
            return Unauthorized(new HTTPResponseStructure(false, "User not found"));
        }

        var simulation = await simRepo.GetSimulationById(id);
        if (simulation == null)
        {
            Console.WriteLine("Simulation is null in UpdateSimulation");
            return NotFound(new HTTPResponseStructure(false, "Simulation not found"));
        }

        if (simulation.OwnerId != user.Id)
        {
            Console.WriteLine("User is not the owner in UpdateSimulation");
            return Unauthorized(new HTTPResponseStructure(false, "You cannot modify this simulation"));
        }

        bool updated = await simRepo.UpdateSimulationData(id, dto.DataJson);

        if (!updated)
        {
            Console.WriteLine("Failed to update simulation in UpdateSimulation");
            return BadRequest(new HTTPResponseStructure(false, "Could not update simulation"));
        }

        Console.WriteLine("Simulation updated successfully in UpdateSimulation");
        return Ok(new HTTPResponseStructure(true, "Simulation updated successfully"));
    }

    [Authorize]
    [HttpDelete("{simulationId}")]
    public async Task<IActionResult> DeleteSimulation([FromRoute] int simulationId)
    {
        var ownerId = await postsRepo.GetLoggedInUser(User);
        HTTPResponseStructure response;
        if (ownerId == null)
        {
            response = new HTTPResponseStructure(false, "User not found");
            return Unauthorized(response);
        }
        var simulation = await simRepo.GetSimulationById(simulationId);
        if (simulation == null || simulation.OwnerId != ownerId.Id)
        {
           response = new HTTPResponseStructure(false, "Simulation not found or access denied");
            return NotFound(response);
        }
        var result = await simRepo.DeleteSimulation(simulationId);
        if (!result)
        {
            response = new HTTPResponseStructure(false, "Could not delete simulation");
            return BadRequest(response);
        }
        response = new HTTPResponseStructure(true, "Simulation deleted successfully");
        return Ok(response);
    }
}
