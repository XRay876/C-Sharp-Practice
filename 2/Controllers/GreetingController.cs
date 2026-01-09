using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
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
}
