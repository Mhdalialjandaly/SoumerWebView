using Models;
using SoumerMVCView.Models;

public interface IBalanceService
{
    Task<BalanceViewModel> GetUserBalance(string userId);
    Task<BalanceTransactionDto> AddPoints(string userId, decimal amount, string description);
    Task<BalanceTransactionDto> DeductPoints(string userId, decimal amount, string description);
    Task<List<BalanceTransactionDto>> GetUserTransactions(string userId, int page = 1, int pageSize = 20);
    Task<bool> TransferPoints(string fromUserId, string toUserId, decimal amount, string description);
    Task<List<UserSearchResult>> SearchUsers(string query, string currentUserId = null);
    Task<decimal> GetUserBalanceAmount(string userId);
    Task<bool> HasEnoughBalance(string userId, decimal amount);

    // طرق جديدة للأكواد
    Task<List<PointsCodeDto>> GenerateCodes(decimal pointsValue, int numberOfCodes, DateTime? expiryDate);
    Task<PointsCodeDto> RedeemCode(string userId, string code);
    Task<List<PointsCodeDto>> GetValidCodes();
    Task<List<PointsCodeDto>> GetUsedCodes();
}