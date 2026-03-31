
namespace Models
{
    public class TeacherCourseDto
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public TeacherDto Teacher { get; set; }
        public int CourseId { get; set; }
        public CourseDto Course { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
    }
}
