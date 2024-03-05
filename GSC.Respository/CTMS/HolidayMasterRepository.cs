using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.ProjectRight;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class HolidayMasterRepository : GenericRespository<HolidayMaster>, IHolidayMasterRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IProjectRightRepository _projectRightRepository;
        public HolidayMasterRepository(IGSCContext context,
            IProjectRightRepository projectRightRepository,
            IMapper mapper) : base(context)
        {
            _mapper = mapper;
            _context = context;
            _projectRightRepository = projectRightRepository;
        }

        public List<HolidayMasterGridDto> GetHolidayList(bool isDeleted)
        {
            //Add by Mitul On 09-11-2023 GS1-I3112 -> If CTMS On By default Add CTMS Access table.
            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && projectList.Contains(x.ProjectId)).OrderByDescending(x => x.Id).
                   ProjectTo<HolidayMasterGridDto>(_mapper.ConfigurationProvider).ToList();
            var data = result.Select(r =>
            {
                r.ProjectCode = r.IsSite == true ? _context.Project.Find(Convert.ToInt32(r.ProjectCode)).ProjectCode : r.ProjectCode;
                return r;
            }).ToList();
            return data;
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

        public List<HolidayMasterListDto> GetProjectWiseHolidayList(int ProjectId)
        {
            var result = All.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null).OrderByDescending(x => x.Id).
                   ProjectTo<HolidayMasterListDto>(_mapper.ConfigurationProvider).ToList();
            return result;
        }
        public string DuplicateHoliday(HolidayMaster objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId && x.HolidayName == objSave.HolidayName.Trim() && x.DeletedDate == null))
                return "Duplicate Holiday : " + objSave.HolidayName;

            return "";
        }
    }
}
