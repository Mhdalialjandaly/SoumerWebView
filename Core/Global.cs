using System.ComponentModel;
using System.Reflection;

namespace Core
{
    public static class Global
    {
        private static Dictionary<string, object> Dictionary { get; set; }

        public static void AddValue(string key, object item)
        {
            Dictionary ??= new Dictionary<string, object>();

            if (Dictionary.TryGetValue(key, out object value))
            {
                Dictionary.Remove(key);
            }

            Dictionary.Add(key, item);
        }

        public static void RemoveValue(string key)
        {
            if (Dictionary == null)
                return;

            if (Dictionary.TryGetValue(key, out object value))
            {
                Dictionary.Remove(key);
            }
        }

        public static object GetValue(string key)
        {
            if (Dictionary == null)
                return null;

            Dictionary.TryGetValue(key, out object item);
            return item;
        }

        public static string DateFormat = "dd/MM/yyyy";
        public static string DeviceName { get; set; }

    }
    public static class GlobalKeys
    {
        public const string LoggedUser = nameof(LoggedUser);
        public const string UserName = nameof(UserName);
        public const string LoggedDepartmentId = nameof(LoggedDepartmentId);
        public const string LoggedUserDepartment = nameof(LoggedUserDepartment);
        public const string SelectedPrinter = nameof(SelectedPrinter);
    }

    public static class PasswordValidator
    {
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            return HasUpperCase(password) &&
                   HasDigit(password) &&
                   HasSpecialChar(password);
        }

        private static bool HasUpperCase(string password)
        {
            return password.Any(char.IsUpper);
        }

        private static bool HasDigit(string password)
        {
            return password.Any(char.IsDigit);
        }

        private static bool HasSpecialChar(string password)
        {
            string specialChars = "!@#$%^&*()_+-=[]{};':\"\\|,.<>/?";
            return password.Any(c => specialChars.Contains(c));
        }
    }

    public static class GetEnumsNamesClass
    {
        public static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }
    }

}
