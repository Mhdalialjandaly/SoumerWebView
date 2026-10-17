using DataAccess.IRepositories;
using Models;
using SoumerMVCView.Services.BalanceService;

namespace SoumerMVCView.Services.CourseService
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseRegistrationRepository _registrationRepository;
        private readonly IBalanceService _balanceService;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            ICourseRepository courseRepository,
            ICourseRegistrationRepository registrationRepository,
            IBalanceService balanceService,
            ILogger<CourseService> logger = null)
        {
            _courseRepository = courseRepository;
            _registrationRepository = registrationRepository;
            _balanceService = balanceService;
            _logger = logger;
        }

        // ==========================================
        // قراءة الكورسات
        // ==========================================
        public async Task<List<CourseDto>> GetAllCourses()
        {
            return await _courseRepository.GetAll();
        }

        public async Task<CourseDto> GetCourseById(int courseId)
        {
            return await _courseRepository.GetById(courseId);
        }

        public async Task<List<CourseDto>> GetAvailableCourses()
        {
            return await _courseRepository.GetAvailableCourses();
        }

        // ==========================================
        // التسجيل في كورس
        // ==========================================
        public async Task<bool> EnrollInCourse(int courseId, string userId)
        {
            try
            {
                // 1. التحقق من أهلية التسجيل
                var eligibility = await CheckEnrollmentEligibility(courseId, userId);
                if (!eligibility.IsEligible)
                {
                    _logger?.LogWarning(
                        "Enrollment rejected for user {UserId} in course {CourseId}: {Message}",
                        userId, courseId, eligibility.Message);
                    return false;
                }

                // 2. جلب تفاصيل الكورس لبناء وصف المعاملة
                var course = await _courseRepository.GetById(courseId);
                if (course == null)
                {
                    _logger?.LogWarning("Course {CourseId} not found during enrollment", courseId);
                    return false;
                }

                // 3. تجهيز وصف المعاملة مع مراعاة العرض
                var description = course.HasActiveDiscount
                    ? $"الاشتراك في كورس '{course.Name}' (سعر عرض {course.DiscountPercent}% — خصم {course.DiscountAmount:N0} نقطة)"
                    : $"الاشتراك في كورس '{course.Name}' ({course.Price:N0} نقطة)";

                // 4. خصم النقاط بالسعر الفعلي (Eligibility.CoursePrice = EffectivePrice)
                var deduction = await _balanceService.DeductPoints(
                    userId,
                    eligibility.CoursePrice,
                    description);

                if (deduction == null)
                {
                    _logger?.LogWarning(
                        "Failed to deduct points for user {UserId} course {CourseId} amount {Amount}",
                        userId, courseId, eligibility.CoursePrice);
                    return false;
                }

                // 5. تسجيل المستخدم في الكورس
                var registration = new CourseRegistrationDto
                {
                    CourseId = courseId,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };

                var result = await _registrationRepository.Add(registration);

                if (result == null)
                {
                    // محاولة استرجاع النقاط لو فشل التسجيل بعد الخصم
                    _logger?.LogError(
                        "Registration failed after deducting points for user {UserId} course {CourseId}. Attempting refund.",
                        userId, courseId);

                    try
                    {
                        await _balanceService.AddPoints(
                            userId,
                            eligibility.CoursePrice,
                            $"استرجاع نقاط بعد فشل التسجيل في كورس {courseId}");
                    }
                    catch (Exception refundEx)
                    {
                        _logger?.LogError(refundEx,
                            "Failed to refund points for user {UserId} course {CourseId}",
                            userId, courseId);
                    }

                    return false;
                }

                _logger?.LogInformation(
                    "User {UserId} enrolled in course {CourseId} for {Amount} points (discount: {HasDiscount})",
                    userId, courseId, eligibility.CoursePrice, course.HasActiveDiscount);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "Error enrolling user {UserId} in course {CourseId}",
                    userId, courseId);
                return false;
            }
        }

        // ==========================================
        // إلغاء التسجيل
        // ==========================================
        public async Task<bool> UnenrollFromCourse(int courseId, string userId)
        {
            try
            {
                var registration = await _registrationRepository.GetRegistration(courseId, userId);
                if (registration == null)
                {
                    return false;
                }

                await _registrationRepository.Delete(registration.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "Error unenrolling user {UserId} from course {CourseId}",
                    userId, courseId);
                return false;
            }
        }

        // ==========================================
        // كورسات المستخدم
        // ==========================================
        public async Task<List<CourseRegistrationDto>> GetUserEnrolledCourses(string userId)
        {
            return await _registrationRepository.GetUserRegistrations(userId);
        }

        public async Task<bool> IsUserEnrolled(int courseId, string userId)
        {
            return await _registrationRepository.IsUserEnrolled(courseId, userId);
        }

        // ==========================================
        // التحقق من أهلية التسجيل
        // ==========================================
        public async Task<CourseEnrollmentResult> CheckEnrollmentEligibility(int courseId, string userId)
        {
            var result = new CourseEnrollmentResult();

            try
            {
                // 1. جلب تفاصيل الكورس
                var course = await _courseRepository.GetById(courseId);
                if (course == null)
                {
                    result.IsEligible = false;
                    result.Message = "الكورس غير موجود";
                    return result;
                }

                // 2. تحديد السعر الفعلي (مع مراعاة عرض الفترة المحدودة)
                //    CourseDto يُوفّر EffectivePrice و HasActiveDiscount جاهزين
                result.CoursePrice = course.EffectivePrice;
                result.OriginalPrice = course.Price;
                result.HasActiveDiscount = course.HasActiveDiscount;
                result.DiscountAmount = course.DiscountAmount;
                result.DiscountPercent = course.DiscountPercent;

                // 3. التحقق من عدم التسجيل مسبقاً
                var isEnrolled = await _registrationRepository.IsUserEnrolled(courseId, userId);
                if (isEnrolled)
                {
                    result.IsEligible = false;
                    result.Message = "أنت مسجل بالفعل في هذا الكورس";
                    result.IsAlreadyEnrolled = true;
                    return result;
                }

                // 4. التحقق من الرصيد (مقارنة بالسعر الفعلي)
                var userBalance = await _balanceService.GetUserBalance(userId);
                result.UserBalance = userBalance?.CurrentBalance ?? 0;

                if (result.UserBalance < result.CoursePrice)
                {
                    result.IsEligible = false;

                    var shortage = result.CoursePrice - result.UserBalance;

                    result.Message = course.HasActiveDiscount
                        ? $"الرصيد غير كافٍ. سعر العرض {result.CoursePrice:N0} نقطة، ورصيدك {result.UserBalance:N0} نقطة. ينقصك {shortage:N0} نقطة"
                        : $"الرصيد غير كافٍ. سعر الكورس {result.CoursePrice:N0} نقطة، ورصيدك {result.UserBalance:N0} نقطة. ينقصك {shortage:N0} نقطة";
                    return result;
                }

                // 5. مؤهل للتسجيل
                result.IsEligible = true;
                result.Message = course.HasActiveDiscount
                    ? $"يمكنك التسجيل بسعر العرض ({result.CoursePrice:N0} نقطة بدلاً من {course.Price:N0} نقطة)"
                    : "يمكنك التسجيل في هذا الكورس";

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "Error checking enrollment eligibility for user {UserId} course {CourseId}",
                    userId, courseId);

                result.IsEligible = false;
                result.Message = "حدث خطأ أثناء التحقق من الأهلية. يرجى المحاولة لاحقاً";
                return result;
            }
        }
    }
}