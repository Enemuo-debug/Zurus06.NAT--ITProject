using Microsoft.AspNetCore.Mvc;
using server.Interfaces;

namespace server.controllers;
[ApiController]
[Route("diagrams")]
public class SimulationControllers
{
    private readonly ISimulation simRepo;
    public SimulationControllers(ISimulation _simRepo)
    {
        simRepo = _simRepo;
    }
}
