using Cursus.Services;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InstructorController : ControllerBase
{
    private readonly IInstructorService _instructorService;

    public InstructorController(IInstructorService instructorService)
    {
        _instructorService = instructorService;
    }
    
    [HttpGet("all-get-public-profile")]
    public async Task<IActionResult> GetAllPublicProfile()
    {
        var result = await _instructorService.GetAllInstructorPublicProfile();
        return StatusCode(result._statusCode, result);
    }

    [HttpGet("get-public-profile")]
    public async Task<IActionResult> GetPublicProfile(Guid instructorId)
    {
        var result = await _instructorService.GetInstructorPublicProfile(instructorId);
        return StatusCode(result._statusCode, result);
    }
}