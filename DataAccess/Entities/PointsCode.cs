using Core;

namespace DataAccess.Entities
{
    public class PointsCode : IEntity
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal PointsValue { get; set; }
        public bool IsUsed { get; set; }
        public string UsedByUserId { get; set; }
        public virtual User UsedByUser { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
    }
}