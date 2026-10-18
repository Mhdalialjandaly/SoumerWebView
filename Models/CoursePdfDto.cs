namespace Models
{
    public class CoursePdfDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string OriginalFileName { get; set; }
        public long FileSize { get; set; }
        public int Order { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFree { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public string FileSizeFormatted =>
            FileSize < 1024 ? $"{FileSize} B" :
            FileSize < 1024 * 1024 ? $"{FileSize / 1024.0:F1} KB" :
            $"{FileSize / (1024.0 * 1024.0):F1} MB";
    }
}