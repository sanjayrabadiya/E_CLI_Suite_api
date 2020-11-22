using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Screening
{
    public class ScreeningVisitHistoryRepository : GenericRespository<ScreeningVisitHistory>, IScreeningVisitHistoryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        public ScreeningVisitHistoryRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
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
