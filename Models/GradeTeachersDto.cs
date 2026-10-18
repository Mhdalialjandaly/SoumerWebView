namespace Models
{
    public class GradeTeachersDto
    {
        public int Grade { get; set; }
        public string GradeName { get; set; }
        public string GradeLevel { get; set; }
        public List<TeacherDto> Teachers { get; set; }
    }
}
