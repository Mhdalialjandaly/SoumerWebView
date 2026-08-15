using Core;

namespace DataAccess.Entities
{
    public class Teacher : IEntity
    {
        public Teacher()
        {
            TeacherCourses = new HashSet<TeacherCourse>();
        }
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<TeacherCourse> TeacherCourses { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }

    }
}
