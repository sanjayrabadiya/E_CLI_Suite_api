using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.Extension;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringStatusRepository : GenericRespository<CtmsMonitoringStatus>, ICtmsMonitoringStatusRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public CtmsMonitoringStatusRepository(IGSCContext context, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }

        public List<CtmsMonitoringStatusGridDto> GetCtmsMonitoringStatusList(int CtmsMonitoringId)
        {
            return All.Where(x => x.CtmsMonitoringId == CtmsMonitoringId).
                   ProjectTo<CtmsMonitoringStatusGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public CtmsMonitoringStatusGridDto GetSiteStatus(int ProjectId)
        {
            var result = All.Where(x => x.CtmsMonitoring.ProjectId == ProjectId && x.DeletedDate == null)
                        .Select(x => new CtmsMonitoringStatusGridDto
                        {
                            Id = x.Id,
                            ActivityName = x.CtmsMonitoring.StudyLevelForm.Activity.CtmsActivity.ActivityName,
                            StatusName = x.Status.GetDescription(),
                            Status = x.Status
                        }).OrderByDescending(x => x.Id).ToList();

            return result.FirstOrDefault();
        }

        public string GetFormApprovedOrNot(int projectId, int siteId, int tabNumber)
        {
            return "";
        }

        public void UpdateSiteStatus(CtmsMonitoringStatusDto ctmsMonitoringDto)
        {
            var data = _context.CtmsMonitoring.Include(s => s.Project).Where(s => s.Id == ctmsMonitoringDto.CtmsMonitoringId).FirstOrDefault();
            if (data != null && data.Project != null)
            {
                var project = data.Project;
                project.Status = ctmsMonitoringDto.Status;
                _context.Project.Update(project);
                _context.Save();
            }
        }
    }
}