namespace SystemQMS.Services.Interface
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string username, string role);
    }
}
