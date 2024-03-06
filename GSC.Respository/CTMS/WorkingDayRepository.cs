using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.ProjectRight;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class WorkingDayRepository : GenericRespository<WorkingDay>, IWorkingDayRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IProjectRightRepository _projectRightRepository;
        public WorkingDayRepository(IGSCContext context,
            IProjectRightRepository projectRightRepository,
            IMapper mapper) : base(context)
        {
            _mapper = mapper;
            _context = context;
            _projectRightRepository = projectRightRepository;
        }
        public List<WorkingDayListDto> GetWorkingDayList(bool isDeleted)
        {
            //Add by Mitul On 09-11-2023 GS1-I3112 -> f CTMS On By default Add CTMS Access table.
            var projectList = _projectRightRepository.GetProjectCTMSRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && projectList.Contains(x.ParentProjectId)).OrderByDescending(x => x.Id).
            ProjectTo<WorkingDayListDto>(_mapper.ConfigurationProvider).ToList();
            return result.Select(r =>
            {
                r.ProjectCode = _context.Project.Where(x => x.Id == r.ParentProjectId).Select(s => s.ProjectCode).FirstOrDefault();
                return r;
            }).ToList();
        }
        public void AddSiteType(WorkingDayDto workingDayListDto)
        {
            if (workingDayListDto.siteTypes != null && workingDayListDto.siteTypes[0].ProjectId != 0)
            {
                foreach (var item in workingDayListDto.siteTypes)
                {
                    item.WorkingDayId = workingDayListDto.Id;
                    _context.SiteTypes.Add(item);
                    _context.Save();
                }
            }
        }
    }
}
