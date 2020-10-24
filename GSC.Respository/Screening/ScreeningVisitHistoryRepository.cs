using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
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
        private readonly IMapper _mapper;
        public ScreeningVisitHistoryRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public void SaveByScreeningVisit(ScreeningVisit screeningVisit, ScreeningVisitStatus screeningVisitStatus, DateTime? statusDate)
        {
            var history = new ScreeningVisitHistory();
            history.ScreeningVisit = screeningVisit;
            history.StatusDate = statusDate;
            history.RoleId = _jwtTokenAccesser.RoleId;
            history.VisitStatusId = screeningVisitStatus;
            Add(history);
        }

        public void Save(ScreeningVisitHistoryDto screeningVisitHistoryDto)
        {
            var history = _mapper.Map<ScreeningVisitHistory>(screeningVisitHistoryDto);
            history.RoleId = _jwtTokenAccesser.RoleId;
            Add(history);
        }

    }
}
