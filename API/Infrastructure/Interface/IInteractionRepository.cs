public interface IInteractionRepository
{
    public Task<bool> SetLikeStatus(int userId, string showId, bool isLiked);

    public Task<List<UserInteractions>> GetUserInteractionHistory(int userId);

    public Task<bool> IsContentLikedByUser(int userId, string showId);

    public Task<List<int>> GetUsersWhoLikedThisContent(string showId);

    public Task<bool> UserHasInteractions(int userId);

    public Task<List<int>> GetLikedShowIdsByUser(List<int> userIds);

    public Task<List<int>> GetUsersWhoLikedContentsInClusters(List<int> favoriteClusterIds, int userId);
}