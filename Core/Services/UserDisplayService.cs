using System.Security.Cryptography;
using System.Text;

namespace Core.Services
{
    public class UserDisplayService : IUserDisplayService
    {
        public string GetRoleBadgeClass(string roleName)
        {
            return roleName switch
            {
                "مدير النظام" => "bg-primary",
                "مسؤول" => "bg-success",
                "مستخدم" => "bg-info",
                "مراجع" => "bg-warning",
                "مشرف" => "bg-purple",
                "محاضر" => "bg-indigo",
                "طالب" => "bg-teal",
                _ => "bg-secondary"
            };
        }

        public string GetUserColor(string username)
        {
            if (string.IsNullOrEmpty(username))
                return "#4e73df";

            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(username));
            var hashInt = BitConverter.ToInt32(hash, 0);

            string[] colors = new[] {
                "#4e73df", "#1cc88a", "#36b9cc", "#f6c23e",
                "#e74a3b", "#6f42c1", "#fd7e14", "#20c9a6"
            };
            return colors[Math.Abs(hashInt) % colors.Length];
        }

        public string GetUserInitials(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "??";

            var parts = username.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();

            return username.Length >= 2 ? username.Substring(0, 2).ToUpper() : username.ToUpper();
        }

        public string GetUserAvatarUrl(string username, int size = 80)
        {
            // يمكنك استخدام Gravatar أو إنشاء رابط افتراضي
            var initials = GetUserInitials(username);
            var color = GetUserColor(username).TrimStart('#');
            return $"https://ui-avatars.com/api/?name={initials}&background={color}&color=fff&size={size}&bold=true";
        }
    }
}