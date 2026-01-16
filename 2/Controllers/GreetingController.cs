using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class GreetingController : ControllerBase
{
    [HttpGet]
    public IActionResult GetWelcomeMessage()
    {
        return Ok(new { Message = "Hello from the Controller!", Status = "Success"});
    }

    [HttpGet("hello/{name}")]
    public IActionResult SayHello(string name)
    {
        return Ok($"Hello, {name}");
    }

    [HttpGet("/ping")]
    public IActionResult PingPong()
    {
        return Ok("Pong");
    }

}
