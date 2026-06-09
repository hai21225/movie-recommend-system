
using Microsoft.EntityFrameworkCore;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;
    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsUsernameTaken(string username)
    {
        return await _context.Users.AnyAsync(u => u.UserName == username);
    }

    public async Task<Users?> Login(string username, string password)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);
    }

    public async Task<bool> Register(string username, string password)
    {
        var user = new Users
        {
            UserName = username,
            Password = password
        };

        await _context.Users.AddAsync(user);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
