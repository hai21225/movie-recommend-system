using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationController(
        IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet("genres/{userId}")]
    public async Task<IActionResult> GetByPreferredGenres(
        int userId,
        [FromQuery] int limit = 10)
    {
        var result =
            await _recommendationService
                .GetRecomendationsByPreferredGenres(
                    userId,
                    limit);

        return Ok(result);
    }

    [HttpGet("similar/{showId}")]
    public async Task<IActionResult> GetCollaborativeRecommendations(
        string showId,
        [FromQuery] int limit = 10)
    {
        var result =
            await _recommendationService
                .GetCollaborativeRecommendations(
                    showId,
                    limit);

        return Ok(result);
    }

    [HttpGet("home-feed/{userId}")]
    public async Task<IActionResult> GetHomeFeed(
        int userId,
        [FromQuery] int limit = 20)
    {
        var result =
            await _recommendationService
                .GetHomeFeedRecommendations(
                    userId,
                    limit);

        return Ok(result);
    }
}