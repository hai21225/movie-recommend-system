
public class GenreService: IGenreService
{
    private readonly IGenreRepository _genreRepository;

    public GenreService(IGenreRepository genreRepository)
    {
        _genreRepository = genreRepository;
    }

    public async Task<List<GenresDto>> GetAllGenres()
    {
        var genres = await _genreRepository.GetAllGenres();
        return genres.Select(g => new GenresDto
        {
            GenreId = g.GenreId,
            GenreName = g.GenreName
        }).ToList();
    }

    public async Task<List<int>> GetUserpreference(int userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("Invalid user ID");
        }
        return await _genreRepository.GetPreferredByUser(userId);
    }

    public async Task<bool> SaveUserpreference(int userId, List<int> genreIdList)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("Invalid user ID");
        }
        if (genreIdList == null || genreIdList.Count == 0)
        {
            throw new ArgumentException("Genre ID list cannot be null or empty");
        }
        return await _genreRepository.SavePreferredGenres(userId, genreIdList);
    }
}