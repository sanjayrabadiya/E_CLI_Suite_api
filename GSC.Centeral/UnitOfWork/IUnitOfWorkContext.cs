using GSC.Centeral.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Centeral.UnitOfWork
{
    public interface IUnitOfWorkCenteral<TContext> : IUnitOfWorkCenteral, IDisposable
        where TContext : CenteralContext
    {
        TContext Context { get; }
    }
}
