using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Screening
{
    public class ScreeningVisitHistoryRepository : GenericRespository<ScreeningVisitHistory, GscContext>, IScreeningVisitHistoryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ScreeningVisitHistoryRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public void SaveByScreeningVisit(ScreeningVisit screeningVisit, ScreeningVisitStatus screeningVisitStatus)
        {
            var history = new ScreeningVisitHistory();
            history.ScreeningVisit = screeningVisit;
            history.RoleId = _jwtTokenAccesser.RoleId;
            history.VisitStatusId = screeningVisitStatus;
            Add(history);
        }

        public void Save(int screeningVisitId, ScreeningVisitStatus screeningVisitStatus,string note)
        {
            var history = new ScreeningVisitHistory();
            history.ScreeningVisitId = screeningVisitId;
            history.RoleId = _jwtTokenAccesser.RoleId;
            history.Notes = note;
            history.ScreeningVisitId = screeningVisitId;
            Add(history);
        }

    }
}
