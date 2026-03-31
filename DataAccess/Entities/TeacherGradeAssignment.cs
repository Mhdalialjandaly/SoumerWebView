using Core;

namespace DataAccess.Entities
{
    public class TeacherGradeAssignment : IEntity
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int Grade { get; set; }
        public int Priority { get; set; } // لتحديد أولوية التدريس
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }

        // Navigation properties
        public virtual Teacher Teacher { get; set; }
    }
}
