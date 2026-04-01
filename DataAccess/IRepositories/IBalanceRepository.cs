using Core.Enums;
using Core.Interface;
using DataAccess.Base;
using DataAccess.Entities;
using Models;

namespace DataAccess.IRepositories
{
    public interface IBalanceRepository : IBaseRepository<BalanceDto,Balance>,IInjectable
    {
        Task<BalanceDto> GetBalanceByUserId(string userId);
        Task<BalanceTransactionDto> AddTransaction(int balanceId, decimal amount, TransactionType transactionType, string description = null);
        Task<List<BalanceTransactionDto>> GetTransactionsByBalanceId(int balanceId, int page = 1, int pageSize = 20);
        Task<decimal> GetTotalBalanceByUserId(string userId);
    }
}
