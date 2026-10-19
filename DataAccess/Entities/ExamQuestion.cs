using Core;

namespace DataAccess.Entities
{
    public class ExamQuestion : IEntity
    {
        public ExamQuestion()
        {
            Options = new HashSet<ExamOption>();
        }

        public int Id { get; set; }

        public int ExamId { get; set; }
        public Exam Exam { get; set; }

        public string QuestionText { get; set; }
        public int Order { get; set; }

        /// <summary>درجة السؤال</summary>
        public int Points { get; set; } = 1;

        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }

        public virtual ICollection<ExamOption> Options { get; set; }
    }
}