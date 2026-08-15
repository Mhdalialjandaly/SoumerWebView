namespace SoumerMVCView.Models
{
    public class UserHeaderViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }

        public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : UserName;
        public string Initial => !string.IsNullOrEmpty(DisplayName) ? DisplayName.Substring(0, 1).ToUpper() : "م";
    }
}
