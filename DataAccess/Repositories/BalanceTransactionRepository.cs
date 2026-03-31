using AutoMapper;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Models;

namespace DataAccess.Repositories
{
    public class BalanceTransactionRepository : BaseRepository<BalanceTransactionDto, BalanceTransaction>, IBalanceTransactionRepository
    {
        public BalanceTransactionRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}
