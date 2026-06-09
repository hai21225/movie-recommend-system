public interface IGenreService
{
    public Task<List<GenresDto>> GetAllGenres();
    //public Task<GenresDto?> GetGenreById(int id);


    // save 3 genres for user, when user register
    public Task<bool> SaveUserpreference(int userId, List<int> genreIdList);

    // get user preference, return list of genreId
    public Task<List<int>> GetUserpreference(int userId);
}