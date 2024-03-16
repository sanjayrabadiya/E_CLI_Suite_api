using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GSC.Common.Base;
using GSC.Shared.Generic;
using Microsoft.EntityFrameworkCore;

namespace GSC.Common.GenericRespository
{
    public abstract class GenericRespository<TC> : IGenericRepository<TC> where TC : class
    {
        internal IContext Context { get; private set; }
        internal readonly DbSet<TC> _dbSet;
        protected GenericRespository(IContext context)
        {
            Context = context;
            _dbSet = Context.Set<TC>();
        }

        public void SetDbConnection(string connectionString)
        {
            this.Context.SetDBConnection(connectionString);
        }

        public IQueryable<TC> All => Context.Set<TC>();

        public virtual void Add(TC entity)
        {
            Context.SetAdd(entity);
        }

        public IQueryable<TC> AllIncluding(params Expression<Func<TC, object>>[] includeProperties)
        {
            return GetAllIncluding(includeProperties);
        }

        public IEnumerable<TC> FindByInclude(Expression<Func<TC, bool>> predicate,
            params Expression<Func<TC, object>>[] includeProperties)
        {
            var query = GetAllIncluding(includeProperties);
            IEnumerable<TC> results = query.Where(predicate).ToList();
            return results;
        }

        public IEnumerable<TC> FindBy(Expression<Func<TC, bool>> predicate)
        {
            var queryable = _dbSet.AsNoTracking();
            IEnumerable<TC> results = queryable.Where(predicate).ToList();
            return results;
        }

        public async Task<IEnumerable<TC>> FindByAsync(Expression<Func<TC, bool>> predicate)
        {
            IEnumerable<TC> results = await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
            return results;
        }

        public TC Find(int id)
        {
            return Context.Set<TC>().Find(id);
        }

        public async Task<TC> FindAsync(int id)
        {
            return await Context.Set<TC>().FindAsync(id);
        }

        public virtual void Update(TC entity)
        {
            Context.SetModified(entity);
        }

        public void InsertUpdateGraph(TC entity)
        {
            Context.Set<TC>().Add(entity);
        }

        public virtual void Remove(TC entity)
        {
            Context.Set<TC>().Remove(entity);
        }


        public virtual void Delete(TC entityData)
        {
            var entity = entityData as BaseEntity;
            if (entity != null)
            {
                entity.AuditAction = AuditAction.Deleted;
                Context.SetModified(entity);
            }
        }

        public virtual void Delete(int id)
        {
            var entity = Context.Set<TC>().Find(id);
            if (entity != null) Delete(entity);
        }

        public virtual void Active(TC entityData)
        {
            var entity = entityData as BaseEntity;
            if (entity != null)
            {
                entity.DeletedBy = null;
                entity.DeletedDate = null;
                entity.AuditAction = AuditAction.Activated;
                Context.SetModified(entity);
            }
        }

        private IQueryable<TC> GetAllIncluding(params Expression<Func<TC, object>>[] includeProperties)
        {
            var queryable = _dbSet.AsNoTracking();

            return includeProperties.Aggregate
                (queryable, (current, includeProperty) => current.Include(includeProperty));
        }


        public void Dispose()
        {
            Context.Dispose();
        }

        public virtual void AddOrUpdate(TC entity)
        {
           
            var record = entity as BaseEntity;
            if (record.Id ==0)
            {
                Add(entity);
            }
            else
            {
                var referenceExists = false;
                try
                {
                    Context.Begin();
                    Context.SetRemove(entity);
                    Context.SaveWithOutAuditAsync().Wait();
                    Context.Rollback();
                }
                catch (Exception)
                {
                    referenceExists = true;
                    Context.Rollback();
                }

                Context.Entry(entity).State = EntityState.Detached;

                if (referenceExists)
                {

                    var oldRecord = Find(record.Id) as BaseEntity;
                    oldRecord.Id = 0;
                    Context.Entry(oldRecord).State = EntityState.Added;
                    Context.SaveWithOutAuditAsync().Wait();
                    Context.DetachAllEntities();

                    Delete(record.Id);
                    record.Id = oldRecord.Id;
                    Context.Entry(entity).State = EntityState.Modified;
                    Update(entity);

                }
                else
                {
                    Update(entity);
                }

            }
            
        }


    }
}