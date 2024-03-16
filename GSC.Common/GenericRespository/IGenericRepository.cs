using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GSC.Common.GenericRespository
{
    public interface IGenericRepository<TC> : IDisposable
    {
        IQueryable<TC> All { get; }
        IQueryable<TC> AllIncluding(params Expression<Func<TC, object>>[] includeProperties);

        IEnumerable<TC> FindByInclude(Expression<Func<TC, bool>> predicate,
            params Expression<Func<TC, object>>[] includeProperties);

        IEnumerable<TC> FindBy(Expression<Func<TC, bool>> predicate);

        Task<IEnumerable<TC>> FindByAsync(Expression<Func<TC, bool>> predicate);
        TC Find(int id);
        void SetDbConnection(string connectionString);
        Task<TC> FindAsync(int id);
        void Add(TC entity);
        void Update(TC entity);
        void Remove(TC entity);
        void Delete(int id);
        void Delete(TC entityData);
        void Active(TC entityData);
        void InsertUpdateGraph(TC entity);
        /// <summary>
        /// Adds new record and marks existing record as deleted if reference exists, or updates the record if no referece exists
        /// </summary>
        /// <param name="entity"></param>
        void AddOrUpdate(TC entity);
    }
}