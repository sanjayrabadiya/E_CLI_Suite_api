using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Entities.Project.Schedule;
using GSC.Domain.Context;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Project.Schedule
{
    public class ProjectScheduleRepository : GenericRespository<ProjectSchedule>, IProjectScheduleRepository
    {
        private readonly IGSCContext _context;
        public ProjectScheduleRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _context = context;
        }

        public IList<ProjectScheduleTemplateDto> GetDataByPeriod(long periodId, long projectId)
        {
            var projectScheduleTemplate = (from projectschedule in _context.ProjectSchedule
                    .Where(t => t.DeletedBy == null).Where(x =>
                        x.ProjectDesignId == projectId && x.ProjectDesignPeriodId == periodId)
                                           join projectscheduletemp in _context.ProjectScheduleTemplate.Where(t => t.DeletedBy == null) on
                                               projectschedule.Id equals projectscheduletemp.ProjectScheduleId
                                           join period in _context.ProjectDesignPeriod on projectscheduletemp.ProjectDesignPeriodId equals period.Id
                                               into gj
                                           from subpet in gj.DefaultIfEmpty()
                                           join visit in _context.ProjectDesignVisit on projectscheduletemp.ProjectDesignVisitId equals visit.Id
                                           join template in _context.ProjectDesignTemplate on projectscheduletemp.ProjectDesignTemplateId equals
                                               template.Id
                                           join variable in _context.ProjectDesignVariable on projectscheduletemp.ProjectDesignVariableId equals
                                               variable.Id
                                           select new ProjectScheduleTemplateDto
                                           {
                                               Id = projectscheduletemp.Id,
                                               ProjectScheduleId = projectscheduletemp.ProjectScheduleId,
                                               PeriodName = subpet.DisplayName,
                                               TemplateName = template.TemplateName,
                                               VisitName = visit.DisplayName,
                                               VariableName = variable.VariableName,
                                               Operator = projectscheduletemp.Operator,
                                               OperatorName = projectscheduletemp.Operator != null
                                                   ? projectscheduletemp.Operator.GetDescription()
                                                   : "",
                                               PositiveDeviation = projectscheduletemp.PositiveDeviation,
                                               NegativeDeviation = projectscheduletemp.NegativeDeviation,
                                               NoOfDay = projectscheduletemp.NoOfDay,
                                               HH = (int)projectscheduletemp.HH,
                                               MM = (int)projectscheduletemp.MM,
                                               Message = projectscheduletemp.Message
                                           }).ToList();

            return projectScheduleTemplate;
        }


        public int GetRefVariableValuefromTargetVariable(int projectDesignVariableId)
        {
            //int id = 0;
            var referenceVariable = (from projectScheduletemp in _context.ProjectScheduleTemplate.Where(x =>
                    x.ProjectDesignVariableId == projectDesignVariableId && x.DeletedBy == null)
                                     join projectSchedule in _context.ProjectSchedule.Where(x => x.DeletedBy == null) on projectScheduletemp
                                         .ProjectScheduleId equals projectSchedule.Id
                                     select new
                                     {
                                         id = projectSchedule.ProjectDesignVariableId
                                     }).FirstOrDefault();

            //int referenceVariableId = referenceVariable ? referenceVariable.id : null;
            if (referenceVariable == null)
                return 0;
            return referenceVariable.id;
        }
    }
}