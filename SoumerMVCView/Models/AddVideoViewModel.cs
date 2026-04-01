namespace SoumerMVCView.Models
{
    public class AddVideoViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; } 
        public int Order { get; set; }
        public bool IsFree { get; set; }
        public bool IsPublished { get; set; }
    }
}
