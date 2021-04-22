using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.CTMS
{
    public class HolidayMasterRepository : GenericRespository<HolidayMaster>, IHolidayMasterRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public HolidayMasterRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<HolidayMasterGridDto> GetHolidayList(bool isDeleted)
        {
            var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).
                   ProjectTo<HolidayMasterGridDto>(_mapper.ConfigurationProvider).ToList();
            return result;

        }

        public List<DateTime> GetHolidayList(int projectId)
        {
            var holidaylist = new List<DateTime>();
            var holiday = All.Where(x => x.ProjectId == projectId && x.DeletedDate == null).ToList();
            foreach (var item in holiday)
            {
                var holidayarray = WorkingDayHelper.GetDatesBetween(item.FromDate, item.ToDate);
                foreach (var itemholiday in holidayarray)
                {
                    holidaylist.Add(itemholiday);
                }
            }
            return holidaylist.Distinct().ToList();
        }

        public List<HolidayMasterListDto> GetProjectWiseHolidayList(int StudyPlanId)
        {
            int ProjectId = _context.StudyPlan.Where(x => x.Id == StudyPlanId).FirstOrDefault().ProjectId;
            var result = All.Where(x => x.ProjectId== ProjectId &&  x.DeletedDate == null).OrderByDescending(x => x.Id).
                   ProjectTo<HolidayMasterListDto>(_mapper.ConfigurationProvider).ToList();
            return result;

        }
    }
}
