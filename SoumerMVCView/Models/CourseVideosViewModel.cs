using Models;

namespace SoumerMVCView.Models
{
    public class CourseVideosViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public List<CourseVideoDto> Videos { get; set; }
        public bool IsEnrolled { get; set; }
        public int TotalVideos { get; set; }
        public int WatchedVideos { get; set; } // للتتبع المستقبلي
        public int ProgressPercentage { get; set; } // للتتبع المستقبلي
    }

    public class VideoProgressModel
    {
        public int VideoId { get; set; }
        public double Progress { get; set; }
        public double WatchTime { get; set; }
    }

    public class VideoReportModel
    {
        public int VideoId { get; set; }
        public string Message { get; set; }
    }
}
