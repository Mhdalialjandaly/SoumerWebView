namespace Models
{
    public class TeacherGradeAssignmentDto
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int Grade { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }

        public TeacherDto Teacher { get; set; }
    }
}
