using Core.Enums;
using Models;

namespace SoumerMVCView.Models
{
    public class BalanceViewModel
    {
        public string UserName { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal TotalPoints { get; set; }
        public List<BalanceTransactionDto> RecentTransactions { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public int TotalTransactions { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<UserSearchResult> UserSearchResults { get; set; }
    }

    public class PointsTransactionViewModel
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public TransactionType TransactionType { get; set; }
        public string RelatedUserName { get; set; }
    }

    public class TransferPointsViewModel
    {
        public string ToUserId { get; set; }
        public string ToUserName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}