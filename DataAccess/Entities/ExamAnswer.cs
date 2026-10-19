using Core;

namespace DataAccess.Entities
{
    public class ExamAnswer : IEntity
    {
        public int Id { get; set; }

        // ==========================================
        // المحاولة التي تنتمي إليها الإجابة
        // ==========================================
        public int ExamAttemptId { get; set; }
        public ExamAttempt ExamAttempt { get; set; }

        // ==========================================
        // السؤال الذي تمت الإجابة عليه
        // ==========================================
        public int ExamQuestionId { get; set; }
        public ExamQuestion ExamQuestion { get; set; }

        // ==========================================
        // الخيار المختار (null = لم يجب الطالب)
        // ==========================================
        public int? SelectedOptionId { get; set; }
        public ExamOption SelectedOption { get; set; }

        // ==========================================
        // نتائج التصحيح
        // ==========================================
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }

        // ==========================================
        // بيانات التتبع
        // ==========================================
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
    }
}