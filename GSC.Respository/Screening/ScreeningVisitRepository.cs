using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.EditCheckImpact;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Schedule;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Screening
{
    public class ScreeningVisitRepository : GenericRespository<ScreeningVisit>, IScreeningVisitRepository
    {
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IScreeningVisitHistoryRepository _screeningVisitHistoryRepository;
        private readonly IProjectDesignVisitStatusRepository _projectDesignVisitStatusRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IGSCContext _context;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningProgress _screeningProgress;
        private readonly IScheduleRuleRespository _scheduleRuleRespository;
        private readonly IImpactService _impactService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IScreeningTemplateValueAuditRepository _screeningTemplateValueAuditRepository;
        private readonly IProjectScheduleRepository _projectScheduleRepository;
        private readonly IVisitEmailConfigurationRepository _visitEmailConfigurationRepository;
        public ScreeningVisitRepository(IGSCContext context,
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
            IScreeningTemplateValueAuditRepository screeningTemplateValueAuditRepository,
            IImpactService impactService,
            IProjectScheduleRepository projectScheduleRepository,
            IVisitEmailConfigurationRepository visitEmailConfigurationRepository)
            : base(context)
        {
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _screeningVisitHistoryRepository = screeningVisitHistoryRepository;
            _randomizationRepository = randomizationRepository;
            _projectDesignVisitStatusRepository = projectDesignVisitStatusRepository;
            _context = context;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _screeningProgress = screeningProgress;
            _scheduleRuleRespository = scheduleRuleRespository;
            _impactService = impactService;
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateValueAuditRepository = screeningTemplateValueAuditRepository;
            _projectScheduleRepository = projectScheduleRepository;
            _visitEmailConfigurationRepository= visitEmailConfigurationRepository;
        }


        public List<ScreeningVisitTree> GetVisitTree(int screeningEntryId)
        {

            var result = All.Where(s => s.ScreeningEntryId == screeningEntryId && s.DeletedDate == null
            && s.Status >= ScreeningVisitStatus.Open).Select(s => new ScreeningVisitTree
            {
                ScreeningVisitId = s.Id,
                VisitSeqNo = s.RepeatedVisitNumber,
                DesignOrder = s.ProjectDesignVisit.DesignOrder,
                ProjectDesignVisitId = s.ProjectDesignVisitId,
                ScreeningVisitName = (_jwtTokenAccesser.Language != 1 ?
                s.ProjectDesignVisit.VisitLanguage.Where(x => x.LanguageId == (int)_jwtTokenAccesser.Language).Select(a => a.Display).FirstOrDefault()
                : s.ScreeningVisitName) +
                //: s.ProjectDesignVisit.DisplayName) +
                                         Convert.ToString(s.RepeatedVisitNumber == null ? "" : "_" + s.RepeatedVisitNumber),
                VisitStatus = s.Status,
                VisitStatusName = s.Status.GetDescription(),
                ParentScreeningVisitId = s.ParentId,
                IsVisitRepeated = s.ProjectDesignVisit.IsRepeated,
            }).OrderBy(o => o.DesignOrder).ThenBy(t => t.VisitSeqNo).ToList();

            return result;
        }

        public void ScreeningVisitSave(ScreeningEntry screeningEntry, int projectDesignPeriodId, int projectDesignVisitId, DateTime visitDate, double? studyVersion)
        {
            var designVisits = _projectDesignVisitRepository.GetVisitAndTemplateByPeriordId(projectDesignPeriodId);

            designVisits = designVisits.Where(x => (x.StudyVersion == null || x.StudyVersion <= studyVersion) &&
            (x.InActiveVersion == null || x.InActiveVersion > studyVersion)).ToList();


            screeningEntry.ScreeningVisit = new List<ScreeningVisit>();
            designVisits.ForEach(r =>
            {
                var screeningVisit = new ScreeningVisit
                {
                    ProjectDesignVisitId = r.Id,
                    Status = projectDesignVisitId == r.Id ? ScreeningVisitStatus.Open : ScreeningVisitStatus.NotStarted,
                    IsSchedule = r.IsSchedule ?? false,
                    ScreeningVisitName=r.DisplayName,
                    ScreeningTemplates = new List<ScreeningTemplate>()
                };

                if (screeningVisit.Status == ScreeningVisitStatus.Open)
                {
                    screeningVisit.VisitStartDate = visitDate;
                    _screeningVisitHistoryRepository.SaveByScreeningVisit(screeningVisit, ScreeningVisitStatus.Open, visitDate);
                    //// send email
                    //_visitEmailConfigurationRepository.SendEmailForVisitStatus(screeningVisit);
                }

                r.Templates.Where(b => (b.StudyVersion == null || b.StudyVersion <= studyVersion)
                && (b.InActiveVersion == null || b.InActiveVersion > studyVersion)).ToList().ForEach(t =>
                {
                    var screeningTemplate = new ScreeningTemplate
                    {
                        ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                        Status = ScreeningTemplateStatus.Pending,
                        ScreeningTemplateName=t.ScreeningTemplateName
                    };
                    _screeningTemplateRepository.Add(screeningTemplate);
                    screeningVisit.ScreeningTemplates.Add(screeningTemplate);

                });

                Add(screeningVisit);

                screeningEntry.ScreeningVisit.Add(screeningVisit);
            });
        }

        public void FindOpenVisitVarible(int projectDesignVisitId, int screeningVisitId, DateTime visitDate, int screeningEntryId)
        {
            var openVariable = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVisitId == projectDesignVisitId
            && x.VisitStatusId == ScreeningVisitStatus.Open && x.DeletedDate == null).
              Select(t => new
              {
                  t.ProjectDesignVariable.Id,
                  t.ProjectDesignVariable.ProjectDesignTemplateId,
                  t.ProjectDesignVisitId
              }).FirstOrDefault();

            if (openVariable != null && openVariable.ProjectDesignVisitId == projectDesignVisitId)
            {
                var screeningVisit = Find(screeningVisitId);
                var screeningTemplate = _screeningTemplateRepository.All.AsNoTracking().Where(x => x.ScreeningVisitId == screeningVisitId && x.ProjectDesignTemplateId == openVariable.ProjectDesignTemplateId && x.ParentId == null).FirstOrDefault();
                if (screeningTemplate != null && SaveVariableValue(visitDate, screeningTemplate, openVariable.Id, openVariable.ProjectDesignTemplateId, screeningEntryId, screeningVisit.ParentId == null))
                {
                    screeningVisit.Status = ScreeningVisitStatus.InProgress;
                    screeningTemplate.Status = ScreeningTemplateStatus.InProcess;
                    _screeningTemplateRepository.Update(screeningTemplate);
                    _screeningVisitHistoryRepository.SaveByScreeningVisit(screeningVisit, ScreeningVisitStatus.InProgress, visitDate);
                    Update(screeningVisit);
                }
                _context.Save();
                if (screeningTemplate != null)
                    _screeningProgress.GetScreeningProgress(screeningEntryId, screeningTemplate.Id);
            }
        }

        private bool SaveVariableValue(DateTime value, ScreeningTemplate screeningTemplate, int projectDesignVariableId, int projectDesignTemplateId, int screeningEntryId, bool isParentVisit)
        {

            if (!_projectDesignVariableRepository.All.Any(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.Id == projectDesignVariableId))
                return false;



            var screeningValue = _screeningTemplateValueRepository.All.Where(x => x.ProjectDesignVariableId == projectDesignVariableId && x.ScreeningTemplateId == screeningTemplate.Id).FirstOrDefault();

            var strValue = value.ToString("MM/dd/yyyy HH:mm:ss");

            if (screeningValue == null)
            {
                screeningValue = new ScreeningTemplateValue
                {
                    ScreeningTemplateId = screeningTemplate.Id,
                    ProjectDesignVariableId = projectDesignVariableId,
                    Value = strValue
                };
                _screeningTemplateValueRepository.Add(screeningValue);
            }
            else
            {
                screeningValue.Value = strValue;
                _screeningTemplateValueRepository.Update(screeningValue);
            }

            var audit = new ScreeningTemplateValueAudit
            {
                ScreeningTemplateValue = screeningValue,
                ScreeningTemplateValueId = screeningValue.Id,
                Value = strValue,
                Note = "Save value from open visit"
            };
            _screeningTemplateValueAuditRepository.Save(audit);

            _context.Save();

            if (screeningTemplate.ParentId == null && isParentVisit)
                _scheduleRuleRespository.ValidateByVariable(screeningEntryId, screeningTemplate.ScreeningVisitId, strValue, screeningTemplate.ProjectDesignTemplateId, projectDesignVariableId, true);

            return true;

        }

        public void StatusUpdate(ScreeningVisitHistoryDto screeningVisitHistoryDto)
        {
            var visit = Find(screeningVisitHistoryDto.ScreeningVisitId);

            visit.Status = screeningVisitHistoryDto.VisitStatusId;

            if (screeningVisitHistoryDto.VisitStatusId == ScreeningVisitStatus.ReSchedule)
                visit.ScheduleDate = screeningVisitHistoryDto.StatusDate;

            Update(visit);

            _screeningVisitHistoryRepository.Save(screeningVisitHistoryDto);

            _context.Save();

            PatientStatus(visit.ScreeningEntryId);

            // send email
            _visitEmailConfigurationRepository.SendEmailForVisitStatus(visit);

        }

        public bool IsPatientScreeningFailure(int screeningVisitId)
        {
            var visit = All.Where(x => x.Id == screeningVisitId).Select(t => new
            {
                t.ScreeningEntryId,
                t.ProjectDesignVisit.IsSchedule
            }).FirstOrDefault();

            if (visit == null || visit.IsSchedule == false)
                return false;

            return All.Any(t => t.ScreeningEntryId == visit.ScreeningEntryId && (
           t.ScreeningEntry.Randomization.PatientStatusId == ScreeningPatientStatus.ScreeningFailure ||
           t.ScreeningEntry.Randomization.PatientStatusId == ScreeningPatientStatus.Withdrawal));
        }


        public string CheckOpenDate(ScreeningVisitDto screeningVisitDto)
        {
            var visit = Find(screeningVisitDto.ScreeningVisitId);
            if (visit != null
                && (visit.Status == ScreeningVisitStatus.ReSchedule || visit.Status == ScreeningVisitStatus.Scheduled)
                && visit.ScheduleDate != null && visit.ScheduleDate.Value.Date > screeningVisitDto.VisitOpenDate.Date)
                return $"You cannot enter a date in the future!";

            return "";

        }


        public string CheckScheduleDate(ScreeningVisitHistoryDto screeningVisitDto)
        {
            var visit = Find(screeningVisitDto.ScreeningVisitId);

            if (visit == null || visit.ScheduleDate == null)
                return "";

            var openVariable = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVisitId == visit.ProjectDesignVisitId
            && x.VisitStatusId == ScreeningVisitStatus.Open && x.DeletedDate == null).
              Select(t => new { t.ProjectDesignVariable.Id, t.ProjectDesignVariable.ProjectDesignTemplateId }).FirstOrDefault();

            if (openVariable == null) return "";

            var scheduleTemplates = _impactService.GetTargetSchedule(openVariable.ProjectDesignTemplateId, false);
            var projectScheduleId = scheduleTemplates.Select(t => t.ProjectScheduleId).ToList();
            var refrenceSchedule = _impactService.GetReferenceSchedule(projectScheduleId);

            if (scheduleTemplates == null || refrenceSchedule == null) return "";

            var scheduleTemplate = scheduleTemplates.FirstOrDefault(r => r.ProjectDesignVariableId == openVariable.Id);
            if (scheduleTemplate == null) return "";

            var scheduleScreeningTemplate = _impactService.GetScreeningTemplateId(refrenceSchedule.FirstOrDefault().ProjectDesignTemplateId, visit.ScreeningEntryId);
            if (scheduleScreeningTemplate == null) return "";
            var refDate = _impactService.GetVariableValue(scheduleScreeningTemplate.ScreeningTemplateId, refrenceSchedule.FirstOrDefault().ProjectDesignVariableId);

            if (!_scheduleRuleRespository.Validate(scheduleTemplate, screeningVisitDto.StatusDate.ToString(), refDate.ToString()))
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
            _context.Save();

            FindOpenVisitVarible(visit.ProjectDesignVisitId, visit.Id, screeningVisitDto.VisitOpenDate, visit.ScreeningEntryId);

            PatientStatus(visit.ScreeningEntryId);
            var isScheduleReference = _projectScheduleRepository.All.Any(x => x.ProjectDesignVisitId == visit.ProjectDesignVisitId && x.DeletedDate == null);
            if (visit.IsSchedule || isScheduleReference)
                ScheduleVisitUpdate(visit.ScreeningEntryId);

            // send email
            _visitEmailConfigurationRepository.SendEmailForVisitStatus(visit);
        }

        public void ScheduleVisitUpdate(int screeningEntryId)
        {
            if (!All.Any(x => x.ScreeningEntryId == screeningEntryId && x.DeletedDate == null && x.Status == ScreeningVisitStatus.Scheduled))
            {
                var scheduleVisit = All.Where(x => x.ScreeningEntryId == screeningEntryId && x.ScheduleDate != null && x.IsSchedule && x.Status == ScreeningVisitStatus.NotStarted).
                 OrderBy(t => t.ScheduleDate).FirstOrDefault();

                if (scheduleVisit != null)
                {
                    scheduleVisit.Status = ScreeningVisitStatus.Scheduled;
                    Update(scheduleVisit);
                }
            }

        }


        public void PatientStatus(int screeningEntryId)
        {
            var visitStatus = All.Where(x => x.ScreeningEntryId == screeningEntryId).GroupBy(t => t.Status).Select(r => r.Key).ToList();
            var patientStatus = ScreeningPatientStatus.OnTrial;


            if (visitStatus.Any(x => x == ScreeningVisitStatus.OnHold))
                patientStatus = ScreeningPatientStatus.OnHold;

            if (visitStatus.Any(x => x == ScreeningVisitStatus.Withdrawal))
                patientStatus = ScreeningPatientStatus.Withdrawal;

            if (visitStatus.Any(x => x == ScreeningVisitStatus.ScreeningFailure))
                patientStatus = ScreeningPatientStatus.ScreeningFailure;

            if (!visitStatus.Any(x => x != ScreeningVisitStatus.Completed))
                patientStatus = ScreeningPatientStatus.Completed;

            _randomizationRepository.PatientStatus(patientStatus, screeningEntryId);


        }

        public ScreeningVisitStatus? AutomaticStatusUpdate(int screeningTemplateId)
        {
            var screeningTemplate = _screeningTemplateRepository.All.AsNoTracking().Where(x => x.Id == screeningTemplateId).Select(t => new
            {
                t.ProjectDesignTemplate.ProjectDesignVisitId,
                t.ScreeningVisitId,
                t.Status,
                t.ScreeningVisit.ScreeningEntryId,
                VisitStatus = t.ScreeningVisit.Status,

            }).FirstOrDefault();

            if (screeningTemplate == null) return null;

            if (screeningTemplate.VisitStatus == ScreeningVisitStatus.Missed ||
            screeningTemplate.VisitStatus == ScreeningVisitStatus.OnHold)
                return screeningTemplate.VisitStatus;

            var designVisitStatus = _projectDesignVisitStatusRepository.All.Where(x => x.DeletedDate == null
                && x.ProjectDesignVisitId == screeningTemplate.ProjectDesignVisitId &&
                (x.VisitStatusId == ScreeningVisitStatus.ScreeningFailure || x.VisitStatusId == ScreeningVisitStatus.Withdrawal)).Select(
                t => new { t.ProjectDesignVariableId, t.VisitStatusId }).ToList();


            DateTime? statusDate = _jwtTokenAccesser.GetClientDate();

            ScreeningVisitStatus? visitStatus = ScreeningVisitStatus.InProgress;

            if (screeningTemplate.Status >= ScreeningTemplateStatus.Submitted)
            {
                if (!_screeningTemplateRepository.All.AsNoTracking().Any(x => x.ScreeningVisit.ScreeningEntryId == screeningTemplate.ScreeningEntryId
                && x.ScreeningVisitId == screeningTemplate.ScreeningVisitId && !x.IsDisable && x.Status < ScreeningTemplateStatus.Submitted))
                {
                    statusDate = _jwtTokenAccesser.GetClientDate();
                    visitStatus = ScreeningVisitStatus.Completed;
                }
            }

            designVisitStatus.ForEach(x =>
            {
                var screeningValue = _screeningTemplateValueRepository.All.Where(t => t.ProjectDesignVariableId == x.ProjectDesignVariableId
                      && t.ScreeningTemplate.ScreeningVisitId == screeningTemplate.ScreeningVisitId).Select(r => r.Value).FirstOrDefault();

                if (!string.IsNullOrEmpty(screeningValue))
                {
                    DateTime convertDte;
                    var isSucess = DateTime.TryParse(screeningValue, out convertDte);

                    if (isSucess)
                    {
                        statusDate = convertDte;
                        visitStatus = x.VisitStatusId;
                    }
                }
            });


            if (visitStatus != null && statusDate != null && screeningTemplate.VisitStatus != visitStatus)
            {
                StatusUpdate(new ScreeningVisitHistoryDto
                {
                    VisitStatusId = (ScreeningVisitStatus)visitStatus,
                    ScreeningVisitId = screeningTemplate.ScreeningVisitId,
                    StatusDate = statusDate
                });
            }

            return visitStatus;
        }

        public bool ValidateRepeatVisit(int id)
        {
            if (All.Any(x => x.Id == id && x.ParentId == null && x.ScreeningTemplates.Any(t => t.Status > ScreeningTemplateStatus.Pending)))
                return true;

            return false;
        }

        public void VisitRepeat(ScreeningVisitDto screeningVisitDto)
        {
            var repeatedCount = 0;
            var cloneVisit = All.Where(x => x.Id == screeningVisitDto.ScreeningVisitId && x.ParentId == null).FirstOrDefault();

            if (cloneVisit != null)
                repeatedCount = All.Where(x => x.ScreeningEntryId == cloneVisit.ScreeningEntryId && x.ProjectDesignVisitId == cloneVisit.ProjectDesignVisitId).Max(t => t.RepeatedVisitNumber) ?? 0;

            var templates = _screeningTemplateRepository.All.Where(r => r.DeletedDate == null 
            && r.ScreeningVisitId == screeningVisitDto.ScreeningVisitId
            && r.ParentId ==null).Select(t => new
            {
                t.ProjectDesignTemplateId,
                t.ScreeningTemplateName
            }).ToList();

            var screeningVisit = new ScreeningVisit
            {

                ProjectDesignVisitId = cloneVisit.ProjectDesignVisitId,
                Status = ScreeningVisitStatus.Open,
                ScreeningEntryId = cloneVisit.ScreeningEntryId,
                RepeatedVisitNumber = repeatedCount + 1,
                ParentId = cloneVisit.Id,
                VisitStartDate = screeningVisitDto.VisitOpenDate,
                ScreeningTemplates = new List<ScreeningTemplate>(),
                ScreeningVisitName= screeningVisitDto.ScreeningVisitName
            };

            templates.ForEach(t =>
            {
                var template = new ScreeningTemplate
                {
                    ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                    Status = ScreeningTemplateStatus.Pending,
                    ScreeningTemplateName=t.ScreeningTemplateName
                };
                _screeningTemplateRepository.Add(template);
                screeningVisit.ScreeningTemplates.Add(template);
            });

            Add(screeningVisit);

            _context.Save();

            screeningVisitDto.ScreeningVisitId = screeningVisit.Id;

            FindOpenVisitVarible(screeningVisit.ProjectDesignVisitId, screeningVisit.Id, screeningVisitDto.VisitOpenDate, screeningVisit.ScreeningEntryId);

            PatientStatus(screeningVisit.ScreeningEntryId);
        }

        public IList<DropDownDto> GetVisitByLockedDropDown(LockUnlockDDDto lockUnlockDDDto)
        {
            //var ParentProject = _context.Project.FirstOrDefault(x => x.Id == lockUnlockDDDto.ChildProjectId).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == lockUnlockDDDto.ProjectId).ToList().Select(x => x.Id).ToList(); // Change by Tinku for add separate dropdown for parent project (24/06/2022) 

            var Visit = _context.ScreeningVisit.Include(a => a.ScreeningEntry).Include(a => a.ProjectDesignVisit)
                .Include(a => a.ScreeningTemplates)
                .Where(a => a.DeletedDate == null && lockUnlockDDDto.ChildProjectId > 0 ? a.ScreeningEntry.ProjectId == lockUnlockDDDto.ChildProjectId : sites.Contains(a.ScreeningEntry.ProjectId) // Change by Tinku for add separate dropdown for parent project (24/06/2022) 
                ).ToList();

            if (lockUnlockDDDto.SubjectIds != null)
                Visit = Visit.Where(a => lockUnlockDDDto.SubjectIds.Contains(a.ScreeningEntryId)).ToList();

            if (lockUnlockDDDto.Id != null)
                Visit = Visit.Where(a => lockUnlockDDDto.Id.Contains(a.ProjectDesignVisit.ProjectDesignPeriodId)).ToList();

            Visit = Visit.Where(a => a.ScreeningTemplates.Where(t => t.IsLocked == lockUnlockDDDto.IsLocked && t.IsHardLocked == lockUnlockDDDto.IsHardLocked).Count() > 0
                  && a.ScreeningTemplates != null).ToList();

            return Visit.GroupBy(x => x.ProjectDesignVisitId).Select(x => new DropDownDto
            {
                Id = x.Key,
                // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                Value = x.FirstOrDefault().ScreeningVisitName
            }).Distinct().ToList();
        }

        // Dashboard chart for Visit Status
        public List<DashboardQueryStatusDto> GetVisitStatus(int projectId)
        {
            var result = All.Where(x => (x.ScreeningEntry.ProjectId == projectId ||
           x.ScreeningEntry.Project.ParentProjectId == projectId) && (x.ScreeningEntry.Project.IsTestSite != true) && x.DeletedDate == null).GroupBy(
               t => new { t.Status }).Select(g => new DashboardQueryStatusDto
               {
                   DisplayName = g.Key.Status.GetDescription(),
                   Total = g.Count()
               }).ToList();
            return result;
        }

        //Add by Tinku Mahato for voleenter screening on 21-06-2022
        public List<int> GetScreeningVisitList(int screeningEntryId)
        {
            return All.Where(q => q.ScreeningEntryId == screeningEntryId && q.Status == ScreeningVisitStatus.NotStarted).Select(s => s.Id).ToList();
        }
    }
}
