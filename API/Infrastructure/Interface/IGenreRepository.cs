public interface IGenreRepository
{
    public Task<List<Genres>> GetAllGenres();

    public Task<List<int>> GetPreferredByUser(int userId);

    public Task<bool> SavePreferredGenres(int userId, List<int> genreIdList);
    
}