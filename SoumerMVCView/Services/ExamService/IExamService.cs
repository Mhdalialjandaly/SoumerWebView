using Models;

public interface IExamService
{
    Task<List<ExamListItemDto>> GetExamsForCourse(int courseId, string userId);

    // ✅ تم تغييره: يعيد Tuple بدلاً من out
    Task<(bool CanStart, string Reason)> CanUserTakeExam(int examId, string userId);

    Task<ExamSessionDto> StartExam(int examId, string userId);
    Task<ExamSessionDto?> GetActiveSession(int attemptId, string userId);
    Task<SubmitAnswerResultDto> SubmitAnswer(SubmitAnswerDto dto, string userId);
    Task<ExamResultDto> SubmitExam(int attemptId, string userId, string endReason = "ManualSubmit");
    Task<ExamResultDto?> GetResult(int attemptId, string userId);
}