namespace Models
{
    // ==========================================
    // للطالب
    // ==========================================
    public class ExamListItemDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
        public int PassingScore { get; set; }
        public int QuestionCount { get; set; }
        public bool IsPublished { get; set; }

        // حالة المستخدم
        public bool HasAttempt { get; set; }
        public int AttemptsCount { get; set; }
        public int MaxAttempts { get; set; }
        public bool CanStart { get; set; }
        public string BlockReason { get; set; }
        public ExamAttemptSummaryDto LastAttempt { get; set; }
    }

    public class ExamAttemptSummaryDto
    {
        public int Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsPassed { get; set; }
        public int Status { get; set; }
        public string EndReason { get; set; }
    }

    // ==========================================
    // للامتحان الجاري
    // ==========================================
    public class ExamSessionDto
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public string CourseName { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int RemainingSeconds { get; set; }
        public int PassingScore { get; set; }
        public int TotalQuestions { get; set; }
        public List<ExamQuestionDto> Questions { get; set; } = new();
    }

    public class ExamQuestionDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public int Order { get; set; }
        public int Points { get; set; }
        public List<ExamOptionDto> Options { get; set; } = new();
    }

    public class ExamOptionDto
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
        public int Order { get; set; }
    }

    // ==========================================
    // إرسال إجابة
    // ==========================================
    public class SubmitAnswerDto
    {
        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }
    }

    public class SubmitAnswerResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int AnsweredCount { get; set; }
        public int TotalQuestions { get; set; }
        public int RemainingSeconds { get; set; }
    }

    // ==========================================
    // النتيجة
    // ==========================================
    public class ExamResultDto
    {
        public int AttemptId { get; set; }
        public string ExamTitle { get; set; }
        public string CourseName { get; set; }
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Percentage { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsPassed { get; set; }
        public int PassingScore { get; set; }
        public int Status { get; set; }
        public string EndReason { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int DurationSeconds { get; set; }
        public List<ExamQuestionResultDto> Questions { get; set; } = new();
    }

    public class ExamQuestionResultDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int Points { get; set; }
        public int? SelectedOptionId { get; set; }
        public string SelectedOptionText { get; set; }
        public int? CorrectOptionId { get; set; }
        public string CorrectOptionText { get; set; }
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
    }
}