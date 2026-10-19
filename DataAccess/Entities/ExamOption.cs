using Core;

namespace DataAccess.Entities
{
    public class ExamOption : IEntity
    {
        public int Id { get; set; }

        public int ExamQuestionId { get; set; }
        public ExamQuestion ExamQuestion { get; set; }

        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public int Order { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
    }
}