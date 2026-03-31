using Core;

namespace DataAccess.Entities
{
    public class TeacherCourse : IEntity
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? DeletedAt { get ; set ; }
        public DateTime? ModifiedAt { get ; set ; }
        public string ModifiedBy { get ; set ; }
        public string DeletedBy { get ; set ; }
    }
}
