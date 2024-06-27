using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.EditCheck;
using GSC.Respository.Project.Workflow;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class DataEntryRespository : GenericRespository<ScreeningEntry>, IDataEntryRespository
    {
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly ITemplateVariableSequenceNoSettingRepository _templateVariableSequenceNoSettingRepository;
        private readonly IScreeningEntryRepository _screeningEntryRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        private readonly IEditCheckDetailRepository _editCheckDetailRepository;
        private readonly IProjectDesingTemplateRestrictionRepository _projectDesingTemplateRestrictionRepository;
        public DataEntryRespository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IRandomizationRepository randomizationRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            ITemplateVariableSequenceNoSettingRepository templateVariableSequenceNoSettingRepository,
            IScreeningEntryRepository screeningEntryRepository,
            IProjectDesignRepository projectDesignRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IScreeningVisitRepository screeningVisitRepository,
            IEditCheckDetailRepository editCheckDetailRepository
, IProjectDesingTemplateRestrictionRepository projectDesingTemplateRestrictionRepository)
            : base(context)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _randomizationRepository = randomizationRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _screeningEntryRepository = screeningEntryRepository;
            _templateVariableSequenceNoSettingRepository = templateVariableSequenceNoSettingRepository;
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _projectDesignRepository = projectDesignRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _screeningVisitRepository = screeningVisitRepository;
            _editCheckDetailRepository = editCheckDetailRepository;
            _projectDesingTemplateRestrictionRepository = projectDesingTemplateRestrictionRepository;
        }

        public async Task<DataCaptureGridDto> GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int parentProjectId, int projectId, bool IsPatientLevel)
        {
            var result = new DataCaptureGridDto();

            var projectDesignId = await _projectDesignRepository.All.Where(r => r.ProjectId == parentProjectId).Select(t => t.Id).FirstOrDefaultAsync();

            var editCheckVisit = _editCheckDetailRepository.GetProjectDesignVisitIds(projectDesignId);

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesignId);
            result.WorkFlowText = workflowlevel.WorkFlowText;
            result.ReviewLevel = workflowlevel.LevelNo;
            result.IsStartTemplate = workflowlevel.IsStartTemplate;

            var projectDesignVisit = await _projectDesignVisitRepository.All.
                Where(x => x.ProjectDesignPeriod.ProjectDesignId == projectDesignId && x.IsSchedule != true
                && (IsPatientLevel ? x.IsPatientLevel : !x.IsPatientLevel) && x.DeletedDate == null).
                OrderBy(a => a.DesignOrder).
            Select(t => new DataEntryVisitTemplateDto
            {
                ProjectDesignVisitId = t.Id,
                VisitName = t.DisplayName,
                VisitStatus = ScreeningVisitStatus.NotStarted.GetDescription(),
                VisitStatusId = (int)ScreeningVisitStatus.NotStarted,
                StudyVersion = t.StudyVersion,
                InActiveVersion = t.InActiveVersion
            }).ToListAsync();

            projectDesignVisit.ForEach(a =>
            {
                var editCheck = editCheckVisit.FirstOrDefault(r => r.ProjectDesignVisitId == a.ProjectDesignVisitId && r.HideDisableType == HideDisableType.Disable);
                if (editCheck != null)
                    a.HideDisableType = editCheck.HideDisableType;

                editCheck = editCheckVisit.FirstOrDefault(r => r.ProjectDesignVisitId == a.ProjectDesignVisitId && r.HideDisableType == HideDisableType.Hide);
                if (editCheck != null)
                    a.HideDisableType = editCheck.HideDisableType;
            });

            var randomizationData = await _randomizationRepository.All.Where(x => x.ProjectId == projectId && x.DeletedDate == null
             && x.PatientStatusId == ScreeningPatientStatus.Screening && x.ScreeningEntry == null && (IsPatientLevel ? !x.IsGeneric : x.IsGeneric)).Select(t => new DataCaptureGridData
             {
                 RandomizationId = t.Id,
                 VolunteerName = t.Initial,
                 IsRandomization = true,
                 SubjectNo = t.ScreeningNumber,
                 PatientStatusId = t.PatientStatusId,
                 PatientStatusName = !IsPatientLevel ? "NA" : t.PatientStatusId.GetDescription(),
                 RandomizationNumber = t.RandomizationNumber,
                 StudyVersion = t.StudyVersion ?? 1,
                 IsEconsentCompleted = true
             }).ToListAsync();

            var hideTemplateIds = await _projectDesingTemplateRestrictionRepository.All.Where(x => x.SecurityRoleId == _jwtTokenAccesser.RoleId && x.IsHide && x.DeletedDate == null).Select(r => r.ProjectDesignTemplateId).ToListAsync();
            var siteAccessTemplates = await _context.ProjectDesignTemplateSiteAccess.Where(s => s.DeletedDate == null && s.ProjectId == projectId).Select(s => s.ProjectDesignTemplateId).ToListAsync();

            siteAccessTemplates.AddRange(hideTemplateIds);

            var tempTemplate = _screeningTemplateRepository.All.
             Where(r => r.ScreeningVisit.ScreeningEntry.ProjectId == projectId);

            if (siteAccessTemplates.Count > 0)
            {
                tempTemplate = tempTemplate.Where(r => !siteAccessTemplates.Contains(r.ProjectDesignTemplateId));
            }

            var templates = await tempTemplate.
             Where(r => r.ScreeningVisit.ScreeningEntry.ProjectId == projectId).
              GroupBy(c => new
              {
                  c.ScreeningVisit.ScreeningEntryId,
                  c.ScreeningVisitId,
                  c.ReviewLevel,
                  c.Status,
                  c.IsLocked,
                  c.IsNA,
                  c.IsDisable,
                  IsHide = c.IsHide == true
              }).Select(t => new
              {
                  t.Key.ScreeningEntryId,
                  t.Key.Status,
                  t.Key.ScreeningVisitId,
                  t.Key.ReviewLevel,
                  t.Key.IsLocked,
                  t.Key.IsNA,
                  t.Key.IsHide,
                  t.Key.IsDisable,
                  TotalTemplate = t.Count()
              }).ToListAsync();

            templates = templates.Where(r => !r.IsDisable && !r.IsHide).ToList();
            templates = templates.Where(r => !r.IsNA).ToList();

            var queries = await _screeningTemplateValueRepository.All.Where(r =>
            r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId &&
             r.QueryStatus != null &&
            r.ProjectDesignVariable.DeletedDate == null && r.DeletedDate == null).
                GroupBy(c => new
                {
                    c.ScreeningTemplate.ScreeningVisitId,
                    c.AcknowledgeLevel,
                    c.UserRoleId,
                    c.ReviewLevel,
                    c.QueryStatus
                }).Select(t => new
                {
                    t.Key.AcknowledgeLevel,
                    t.Key.ScreeningVisitId,
                    t.Key.ReviewLevel,
                    t.Key.UserRoleId,
                    t.Key.QueryStatus,
                    TotalQuery = t.Count()
                }).ToListAsync();


            var screeningData = await _screeningEntryRepository.All.Where(r => r.ProjectId == projectId
            && r.DeletedDate == null).Select(x => new DataCaptureGridData
            {
                ScreeningEntryId = x.Id,
                RandomizationId = x.RandomizationId,
                AttendanceId = x.AttendanceId,
                VolunteerName = x.RandomizationId != null ? x.Randomization.Initial : x.Attendance.Volunteer.AliasName,
                IsRandomization = x.RandomizationId != null,
                SubjectNo = x.RandomizationId != null ? x.Randomization.ScreeningNumber : x.Attendance.Volunteer.VolunteerNo,
                ScreeningPatientStatus = x.RandomizationId != null ? x.Randomization.PatientStatusId : ScreeningPatientStatus.Screening,
                PatientStatusId = x.RandomizationId != null ? x.Randomization.PatientStatusId : 0,
                PatientStatusName = x.RandomizationId != null ? IsPatientLevel ? x.Randomization.PatientStatusId.GetDescription() : "NA" : "",
                RandomizationNumber = x.RandomizationId != null ? x.Randomization.RandomizationNumber : "",
                StudyVersion = x.Randomization.StudyVersion ?? 1,
                IsEconsentCompleted = true,
                IsGeneric = x.Randomization.IsGeneric
            }).ToListAsync();

            if (!IsPatientLevel && screeningData.Count > 0)
            {
                screeningData = screeningData.Where(s => s.IsGeneric).ToList();
            }
            if (IsPatientLevel && screeningData.Count > 0)
            {
                screeningData = screeningData.Where(s => !s.IsGeneric).ToList();
            }
            var visits = await _screeningVisitRepository.All.
               Where(r => r.ScreeningEntry.ProjectId == projectId && r.DeletedDate == null).Select(a => new DataEntryVisitTemplateDto
               {
                   ScreeningVisitId = a.Id,
                   ProjectDesignVisitId = a.ProjectDesignVisitId,
                   VisitName = //a.ProjectDesignVisit.DisplayName 
                   a.ScreeningVisitName
                   + Convert.ToString(a.ParentId != null ? "-" + a.RepeatedVisitNumber.ToString() : ""),
                   VisitStatus = !a.IsNA ? a.Status.GetDescription() : "Not Applicable",
                   //a.Status.GetDescription(),
                   VisitStatusId = (int)a.Status,
                   ActualDate = (int)a.Status > 3 ? a.VisitStartDate : null,
                   ScheduleDate = a.ScheduleDate,
                   IsSchedule = a.IsSchedule,
                   DesignOrder = a.ProjectDesignVisit.DesignOrder,
                   StudyVersion = a.ProjectDesignVisit.StudyVersion,
                   IsScheduleTerminate = a.IsScheduleTerminate,
                   ScreeningEntryId = a.ScreeningEntryId,
                   IsPatientLevel = a.ProjectDesignVisit.IsPatientLevel,
                   IsNA = a.IsNA,
               }).OrderBy(b => b.DesignOrder).ToListAsync();

            if (!IsPatientLevel && visits.Count > 0)
            {
                visits = visits.Where(s => !s.IsPatientLevel).ToList();
            }
            if (IsPatientLevel && visits.Count > 0)
            {
                visits = visits.Where(s => s.IsPatientLevel).ToList();
            }

            randomizationData.ForEach(r =>
            {
                r.Visit = projectDesignVisit.Where(t => (t.StudyVersion == null || t.StudyVersion <= r.StudyVersion) && (t.InActiveVersion == null || t.InActiveVersion > r.StudyVersion)).ToList();
                r.TemplateCount = result.WorkFlowText.Select(x => new WorkFlowTemplateCount
                {
                    LevelNo = x.LevelNo
                }).ToList();
            });

            screeningData.ForEach(r =>
            {
                r.Visit = visits.Where(a => a.ScreeningEntryId == r.ScreeningEntryId &&
                 (!a.IsSchedule || a.IsScheduleTerminate == true || a.VisitStatusId > (int)ScreeningVisitStatus.NotStarted)).OrderBy(t=>t.DesignOrder).ToList();

                r.Visit.Where(x => x.VisitStatusId > 3).ToList().ForEach(v =>
                {
                    v.IsLocked = templates.Any(x => x.IsLocked == false && x.ScreeningVisitId == v.ScreeningVisitId) ? false : true;
                    v.NotStarted = templates.Where(x => x.ScreeningVisitId == v.ScreeningVisitId && x.Status == ScreeningTemplateStatus.Pending).Sum(t => t.TotalTemplate);
                    v.InProgress = templates.Where(x => x.ScreeningVisitId == v.ScreeningVisitId && x.Status == ScreeningTemplateStatus.InProcess).Sum(t => t.TotalTemplate);

                    v.MyQuery = queries.Where(x => x.ScreeningVisitId == v.ScreeningVisitId && (
                    x.AcknowledgeLevel == workflowlevel.LevelNo ||
                    ((x.QueryStatus == QueryStatus.Open || x.QueryStatus == QueryStatus.Reopened) && workflowlevel.LevelNo == 0 && workflowlevel.IsStartTemplate) ||
                    ((x.QueryStatus == QueryStatus.Resolved || x.QueryStatus == QueryStatus.Answered) && workflowlevel.LevelNo == 0 && x.UserRoleId == _jwtTokenAccesser.RoleId)
                    )).Sum(t => t.TotalQuery);

                    v.ReOpen = queries.Where(x => x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Reopened).Sum(t => t.TotalQuery);
                    v.Open = queries.Where(x => x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Open).Sum(t => t.TotalQuery);
                    v.Answered = queries.Where(x => x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Answered).Sum(t => t.TotalQuery);
                    v.Resolved = queries.Where(x => x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Resolved).Sum(t => t.TotalQuery);
                    //v.Closed = closeQueries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId).Sum(t => t.TotalQuery);
                    v.SelfCorrection = queries.Where(x => x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.SelfCorrection).Sum(t => t.TotalQuery);
                    v.Acknowledge = queries.Where(x => x.ScreeningVisitId == v.ScreeningVisitId && x.AcknowledgeLevel != x.ReviewLevel && (x.QueryStatus == QueryStatus.Resolved || x.QueryStatus == QueryStatus.SelfCorrection)).Sum(t => t.TotalQuery);
                    v.TemplateCount = result.WorkFlowText.Select(x => new WorkFlowTemplateCount
                    {
                        LevelNo = x.LevelNo,
                        Count = templates.Where(a => a.ScreeningVisitId == v.ScreeningVisitId && a.ReviewLevel == x.LevelNo).Sum(t => t.TotalTemplate)
                    }).ToList();
                });

                r.NotStarted = r.Visit.Sum(x => x.NotStarted);
                r.InProgress = r.Visit.Sum(x => x.InProgress);
                r.MyQuery = r.Visit.Sum(x => x.MyQuery);
                r.ReOpen = r.Visit.Sum(x => x.ReOpen);
                r.Open = r.Visit.Sum(x => x.Open);
                r.Answered = r.Visit.Sum(x => x.Answered);
                r.Resolved = r.Visit.Sum(x => x.Resolved);
                r.Closed = r.Visit.Sum(x => x.Closed);
                r.SelfCorrection = r.Visit.Sum(x => x.SelfCorrection);
                r.Acknowledge = r.Visit.Sum(x => x.Acknowledge);
                r.TemplateCount = result.WorkFlowText.Select(x => new WorkFlowTemplateCount
                {
                    LevelNo = x.LevelNo,
                    Count = templates.Where(a => a.ScreeningEntryId == r.ScreeningEntryId && a.ReviewLevel == x.LevelNo).Sum(t => t.TotalTemplate)
                }).ToList();
            });

            result.Data.AddRange(randomizationData);
            result.Data.AddRange(screeningData);

            result.Data = result.Data.OrderByDescending(t => t.SubjectNo).ToList();

            if (editCheckVisit.Count > 0)
            {
                result.Data.ForEach(x =>
                {
                    x.Visit.ForEach(a =>
                    {
                        var editCheck = editCheckVisit.FirstOrDefault(r => r.ProjectDesignVisitId == a.ProjectDesignVisitId && r.HideDisableType == HideDisableType.Disable);
                        if (editCheck != null)
                            a.EditCheckMsg = editCheck.EditCheckMsg;

                    });

                    x.Visit = x.Visit.Where(a => a.HideDisableType != HideDisableType.Hide).ToList();
                });
            }


            if (hideTemplateIds.Count() > 0)
            {
                var visitTemplates = await _projectDesignVisitRepository.All.
                Where(x => x.DeletedDate == null && x.ProjectDesignPeriod.ProjectDesignId == projectDesignId).Select(r => new
                {
                    r.Id,
                    TemplatesIds = r.Templates.Where(c => c.DeletedDate == null).Select(a => a.Id).ToList()
                }).ToListAsync();


                result.Data.ForEach(x =>
                {
                    x.Visit.ForEach(a =>
                    {
                        var templatesIds = visitTemplates.Where(r => r.Id == a.ProjectDesignVisitId).Select(v => v.TemplatesIds).FirstOrDefault();

                        if (templatesIds != null && templatesIds.Where(r => !hideTemplateIds.Contains(r)).Count() < 1)
                            a.HideDisableType = HideDisableType.Hide;
                    });

                    x.Visit = x.Visit.Where(a => a.HideDisableType != HideDisableType.Hide).ToList();
                });
            }




            return result;

        }

        public List<DataEntryTemplateCountDisplayTree> GetTemplateForVisit(int screeningVisitId, ScreeningTemplateStatus templateStatus, int? projectId)
        {
            var restrictTemplates = _context.ProjectDesignTemplateSiteAccess.Where(s => s.DeletedDate == null && s.ProjectId == projectId).Select(s => s.ProjectDesignTemplateId).ToList();
            var projectDesignId = _context.ScreeningVisit.Where(s => s.Id == screeningVisitId).Select(r => r.ScreeningEntry.ProjectDesignId).FirstOrDefault();

            var sequenseDeatils = _templateVariableSequenceNoSettingRepository.All.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).FirstOrDefault();

            var result = _screeningTemplateRepository.All.Where(s => s.ScreeningVisitId == screeningVisitId
            && s.DeletedDate == null
             && !s.IsNA
            && !restrictTemplates.Contains(s.ProjectDesignTemplateId)
            && s.Status == templateStatus && (s.IsHide == null || s.IsHide == false))
                 .Select(t => new DataEntryTemplateCountDisplayTree
                 {
                     Id = t.Id,
                     ScreeningEntryId = t.ScreeningVisit.ScreeningEntryId,
                     ScreeningVisitId = t.ScreeningVisitId,
                     ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                     Status = t.Status,
                     // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                     ScreeningTemplateName = t.ScreeningTemplateName,

                     //ProjectDesignTemplateName = t.ProjectDesignTemplate.TemplateName,
                     DesignOrder = sequenseDeatils.IsTemplateSeqNo == true ? t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder.ToString() : t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString() : "",
                     DesignOrderForOrderBy = t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString()),
                     Progress = t.Progress ?? 0,
                     ReviewLevel = t.ReviewLevel,
                     IsLocked = t.IsLocked,
                     MyReview = false,
                     ParentId = t.ParentId,
                     ScheduleDate = t.ScheduleDate,
                     // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                     TemplateName = t.ScreeningTemplateName,
                     // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                     VisitName = t.ScreeningVisit.ScreeningVisitName,
                     SubjectName = t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                                         ? t.ScreeningVisit.ScreeningEntry.Randomization.Initial
                                         : t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,
                     PreLabel = PreLabelSetting(t, t.ProjectDesignTemplate, sequenseDeatils)
                 }).ToList().OrderBy(t => t.DesignOrderForOrderBy).ToList();


            return result;


        }



        public List<DataEntryTemplateCountDisplayTree> GetTemplateVisitQuery(int screeningVisitId, QueryStatus queryStatus)
        {
            var tempResult = _screeningTemplateRepository.All.Where(s => s.ScreeningVisitId == screeningVisitId && s.DeletedDate == null);
            if (queryStatus == QueryStatus.Acknowledge)
            {
                tempResult = tempResult.Where(s => s.ScreeningTemplateValues.Any(r => r.AcknowledgeLevel != r.ReviewLevel && (r.QueryStatus == QueryStatus.Resolved || r.QueryStatus == QueryStatus.SelfCorrection) && r.DeletedDate == null));
            }
            else
                tempResult = tempResult.Where(s => s.ScreeningTemplateValues.Any(r => r.QueryStatus == queryStatus && r.DeletedDate == null));

            tempResult = tempResult.Where(x => (x.IsHide == null || x.IsHide == false));

            var projectDesignId = _context.ScreeningVisit.Where(s => s.Id == screeningVisitId).Select(r => r.ScreeningEntry.ProjectDesignId).FirstOrDefault();

            var sequenseDeatils = _templateVariableSequenceNoSettingRepository.All.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).FirstOrDefault();

            var result = tempResult.Select(t => new DataEntryTemplateCountDisplayTree
            {
                Id = t.Id,
                ScreeningEntryId = t.ScreeningVisit.ScreeningEntryId,
                ScreeningVisitId = t.ScreeningVisitId,
                ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                Status = t.Status,
                // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                ScreeningTemplateName = t.ScreeningTemplateName,
                DesignOrder = sequenseDeatils.IsTemplateSeqNo == true ? t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder.ToString() : t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString() : "",
                DesignOrderForOrderBy = t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString()),
                Progress = t.Progress ?? 0,
                ReviewLevel = t.ReviewLevel,
                IsLocked = t.IsLocked,
                MyReview = false,
                ParentId = t.ParentId,
                ScheduleDate = t.ScheduleDate,
                // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                TemplateName = t.ScreeningTemplateName,
                // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                VisitName = t.ScreeningVisit.ScreeningVisitName,
                SubjectName = t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                                         ? t.ScreeningVisit.ScreeningEntry.Randomization.Initial
                                         : t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,
                PreLabel = PreLabelSetting(t, t.ProjectDesignTemplate, sequenseDeatils)
            }).ToList().OrderBy(t => t.DesignOrderForOrderBy).ToList();


            return result;

        }

        public List<DataEntryTemplateCountDisplayTree> GetTemplateVisitMyQuery(int screeningVisitId, int parentProjectId)
        {
            var projectDesignId = _projectDesignRepository.All.Where(r => r.ProjectId == parentProjectId).Select(t => t.Id).FirstOrDefault();
            var sequenseDeatils = _templateVariableSequenceNoSettingRepository.All.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).FirstOrDefault();
            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesignId);

            var result = _screeningTemplateRepository.All.Where(s => s.ScreeningVisitId == screeningVisitId && s.DeletedDate == null
            && (s.IsHide == null || s.IsHide == false)
            && s.ScreeningTemplateValues.Any(r => (r.AcknowledgeLevel == workflowlevel.LevelNo ||
            ((r.QueryStatus == QueryStatus.Open || r.QueryStatus == QueryStatus.Reopened) && workflowlevel.LevelNo == 0 && workflowlevel.IsStartTemplate)) ||
            ((r.QueryStatus == QueryStatus.Resolved || r.QueryStatus == QueryStatus.Answered) && workflowlevel.LevelNo == 0 && r.UserRoleId == _jwtTokenAccesser.RoleId)
            && r.DeletedDate == null))
                 .Select(t => new DataEntryTemplateCountDisplayTree
                 {
                     Id = t.Id,
                     ScreeningEntryId = t.ScreeningVisit.ScreeningEntryId,
                     ScreeningVisitId = t.ScreeningVisitId,
                     ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                     Status = t.Status,
                     // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                     ScreeningTemplateName = t.ScreeningTemplateName,
                     DesignOrder = sequenseDeatils.IsTemplateSeqNo == true ? t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder.ToString() : t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString() : "",
                     DesignOrderForOrderBy = t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString()),
                     Progress = t.Progress ?? 0,
                     ReviewLevel = t.ReviewLevel,
                     IsLocked = t.IsLocked,
                     MyReview = false,
                     ParentId = t.ParentId,
                     ScheduleDate = t.ScheduleDate,
                     // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                     TemplateName = t.ScreeningTemplateName,
                     // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                     VisitName = t.ScreeningVisit.ScreeningVisitName,
                     SubjectName = t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                                         ? t.ScreeningVisit.ScreeningEntry.Randomization.Initial
                                         : t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,
                     PreLabel = PreLabelSetting(t, t.ProjectDesignTemplate, sequenseDeatils)
                 }).ToList().OrderBy(t => t.DesignOrderForOrderBy).ToList();


            return result;
        }


        public List<DataEntryTemplateCountDisplayTree> GetTemplateVisitWorkFlow(int screeningVisitId, short reviewLevel)
        {
            var details = _context.ScreeningTemplate.Where(s => s.ScreeningVisitId == screeningVisitId).Include(d => d.ScreeningVisit).ThenInclude(d => d.ScreeningEntry).FirstOrDefault();

            var sequenseDeatils = _templateVariableSequenceNoSettingRepository.All.Where(x => x.ProjectDesignId == details.ScreeningVisit.ScreeningEntry.ProjectDesignId && x.DeletedDate == null).FirstOrDefault();

            var result = _screeningTemplateRepository.All.Where(s => s.ScreeningVisitId == screeningVisitId && s.DeletedDate == null && s.ReviewLevel == reviewLevel)
                    .Select(t => new DataEntryTemplateCountDisplayTree
                    {
                        Id = t.Id,
                        ScreeningEntryId = t.ScreeningVisit.ScreeningEntryId,
                        ScreeningVisitId = t.ScreeningVisitId,
                        ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                        Status = t.Status,
                        // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                        ScreeningTemplateName = t.ScreeningTemplateName,
                        DesignOrder = sequenseDeatils.IsTemplateSeqNo == true ? t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder.ToString() : t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString() : "",
                        DesignOrderForOrderBy = t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString()),
                        Progress = t.Progress ?? 0,
                        ReviewLevel = t.ReviewLevel,
                        IsLocked = t.IsLocked,
                        MyReview = false,
                        ParentId = t.ParentId,
                        ScheduleDate = t.ScheduleDate,
                        // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                        TemplateName = t.ScreeningTemplateName,
                        // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                        VisitName = t.ScreeningVisit.ScreeningVisitName,
                        SubjectName = t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                                            ? t.ScreeningVisit.ScreeningEntry.Randomization.Initial
                                            : t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,
                        PreLabel = PreLabelSetting(t, t.ProjectDesignTemplate, sequenseDeatils)
                    }).ToList().OrderBy(t => t.DesignOrderForOrderBy).ToList();

            return result;

        }
        public List<DataEntryTemplateCountDisplayTree> GetMyTemplateView(int parentProjectId, int projectId)
        {
            var projectDesignId = _projectDesignRepository.All.Where(r => r.ProjectId == parentProjectId).Select(t => t.Id).FirstOrDefault();
            var sequenseDeatils = _templateVariableSequenceNoSettingRepository.All.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).FirstOrDefault();

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesignId);

            var result = _screeningTemplateRepository.All.Where(s => s.ScreeningVisit.ScreeningEntry.ProjectId == projectId && s.DeletedDate == null && s.ReviewLevel == workflowlevel.LevelNo)
                    .Select(t => new DataEntryTemplateCountDisplayTree
                    {
                        Id = t.Id,
                        ScreeningEntryId = t.ScreeningVisit.ScreeningEntryId,
                        SubjectNo = t.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber,
                        RandomizationNumber = t.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber,
                        ScreeningVisitId = t.ScreeningVisitId,
                        ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                        ProjectDesignPeriodId = t.ScreeningVisit.ScreeningEntry.ProjectDesignPeriodId,
                        Status = t.Status,
                        // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                        ScreeningTemplateName = t.ScreeningTemplateName,
                        DesignOrder = sequenseDeatils.IsTemplateSeqNo == true ? t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder.ToString() : t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString() : "",
                        DesignOrderForOrderBy = t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString()),
                        Progress = t.Progress ?? 0,
                        ReviewLevel = t.ReviewLevel,
                        IsLocked = t.IsLocked,
                        MyReview = false,
                        ParentId = t.ParentId,
                        ScheduleDate = t.ScheduleDate,
                        // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                        TemplateName = t.ScreeningTemplateName,
                        // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                        VisitName = t.ScreeningVisit.ScreeningVisitName + Convert.ToString(t.ScreeningVisit.RepeatedVisitNumber == null ? "" : "-" + t.ScreeningVisit.RepeatedVisitNumber),
                        SubjectName = t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                                            ? t.ScreeningVisit.ScreeningEntry.Randomization.Initial
                                            : t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName
                    }).ToList().OrderBy(t => t.DesignOrderForOrderBy).ToList();

            return result;
        }

        // Dashboard chart for data entry status
        public List<DashboardQueryStatusDto> GetDataEntriesStatus(int projectId)
        {
            var result = _screeningTemplateRepository.All.Where(x => (x.ScreeningVisit.ScreeningEntry.ProjectId == projectId ||
          x.ScreeningVisit.ScreeningEntry.Project.ParentProjectId == projectId) && (!x.ScreeningVisit.ScreeningEntry.Project.IsTestSite) && x.DeletedDate == null).GroupBy(
              t => new { t.ProjectDesignTemplate.ProjectDesignVisit.DisplayName, t.ProjectDesignTemplate.ProjectDesignVisit.Id }).Select(g => new DashboardQueryStatusDto
              {
                  DisplayName = g.Key.DisplayName,
                  Avg = Math.Round((g.Count() - g.Where(a => a.Status == ScreeningTemplateStatus.Pending).Count()) * 100d / g.Count(), 2),
                  // For order by visit id store visit id 
                  Total = g.Key.Id
              }).OrderBy(g => g.Total).ToList();
            return result;
        }

        private static string PreLabelSetting(ScreeningTemplate t, ProjectDesignTemplate pt, TemplateVariableSequenceNoSetting seq)
        {
            string str = "";
            if (!String.IsNullOrEmpty(pt.PreLabel))
                str = pt.PreLabel;

            if (!seq.IsTemplateSeqNo && t.RepeatSeqNo != null)
            {
                if (!String.IsNullOrEmpty(seq.RepeatPrefix))
                    str += ((!String.IsNullOrEmpty(pt.PreLabel)) ? seq.SeparateSign : "") + seq.RepeatPrefix;
                if (seq.RepeatSeqNo != null)
                {
                    if (seq.RepeatSubSeqNo == null)
                        str += ((!String.IsNullOrEmpty(seq.RepeatPrefix) || (!String.IsNullOrEmpty(pt.PreLabel))) ? seq.SeparateSign : "") + (seq.RepeatSeqNo + t.RepeatSeqNo.Value - 1).ToString();
                    else
                        str += ((!String.IsNullOrEmpty(seq.RepeatPrefix) || (!String.IsNullOrEmpty(pt.PreLabel))) ? seq.SeparateSign : "") + seq.RepeatSeqNo + seq.SeparateSign + (seq.RepeatSubSeqNo + t.RepeatSeqNo.Value - 1).ToString();
                }
            }
            return str;
        }


        public List<DataEntryVisitTemplateDto> GetVisitForDataEntryDropDown(int ScreeningEntryId)
        {
            var visits = _screeningVisitRepository.All.
              Where(r => r.ScreeningEntryId == ScreeningEntryId && r.DeletedDate == null).Select(a => new DataEntryVisitTemplateDto
              {
                  ScreeningVisitId = a.Id,
                  ProjectDesignVisitId = a.ProjectDesignVisitId,
                  VisitName = a.ScreeningVisitName + Convert.ToString(a.ParentId != null ? "-" + a.RepeatedVisitNumber.ToString() : ""),
                  VisitStatus = a.Status.GetDescription(),
                  VisitStatusId = (int)a.Status,
                  ActualDate = (int)a.Status > 3 ? a.VisitStartDate : null,
                  ScheduleDate = a.ScheduleDate,
                  IsSchedule = a.IsSchedule,
                  DesignOrder = a.ProjectDesignVisit.DesignOrder,
                  StudyVersion = a.ProjectDesignVisit.StudyVersion,
                  IsScheduleTerminate = a.IsScheduleTerminate,
                  ScreeningEntryId = a.ScreeningEntryId,
                  // added for visit order in data capture annd review create 04/06/2023
                  VisitSeqNo = a.RepeatedVisitNumber,
                  HideDisableType = a.HideDisableType,
                  ProjectDesignId = a.ScreeningEntry.ProjectDesignId,
                  IsPatientLevel = a.ProjectDesignVisit.IsPatientLevel
              }).OrderBy(o => o.DesignOrder).ThenBy(t => t.VisitSeqNo).ToList();

            visits = visits.Where(a => a.ScreeningEntryId == ScreeningEntryId && a.HideDisableType != HideDisableType.Hide &&
                 (!a.IsSchedule || a.IsScheduleTerminate == true || a.VisitStatusId > (int)ScreeningVisitStatus.NotStarted)).ToList();

            var hideTemplateIds = _projectDesingTemplateRestrictionRepository.All.Where(x => x.SecurityRoleId == _jwtTokenAccesser.RoleId && x.IsHide && x.DeletedDate == null).Select(r => r.ProjectDesignTemplateId).ToList();

            if (hideTemplateIds.Count() > 0)
            {
                var visitTemplates = _projectDesignVisitRepository.All.
                Where(x => x.DeletedDate == null && x.ProjectDesignPeriod.ProjectDesignId == visits.FirstOrDefault().ProjectDesignId).Select(r => new
                {
                    r.Id,
                    TemplatesIds = r.Templates.Where(c => c.DeletedDate == null).Select(a => a.Id).ToList()
                }).ToList();

                visits.ForEach(a =>
                {
                    var templatesIds = visitTemplates.Where(r => r.Id == a.ProjectDesignVisitId).Select(v => v.TemplatesIds).FirstOrDefault();
                    if (templatesIds != null && templatesIds.Where(r => !hideTemplateIds.Contains(r)).Count() < 1)
                        a.HideDisableType = HideDisableType.Hide;
                });
            }



            visits = visits.Where(x => x.HideDisableType != HideDisableType.Hide).ToList();

            if (visits.Any(x => x.HideDisableType == HideDisableType.Disable) && visits.Count != 0)
            {
                var editCheckVisit = _editCheckDetailRepository.GetProjectDesignVisitIds(visits.FirstOrDefault()?.ProjectDesignId ?? 0);
                visits.ForEach(x =>
                {
                    x.EditCheckMsg = editCheckVisit.FirstOrDefault(r => r.ProjectDesignVisitId == x.ProjectDesignVisitId && r.HideDisableType == HideDisableType.Disable)?.EditCheckMsg;
                });
            }
            return visits;
        }
    }
}