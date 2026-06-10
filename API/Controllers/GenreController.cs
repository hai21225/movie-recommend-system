using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]

public class GenreController : ControllerBase
{
    private readonly IGenreService _genreService;

    public GenreController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GenresDto>>> GetAllGenres()
    {
        var genres = await _genreService.GetAllGenres();
        return Ok(genres);
    }

    [HttpPost("save-preference")]
    public async Task<IActionResult> SavePreference([FromBody] UserPreferenceRequest request)
    {
        if (request == null || request.GenreIds == null || request.GenreIds.Count == 0)
        {
            return BadRequest("Invalid request data");
        }
        var result = await _genreService.SaveUserpreference(request.UserId, request.GenreIds);
        if (result)
        {
            return Ok("Preferences saved successfully");
        }
        return StatusCode(500, "An error occurred while saving preferences");
    }

    [HttpGet("user-preference/{userId}")]
    public async Task<IActionResult> GetUserPreference(int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("Invalid user ID");
        }
        var preferences = await _genreService.GetUserpreference(userId);
        return Ok(preferences);
    }
}