using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.dtos;
using server.Interfaces;
using server.NATModels;
using server.tools;

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

    [Authorize]
    [HttpGet("embed-code/{simulationId}")]
    public async Task<IActionResult> GetEmbedCode([FromRoute] int simulationId)
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

        var embedCode = Environment.GetEnvironmentVariable("URL") + $"diagrams/embed/{user.Id}/{simulationId}";
        response = new HTTPResponseStructure(true, "Embed code generated successfully", embedCode);
        return Ok(response);
    }

    [HttpGet("embed/{ownerId}/{simulationId}")]
    public async Task<IActionResult> GetSimulationEmbed([FromRoute] string ownerId, [FromRoute] int simulationId)
    {
        var simulation = await simRepo.GetSimulationById(simulationId);
        HTTPResponseStructure response;
        if (simulation == null || simulation.OwnerId != ownerId)
        {
            response = new HTTPResponseStructure(false, "Simulation not found or access denied");
            return NotFound(response);
        }
        response = new HTTPResponseStructure(true, "Simulation embed fetched successfully", simulation.DataJson);
        return Ok(response);
    }

    [HttpPost("verify-embed/{postId}")]
    public async Task<IActionResult> VerifyEmbed([FromRoute] int postId, [FromBody] string embedCode)
    {
        Console.WriteLine(embedCode);
        var user = await postsRepo.GetLoggedInUser(User);
        HTTPResponseStructure response;
        if (user == null)
        {
            response = new HTTPResponseStructure(false, "User not found");
            return Unauthorized(response);
        }

        string isValid = await simRepo.VerifyEmbed(postId, embedCode, user.Id);
        if (string.IsNullOrEmpty(isValid))
        {
            response = new HTTPResponseStructure(false, "Embed verification failed");
            return BadRequest(response);
        }

        response = new HTTPResponseStructure(true, "Embed verified successfully", isValid);
        return Ok(response);
    }

    [HttpGet("sssp/{simId}/{startId}")]
    public async Task<IActionResult> DjikstraAlgorithm ([FromRoute] int simId, [FromRoute] string startId)
    {
        HTTPResponseStructure response;
        Graph simGraph = await simRepo.ConvertSimulationToGraphForAnalysis(simId);
        if (simGraph == null)
        {
            response = new HTTPResponseStructure(false, "Server error");
            return StatusCode(500, response);
        }

        List<string> SSSP = await simRepo.SingleSourceShortestPathAlgorithm(simGraph!, startId);
        response = new HTTPResponseStructure(true, "Algorithm Complete", SSSP);
        return Ok(response);
    }

    [Authorize]
    [HttpPut("rename/{id}")]
    public async Task<IActionResult> RenameSimulation ([FromRoute] int id, [FromBody] string name)
    {
        var user = await postsRepo.GetLoggedInUser(User);
        HTTPResponseStructure response;
        if (user == null)
        {
            response = new HTTPResponseStructure(false, "User not found");
            return Unauthorized(response);
        }

        var NATSim = await simRepo.GetSimulationById(id);
        if (NATSim == null) {
            response = new HTTPResponseStructure(false, "Simulation doesn't exist");
            return NotFound(response);
        }

        if (NATSim.OwnerId != user.Id)
        {
            response = new HTTPResponseStructure(false, "You cannot modify this simulation");
            return Unauthorized(response);
        }

        var result = await simRepo.RenameSim(name, id);
        if (!result)
        {
            response = new HTTPResponseStructure(false, "Could not rename simulation");
            return BadRequest(response);
        }

        response = new HTTPResponseStructure(true, "Simulation renamed successfully");
        return Ok(response);
    }
}
