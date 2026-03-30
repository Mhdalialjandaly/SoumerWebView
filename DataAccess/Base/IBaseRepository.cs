using Core;
using System.Linq.Expressions;

namespace DataAccess.Base
{
    public interface IBaseRepository<TModel, TEntity>
       where TModel : class
       where TEntity : class, IEntity
    {
        #region Get
        IQueryable<TEntity> Get();
        Task<List<TModel>> GetAll();
        Task<List<TModel>> GetAllByUserId();
        Task<TModel> GetById(object id);
        Task<TModel> GetByIdByUserId(object id);
        Task<List<TModel>> GetByKey(Expression<Func<TEntity, bool>> predicate);
        Task<List<TModel>> GetByKeyByUserId(Expression<Func<TEntity, bool>> predicate);
        #endregion

        #region Add
        Task<TModel> Add(TModel modelToAdd);
        Task AddRange(IList<TModel> modelsToAdd);
        #endregion

        #region Send And Mode
        Task<TModel> Send(TModel modelToAdd,int userid);
        Task<bool> Move(TModel modelToAdd,int userid);
        Task SendRange(IList<TModel> modelsToAdd, int userid);
        #endregion

        #region Update
        Task Update(TModel modelToUpdate);
        Task UpdateRange(IList<TModel> modelsToUpdate);
        #endregion

        #region Delete
        Task Delete(int id);
        Task Delete(Expression<Func<TEntity, bool>> predicate);
        #endregion


        Task<int> SaveChangesAsync();
    }
}
