using AutoMapper;
using Core;
using Core.Enums;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories
{
    public class BalanceRepository : BaseRepository<BalanceDto, Balance>, IBalanceRepository
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly UserDto _user;
        public BalanceRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
            _mapper = mapper;
            _context = context;
            _user = Global.GetValue(GlobalKeys.LoggedUser) as UserDto;
        }
        public async Task<BalanceDto> GetBalanceByUserId(string userId)
        {
            try
            {
                var balance = await _context.Set<Balance>()
                    .Include(b => b.User)
                    .Include(b => b.BalanceTransactions)
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.DeletedAt == null);

                return _mapper.Map<BalanceDto>(balance);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<BalanceTransactionDto> AddTransaction(int balanceId, decimal amount, TransactionType transactionType, string description = null)
        {
            try
            {
                var balance = await _context.Set<Balance>().FindAsync(balanceId);
                if (balance == null)
                    return null;

                // تحديث الرصيد
                if (transactionType == TransactionType.Credit)
                    balance.Amount += amount;
                else if (transactionType == TransactionType.Debit)
                    balance.Amount -= amount;

                balance.ModifiedAt = DateTime.Now;
                balance.ModifiedBy = _user?.UserName ?? "System";

                // إنشاء سجل المعاملة
                var transaction = new BalanceTransaction
                {
                    Amount = amount,
                    TransactionType = transactionType,
                    BalanceId = balanceId,
                    Description = description,
                    CreatedAt = DateTime.Now
                };

                await _context.Set<BalanceTransaction>().AddAsync(transaction);
                await SaveChangesAsync();

                return _mapper.Map<BalanceTransactionDto>(transaction);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<BalanceTransactionDto>> GetTransactionsByBalanceId(int balanceId, int page = 1, int pageSize = 20)
        {
            try
            {
                var transactions = await _context.Set<BalanceTransaction>()
                    .Where(t => t.BalanceId == balanceId && t.DeletedAt == null)
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return _mapper.Map<List<BalanceTransactionDto>>(transactions);
            }
            catch (Exception)
            {
                return new List<BalanceTransactionDto>();
            }
        }

        public async Task<decimal> GetTotalBalanceByUserId(string userId)
        {
            try
            {
                var balance = await GetBalanceByUserId(userId);
                return balance?.Amount ?? 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}