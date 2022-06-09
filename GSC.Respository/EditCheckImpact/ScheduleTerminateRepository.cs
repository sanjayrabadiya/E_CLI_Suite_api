using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.EditCheck;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Respository.Project.Schedule;
using GSC.Respository.Screening;
using GSC.Shared.Extension;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.EditCheckImpact
{
    public class ScheduleTerminateRepository : GenericRespository<ScreeningTemplate>, IScheduleTerminate
    {
        private readonly IGSCContext _context;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScheduleTerminateDetailRepository _scheduleTerminateDetailRepository;
        private readonly IEditCheckRuleRepository _editCheckRuleRepository;
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        private readonly IScheduleRuleRespository _scheduleRuleRespository;
        public ScheduleTerminateRepository(IGSCContext context,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IScheduleTerminateDetailRepository scheduleTerminateDetailRepository,
            IEditCheckRuleRepository editCheckRuleRepository,
            IScreeningVisitRepository screeningVisitRepository,
            IScheduleRuleRespository scheduleRuleRespository) : base(context)
        {
            _context = context;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _scheduleTerminateDetailRepository = scheduleTerminateDetailRepository;
            _editCheckRuleRepository = editCheckRuleRepository;
            _screeningVisitRepository = screeningVisitRepository;
            _scheduleRuleRespository = scheduleRuleRespository;

        }

        public void TerminateScheduleTemplateVisit(int projectDesignTemplateId, int screeningEntryId, bool isSelfCorrection)
        {
            var scheduleTerminates = _scheduleTerminateDetailRepository.All.Where(x => x.DeletedDate == null
            && x.ProjectDesignVariable.ProjectDesignTemplateId == projectDesignTemplateId).Select(r => new ScheduleTerminateDto
            {
                Operator = r.Operator,
                Value = r.Value,
                ProjectDesignVariableId = r.ProjectDesignVariableId,
                CollectionSource = r.ProjectDesignVariable.CollectionSource,
                DataType = r.ProjectDesignVariable.DataType,
                TargetProjectDesignTemplateId = r.ProjectScheduleTemplate.ProjectDesignTemplateId,
                TargetProjectDesignVariableId = r.ProjectScheduleTemplate.ProjectDesignVariableId,
                TargetProjectDesignVisitId = r.ProjectScheduleTemplate.ProjectDesignVisitId,
                ProjectScheduleId = r.ProjectScheduleTemplate.ProjectScheduleId
            }).ToList();

            var isValid = false;



            var templateValues = _screeningTemplateValueRepository.All.Where(x => x.DeletedDate == null
            && x.ScreeningTemplate.ProjectDesignTemplateId == projectDesignTemplateId
            && x.ScreeningTemplate.ScreeningVisit.ScreeningEntryId == screeningEntryId
            && x.ScreeningTemplate.ParentId == null).ToList();

            scheduleTerminates.ForEach(x =>
            {
                var editChecks = new List<EditCheckValidate>();
                var editCheck = new EditCheckValidate();
                editCheck.Operator = x.Operator;
                editCheck.CollectionValue = x.Value;
                editCheck.DataType = x.DataType;
                editCheck.CollectionSource = x.CollectionSource;
                editCheck.OperatorName = x.Operator.GetDescription();

                var value = GetScreeningValue(projectDesignTemplateId, x.ProjectDesignVariableId, screeningEntryId);
                if (value != null)
                {
                    editCheck.InputValue = value.Value;
                    editChecks.Add(editCheck);
                    var result = _editCheckRuleRepository.ValidateRuleReference(editChecks, true);
                    isValid = true;

                    value = GetScreeningValue(x.TargetProjectDesignTemplateId, x.TargetProjectDesignVariableId, screeningEntryId);
                    if (value != null)
                    {
                        x.ScreeningTemplateId = value.ScreeningTemplateId;
                        value.IsScheduleTerminate = result.IsValid;
                        _screeningTemplateValueRepository.Update(value);

                    }
                }


            });
            _context.Save();
            _context.DetachAllEntities();

            if (isValid)
            {
                var screeningTemplateIds = scheduleTerminates.Where(x => x.ScreeningTemplateId > 0).Select(r => r.ScreeningTemplateId).Distinct().ToList();
                screeningTemplateIds.ForEach(x =>
                {
                    var scheduleDate = _screeningTemplateValueRepository.All.Where(c => c.DeletedDate == null
                    && c.ScreeningTemplateId == x && c.IsScheduleTerminate != true).Max(r => r.ScheduleDate);
                    var template = Find(x);
                    template.ScheduleDate = scheduleDate;
                    Update(template);
                });
                _context.Save();
                var visitIds = scheduleTerminates.Select(r => r.TargetProjectDesignVisitId).Distinct().ToList();

                var screeningVisists = _screeningVisitRepository.All.Where(x => x.DeletedDate == null && x.ScreeningEntryId == screeningEntryId &&
                  visitIds.Contains(x.ProjectDesignVisitId)).ToList();

                screeningVisists.ForEach(e =>
                {
                    var scheduleDate = All.Where(c => c.DeletedDate == null && c.ScreeningVisitId == e.Id).Max(r => r.ScheduleDate);
                    e.ScheduleDate = scheduleDate;
                    e.IsScheduleTerminate = scheduleDate == null;
                    if (e.IsScheduleTerminate == true && (e.Status == Helper.ScreeningVisitStatus.Scheduled || e.Status == Helper.ScreeningVisitStatus.ReSchedule))
                    {
                        e.Status = Helper.ScreeningVisitStatus.NotStarted;
                    }
                    else if (e.IsScheduleTerminate != true && e.Status == Helper.ScreeningVisitStatus.NotStarted && e.ScheduleDate!=null)
                    {
                        e.Status = Helper.ScreeningVisitStatus.Scheduled;
                    }
                    _screeningVisitRepository.Update(e);
                });
                _context.Save();
                _context.DetachAllEntities();
                _scheduleRuleRespository.SetScheduleStatus(screeningEntryId);
                _context.Save();
                _context.DetachAllEntities();

            }


        }


        ScreeningTemplateValue GetScreeningValue(int projectDesignTemplateId, int projectDesignVariableId, int screeningEntryId)

        {
            var value = _screeningTemplateValueRepository.All.Where(c => c.DeletedDate == null
               && c.ScreeningTemplate.ProjectDesignTemplateId == projectDesignTemplateId
               && c.ProjectDesignVariableId == projectDesignVariableId
               && c.ScreeningTemplate.ScreeningVisit.ScreeningEntryId == screeningEntryId
               && c.ScreeningTemplate.ParentId == null).FirstOrDefault();

            return value;
        }

    }
}
