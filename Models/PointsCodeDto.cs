namespace Models
{
    public class PointsCodeDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal PointsValue { get; set; }
        public bool IsUsed { get; set; }
        public string UsedByUserId { get; set; }
        public string UsedByUserName { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate < DateTime.Now;
        public bool IsValid => !IsUsed && !IsExpired && PointsValue > 0;
    }

    public class CreatePointsCodeDto
    {
        public decimal PointsValue { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int NumberOfCodes { get; set; } = 1;
    }

    public class RedeemPointsCodeDto
    {
        public string Code { get; set; }
    }
}