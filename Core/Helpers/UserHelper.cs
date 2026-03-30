using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Core.Helpers
{
    public static class UserHelper
    {
        private static readonly string[] Colors = new[]
        {
            "#4e73df", "#1cc88a", "#36b9cc", "#f6c23e",
            "#e74a3b", "#6f42c1", "#fd7e14", "#20c9a6"
        };

        public static string GetRoleBadgeClass(string roleName)
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

        public static string GetUserColor(string username)
        {
            if (string.IsNullOrEmpty(username))
                return Colors[0];

            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(username));
            var hashInt = BitConverter.ToInt32(hash, 0);

            return Colors[Math.Abs(hashInt) % Colors.Length];
        }

        public static string GetUserInitials(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "??";

            var parts = username.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();

            return username.Length >= 2 ? username.Substring(0, 2).ToUpper() : username.ToUpper();
        }

        public static string GetUserAvatarUrl(string username, int size = 80)
        {
            var initials = GetUserInitials(username);
            var color = GetUserColor(username).TrimStart('#');
            return $"https://ui-avatars.com/api/?name={initials}&background={color}&color=fff&size={size}&bold=true";
        }

        public static string FormatDate(DateTime? date)
        {
            if (!date.HasValue)
                return "غير محدد";

            return date.Value.ToString("yyyy/MM/dd");
        }

        public static string FormatDateTime(DateTime? date)
        {
            if (!date.HasValue)
                return "غير محدد";

            return date.Value.ToString("yyyy/MM/dd HH:mm");
        }     
    }
}