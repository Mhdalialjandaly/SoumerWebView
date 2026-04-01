using Core.Enums;
using DataAccess.IRepositories;
using Models;
using SoumerMVCView.Models;

namespace SoumerMVCView.Services.BalanceService
{
    public class BalanceService : IBalanceService
    {
        private readonly IBalanceRepository _balanceRepository;
        private readonly IUserRepository _userRepository;

        public BalanceService(IBalanceRepository balanceRepository, IUserRepository userRepository)
        {
            _balanceRepository = balanceRepository;
            _userRepository = userRepository;
        }

        public async Task<BalanceViewModel> GetUserBalance(string userId)
        {
            try
            {
                var balance = await _balanceRepository.GetBalanceByUserId(userId);

                if (balance == null)
                {
                    // إنشاء رصيد جديد للمستخدم إذا لم يكن موجوداً
                    balance = await CreateInitialBalance(userId);
                }

                var recentTransactions = await _balanceRepository.GetTransactionsByBalanceId(balance.Id, 1, 5);

                return new BalanceViewModel
                {
                    CurrentBalance = balance.Amount,
                    TotalPoints = balance.Amount,
                    RecentTransactions = recentTransactions,
                    LastUpdateDate = balance.ModifiedAt ?? balance.CreatedAt
                };
            }
            catch (Exception)
            {
                return new BalanceViewModel
                {
                    CurrentBalance = 0,
                    TotalPoints = 0,
                    RecentTransactions = new List<BalanceTransactionDto>()
                };
            }
        }

        private async Task<BalanceDto> CreateInitialBalance(string userId)
        {
            var newBalance = new BalanceDto
            {
                User = new UserDto { Id = userId },
                Amount = 0,
                CreatedAt = DateTime.Now
            };

            return await _balanceRepository.Add(newBalance);
        }

        public async Task<BalanceTransactionDto> AddPoints(string userId, decimal amount, string description)
        {
            var balance = await _balanceRepository.GetBalanceByUserId(userId);
            if (balance == null)
            {
                balance = await CreateInitialBalance(userId);
            }

            return await _balanceRepository.AddTransaction(balance.Id, amount, TransactionType.Credit, description);
        }

        public async Task<BalanceTransactionDto> DeductPoints(string userId, decimal amount, string description)
        {
            var balance = await _balanceRepository.GetBalanceByUserId(userId);
            if (balance == null || balance.Amount < amount)
                throw new Exception("الرصيد غير كافي");

            return await _balanceRepository.AddTransaction(balance.Id, amount, TransactionType.Debit, description);
        }

        public async Task<List<BalanceTransactionDto>> GetUserTransactions(string userId, int page = 1, int pageSize = 20)
        {
            var balance = await _balanceRepository.GetBalanceByUserId(userId);
            if (balance == null)
                return new List<BalanceTransactionDto>();

            return await _balanceRepository.GetTransactionsByBalanceId(balance.Id, page, pageSize);
        }

        public async Task<bool> TransferPoints(string fromUserId, string toUserId, decimal amount, string description)
        {
            try
            {
                // خصم النقاط من المرسل
                var debitTransaction = await DeductPoints(fromUserId, amount, $"تحويل نقاط إلى المستخدم {toUserId} - {description}");
                if (debitTransaction == null)
                    return false;

                // إضافة النقاط للمستلم
                var creditTransaction = await AddPoints(toUserId, amount, $"استلام نقاط من المستخدم {fromUserId} - {description}");
                if (creditTransaction == null)
                    return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
