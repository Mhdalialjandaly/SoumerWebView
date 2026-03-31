namespace Models
{
    public class BalanceDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }

        // إضافة العلاقة العكسية
        public  UserDto User { get; set; }

        public List<BalanceTransactionDto> BalanceTransactions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
    }
}
