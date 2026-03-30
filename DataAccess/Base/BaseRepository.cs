using AutoMapper;
using Core;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Linq.Expressions;

namespace DataAccess.Base
{
    public class BaseRepository<TModel, TEntity> : IBaseRepository<TModel, TEntity>
      where TEntity : class, IEntity
      where TModel : class
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly UserDto _user;
        public BaseRepository(IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;


            _user = Global.GetValue(GlobalKeys.LoggedUser) as UserDto;
            if (_user is null)
                _user = new UserDto();
            
        }

        public virtual IQueryable<TEntity> Get()
        {
            return _context.Set<TEntity>();
        }

        public virtual async Task<List<TModel>> GetAll()
        {
            try
            {
                var result = await _context.Set<TEntity>().AsNoTracking()
                .Where(e => e.DeletedAt == null)
                .ToListAsync();
                return _mapper.Map<List<TModel>>(result);
            }
            catch (Exception)
            {
                return null;
            }

        }
        public virtual async Task<List<TModel>> GetAllByUserId()
        {
            try
            {
                var result = await _context.Set<TEntity>().AsNoTracking()
                .Where(e => e.DeletedAt == null)
                .ToListAsync();
                return _mapper.Map<List<TModel>>(result);
            }
            catch (Exception)
            {
                return null;
            }

        }

        public virtual async Task<TModel> GetById(object id)
        {
            var result = await _context.Set<TEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => EF.Property<object>(e, "Id").Equals(id));
            return _mapper.Map<TModel>(result);
        }

        public virtual async Task<TModel> GetByIdByUserId(object id)
        {
            var result = await _context.Set<TEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => EF.Property<object>(e, "Id").Equals(id));
            return _mapper.Map<TModel>(result);
        }

        public virtual async Task<List<TModel>> GetByKey(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                var result = await _context.Set<TEntity>().AsNoTracking()
                .Where(e => e.DeletedAt == null)
                .Where(predicate)
                .ToListAsync();
                return _mapper.Map<List<TModel>>(result);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<List<TModel>> GetByKeyByUserId(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                var result = await _context.Set<TEntity>().AsNoTracking()
                .Where(e => e.DeletedAt == null)
                .Where(predicate)
                .ToListAsync();
                return _mapper.Map<List<TModel>>(result);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<TModel> Add(TModel modelToAdd)
        {
            try
            {
                var entityToAdd = _mapper.Map<TEntity>(modelToAdd);
                entityToAdd.CreatedAt = DateTime.Now;
                var result = await _context.AddAsync(entityToAdd);

                var changedEntriesCopy = _context.ChangeTracker.Entries()
                    .Where(e => e.State is EntityState.Added or
                            EntityState.Modified or EntityState.Deleted)
                    .ToList();

                await SaveChangesAsync();

                foreach (var entry in changedEntriesCopy)
                    entry.State = EntityState.Detached;

                return _mapper.Map<TModel>(result.Entity);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task AddRange(IList<TModel> modelsToAdd)
        {
            var entitiesToAdd = _mapper.Map<List<TEntity>>(modelsToAdd);
            entitiesToAdd.ForEach(e => e.ModifiedAt = DateTime.Now);
            await _context.AddRangeAsync(entitiesToAdd);

            var changedEntriesCopy = _context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or
                    EntityState.Modified or EntityState.Deleted)
                .ToList();

            await SaveChangesAsync();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }

        public virtual async Task Update(TModel modelToUpdate)
        {
            var entityToUpdate = _mapper.Map<TEntity>(modelToUpdate);
            entityToUpdate.ModifiedAt = DateTime.Now;
            entityToUpdate.ModifiedBy = _user.UserName;
            _context.Update(entityToUpdate);

            var changedEntriesCopy = _context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or
                    EntityState.Modified or EntityState.Deleted)
                .ToList();
            await SaveChangesAsync();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;


        }

        public async Task UpdateRange(IList<TModel> modelsToUpdate)
        {
            var entitiesToUpdate = _mapper.Map<List<TEntity>>(modelsToUpdate);
            entitiesToUpdate.ForEach(e => e.ModifiedAt = DateTime.Now);
            _context.UpdateRange(entitiesToUpdate);

            var changedEntriesCopy = _context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or
                    EntityState.Modified or EntityState.Deleted)
                .ToList();

            await SaveChangesAsync();
            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }

        public virtual async Task Delete(int id)
        {
            var entityToDelete = await _context.Set<TEntity>().FindAsync(id);
            entityToDelete.DeletedAt = DateTime.Now;
            entityToDelete.DeletedBy = _user.UserName;
            await SaveChangesAsync();
        }

        public virtual async Task Delete(Expression<Func<TEntity, bool>> predicate)
        {
            var entityToDelete = await _context.Set<TEntity>()
                .Where(predicate).ToListAsync();
            foreach (var entityModel in entityToDelete)
            {
                entityModel.DeletedAt = DateTime.Now;
                entityModel.DeletedBy = Global.DeviceName;
            }
            await SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {

            return await _context.SaveChangesAsync();

        }

        public async Task<TModel> Send(TModel modelToAdd, int userid)
        {
            try
            {
                var entityToAdd = _mapper.Map<TEntity>(modelToAdd);
                entityToAdd.CreatedAt = DateTime.Now;
                var result = await _context.AddAsync(entityToAdd);

                var changedEntriesCopy = _context.ChangeTracker.Entries()
                    .Where(e => e.State is EntityState.Added or
                            EntityState.Modified or EntityState.Deleted)
                    .ToList();

                await SaveChangesAsync();

                foreach (var entry in changedEntriesCopy)
                    entry.State = EntityState.Detached;

                return _mapper.Map<TModel>(result.Entity);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> Move(TModel modelToAdd, int userid)
        {
            var entityToUpdate = _mapper.Map<TEntity>(modelToAdd);
            entityToUpdate.ModifiedAt = DateTime.Now;
            entityToUpdate.ModifiedBy = _user.UserName;

            _context.Update(entityToUpdate);

            var changedEntriesCopy = _context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or
                    EntityState.Modified or EntityState.Deleted)
                .ToList();
            await SaveChangesAsync();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;

            return true;
        }

        public async Task SendRange(IList<TModel> modelsToAdd, int userid)
        {
            var entitiesToAdd = _mapper.Map<List<TEntity>>(modelsToAdd);
            entitiesToAdd.ForEach(e => e.ModifiedAt = DateTime.Now);
            await _context.AddRangeAsync(entitiesToAdd);

            var changedEntriesCopy = _context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or
                    EntityState.Modified or EntityState.Deleted)
                .ToList();

            await SaveChangesAsync();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }
    }
}
