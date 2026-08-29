using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            message = "API is running"
        });
    }
    [HttpGet("/")] 
    public IActionResult GetRoot() 
    { 
        return Ok(new 
        { 
            message = "ASP .NET Backend 1.0" 
        });
    }
}
