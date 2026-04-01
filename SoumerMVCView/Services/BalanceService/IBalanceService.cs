using Models;
using SoumerMVCView.Models;

namespace SoumerMVCView.Services.BalanceService
{

    public interface IBalanceService
    {
        Task<BalanceViewModel> GetUserBalance(string userId);
        Task<BalanceTransactionDto> AddPoints(string userId, decimal amount, string description);
        Task<BalanceTransactionDto> DeductPoints(string userId, decimal amount, string description);
        Task<List<BalanceTransactionDto>> GetUserTransactions(string userId, int page = 1, int pageSize = 20);
        Task<bool> TransferPoints(string fromUserId, string toUserId, decimal amount, string description);
    }
}