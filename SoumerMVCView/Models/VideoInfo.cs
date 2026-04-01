using Core.Enums;

namespace SoumerMVCView.Models
{
    public class VideoInfo
    {
        public VideoPlatform Platform { get; set; }
        public string VideoId { get; set; }
        public string EmbedUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string WatchUrl { get; set; }
        public int? Duration { get; set; }
        public string Title { get; set; }
    }
}
