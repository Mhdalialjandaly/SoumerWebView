using Core;

namespace DataAccess.Entities
{
    public class CourseRegistration : IEntity
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? DeletedAt { get ; set ; }
        public DateTime? ModifiedAt { get ; set ; }
        public string ModifiedBy { get ; set ; }
        public string DeletedBy { get ; set ; }
    }
}
