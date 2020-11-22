using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Shared
{
    public interface IGSCContextExtension
    {
        IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class;
    }
}
