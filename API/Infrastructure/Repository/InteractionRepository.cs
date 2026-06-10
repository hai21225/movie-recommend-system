
using Microsoft.EntityFrameworkCore;

public class InteractionRepository: IInteractionRepository
{
    private readonly AppDbContext _context;
    public InteractionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserInteractions>> GetUserInteractionHistory(int userId)
    {
        return await _context.UserInteractions
            .Where(ui => ui.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> IsContentLikedByUser(int userId, string showId)
    {
        return await _context.UserInteractions
            .AnyAsync( ui=> ui.UserId == userId && ui.ShowId == showId && ui.Liked);
    }

    public async Task<bool> SetLikeStatus(int userId, string showId, bool isLiked)
    {
        var interaction = await _context.UserInteractions
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ShowId == showId);
        if (interaction == null)
        {
               interaction = new UserInteractions
               {
                UserId = userId,
                ShowId = showId,
                Liked = isLiked
            };
            _context.UserInteractions.Add(interaction);
        }
        else
        {
            interaction.Liked = isLiked;
            _context.UserInteractions.Update(interaction);
        }
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<int>> GetUsersWhoLikedThisContent(string showId)
    {
        return await _context.UserInteractions
            .Where(ui => ui.ShowId == showId && ui.Liked)
            .Select(ui => ui.UserId)
            .ToListAsync();
    }

    public async Task<List<string>> GetLikedShowIdsByUser(int userId)
    {
        return await _context.UserInteractions
            .Where(ui => ui.UserId == userId && ui.Liked)
            .Select(ui => ui.ShowId)
            .ToListAsync();
    }

    public async Task<bool> UserHasInteractions(int userId)
    {
        var hasInteractions = await _context.UserInteractions.AnyAsync(ui => ui.UserId == userId);
        return hasInteractions;
    }

    public async Task<List<int>> GetUsersWhoLikedContentsInClusters(List<FavoriteClusterDto> favoriteClusters, int currentUserId)
    {
        var query = _context.UserInteractions
        .Where(ui => ui.UserId != currentUserId && ui.Liked)
        .Join(
            _context.Contents,
            ui => ui.ShowId,
            c => c.ShowId,
            (ui, c) => new
            {
                ui.UserId,
                c.ClusterId,
                c.ContentType
            });
        var clusterKeys = favoriteClusters.Select(fc => $"{fc.ContentType}_{fc.ClusterId}").ToList();

        var result = await query
            .Where(x =>clusterKeys.Contains(
                    x.ContentType + "_" + x.ClusterId))
            .GroupBy(x => x.UserId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(15)
            .ToListAsync();

        return result;
    }
}