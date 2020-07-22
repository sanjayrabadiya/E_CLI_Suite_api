using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Common.UnitOfWork
{
    public interface IUnitOfWork<TContext> : IUnitOfWork, IDisposable
        where TContext : GscContext
    {
        TContext Context { get; }
    }
}
