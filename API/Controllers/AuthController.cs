using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]

public class AuthController:ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.Register(registerDto.Username,registerDto.Password,registerDto.ConfirmPassword);
        if (!result)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.Login(loginDto.Username,loginDto.Password);
        if (result==null)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("check-username")]
    public async Task<IActionResult> IsUsernameTaken(
    string username)
    {
        return Ok(
            await _authService.IsUsernameTaken(username));
    }
}