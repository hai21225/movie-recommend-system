public interface IContentRepository
{
    public Task<Content?> GetContentByShowId(string showId);
    public Task<List<Content>> GetContentsByGenresId(int userId,List<int> genresId,int limit=20);

    public Task<List<Content>?> GetSimilarContents(List<int> whoLikedThis, string showId, int clusterId, string type, int limit = 10);

    public Task<List<Content>> GetGlobalPopularContents(int userId, int limit=10);
}