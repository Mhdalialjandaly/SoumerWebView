using Core;

namespace DataAccess.Entities
{
    public class ExamAttempt : IEntity
    {
        public ExamAttempt()
        {
            Answers = new HashSet<ExamAnswer>();
        }

        public int Id { get; set; }

        public int ExamId { get; set; }
        public Exam Exam { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        /// <summary>وقت بدء المحاولة</summary>
        public DateTime StartedAt { get; set; }

        /// <summary>وقت تسليم المحاولة</summary>
        public DateTime? SubmittedAt { get; set; }

        /// <summary>المدة الممنوحة بالثواني (لقطة وقت البدء)</summary>
        public int AllowedDurationSeconds { get; set; }

        /// <summary>حالة المحاولة: 0=Pending, 1=InProgress, 2=Submitted, 3=Expired, 4=ForcedFail</summary>
        public int Status { get; set; }

        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }

        /// <summary>هل نجح؟</summary>
        public bool IsPassed { get; set; }

        /// <summary>سبب انتهاء المحاولة (اختياري للتوثيق)</summary>
        public string EndReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }

        public virtual ICollection<ExamAnswer> Answers { get; set; }
    }
}