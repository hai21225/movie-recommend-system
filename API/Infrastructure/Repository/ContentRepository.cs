
using Microsoft.EntityFrameworkCore;

public class ContentRepository : IContentRepository
{
    private readonly AppDbContext _context;
    public ContentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Content>> GetContentsByGenresId(int userId,List<int> genresId, int limit = 20)
    {
        return await _context.Contents
            .Where(c => _context.ContentGenres
                .Any(cg => cg.ShowId == c.ShowId && genresId
                    .Contains(cg.GenreId) && !_context.UserInteractions
                        .Any(ui => ui.UserId == userId && ui.ShowId == c.ShowId)
            ))
            .OrderByDescending(c => _context.UserInteractions.Count(ui => ui.ShowId == c.ShowId && ui.Liked))
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Content?> GetContentByShowId(string showId)
    {
        return await _context.Contents.FirstOrDefaultAsync(c => c.ShowId == showId);
    }

    public async Task<List<Content>?> GetSimilarContents(List<int> whoLikedThis,string showId ,int clusterId, string type, int limit=10)
    {
        return await _context.Contents
            .Where(c => c.ClusterId == clusterId && c.ContentType == type && c.ShowId != showId)
            .OrderByDescending(c => _context.UserInteractions
                .Count(ui=> whoLikedThis
                    .Contains(ui.UserId) && ui.ShowId==c.ShowId && ui.Liked))
            .ThenByDescending(c => _context.UserInteractions.Count(ui => ui.ShowId == c.ShowId && ui.Liked))
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Content>> GetGlobalPopularContents(int userId, int limit = 20)
    {
        return await _context.Contents
            .Where(c => !_context.UserInteractions.Any(ui => ui.UserId == userId && ui.ShowId == c.ShowId))
            .OrderByDescending(c => _context.UserInteractions.Count(ui => ui.ShowId == c.ShowId && ui.Liked))
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<FavoriteClusterDto>> GetFavoriteClusters(List<string> likedShowIds)
    {
        return await _context.Contents
            .Where(c => likedShowIds.Contains(c.ShowId))
            .GroupBy(c => new
            {
                c.ClusterId,
                c.ContentType
            })
            .OrderByDescending(g => g.Count())
            .Select(g => new FavoriteClusterDto
            {
                ClusterId = g.Key.ClusterId,
                ContentType = g.Key.ContentType
            })
            .Take(2)
            .ToListAsync();
    }

    public async Task<List<Content>> GetContentsLikedByUsers(List<FavoriteClusterDto> favoriteClusters, List<int> similarUserIds, int currentUserId, int limit = 20)
    {
        var clusterKeys = favoriteClusters
    .Select(fc => $"{fc.ContentType}_{fc.ClusterId}")
    .ToList();

        return await _context.Contents
.Where(c =>
    clusterKeys.Contains(
        c.ContentType + "_" + c.ClusterId)
    &&
    !_context.UserInteractions.Any(
        ui => ui.UserId == currentUserId &&
              ui.ShowId == c.ShowId))
        .OrderByDescending(c =>
            _context.UserInteractions.Count(ui =>
                similarUserIds.Contains(ui.UserId)
                && ui.ShowId == c.ShowId
                && ui.Liked))
        .Take(limit)
        .ToListAsync();
    }
}