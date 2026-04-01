using Core.Enums;

namespace Models
{
    public class CourseVideoDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public VideoPlatform Platform { get; set; }
        public string VideoId { get; set; }         
        public string EmbedUrl { get; set; }       
        public int Duration { get; set; }
        public int Order { get; set; }
        public bool IsFree { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
