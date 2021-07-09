using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GSC.Helper;
using GSC.Common;

namespace GSC.Respository.CTMS
{
    public class StudyPlanTaskResourceRepository : GenericRespository<StudyPlanTaskResource>, IStudyPlanTaskResourceRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public StudyPlanTaskResourceRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<StudyPlanTaskResourceGridDto> GetStudyPlanTaskResourceList(bool isDeleted, int studyPlanTaskId)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanTaskId == studyPlanTaskId).
                   ProjectTo<StudyPlanTaskResourceGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(StudyPlanTaskResource objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.SecurityRoleId == objSave.SecurityRoleId && x.UserId == objSave.UserId && x.StudyPlanTaskId == objSave.StudyPlanTaskId && x.DeletedDate == null))
            {
                objSave.User = _context.Users.Where(x => x.Id == objSave.UserId).FirstOrDefault();
                return "Duplicate User : " + objSave.User.FirstName + ' ' + objSave.User.LastName;
            }
            return "";
        }
    }
}

