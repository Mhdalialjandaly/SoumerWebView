namespace Core.Services
{
    public interface IUserDisplayService
    {
        string GetRoleBadgeClass(string roleName);
        string GetUserColor(string username);
        string GetUserInitials(string username);
        string GetUserAvatarUrl(string username, int size = 80);
    }
}
