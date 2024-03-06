using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.ProjectRight;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using static GSC.Common.WorkingDayHelper;

namespace GSC.Respository.CTMS
{
    public class WeekEndMasterRepository : GenericRespository<WeekEndMaster>, IWeekEndMasterRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IProjectRightRepository _projectRightRepository;
        public WeekEndMasterRepository(IGSCContext context,
             IProjectRightRepository projectRightRepository,
            IMapper mapper) : base(context)
        {
            _mapper = mapper;
            _context = context;
            _projectRightRepository = projectRightRepository;
        }
        public List<WeekEndGridDto> GetWeekendList(bool isDeleted)
        {
            //Add by Mitul On 09-11-2023 GS1-I3112 -> f CTMS On By default Add CTMS Access table.
            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && projectList.Contains(x.ProjectId)).
                   ProjectTo<WeekEndGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            var data = result.Select(r =>
            {
                r.ProjectCode = r.IsSite ? _context.Project.Find(Convert.ToInt32(r.ProjectCode)).ProjectCode : r.ProjectCode;
                return r;
            }).ToList();

            return data;
        }

        public List<WeekendData> GetWorkingDayList(int ProjectId)
        {
            return All.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null).Select(t => new WeekendData
            {
                Weekend = t.AllWeekOff.GetDescription(),
                Frequency = t.Frequency.GetDescription()
            }).ToList();
        }

        public List<string> GetWeekEndDay(int ProjectId)
        {
            var weekend = All.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null).ToList();
            var weekendlis = new List<string>();

            var days = new List<string> { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            weekendlis = days.Where(x => !weekend.Select(y => y.AllWeekOff.ToString()).Contains(x)).ToList();
            return weekendlis;
        }
    }
}
