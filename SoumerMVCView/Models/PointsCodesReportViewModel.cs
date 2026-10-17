using Models;

namespace SoumerMVCView.Models
{
    public class PointsCodesReportViewModel
    {
        // ====== الإحصائيات ======
        public int TotalCodes { get; set; }
        public int ValidCodes { get; set; }
        public int UsedCodes { get; set; }
        public int ExpiredCodes { get; set; }
        public int DeletedCodes { get; set; }

        public decimal TotalPointsValue { get; set; }
        public decimal ValidPointsValue { get; set; }
        public decimal UsedPointsValue { get; set; }
        public decimal ExpiredPointsValue { get; set; }

        // ====== الفلاتر ======
        public string Status { get; set; }       // all | valid | used | expired
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchCode { get; set; }

        // ====== البيانات ======
        public List<PointsCodeDto> Codes { get; set; } = new();

        // ====== إحصائية آخر 30 يوماً ======
        public List<DailyUsageDto> DailyUsage { get; set; } = new();

        // ====== أعلى 5 مستخدمين ======
        public List<TopUserDto> TopUsers { get; set; } = new();
    }

    public class DailyUsageDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Points { get; set; }
    }

    public class TopUserDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int CodesCount { get; set; }
        public decimal TotalPoints { get; set; }
    }
}