using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using System;


namespace GSC.Respository.Screening
{
    public class ScreeningVisitRepository : GenericRespository<ScreeningVisit, GscContext>, IScreeningVisitRepository
    {
        public ScreeningVisitRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}
