﻿using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Volunteer
{
    public class VolunteerHistoryRepository : GenericRespository<VolunteerHistory, GscContext>,
        IVolunteerHistoryRepository
    {
        public VolunteerHistoryRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}