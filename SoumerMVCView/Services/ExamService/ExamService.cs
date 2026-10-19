using DataAccess;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Models;

namespace SoumerMVCView.Services.ExamService
{
    public class ExamService : IExamService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExamService> _logger;

        public ExamService(ApplicationDbContext context, ILogger<ExamService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==========================================
        // قائمة الامتحانات
        // ==========================================
        public async Task<List<ExamListItemDto>> GetExamsForCourse(int courseId, string userId)
        {
            // ✅ إغلاق المحاولات المنتهية زمنياً قبل حساب الحالة
            await ExpireStaleAttempts(userId);

            var exams = await _context.Set<Exam>()
                .Include(e => e.Course)
                .Include(e => e.Questions)
                .Where(e => e.CourseId == courseId
                         && e.DeletedAt == null
                         && e.IsPublished)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var examIds = exams.Select(e => e.Id).ToList();
            var attempts = await _context.Set<ExamAttempt>()
                .Where(a => examIds.Contains(a.ExamId)
                         && a.UserId == userId
                         && a.DeletedAt == null)
                .OrderByDescending(a => a.StartedAt)
                .ToListAsync();

            var result = new List<ExamListItemDto>();

            foreach (var exam in exams)
            {
                var examAttempts = attempts.Where(a => a.ExamId == exam.Id).ToList();
                var lastAttempt = examAttempts.FirstOrDefault();
                var completedAttempts = examAttempts.Count(a => a.Status >= 2);

                var dto = new ExamListItemDto
                {
                    Id = exam.Id,
                    CourseId = exam.CourseId,
                    CourseName = exam.Course?.Name,
                    Title = exam.Title,
                    Description = exam.Description,
                    DurationMinutes = exam.DurationMinutes,
                    PassingScore = exam.PassingScore,
                    QuestionCount = exam.Questions.Count(q => q.DeletedAt == null),
                    IsPublished = exam.IsPublished,
                    HasAttempt = examAttempts.Any(),
                    AttemptsCount = completedAttempts,
                    MaxAttempts = exam.MaxAttempts,
                    LastAttempt = lastAttempt == null ? null : new ExamAttemptSummaryDto
                    {
                        Id = lastAttempt.Id,
                        StartedAt = lastAttempt.StartedAt,
                        SubmittedAt = lastAttempt.SubmittedAt,
                        Score = lastAttempt.Score,
                        MaxScore = lastAttempt.MaxScore,
                        CorrectAnswers = lastAttempt.CorrectAnswers,
                        TotalQuestions = lastAttempt.TotalQuestions,
                        IsPassed = lastAttempt.IsPassed,
                        Status = lastAttempt.Status,
                        EndReason = lastAttempt.EndReason
                    }
                };

                // التحقق من إمكانية البدء
                var (canStart, reason) = CanUserTakeExamInternal(exam, examAttempts);
                dto.CanStart = canStart;
                dto.BlockReason = reason;

                result.Add(dto);
            }

            return result;
        }

        // ==========================================
        // التحقق الداخلي من إمكانية البدء
        // ==========================================
        private (bool CanStart, string Reason) CanUserTakeExamInternal(Exam exam, List<ExamAttempt> attempts)
        {
            // 1. هل هناك محاولة جارية لم تنتهِ زمنياً؟
            var activeAttempt = attempts.FirstOrDefault(a => a.Status == 1);
            if (activeAttempt != null)
            {
                var expiresAt = activeAttempt.StartedAt.AddSeconds(activeAttempt.AllowedDurationSeconds);
                if (DateTime.Now < expiresAt)
                    return (false, "لديك محاولة جارية. أكملها أولاً");

                // المحاولة انتهت زمنياً — يجب إغلاقها
                return (false, "المحاولة السابقة انتهت زمنياً. حدّث الصفحة لإغلاقها");
            }

            // 2. عدد المحاولات المكتملة
            var completedAttempts = attempts.Count(a => a.Status >= 2);
            if (exam.MaxAttempts > 0 && completedAttempts >= exam.MaxAttempts)
            {
                var passedAttempt = attempts.FirstOrDefault(
                    a => (a.Status == 2 || a.Status == 3) && a.IsPassed);
                if (passedAttempt != null)
                    return (false, "لقد نجحت في هذا الامتحان مسبقاً");

                return (false, $"استنفدت عدد المحاولات المسموح ({exam.MaxAttempts})");
            }

            // 3. عدد الأسئلة
            if (exam.Questions == null || !exam.Questions.Any(q => q.DeletedAt == null))
                return (false, "لا يوجد أسئلة في هذا الامتحان");

            return (true, null);
        }

        // ==========================================
        // التحقق العام من إمكانية البدء
        // ==========================================
        public async Task<(bool CanStart, string Reason)> CanUserTakeExam(int examId, string userId)
        {
            var exam = await _context.Set<Exam>()
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Id == examId && e.DeletedAt == null && e.IsPublished);

            if (exam == null)
                return (false, "الامتحان غير موجود");

            var attempts = await _context.Set<ExamAttempt>()
                .Where(a => a.ExamId == examId && a.UserId == userId && a.DeletedAt == null)
                .ToListAsync();

            return CanUserTakeExamInternal(exam, attempts);
        }

