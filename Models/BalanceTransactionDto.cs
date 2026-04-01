using Core.Enums;

namespace Models
{
    public class BalanceTransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public TransactionType TransactionType { get; set; }
        public int BalanceId { get; set; }
        public BalanceDto Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
    }
}
