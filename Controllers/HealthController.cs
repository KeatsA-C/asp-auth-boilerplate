using backend.Data;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;

    public HealthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        bool dbReachable;
        try
        {
            dbReachable = await _db.Database.CanConnectAsync();
        }
        catch
        {
            dbReachable = false;
        }

        return Ok(new
        {
            status = "healthy",
            message = "API is running",
            database = new
            {
                connected = dbReachable ? "yes" : "no"
            }
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

