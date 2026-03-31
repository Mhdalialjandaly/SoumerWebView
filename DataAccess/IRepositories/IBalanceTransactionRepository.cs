using Core.Interface;
using DataAccess.Base;
using DataAccess.Entities;
using Models;

namespace DataAccess.IRepositories
{
    public interface IBalanceTransactionRepository : IBaseRepository<BalanceTransactionDto,BalanceTransaction>,IInjectable
    {
    }
}
