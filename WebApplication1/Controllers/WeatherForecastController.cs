using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class FooController : ControllerBase
{
    [HttpGet("hui", Order = 2)]
    public string Get1()
    {
        return "single hui";
    }
    
    [HttpGet("hui/", Order = 1)]
    public string Get2()
    {
        return "hui dir";
    }
}