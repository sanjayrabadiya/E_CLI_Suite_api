using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Entities.Project.Schedule;
using GSC.Domain.Context;
using GSC.Respository.Project.Design;
using GSC.Respository.ProjectRight;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Project.Schedule
{
    public class ProjectScheduleRepository : GenericRespository<ProjectSchedule>, IProjectScheduleRepository
    {
        private readonly IGSCContext _context;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IStudyVersionRepository _studyVersionRepository;
        public ProjectScheduleRepository(IGSCContext context, IProjectRightRepository projectRightRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IStudyVersionRepository studyVersionRepository) : base(context)
        {
            _context = context;
            _projectRightRepository = projectRightRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _studyVersionRepository = studyVersionRepository;
        }

        public IList<ProjectScheduleTemplateDto> GetDataByPeriod(long periodId, long projectId)
        {
            var projectScheduleTemplate = (from projectschedule in _context.ProjectSchedule
                        .Where(x => x.DeletedBy == null && x.ProjectDesignId == projectId && x.ProjectDesignPeriodId == periodId)
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

        public IList<ProjectScheduleDto> GetData(int id)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<ProjectScheduleDto>();

            var isOnTrial = _studyVersionRepository.IsOnTrialByProjectDesing(id);

            var projectSchedules = All.Where(x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                         && x.DeletedDate == null
                         && x.ProjectDesignId == id
                         && projectList.Any(c => c == x.Project.Id))
                .Select(x => new ProjectScheduleDto
                {
                    Id = x.Id,
                    AutoNumber = x.AutoNumber,
                    ProjectId = x.ProjectId,
                    ProjectName = x.Project.ProjectCode,
                    PeriodName = x.ProjectDesignPeriod.DisplayName,
                    VisitName = x.ProjectDesignVisit.DisplayName,
                    TemplateName = x.ProjectDesignTemplate.TemplateName,
                    VariableName = x.ProjectDesignVariable.VariableName,
                    IsDeleted = x.DeletedDate != null,
                    IsLock = !isOnTrial,
                    CreatedByUser = x.CreatedBy == null ? null : _context.Users.Where(y => y.Id == x.CreatedBy).FirstOrDefault().UserName,
                    CreatedDate = x.CreatedDate,
                    ModifiedByUser = x.ModifiedBy == null ? null : _context.Users.Where(y => y.Id == x.ModifiedBy).FirstOrDefault().UserName,
                    ModifiedDate = x.ModifiedDate
                }).OrderByDescending(x => x.Id).ToList();

            return projectSchedules;
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