        // ==========================================
        // بدء الامتحان
        // ==========================================
        public async Task<ExamSessionDto> StartExam(int examId, string userId)
        {
            // تحميل الامتحان + الأسئلة + الخيارات
            var exam = await _context.Set<Exam>()
                .Include(e => e.Course)
                .Include(e => e.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == examId && e.DeletedAt == null && e.IsPublished);

            if (exam == null)
                throw new InvalidOperationException("الامتحان غير موجود أو غير منشور");

            // إغلاق أي محاولة جارية منتهية زمنياً
            var staleAttempt = await _context.Set<ExamAttempt>()
                .FirstOrDefaultAsync(a => a.ExamId == examId
                                       && a.UserId == userId
                                       && a.Status == 1
                                       && a.DeletedAt == null);

            if (staleAttempt != null)
            {
                var staleExpiresAt = staleAttempt.StartedAt.AddSeconds(staleAttempt.AllowedDurationSeconds);
                if (DateTime.Now < staleExpiresAt)
                    throw new InvalidOperationException("لديك محاولة جارية لهذا الامتحان");

                // المحاولة انتهت زمنياً — أغلقها كـ TimeExpired
                var staleAnswers = await _context.Set<ExamAnswer>()
                    .Where(ans => ans.ExamAttemptId == staleAttempt.Id && ans.DeletedAt == null)
                    .ToListAsync();

                var staleScore = staleAnswers.Sum(a => (decimal)a.PointsEarned);
                var staleCorrect = staleAnswers.Count(a => a.IsCorrect);
                var stalePercentage = staleAttempt.MaxScore > 0
                    ? (staleScore / staleAttempt.MaxScore) * 100
                    : 0;

                staleAttempt.Status = 3; // TimeExpired
                staleAttempt.SubmittedAt = DateTime.Now;
                staleAttempt.Score = staleScore;
                staleAttempt.CorrectAnswers = staleCorrect;
                staleAttempt.EndReason = "TimeExpired";
                staleAttempt.IsPassed = stalePercentage >= exam.PassingScore;

                await _context.SaveChangesAsync();
            }

            // تحقق من الأهلية (بعد إغلاق المحاولة القديمة)
            var (canStart, reason) = await CanUserTakeExam(examId, userId);
            if (!canStart)
                throw new InvalidOperationException(reason ?? "لا يمكن بدء الامتحان");

            // فلترة الأسئلة والخيارات في الذاكرة
            var validQuestions = exam.Questions
                .Where(q => q.DeletedAt == null)
                .ToList();

            if (validQuestions.Count == 0)
                throw new InvalidOperationException("لا يوجد أسئلة في هذا الامتحان");

            if (exam.ShuffleQuestions)
                validQuestions = validQuestions.OrderBy(_ => Guid.NewGuid()).ToList();

            var attempt = new ExamAttempt
            {
                ExamId = exam.Id,
                UserId = userId,
                StartedAt = DateTime.Now,
                AllowedDurationSeconds = exam.DurationMinutes * 60,
                Status = 1, // InProgress
                MaxScore = validQuestions.Sum(q => q.Points),
                TotalQuestions = validQuestions.Count,
                CreatedAt = DateTime.Now
            };

            _context.Set<ExamAttempt>().Add(attempt);
            await _context.SaveChangesAsync();

            // سجلات فارغة لكل سؤال
            var answers = validQuestions.Select(q => new ExamAnswer
            {
                ExamAttemptId = attempt.Id,
                ExamQuestionId = q.Id,
                SelectedOptionId = null,
                IsCorrect = false,
                PointsEarned = 0,
                CreatedAt = DateTime.Now
            }).ToList();

            _context.Set<ExamAnswer>().AddRange(answers);
            await _context.SaveChangesAsync();

            // بناء الجلسة
            var expiresAt = attempt.StartedAt.AddSeconds(attempt.AllowedDurationSeconds);

            return new ExamSessionDto
            {
                AttemptId = attempt.Id,
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                CourseName = exam.Course?.Name,
                DurationMinutes = exam.DurationMinutes,
                StartedAt = attempt.StartedAt,
                ExpiresAt = expiresAt,
                RemainingSeconds = Math.Max(0, (int)(expiresAt - DateTime.Now).TotalSeconds),
                PassingScore = exam.PassingScore,
                TotalQuestions = validQuestions.Count,
                Questions = validQuestions.Select((q, idx) => new ExamQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Order = idx + 1,
                    Points = q.Points,
                    Options = q.Options
                        .Where(o => o.DeletedAt == null)
                        .OrderBy(o => o.Order)
                        .Select(o => new ExamOptionDto
                        {
                            Id = o.Id,
                            OptionText = o.OptionText,
                            Order = o.Order
                        }).ToList()
                }).ToList()
            };
        }

        // ==========================================
        // الحصول على الجلسة الحالية
        // ==========================================
        public async Task<ExamSessionDto?> GetActiveSession(int attemptId, string userId)
        {
            var attempt = await _context.Set<ExamAttempt>()
                .Include(a => a.Exam)
                    .ThenInclude(e => e.Course)
                .Include(a => a.Exam)
                    .ThenInclude(e => e.Questions)
                        .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.Id == attemptId
                                       && a.UserId == userId
                                       && a.DeletedAt == null);

            if (attempt == null || attempt.Status != 1)
                return null;

            var expiresAt = attempt.StartedAt.AddSeconds(attempt.AllowedDurationSeconds);

            // ✅ إن انتهى الوقت، أغلق المحاولة قبل الإرجاع
            if (DateTime.Now >= expiresAt)
            {
                await CloseAttemptAsTimeExpired(attempt);
                return null;
            }

            // فلترة الأسئلة في الذاكرة
            var questions = attempt.Exam.Questions
                .Where(q => q.DeletedAt == null)
                .OrderBy(q => q.Order)
                .ToList();

            if (questions.Count == 0)
                return null;

            return new ExamSessionDto
            {
                AttemptId = attempt.Id,
                ExamId = attempt.ExamId,
                ExamTitle = attempt.Exam.Title,
                CourseName = attempt.Exam.Course?.Name,
                DurationMinutes = attempt.Exam.DurationMinutes,
                StartedAt = attempt.StartedAt,
                ExpiresAt = expiresAt,
                RemainingSeconds = Math.Max(0, (int)(expiresAt - DateTime.Now).TotalSeconds),
                PassingScore = attempt.Exam.PassingScore,
                TotalQuestions = questions.Count,
                Questions = questions.Select((q, idx) => new ExamQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Order = idx + 1,
                    Points = q.Points,
                    Options = q.Options
                        .Where(o => o.DeletedAt == null)
                        .OrderBy(o => o.Order)
                        .Select(o => new ExamOptionDto
                        {
                            Id = o.Id,
                            OptionText = o.OptionText,
                            Order = o.Order
                        }).ToList()
                }).ToList()
            };
        }

        // ==========================================
        // إرسال إجابة
        // ==========================================
        public async Task<SubmitAnswerResultDto> SubmitAnswer(SubmitAnswerDto dto, string userId)
        {
            var attempt = await _context.Set<ExamAttempt>()
                .FirstOrDefaultAsync(a => a.Id == dto.AttemptId
                                       && a.UserId == userId
                                       && a.DeletedAt == null);

            if (attempt == null || attempt.Status != 1)
                return new SubmitAnswerResultDto { Success = false, Message = "المحاولة غير صالحة أو منتهية" };

            var expiresAt = attempt.StartedAt.AddSeconds(attempt.AllowedDurationSeconds);
            if (DateTime.Now >= expiresAt)
                return new SubmitAnswerResultDto { Success = false, Message = "انتهى وقت الامتحان" };

            var answer = await _context.Set<ExamAnswer>()
                .FirstOrDefaultAsync(a => a.ExamAttemptId == dto.AttemptId
                                       && a.ExamQuestionId == dto.QuestionId
                                       && a.DeletedAt == null);

            if (answer == null)
                return new SubmitAnswerResultDto { Success = false, Message = "السؤال غير موجود" };

            // تحقق أن الخيار ينتمي لهذا السؤال
            if (dto.OptionId.HasValue)
            {
                var optionValid = await _context.Set<ExamOption>()
                    .AnyAsync(o => o.Id == dto.OptionId.Value
                                && o.ExamQuestionId == dto.QuestionId
                                && o.DeletedAt == null);
                if (!optionValid)
                    return new SubmitAnswerResultDto { Success = false, Message = "الخيار غير صالح" };
            }

            answer.SelectedOptionId = dto.OptionId;
            answer.ModifiedAt = DateTime.Now;

            // تصحيح فوري
            if (dto.OptionId.HasValue)
            {
                var correctOption = await _context.Set<ExamOption>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == dto.OptionId.Value);
                var question = await _context.Set<ExamQuestion>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.Id == dto.QuestionId);

                answer.IsCorrect = correctOption?.IsCorrect == true;
                answer.PointsEarned = answer.IsCorrect ? (question?.Points ?? 0) : 0;
            }
            else
            {
                answer.IsCorrect = false;
                answer.PointsEarned = 0;
            }

            await _context.SaveChangesAsync();

            var answeredCount = await _context.Set<ExamAnswer>()
                .CountAsync(a => a.ExamAttemptId == dto.AttemptId
                              && a.SelectedOptionId != null
                              && a.DeletedAt == null);

            return new SubmitAnswerResultDto
            {
                Success = true,
                Message = "تم حفظ الإجابة",
                AnsweredCount = answeredCount,
                TotalQuestions = attempt.TotalQuestions,
                RemainingSeconds = Math.Max(0, (int)(expiresAt - DateTime.Now).TotalSeconds)
            };
        }

        // ==========================================
        // تسليم الامتحان
        // ==========================================
        public async Task<ExamResultDto> SubmitExam(int attemptId, string userId, string endReason = "ManualSubmit")
        {
            var attempt = await _context.Set<ExamAttempt>()
                .Include(a => a.Answers)
                .Include(a => a.Exam)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(a => a.Id == attemptId
                                       && a.UserId == userId
                                       && a.DeletedAt == null);

            if (attempt == null)
                throw new InvalidOperationException("المحاولة غير موجودة");

            // إذا كانت مسلّمة سابقاً، أعد النتيجة
            if (attempt.Status >= 2)
                return await GetResult(attempt.Id, userId)
                       ?? throw new InvalidOperationException("تعذر جلب النتيجة");

            // حساب النتيجة
            var correctAnswers = attempt.Answers.Count(a => a.IsCorrect);
            var totalScore = attempt.Answers.Sum(a => (decimal)a.PointsEarned);
            var maxScore = attempt.MaxScore;

            var percentage = maxScore > 0 ? (totalScore / maxScore) * 100 : 0;
            var passingScore = attempt.Exam?.PassingScore ?? 50;

            attempt.SubmittedAt = DateTime.Now;
            attempt.Score = totalScore;
            attempt.CorrectAnswers = correctAnswers;
            attempt.Status = 2; // Submitted (default)
            attempt.IsPassed = percentage >= passingScore;
            attempt.EndReason = endReason;

            // بحسب سبب الانتهاء
            if (endReason == "ForcedFail")
            {
                attempt.IsPassed = false;
                attempt.Status = 4;
            }
            else if (endReason == "TimeExpired")
            {
                attempt.Status = 3;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Exam attempt {AttemptId} submitted. Reason: {Reason}, Score: {Score}/{Max}, Passed: {Passed}",
                attempt.Id, endReason, totalScore, maxScore, attempt.IsPassed);

            return await GetResult(attempt.Id, userId)
                   ?? throw new InvalidOperationException("تعذر جلب النتيجة");
        }

        // ==========================================
        // جلب النتيجة
        // ==========================================
        public async Task<ExamResultDto?> GetResult(int attemptId, string userId)
        {
            var attempt = await _context.Set<ExamAttempt>()
                .Include(a => a.Exam)
                    .ThenInclude(e => e.Course)
                .Include(a => a.Exam)
                    .ThenInclude(e => e.Questions)
                        .ThenInclude(q => q.Options)
                .Include(a => a.Answers)
                    .ThenInclude(ans => ans.SelectedOption)
                .FirstOrDefaultAsync(a => a.Id == attemptId
                                       && a.UserId == userId
                                       && a.DeletedAt == null);

            if (attempt == null)
                return null;

            var percentage = attempt.MaxScore > 0
                ? (attempt.Score / attempt.MaxScore) * 100
                : 0;

            var durationSeconds = attempt.SubmittedAt.HasValue
                ? (int)(attempt.SubmittedAt.Value - attempt.StartedAt).TotalSeconds
                : 0;

            var result = new ExamResultDto
            {
                AttemptId = attempt.Id,
                ExamTitle = attempt.Exam?.Title,
                CourseName = attempt.Exam?.Course?.Name,
                Score = attempt.Score,
                MaxScore = attempt.MaxScore,
                Percentage = Math.Round(percentage, 1),
                CorrectAnswers = attempt.CorrectAnswers,
                TotalQuestions = attempt.TotalQuestions,
                IsPassed = attempt.IsPassed,
                PassingScore = attempt.Exam?.PassingScore ?? 50,
                Status = attempt.Status,
                EndReason = attempt.EndReason,
                StartedAt = attempt.StartedAt,
                SubmittedAt = attempt.SubmittedAt,
                DurationSeconds = durationSeconds,
                Questions = new List<ExamQuestionResultDto>()
            };

            // تفاصيل الأسئلة
            var questions = attempt.Exam?.Questions?
                .Where(q => q.DeletedAt == null)
                .OrderBy(q => q.Order)
                .ToList() ?? new List<ExamQuestion>();

            foreach (var q in questions)
            {
                var answer = attempt.Answers.FirstOrDefault(a => a.ExamQuestionId == q.Id);
                var correctOption = q.Options.FirstOrDefault(o => o.IsCorrect && o.DeletedAt == null);
                var selectedOption = answer?.SelectedOption;

                result.Questions.Add(new ExamQuestionResultDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    Points = q.Points,
                    SelectedOptionId = selectedOption?.Id,
                    SelectedOptionText = selectedOption?.OptionText,
                    CorrectOptionId = correctOption?.Id,
                    CorrectOptionText = correctOption?.OptionText,
                    IsCorrect = answer?.IsCorrect ?? false,
                    PointsEarned = answer?.PointsEarned ?? 0
                });
            }

            return result;
        }

        // ==========================================
        // ✅ دالة مساعدة: إغلاق محاولة منتهية كـ TimeExpired
        // ==========================================
        private async Task CloseAttemptAsTimeExpired(ExamAttempt attempt)
        {
            var answers = await _context.Set<ExamAnswer>()
                .Where(a => a.ExamAttemptId == attempt.Id && a.DeletedAt == null)
                .ToListAsync();

            var score = answers.Sum(a => (decimal)a.PointsEarned);
            var correct = answers.Count(a => a.IsCorrect);
            var percentage = attempt.MaxScore > 0 ? (score / attempt.MaxScore) * 100 : 0;

            var exam = await _context.Set<Exam>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == attempt.ExamId);

            var expiresAt = attempt.StartedAt.AddSeconds(attempt.AllowedDurationSeconds);

            attempt.Status = 3;
            attempt.SubmittedAt = expiresAt;
            attempt.Score = score;
            attempt.CorrectAnswers = correct;
            attempt.IsPassed = percentage >= (exam?.PassingScore ?? 50);
            attempt.EndReason = "TimeExpired";

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Stale attempt {AttemptId} closed as TimeExpired. Score: {Score}/{Max}",
                attempt.Id, score, attempt.MaxScore);
        }

        // ==========================================
        // ✅ دالة مساعدة: إغلاق كل المحاولات المنتهية للمستخدم
        // ==========================================
        private async Task ExpireStaleAttempts(string userId)
        {
            var now = DateTime.Now;
            var stale = await _context.Set<ExamAttempt>()
                .Where(a => a.UserId == userId
                         && a.Status == 1
                         && a.DeletedAt == null)
                .ToListAsync();

            if (stale.Count == 0) return;

            bool anyUpdated = false;

            foreach (var s in stale)
            {
                var expiresAt = s.StartedAt.AddSeconds(s.AllowedDurationSeconds);
                if (now < expiresAt) continue;

                // حساب النتيجة من الإجابات
                var answers = await _context.Set<ExamAnswer>()
                    .Where(a => a.ExamAttemptId == s.Id && a.DeletedAt == null)
                    .ToListAsync();

                var score = answers.Sum(a => (decimal)a.PointsEarned);
                var correct = answers.Count(a => a.IsCorrect);
                var percentage = s.MaxScore > 0 ? (score / s.MaxScore) * 100 : 0;

                var exam = await _context.Set<Exam>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == s.ExamId);

                s.Status = 3;
                s.SubmittedAt = expiresAt;
                s.Score = score;
                s.CorrectAnswers = correct;
                s.IsPassed = percentage >= (exam?.PassingScore ?? 50);
                s.EndReason = "TimeExpired";

                anyUpdated = true;
            }

            if (anyUpdated)
                await _context.SaveChangesAsync();
        }
    }
}