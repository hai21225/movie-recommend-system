public interface IAuthRepository
{
    public Task<bool> Register(string username, string password);
    public Task<Users?> Login(string username, string password);
    public Task<bool> IsUsernameTaken(string username);
}