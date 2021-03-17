using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.CTMS
{
    public class StudyPlanTaskRepository : GenericRespository<StudyPlanTask>, IStudyPlanTaskRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public StudyPlanTaskRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }
    }
}
