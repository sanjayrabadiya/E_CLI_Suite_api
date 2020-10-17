using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Project.Schedule;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Project.Design;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Project.Schedule
{
    public class ProjectScheduleTemplateRepository : GenericRespository<ProjectScheduleTemplate, GscContext>,
        IProjectScheduleTemplateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IProjectScheduleRepository _projectScheduleRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        public ProjectScheduleTemplateRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IProjectScheduleRepository projectScheduleRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository) :
            base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _projectScheduleRepository = projectScheduleRepository;
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
            var targetVisits = All.Where(r => r.DeletedDate == null && r.ProjectDesignPeriodId == projectDesignPeriodId).Select(t => t.ProjectDesignVisitId).ToList();
            var refVisits = _projectScheduleRepository.All.Where(r => r.DeletedDate == null && r.ProjectDesignPeriodId == projectDesignPeriodId).Select(t => t.ProjectDesignVisitId).ToList();

            var projectDesingVisit = _projectDesignVisitRepository.All.Where(r => r.ProjectDesignPeriodId == projectDesignPeriodId).ToList();
            projectDesingVisit.ForEach(r =>
            {
                r.IsSchedule = false;
                if (targetVisits.Any(x => x == r.Id) && !refVisits.Any(x => x == r.Id))
                    r.IsSchedule = true;
                
                _projectDesignVisitRepository.Update(r);
            });
        }
    }
}