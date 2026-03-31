using AutoMapper;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Models;

namespace DataAccess.Repositories
{
    public class BalanceRepository : BaseRepository<BalanceDto, Balance>, IBalanceRepository
    {
        public BalanceRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}
