using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using SoumerMVCView.Models;
using System.Diagnostics;

namespace SoumerMVCView.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                FeaturedTeachers = GetFeaturedTeachers(),
                GradesTeachers = GetGradesTeachers()
            };
            return View(model);
        }

        public IActionResult GetTeacherCourses(int id)
        {
            var teacher = GetAllTeachers().FirstOrDefault(t => t.Id == id);
            if (teacher == null)
                return NotFound();

            return Json(teacher);
        }

        private List<TeacherDto> GetFeaturedTeachers()
        {
            return GetAllTeachers().Take(6).ToList();
        }

        private List<GradeTeachersDto> GetGradesTeachers()
        {
            var allTeachers = GetAllTeachers();
            var gradesTeachers = new List<GradeTeachersDto>();
            var random = new Random();

            for (int grade = 1; grade <= 12; grade++)
            {
                int teachersCount = grade == 12 ? 5 : (grade % 3 == 0 ? 4 : 3);

                // خلط الأساتذة عشوائياً
                var shuffled = allTeachers.OrderBy(x => random.Next()).ToList();
                var selected = shuffled.Take(teachersCount).ToList();

                string gradeLevel = grade <= 6 ? "الابتدائية" : (grade <= 9 ? "المتوسطة" : "الثانوية");

                gradesTeachers.Add(new GradeTeachersDto
                {
                    Grade = grade,
                    GradeLevel = gradeLevel,
                    Teachers = selected
                });
            }

            return gradesTeachers;
        }

        private List<TeacherDto> GetAllTeachers()
        {
            return new List<TeacherDto>
            {
                new() { Id = 1, Name = "أ. أحمد المنصور", Subject = "الرياضيات", Bio = "خبرة 15 سنة", Image = "https://randomuser.me/api/portraits/men/32.jpg", Courses = new List<string> { "الجبر المتقدم", "الهندسة التحليلية", "التفاضل والتكامل" } },
                new() { Id = 2, Name = "د. ليلى حسن", Subject = "الفيزياء", Bio = "دكتوراه فيزياء", Image = "https://randomuser.me/api/portraits/women/68.jpg", Courses = new List<string> { "الميكانيكا", "الكهرومغناطيسية", "فيزياء الكم" } },
                new() { Id = 3, Name = "أ. سامر خالد", Subject = "اللغة العربية", Bio = "خبير في النحو", Image = "https://randomuser.me/api/portraits/men/45.jpg", Courses = new List<string> { "النحو التطبيقي", "الأدب العباسي", "فن البلاغة" } },
                new() { Id = 4, Name = "أ. نورة السعدي", Subject = "الإنجليزية", Bio = "معتمدة من كامبريدج", Image = "https://randomuser.me/api/portraits/women/44.jpg", Courses = new List<string> { "محادثة متقدمة", "قواعد اللغة", "أدب إنجليزي" } },
                new() { Id = 5, Name = "د. هشام فتحي", Subject = "الكيمياء", Bio = "أستاذ كيمياء", Image = "https://randomuser.me/api/portraits/men/22.jpg", Courses = new List<string> { "الكيمياء العضوية", "الكيمياء التحليلية", "التفاعلات الكيميائية" } },
                new() { Id = 6, Name = "أ. رنا عادل", Subject = "التاريخ", Bio = "متخصصة تاريخ", Image = "https://randomuser.me/api/portraits/women/90.jpg", Courses = new List<string> { "تاريخ مصر القديم", "الحضارة الإسلامية", "تاريخ أوروبا" } },
                new() { Id = 7, Name = "أ. عمر الجابري", Subject = "علوم الحاسب", Bio = "خبير ذكاء اصطناعي", Image = "https://randomuser.me/api/portraits/men/12.jpg", Courses = new List<string> { "برمجة بايثون", "هياكل البيانات", "تعلم الآلة" } },
                new() { Id = 8, Name = "أ. مها الصباح", Subject = "الأحياء", Bio = "أستاذ وراثة", Image = "https://randomuser.me/api/portraits/women/33.jpg", Courses = new List<string> { "الوراثة", "التشريح", "علم البيئة" } },
                new() { Id = 9, Name = "أ. طارق محمود", Subject = "الجغرافيا", Bio = "خرائط ونظم معلومات", Image = "https://randomuser.me/api/portraits/men/52.jpg", Courses = new List<string> { "جغرافيا طبيعية", "نظم المعلومات الجغرافية", "خرائط مناخية" } },
                new() { Id = 10, Name = "أ. سلمى خالد", Subject = "التربية الإسلامية", Bio = "حافظة للقرآن", Image = "https://randomuser.me/api/portraits/women/76.jpg", Courses = new List<string> { "تفسير القرآن", "فقه السيرة", "أخلاق إسلامية" } },
                new() { Id = 11, Name = "د. محمد رشاد", Subject = "الفلسفة", Bio = "دكتوراه فلسفة", Image = "https://randomuser.me/api/portraits/men/36.jpg", Courses = new List<string> { "منطق", "فلسفة الأخلاق", "تاريخ الفلسفة" } },
                new() { Id = 12, Name = "أ. فاطمة الزهراء", Subject = "التربية الفنية", Bio = "فنانة تشكيلية", Image = "https://randomuser.me/api/portraits/women/55.jpg", Courses = new List<string> { "الرسم والتصوير", "الأشغال الفنية", "تاريخ الفن" } }
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    
}