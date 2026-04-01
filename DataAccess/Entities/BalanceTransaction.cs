using Core;
using Core.Enums;

namespace DataAccess.Entities
{
    public class BalanceTransaction : IEntity
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Description { get; set; }
        public int BalanceId { get; set; }
        public Balance Balance { get; set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? DeletedAt { get ; set ; }
        public DateTime? ModifiedAt { get ; set ; }
        public string ModifiedBy { get ; set ; }
        public string DeletedBy { get ; set ; }
    }
}
