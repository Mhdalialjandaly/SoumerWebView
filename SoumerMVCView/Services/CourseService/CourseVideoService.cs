using Core.Enums;
using DataAccess.IRepositories;
using Models;
using SoumerMVCView.Models;
using SoumerMVCView.Services.CourseService;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoumerMVCView.Services.CourseService
{
    public class CourseVideoService : ICourseVideoService
    {
        private readonly ICourseVideoRepository _videoRepository;
        private readonly ICourseRepository _courseRepository;

        public CourseVideoService(
            ICourseVideoRepository videoRepository,
            ICourseRepository courseRepository)
        {
            _videoRepository = videoRepository;
            _courseRepository = courseRepository;
        }

        public async Task<VideoInfo> ExtractVideoInfo(string videoUrl)
        {
            var info = new VideoInfo();

            // دعم جميع صيغ روابط يوتيوب
            if (IsYouTubeUrl(videoUrl))
            {
                info.Platform = VideoPlatform.YouTube;
                info.VideoId = ExtractYouTubeId(videoUrl);
                info.EmbedUrl = $"https://www.youtube.com/embed/{info.VideoId}";
                info.ThumbnailUrl = $"https://img.youtube.com/vi/{info.VideoId}/maxresdefault.jpg";
                info.WatchUrl = $"https://www.youtube.com/watch?v={info.VideoId}";
            }
            // دعم روابط فيميو
            else if (IsVimeoUrl(videoUrl))
            {
                info.Platform = VideoPlatform.Vimeo;
                info.VideoId = ExtractVimeoId(videoUrl);
                info.EmbedUrl = $"https://player.vimeo.com/video/{info.VideoId}";
                info.ThumbnailUrl = $"https://vumbnail.com/{info.VideoId}.jpg";
                info.WatchUrl = videoUrl;
            }

            return info;
        }

        private bool IsYouTubeUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;

            var patterns = new[]
            {
                @"youtube\.com\/watch\?v=([^""&?\/\s]{11})",
                @"youtube\.com\/embed\/([^""&?\/\s]{11})",
                @"youtu\.be\/([^""&?\/\s]{11})",
                @"youtube\.com\/v\/([^""&?\/\s]{11})",
                @"youtube\.com\/shorts\/([^""&?\/\s]{11})"
            };

            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(url, pattern))
                    return true;
            }

            return url.Contains("youtube.com") || url.Contains("youtu.be");
        }

        private string ExtractYouTubeId(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            // قائمة بأنماط الروابط المدعومة
            var patterns = new (string pattern, int group)[]
            {
                (@"youtube\.com\/watch\?v=([^""&?\/\s]{11})", 1),
                (@"youtube\.com\/embed\/([^""&?\/\s]{11})", 1),
                (@"youtu\.be\/([^""&?\/\s]{11})", 1),
                (@"youtube\.com\/v\/([^""&?\/\s]{11})", 1),
                (@"youtube\.com\/shorts\/([^""&?\/\s]{11})", 1),
                (@"youtube\.com\/watch\?.*v=([^""&?\/\s]{11})", 1)
            };

            foreach (var (pattern, group) in patterns)
            {
                var match = Regex.Match(url, pattern);
                if (match.Success && match.Groups.Count > group)
                {
                    return match.Groups[group].Value;
                }
            }

            return string.Empty;
        }

        private bool IsVimeoUrl(string url)
        {
            return !string.IsNullOrEmpty(url) &&
                   (url.Contains("vimeo.com") || url.Contains("player.vimeo.com"));
        }

        private string ExtractVimeoId(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            var patterns = new[]
            {
                @"vimeo\.com\/([0-9]+)",
                @"player\.vimeo\.com\/video\/([0-9]+)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(url, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value;
                }
            }

            return string.Empty;
        }

        public async Task<List<CourseVideoDto>> GetCourseVideos(int courseId)
        {
            var videos = await _videoRepository.GetVideosByCourseId(courseId);
            return videos;
        }

        public async Task<CourseVideoDto> GetVideoById(int videoId)
        {
            return await _videoRepository.GetById(videoId);
        }

        public async Task<bool> AddVideo(CourseVideoDto videoDto)
        {
            // استخراج معلومات الفيديو من الرابط
            var videoInfo = await ExtractVideoInfo(videoDto.VideoUrl);

            videoDto.Platform = videoInfo.Platform;
            videoDto.VideoId = videoInfo.VideoId;
            videoDto.EmbedUrl = videoInfo.EmbedUrl;
            var added = await _videoRepository.Add(videoDto);
            return added != null;
        }

        public async Task<bool> UpdateVideo(CourseVideoDto videoDto)
        {
            // إذا تم تغيير الرابط، قم باستخراج المعلومات الجديدة
            var existingVideo = await _videoRepository.GetById(videoDto.Id);
            if (existingVideo != null && existingVideo.VideoUrl != videoDto.VideoUrl)
            {
                var videoInfo = await ExtractVideoInfo(videoDto.VideoUrl);
                videoDto.Platform = videoInfo.Platform;
                videoDto.VideoId = videoInfo.VideoId;
                videoDto.EmbedUrl = videoInfo.EmbedUrl;
            }
            await _videoRepository.Update(videoDto);
            return true;
        }

        public async Task<bool> DeleteVideo(int videoId)
        {
            await _videoRepository.Delete(videoId);
            return true;
        }

        public async Task<bool> ReorderVideos(int courseId, Dictionary<int, int> videoOrders)
        {
            foreach (var videoOrder in videoOrders)
            {
                var video = await _videoRepository.GetById(videoOrder.Key);
                if (video != null && video.CourseId == courseId)
                {
                    video.Order = videoOrder.Value;
                    await _videoRepository.Update(video);
                }
            }
            return true;
        }

        public async Task<bool> CanWatchVideo(int videoId, string userId)
        {
            var video = await _videoRepository.GetById(videoId);
            if (video == null) return false;

            // إذا كان الفيديو مجاني، يمكن مشاهدته
            if (video.IsFree) return true;

            // التحقق من أن المستخدم مسجل في الكورس
            var course = await _courseRepository.GetById(video.CourseId);
            if (course == null) return false;

            // التحقق من تسجيل المستخدم في الكورس
            var isEnrolled = await _courseRepository.IsUserEnrolled(video.CourseId, userId);
            return isEnrolled;
        }
    }

  
}