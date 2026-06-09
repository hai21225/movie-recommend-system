public class AuthService:IAuthService
{
    private readonly IAuthRepository _authRepository;
    public AuthService(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<bool> IsUsernameTaken(string username)
    {
        return await _authRepository.IsUsernameTaken(username);

    }

    public async Task<bool> Register(string username, string password, string confrimpass)
    {
        if(username=="" || (password != confrimpass))
        {
            return false;
        }
        var checkusername = await IsUsernameTaken(username);
        if (checkusername)
        {
            return false;
        }
        return await _authRepository.Register(username, password);
    }

    public async Task<UserDto?> Login(string username, string password)
    {
        if(username=="" || password=="")
        {
            return null;
        }
        var user = await _authRepository.Login(username, password);
        if(user == null)
        {
            return null;
        }
        return new UserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
        };
    }
}