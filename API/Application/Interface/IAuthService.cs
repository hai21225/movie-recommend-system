public interface IAuthService
{
    public Task<bool> Register(string username, string password, string confrimpass);
    public Task<UserDto?> Login(string username, string password);
    public Task<bool> IsUsernameTaken(string username);
}