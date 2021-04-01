using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Project.Schedule;
using GSC.Domain.Context;
using GSC.Respository.Project.Design;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Project.Schedule
{
    public class ProjectScheduleTemplateRepository : GenericRespository<ProjectScheduleTemplate>,
        IProjectScheduleTemplateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        public ProjectScheduleTemplateRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository) :
            base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
        }

        public void UpdateTemplates(ProjectSchedule projectSchedule)
        {
            var data = FindBy(x => x.ProjectScheduleId == projectSchedule.Id).ToList();
            var deleteTemplates = data.Where(x => !projectSchedule.Templates.Any(c => c.Id == x.Id)).ToList();

            foreach (var template in deleteTemplates)
            {
                template.DeletedBy = _jwtTokenAccesser.UserId;
                template.DeletedDate = DateTime.Now;
                Update(template);
            }
        }

        public void UpdateDesignTemplatesOrder(ProjectSchedule projectSchedule)
        {
            var orderedList = _projectDesignTemplateRepository
                .FindBy(t => t.ProjectDesignVisitId == projectSchedule.ProjectDesignVisitId && t.DeletedDate == null)
                .OrderBy(t => t.DesignOrder).ToList();

            var i = 0;
            foreach (var item in orderedList)
            {
                item.DesignOrder = ++i;
                _projectDesignTemplateRepository.Update(item);
            }
        }


        public void UpdateDesignTemplatesSchedule(int projectDesignPeriodId)
        {
            var targetVisits = All.Where(r => r.DeletedDate == null && (r.Operator == Helper.ProjectScheduleOperator.Equal || r.Operator == Helper.ProjectScheduleOperator.Plus) && r.ProjectDesignPeriodId == projectDesignPeriodId).Select(t => t.ProjectDesignVisitId).ToList();
            var projectDesingVisit = _projectDesignVisitRepository.All.Where(r => r.ProjectDesignPeriodId == projectDesignPeriodId).ToList();
            projectDesingVisit.ForEach(r =>
            {
                r.IsSchedule = false;
                if (targetVisits.Any(x => x == r.Id))
                    r.IsSchedule = true;

                _projectDesignVisitRepository.Update(r);
            });
        }
    }
}