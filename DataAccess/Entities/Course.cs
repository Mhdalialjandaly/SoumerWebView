using Core;

namespace DataAccess.Entities
{
    public class Course : IEntity
    {
        public Course()
        {
            TeacherCourses = new HashSet<TeacherCourse>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description  { get; set; }
        public decimal Price { get; set; }
        public virtual ICollection<TeacherCourse> TeacherCourses { get; set; }
        public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
    }
}
