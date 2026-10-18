using System.ComponentModel.DataAnnotations;

namespace SoumerMVCView.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم المستخدم")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "اسم المستخدم يجب أن يكون بين 3 و 50 حرف")]
        [RegularExpression(@"^[a-zA-Z0-9_\u0600-\u06FF]+$", ErrorMessage = "اسم المستخدم يحتوي على أحرف غير مسموحة")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "الاسم الكامل يجب أن يكون بين 3 و 100 حرف")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "الوصف")]
        [StringLength(500, ErrorMessage = "الوصف لا يمكن أن يتجاوز 500 حرف")]
        public string Description { get; set; }

        [Display(Name = "رقم الهاتف")]
        [Required(ErrorMessage ="رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        public string PhoneNumber { get; set; }

        //[Display(Name = "أوافق على الشروط والأحكام")]
        //[Range(typeof(bool), "true", "true", ErrorMessage = "يجب الموافقة على الشروط والأحكام")]
        public bool AcceptTerms { get; set; }
    }
}