
using Microsoft.EntityFrameworkCore;

public class GenreRepository : IGenreRepository
{
    private readonly AppDbContext _context;
    public GenreRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Genres>> GetAllGenres()
    {
        return await _context.Genres.ToListAsync();
    }

    public Task<List<int>> GetPreferredByUser(int userId)
    {
        return _context.UserPreferedGenres
            .Where(ugp => ugp.UserId == userId)
            .Select(ugp => ugp.GenreId)
            .ToListAsync();
    }

    public async Task<bool> SavePreferredGenres(int userId, List<int> genreIdList)
    {
        var newPreferences = genreIdList.Select(genreId => new UserPreferedGenres
        {
            UserId = userId,
            GenreId = genreId
        });
        await _context.UserPreferedGenres.AddRangeAsync(newPreferences);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}