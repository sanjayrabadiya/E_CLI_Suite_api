using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class ActivityRepository : GenericRespository<Activity>, IActivityRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public ActivityRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<DropDownDto> GetActivityDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.CtmsActivity.ActivityName }).OrderBy(o => o.Value).ToList();
        }

        public List<ActivityGridDto> GetActivityList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<ActivityGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<DropDownDto> GetActivityDropDownByModuleId(int moduleId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null
                    && x.AppScreenId == moduleId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.CtmsActivity.ActivityName }).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Activity objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.AppScreenId == objSave.AppScreenId && x.ActivityCode == objSave.ActivityCode.Trim() && x.DeletedDate == null))
                return "Duplicate Activity code : " + objSave.ActivityCode;
            if (All.Any(x =>
                x.Id != objSave.Id && x.AppScreenId == objSave.AppScreenId && x.CtmsActivityId == objSave.CtmsActivityId && x.DeletedDate == null))
            {
                var ActivityName = _context.CtmsActivity.Where(x => x.Id == objSave.CtmsActivityId).FirstOrDefault().ActivityName;
                return "Duplicate Activity name : " + ActivityName;
            }
            return "";
        }

        public DropDownDto GetActivityForFormList(int tabNumber)
        {
            var result = new DropDownDto();

            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            
            string ActivityCode = tabNumber == 0 ? "act_001" : tabNumber == 1 ? "act_002" : tabNumber == 2 ? "act_003" :
                tabNumber == 3 ? "act_004" : tabNumber == 4 ? "act_005" : "act_006";

            result = All.Include(x => x.CtmsActivity)
            .Where(x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null
            && x.CtmsActivity.ActivityCode == ActivityCode && x.CtmsActivity.DeletedDate == null && x.AppScreenId == appscreen.Id)
            .Select(x => new DropDownDto
            {
                Id = x.Id,
                Value = x.CtmsActivity.ActivityName,
                Code = x.ActivityCode,
                ExtraData = x.CtmsActivity.ActivityCode
            }).FirstOrDefault();

            return result;
        }
    }
}