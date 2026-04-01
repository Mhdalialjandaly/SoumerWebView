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

        public CourseService(
            ICourseRepository courseRepository,
            ICourseRegistrationRepository registrationRepository,
            IBalanceService balanceService)
        {
            _courseRepository = courseRepository;
            _registrationRepository = registrationRepository;
            _balanceService = balanceService;
        }

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

        public async Task<bool> EnrollInCourse(int courseId, string userId)
        {
            try
            {
                // التحقق من أهلية التسجيل
                var eligibility = await CheckEnrollmentEligibility(courseId, userId);
                if (!eligibility.IsEligible)
                {
                    return false;
                }

                // خصم النقاط
                var deduction = await _balanceService.DeductPoints(
                    userId,
                    eligibility.CoursePrice,
                    $"الاشتراك في كورس {eligibility.CoursePrice}");

                if (deduction == null)
                {
                    return false;
                }

                // تسجيل المستخدم في الكورس
                var registration = new CourseRegistrationDto
                {
                    CourseId = courseId,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };

                var result = await _registrationRepository.Add(registration);
                return result != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

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

                // يمكن إضافة منطق لاسترجاع النقاط إذا كان مسموحاً
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<CourseRegistrationDto>> GetUserEnrolledCourses(string userId)
        {
            return await _registrationRepository.GetUserRegistrations(userId);
        }

        public async Task<bool> IsUserEnrolled(int courseId, string userId)
        {
            return await _registrationRepository.IsUserEnrolled(courseId, userId);
        }

        public async Task<CourseEnrollmentResult> CheckEnrollmentEligibility(int courseId, string userId)
        {
            var result = new CourseEnrollmentResult();

            // جلب تفاصيل الكورس
            var course = await _courseRepository.GetById(courseId);
            if (course == null)
            {
                result.IsEligible = false;
                result.Message = "الكورس غير موجود";
                return result;
            }

            result.CoursePrice = course.Price;

            // التحقق من عدم التسجيل مسبقاً
            var isEnrolled = await _registrationRepository.IsUserEnrolled(courseId, userId);
            if (isEnrolled)
            {
                result.IsEligible = false;
                result.Message = "أنت مسجل بالفعل في هذا الكورس";
                result.IsAlreadyEnrolled = true;
                return result;
            }

            // التحقق من الرصيد
            var userBalance = await _balanceService.GetUserBalance(userId);
            result.UserBalance = userBalance.CurrentBalance;

            if (userBalance.CurrentBalance < course.Price)
            {
                result.IsEligible = false;
                result.Message = $"الرصيد غير كافي. الرصيد المتاح: {userBalance.CurrentBalance} نقطة، وسعر الكورس: {course.Price} نقطة";
                return result;
            }

            result.IsEligible = true;
            result.Message = "يمكنك التسجيل في هذا الكورس";
            return result;
        }
    }
}
