using Microsoft.AspNetCore.Mvc;

public class InteractionController : ControllerBase
{
    private readonly IInteractionService _interactionService;

    public InteractionController(
        IInteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    [HttpPost("like")]
    public async Task<IActionResult> SetLikeStatus([FromBody] InteractionDto interactionDto)
    {
        var result =
            await _interactionService.SetLikeStatus(
                interactionDto.UserId,
                interactionDto.ShowId,
                interactionDto.IsLiked);

        return Ok(result);
    }

    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetHistory(int userId)
    {
        return Ok(
            await _interactionService
                .GetUserInteractionHistory(userId));
    }

    [HttpGet("is-liked")]
    public async Task<IActionResult> IsLiked([FromBody] InteractionDto interactiondto)
    {
        return Ok(
            await _interactionService
                .IsContentLikedByUser(interactiondto.UserId, interactiondto.ShowId));
    }
}