using Models;
using SoumerMVCView.Models;

namespace SoumerMVCView.Services.CourseService
{
    public interface ICourseVideoService
    {
        Task<List<CourseVideoDto>> GetCourseVideos(int courseId);
        Task<CourseVideoDto> GetVideoById(int videoId);
        Task<bool> AddVideo(CourseVideoDto videoDto);
        Task<bool> UpdateVideo(CourseVideoDto videoDto);
        Task<bool> DeleteVideo(int videoId);
        Task<bool> ReorderVideos(int courseId, Dictionary<int, int> videoOrders);
        Task<VideoInfo> ExtractVideoInfo(string videoUrl);
        Task<bool> CanWatchVideo(int videoId, string userId);
    }
}
