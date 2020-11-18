using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.EditCheckImpact;
using GSC.Respository.Project.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Screening
{
    public class ScreeningVisitRepository : GenericRespository<ScreeningVisit, GscContext>, IScreeningVisitRepository
    {
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IScreeningVisitHistoryRepository _screeningVisitHistoryRepository;
        private readonly IProjectDesignVisitStatusRepository _projectDesignVisitStatusRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningProgress _screeningProgress;
        private readonly IScheduleRuleRespository _scheduleRuleRespository;
        private readonly IImpactService _impactService;

        public ScreeningVisitRepository(IUnitOfWork<GscContext> uow,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IScreeningVisitHistoryRepository screeningVisitHistoryRepository,
            IRandomizationRepository randomizationRepository,
            IProjectDesignVisitStatusRepository projectDesignVisitStatusRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IScreeningProgress screeningProgress,
            IScheduleRuleRespository scheduleRuleRespository,
            IImpactService impactService)
            : base(uow, jwtTokenAccesser)
        {
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _screeningVisitHistoryRepository = screeningVisitHistoryRepository;
            _randomizationRepository = randomizationRepository;
            _projectDesignVisitStatusRepository = projectDesignVisitStatusRepository;
            _uow = uow;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _screeningProgress = screeningProgress;
            _scheduleRuleRespository = scheduleRuleRespository;
            _impactService = impactService;
        }


        public List<ScreeningVisitTree> GetVisitTree(int screeningEntryId)
        {
            var result = All.Where(s => s.ScreeningEntryId == screeningEntryId && s.DeletedDate == null
            && s.Status >= ScreeningVisitStatus.Open).Select(s => new ScreeningVisitTree
            {
                ScreeningVisitId = s.Id,
                VisitSeqNo = s.RepeatedVisitNumber,
                ProjectDesignVisitId = s.ProjectDesignVisitId,
                ProjectDesignVisitName = s.ProjectDesignVisit.DisplayName +
                                         Convert.ToString(s.RepeatedVisitNumber == null ? "" : "_" + s.RepeatedVisitNumber),
                VisitStatus = s.Status,
                VisitStatusName = s.Status.GetDescription(),
                ParentScreeningVisitId = s.ParentId,
                IsVisitRepeated = s.ProjectDesignVisit.IsRepeated,
            }).OrderBy(o => o.ProjectDesignVisitId).ThenBy(t => t.VisitSeqNo).ToList();

            return result;
        }

        public void ScreeningVisitSave(ScreeningEntry screeningEntry, int projectDesignPeriodId, int projectDesignVisitId, DateTime visitDate)
        {
            var designVisits = _projectDesignVisitRepository.GetVisitAndTemplateByPeriordId(projectDesignPeriodId);
            screeningEntry.ScreeningVisit = new List<ScreeningVisit>();
            designVisits.ForEach(r =>
            {
                var screeningVisit = new ScreeningVisit
                {
                    ProjectDesignVisitId = r.Id,
                    Status = projectDesignVisitId == r.Id ? ScreeningVisitStatus.Open : ScreeningVisitStatus.NotStarted,
                    IsSchedule = r.IsSchedule ?? false,
                    ScreeningTemplates = new List<ScreeningTemplate>()
                };

                if (screeningVisit.Status == ScreeningVisitStatus.Open)
                {
                    screeningVisit.VisitStartDate = visitDate;
                    _screeningVisitHistoryRepository.SaveByScreeningVisit(screeningVisit, ScreeningVisitStatus.Open, visitDate);
                }

                r.Templates.ForEach(t =>
                {
                    var screeningTemplate = new ScreeningTemplate
                    {
                        ProjectDesignTemplateId = t,
                        Status = ScreeningTemplateStatus.Pending
                    };
                    screeningVisit.ScreeningTemplates.Add(screeningTemplate);

                });

                Add(screeningVisit);

                screeningEntry.ScreeningVisit.Add(screeningVisit);
            });
        }

        public void FindOpenVisitVarible(int projectDesignVisitId, int screeningVisitId, DateTime visitDate)
        {
            var openVariable = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVisitId == projectDesignVisitId && x.VisitStatusId == ScreeningVisitStatus.Open).
              Select(t => new
              {
                  t.ProjectDesignVariable.Id,
                  t.ProjectDesignVariable.ProjectDesignTemplateId,
                  t.ProjectDesignVisitId
              }).FirstOrDefault();

            if (openVariable != null && openVariable.ProjectDesignVisitId == projectDesignVisitId)
            {
                var screeningVisit = Find(screeningVisitId);
                var screeningTemplate = _screeningTemplateRepository.All.Where(x => x.ScreeningVisitId == screeningVisit.Id && x.ProjectDesignTemplateId == openVariable.ProjectDesignTemplateId && x.ParentId == null).FirstOrDefault();
                if (screeningVisit != null && screeningTemplate != null && SaveVariableValue(visitDate.ToString(), screeningTemplate, openVariable.Id, openVariable.ProjectDesignTemplateId, screeningVisit.ScreeningEntryId))
                {
                    screeningVisit.Status = ScreeningVisitStatus.InProgress;
                    screeningTemplate.Status = ScreeningTemplateStatus.InProcess;
                    _screeningVisitHistoryRepository.SaveByScreeningVisit(screeningVisit, ScreeningVisitStatus.InProgress, visitDate);
                    Update(screeningVisit);
                }

            }
        }

        private bool SaveVariableValue(string value, ScreeningTemplate screeningTemplate, int projectDesignVariableId, int projectDesignTemplateId, int screeningEntryId)
        {

            if (!_projectDesignVariableRepository.All.Any(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.Id == projectDesignVariableId))
                return false;
            var aduits = new List<ScreeningTemplateValueAudit>
            {
                new ScreeningTemplateValueAudit
                {
                    Value = value,
                    Note = "Save value from open visit"
                }
            };

            var screeningTemplateValue = new ScreeningTemplateValue
            {
                ScreeningTemplate = screeningTemplate,
                ProjectDesignVariableId = projectDesignVariableId,
                Value = value,
                Audits = aduits
            };

            _screeningTemplateValueRepository.Add(screeningTemplateValue);

            _uow.Save();

            _scheduleRuleRespository.ValidateByVariable(screeningEntryId, screeningTemplate.Id, value, screeningTemplate.ProjectDesignTemplateId, projectDesignVariableId, false);
           
            _uow.Save();

            _screeningProgress.GetScreeningProgress(screeningEntryId, screeningTemplate.Id);


            return true;

        }

        public void StatusUpdate(ScreeningVisitHistoryDto screeningVisitHistoryDto)
        {
            var visit = Find(screeningVisitHistoryDto.ScreeningVisitId);

            visit.Status = screeningVisitHistoryDto.VisitStatusId;

            Update(visit);

            _screeningVisitHistoryRepository.Save(screeningVisitHistoryDto);

            _uow.Save();

            PatientStatus(visit.ScreeningEntryId);
        }

        public bool IsPatientScreeningFailure(int screeningVisitId)
        {
            var visit = Find(screeningVisitId);
            return All.Any(t => t.ScreeningEntryId == visit.ScreeningEntryId && (
           t.ScreeningEntry.Randomization.PatientStatusId == ScreeningPatientStatus.ScreeningFailure ||
           t.ScreeningEntry.Randomization.PatientStatusId == ScreeningPatientStatus.Withdrawal));
        }


        public string CheckOpenDate(ScreeningVisitDto screeningVisitDto)
        {
            var visit = Find(screeningVisitDto.ScreeningVisitId);
            if (visit != null
                && (visit.Status == ScreeningVisitStatus.ReSchedule || visit.Status == ScreeningVisitStatus.Scheduled)
                && visit.ScheduleDate != null && visit.ScheduleDate.Value.Date != screeningVisitDto.VisitOpenDate.Date)
                return $"Schedule Date cannot be greater than visit open date";

            return "";

        }


        public string CheckScheduleDate(ScreeningVisitHistoryDto screeningVisitDto)
        {
            var visit = Find(screeningVisitDto.ScreeningVisitId);

            if (visit == null || visit.ScheduleDate == null)
                return "";

            var openVariable = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVisitId == visit.ProjectDesignVisitId
            && x.VisitStatusId == ScreeningVisitStatus.Open).
              Select(t => new { t.ProjectDesignVariable.Id, t.ProjectDesignVariable.ProjectDesignTemplateId }).FirstOrDefault();

            if (openVariable == null) return "";

            var scheduleTemplates = _impactService.GetTargetSchedule(openVariable.ProjectDesignTemplateId, false);

            if (scheduleTemplates == null) return "";
            var scheduleTemplate = scheduleTemplates.FirstOrDefault(r => r.ProjectDesignVariableId == openVariable.Id);
            if (scheduleTemplate == null) return "";
            if (!_scheduleRuleRespository.Validate(scheduleTemplate, screeningVisitDto.StatusDate.ToString(), visit.ScheduleDate.ToString()))
                return scheduleTemplate.Message;

            return "";

        }

        public void OpenVisit(ScreeningVisitDto screeningVisitDto)
        {

            var visit = Find(screeningVisitDto.ScreeningVisitId);

            visit.Status = ScreeningVisitStatus.Open;
            visit.VisitStartDate = screeningVisitDto.VisitOpenDate;

            Update(visit);

            _screeningVisitHistoryRepository.SaveByScreeningVisit(visit, ScreeningVisitStatus.Open, screeningVisitDto.VisitOpenDate);
            _uow.Save();

            FindOpenVisitVarible(visit.ProjectDesignVisitId, visit.Id, screeningVisitDto.VisitOpenDate);

            PatientStatus(visit.ScreeningEntryId);

            ScheduleVisitUpdate(visit.ScreeningEntryId);


        }

        public void ScheduleVisitUpdate(int screeningEntryId)
        {
            var scheduleVisit = All.Where(x => x.ScreeningEntryId == screeningEntryId && x.IsSchedule && x.Status == ScreeningVisitStatus.NotStarted).
                  OrderByDescending(t => t.ScheduleDate).FirstOrDefault();

            if (scheduleVisit != null)
            {
                scheduleVisit.Status = ScreeningVisitStatus.Scheduled;
                Update(scheduleVisit);
            }
        }


        public void PatientStatus(int screeningEntryId)
        {
            var visitStatus = All.Where(x => x.ScreeningEntryId == screeningEntryId).GroupBy(t => t.Status).Select(r => r.Key).ToList();
            var patientStatus = ScreeningPatientStatus.OnTrial;


            if (visitStatus.Any(x => x == ScreeningVisitStatus.Missed || x == ScreeningVisitStatus.OnHold))
                patientStatus = ScreeningPatientStatus.OnHold;

            if (visitStatus.Any(x => x == ScreeningVisitStatus.Withdrawal))
                patientStatus = ScreeningPatientStatus.Withdrawal;

            if (visitStatus.Any(x => x == ScreeningVisitStatus.ScreeningFailure))
                patientStatus = ScreeningPatientStatus.ScreeningFailure;

            if (!visitStatus.Any(x => x != ScreeningVisitStatus.Completed))
                patientStatus = ScreeningPatientStatus.Completed;

            _randomizationRepository.PatientStatus(patientStatus, screeningEntryId);


        }

        public void AutomaticStatusUpdate(int screeningTemplateId)
        {
            var screeningVisit = _screeningTemplateRepository.All.Where(x => x.Id == screeningTemplateId).Select(t => new
            {
                t.ProjectDesignTemplate.ProjectDesignVisitId,
                t.ScreeningVisitId,
                t.Status,
                t.ScreeningVisit.ScreeningEntryId
            }).FirstOrDefault();

            if (screeningVisit == null) return;


            var designVisitStatus = _projectDesignVisitStatusRepository.All.Where(x => x.DeletedDate == null && x.ProjectDesignVisitId == screeningVisit.ProjectDesignVisitId).Select(
                  t => new { t.ProjectDesignVariableId, t.VisitStatusId }).ToList();
            DateTime statusDate = System.DateTime.Now;

            if (designVisitStatus != null && designVisitStatus.Count() > 0)
            {
                var designVariable = designVisitStatus.FirstOrDefault(t => t.VisitStatusId > ScreeningVisitStatus.Scheduled);
                if (designVariable != null)
                {
                    var screeningValue = _screeningTemplateValueRepository.All.Where(t => t.ProjectDesignVariableId == designVariable.ProjectDesignVariableId
                      && t.ScreeningTemplateId == screeningTemplateId).Select(r => r.Value).FirstOrDefault();

                    if (!string.IsNullOrEmpty(screeningValue))
                    {
                        DateTime.TryParse(screeningValue, out statusDate);
                        StatusUpdate(new ScreeningVisitHistoryDto
                        {
                            VisitStatusId = designVariable.VisitStatusId,
                            ScreeningVisitId = screeningVisit.ScreeningVisitId,
                            StatusDate = statusDate
                        });
                    }
                }
            }

            if (screeningVisit.Status == ScreeningTemplateStatus.Submitted)
            {
                if (!_screeningTemplateRepository.All.Any(x => x.ScreeningVisit.ScreeningEntryId == screeningVisit.ScreeningEntryId
                && x.ScreeningVisitId == screeningVisit.ScreeningVisitId && x.Status < ScreeningTemplateStatus.Submitted))
                    StatusUpdate(new ScreeningVisitHistoryDto
                    {
                        VisitStatusId = ScreeningVisitStatus.Completed,
                        ScreeningVisitId = screeningVisit.ScreeningVisitId,
                        StatusDate = statusDate
                    });

            }

        }
        public void VisitRepeat(ScreeningVisitDto screeningVisitDto)
        {
            var repeatedCount = 0;
            var cloneVisit = All.Where(x => x.Id == screeningVisitDto.ScreeningVisitId && x.ParentId == null).FirstOrDefault();

            if (cloneVisit != null)
                repeatedCount = All.Where(x => x.ScreeningEntryId == cloneVisit.ScreeningEntryId && x.ProjectDesignVisitId == cloneVisit.ProjectDesignVisitId).Max(t => t.RepeatedVisitNumber) ?? 0;


            var templates = _projectDesignTemplateRepository.All.Where(x => x.ProjectDesignVisitId == cloneVisit.ProjectDesignVisitId).ToList();

            var screeningVisit = new ScreeningVisit
            {

                ProjectDesignVisitId = cloneVisit.ProjectDesignVisitId,
                Status = ScreeningVisitStatus.Open,
                ScreeningEntryId = cloneVisit.ScreeningEntryId,
                RepeatedVisitNumber = repeatedCount + 1,
                ParentId = cloneVisit.Id,
                VisitStartDate = screeningVisitDto.VisitOpenDate,
                ScreeningTemplates = new List<ScreeningTemplate>()
            };

            templates.ForEach(t =>
            {
                var template = new ScreeningTemplate
                {
                    ProjectDesignTemplateId = t.Id,
                    Status = ScreeningTemplateStatus.Pending
                };
                _screeningTemplateRepository.Add(template);
                screeningVisit.ScreeningTemplates.Add(template);
            });

            Add(screeningVisit);

            _uow.Save();

            FindOpenVisitVarible(screeningVisit.ProjectDesignVisitId, screeningVisit.Id, screeningVisitDto.VisitOpenDate);

            PatientStatus(screeningVisit.ScreeningEntryId);
        }
    }
}
