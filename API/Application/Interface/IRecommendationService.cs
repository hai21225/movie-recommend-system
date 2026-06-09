public interface IRecommendationService
{
    public Task<List<ContentDto>> GetRecomendationsByPreferredGenres(int userId, int limit=10);
    public Task<List<ContentDto>> GetCollaborativeRecommendations(string showId, int limit = 10);
    public Task<List<ContentDto>> GetHomeFeedRecommendations(int userId, int limit = 20);
}