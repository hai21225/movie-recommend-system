public interface IInteractionService
{
    public Task<bool> SetLikeStatus(int userId, string showId, bool isLiked);
    public Task<List<UserInteractionsDto>> GetUserInteractionHistory (int userId);

    public Task<bool> IsContentLikedByUser (int userId, string showId);
}