using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignPeriodRepository : GenericRespository<ProjectDesignPeriod>,
        IProjectDesignPeriodRepository
    {
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IGSCContext _context;
        public ProjectDesignPeriodRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IProjectDesignVisitRepository projectDesignVisitRepository) : base(context)
        {
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _context = context;
        }

        public ProjectDesignPeriod GetPeriod(int id)
        {
            var period = _context.ProjectDesignPeriod.Where(t => t.Id == id)
                .Include(d => d.VisitList)
                .ThenInclude(d => d.Templates)
                .ThenInclude(d => d.Variables)
                .ThenInclude(d => d.Values)
                .AsNoTracking().FirstOrDefault();

            return period;
        }

        public IList<DropDownDto> GetPeriodDropDown(int projectDesignId)
        {
            var periods = All.Where(x => x.DeletedDate == null
                                         && x.ProjectDesignId == projectDesignId).OrderBy(t => t.Id).Select(t =>
                new DropDownDto
                {
                    Id = t.Id,
                    Value = t.DisplayName
                }).ToList();

            return periods;
        }

        public IList<DropDownWithSeqDto> GetPeriodByProjectIdDropDown(int projectId)
        {
            var periods = All.Where(x => x.DeletedDate == null && x.ProjectDesign.DeletedDate == null
                                                               && x.ProjectDesign.ProjectId == projectId &&
                                                               x.ProjectDesign.IsCompleteDesign).OrderBy(t => t.Id)
                .Select(t => new DropDownWithSeqDto
                {
                    Id = t.Id,
                    Value = t.DisplayName
                }).ToList();

            periods = periods.Select((o, i) =>
            {
                o.SeqNo = ++i;
                return o;
            }).ToList();

            return periods.OrderBy(x => x.Value).ToList();
        }

       
    }
}