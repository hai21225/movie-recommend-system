
public class RecommendationService : IRecommendationService
{
    private readonly IGenreService _genreService;
    private readonly IContentRepository _contentRepository;
    private readonly IInteractionRepository _interactionRepository;
    public RecommendationService(
        IGenreService genreService,
        IContentRepository contentRepository, 
        IInteractionRepository interactionRepository)
    {
        _genreService = genreService;
        _contentRepository = contentRepository;
        _interactionRepository = interactionRepository;
    }

    public async Task<List<ContentDto>> GetCollaborativeRecommendations(string showId, int limit = 10)
    {
        // get content usser is watching
        var currentContent = await _contentRepository.GetContentByShowId(showId);
        if (currentContent == null)
        {
            return new List<ContentDto>();
        }

        var usersWhoLikeThis = await _interactionRepository.GetUsersWhoLikedThisContent(showId);

        var similarContents = await _contentRepository
            .GetSimilarContents(usersWhoLikeThis, showId, currentContent.ClusterId, currentContent.ContentType, limit);
        if (similarContents == null || similarContents.Count == 0)
        {
            return new List<ContentDto>();
        }
        return similarContents.Select(c => new ContentDto
        {
            ShowId = c.ShowId,
            Title = c.Title,
            ContentType = c.ContentType,
            ClusterId = c.ClusterId
        }).ToList();

    }

    public async Task<List<ContentDto>> GetHomeFeedRecommendations(int userId, int limit = 20)
    {
        //bool hasInteractions = await _interactionRepository.UserHasInteractions(userId);

        //if (!hasInteractions)
        //{
        //    return await GetRecomendationsByPreferredGenres(userId, limit);
        //}

        ////list showid user liked
        //var userLikedShowIds = await _interactionRepository.GetLikedShowIdsByUser(userId);

        ////use list showid to get cluster id
        //var favoriteClusterIds = await _contentRepository.GetFavoriteClusterIds(userLikedShowIds);

        //var similarUserIds = await _interactionRepository
        //    .GetUsersWhoLikedContentsInClusters(favoriteClusterIds, userId);

        //var homeFeedContents = await _contentRepository
        //    .GetContentsLikedByUsers(favoriteClusterIds,similarUserIds,limit);
    }

    public async Task<List<ContentDto>> GetRecomendationsByPreferredGenres(int userId, int limit = 10)
    {
        var preferredGenresId = await _genreService.GetUserpreference(userId);

        if (!preferredGenresId.Any())
        {
            return await GetGlobalPopularContents(userId, limit);
        }

        var recommendations =  await _contentRepository.GetContentsByGenresId(userId,preferredGenresId, limit);

        return recommendations.Select(c => new ContentDto
        {
            ShowId = c.ShowId,
            Title = c.Title,
            ContentType = c.ContentType,
            ClusterId = c.ClusterId
        }).ToList();
    }


    public async Task<List<ContentDto>> GetGlobalPopularContents(int userId, int limit = 10)
    {
        var popularContents = await _contentRepository.GetGlobalPopularContents(userId, limit);
        return popularContents.Select(c => new ContentDto
        {
            ShowId = c.ShowId,
            Title = c.Title,
            ContentType = c.ContentType,
            ClusterId = c.ClusterId
        }).ToList();
    }
}