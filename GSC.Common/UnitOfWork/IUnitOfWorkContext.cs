using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Common.UnitOfWork
{
    public interface IUnitOfWork<TContext> : IUnitOfWork, IDisposable
        where TContext : IContext
    {
        TContext Context { get; }
    }
}
