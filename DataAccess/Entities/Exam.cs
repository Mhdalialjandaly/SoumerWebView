using Core;

namespace DataAccess.Entities
{
    public class Exam : IEntity
    {
        public Exam()
        {
            Questions = new HashSet<ExamQuestion>();
            Attempts = new HashSet<ExamAttempt>();
        }

        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        /// <summary>مدة الامتحان بالدقائق</summary>
        public int DurationMinutes { get; set; }

        /// <summary>درجة النجاح (نسبة مئوية 0-100)</summary>
        public int PassingScore { get; set; } = 50;

        /// <summary>هل يمكن إعادة المحاولة؟</summary>
        public bool AllowRetake { get; set; } = false;

        /// <summary>عدد المحاولات المسموحة (0 = غير محدود)</summary>
        public int MaxAttempts { get; set; } = 1;

        /// <summary>خلط ترتيب الأسئلة</summary>
        public bool ShuffleQuestions { get; set; } = true;

        /// <summary>هل الامتحان منشور للطلاب؟</summary>
        public bool IsPublished { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }

        public virtual ICollection<ExamQuestion> Questions { get; set; }
        public virtual ICollection<ExamAttempt> Attempts { get; set; }
    }
}