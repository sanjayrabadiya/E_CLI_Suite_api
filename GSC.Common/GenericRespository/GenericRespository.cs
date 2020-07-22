using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Common;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;

namespace GSC.Common.GenericRespository
{
    public class GenericRespository<TC, TContext> : IGenericRepository<TC>
        where TC : class
        where TContext : GscContext
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        protected readonly TContext Context;
        internal readonly DbSet<TC> DbSet;

        protected GenericRespository(IUnitOfWork<TContext> uow, IJwtTokenAccesser jwtTokenAccesser)
        {
            Context = uow.Context;
            DbSet = Context.Set<TC>();
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public IQueryable<TC> All => Context.Set<TC>();

        public void Add(TC entity)
        {
            Context.Add(entity);
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
            var queryable = DbSet.AsNoTracking();
            IEnumerable<TC> results = queryable.Where(predicate).ToList();
            return results;
        }

        public async Task<IEnumerable<TC>> FindByAsync(Expression<Func<TC, bool>> predicate)
        {
            // IQueryable<TC> queryable = DbSet.AsNoTracking();
            IEnumerable<TC> results = await DbSet.AsNoTracking().Where(predicate).ToListAsync();
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
            Context.Update(entity);
        }

        public void InsertUpdateGraph(TC entity)
        {
            Context.Set<TC>().Add(entity);
            Context.ApplyStateChanges(_jwtTokenAccesser);
        }

        public virtual void Delete(int id)
        {
            var entity = Context.Set<TC>().Find(id);
            if (entity != null) Delete(entity);
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
                entity.DeletedBy = _jwtTokenAccesser.UserId;
                entity.DeletedDate = DateTime.Now.ToUniversalTime();
                entity.AuditAction = AuditAction.Deleted;
                Context.Update(entity);
            }
        }

        public virtual void Active(TC entityData)
        {
            var entity = entityData as BaseEntity;
            if (entity != null)
            {
                entity.DeletedBy = null;
                entity.DeletedDate = null;
                entity.AuditAction = AuditAction.Activated;
                Context.Update(entity);
            }
        }

        private IQueryable<TC> GetAllIncluding(params Expression<Func<TC, object>>[] includeProperties)
        {
            var queryable = DbSet.AsNoTracking();

            return includeProperties.Aggregate
                (queryable, (current, includeProperty) => current.Include(includeProperty));
        }

        
        public void Dispose()
        {
            Context.Dispose();
        }

        public virtual void AddOrUpdate(TC entity)
        {
            var referenceExists = false;
            using (var transaction = Context.Database.BeginTransaction())
            {
                try
                {
                    Context.Remove(entity);
                    Context.SaveChanges(1);
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    referenceExists = true;
                    transaction.Rollback();
                }
            }

            Context.Entry(entity).State = EntityState.Detached;

            if (referenceExists)
            {
                var record = entity as BaseEntity;
                Delete(record.Id);

                Context.Entry(entity).State = EntityState.Detached;
                record.Id = 0;

                Add(entity);
            }
            else
            {
                Context.Update(entity);
            }
        }


    }
}