using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Workflow;
using GSC.Respository.ProjectRight;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class DataEntryRespository : GenericRespository<ScreeningEntry>, IDataEntryRespository
    {
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningEntryRepository _screeningEntryRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public DataEntryRespository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectRightRepository projectRightRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IRandomizationRepository randomizationRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IScreeningEntryRepository screeningEntryRepository,
            IProjectDesignRepository projectDesignRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository
        )
            : base(context)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectRightRepository = projectRightRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _randomizationRepository = randomizationRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _screeningEntryRepository = screeningEntryRepository;
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _projectDesignRepository = projectDesignRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public async Task<DataCaptureGridDto> GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int parentProjectId, int projectId)
        {
            var result = new DataCaptureGridDto();

            var projectDesignId = _projectDesignRepository.All.Where(r => r.ProjectId == parentProjectId).Select(t => t.Id).FirstOrDefault();

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesignId);
            result.WorkFlowText = workflowlevel.WorkFlowText;
            result.ReviewLevel = workflowlevel.LevelNo;
            result.IsStartTemplate = workflowlevel.IsStartTemplate;

            var projectDesignVisit = await _projectDesignVisitRepository.All.
                Where(x => x.DeletedDate == null && x.ProjectDesignPeriod.ProjectDesign.ProjectId == parentProjectId && x.IsSchedule != true).
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

            var randomizationData = await _randomizationRepository.All.Where(x => x.ProjectId == projectId && x.DeletedDate == null
             && x.ScreeningEntry == null).Select(t => new DataCaptureGridData
             {
                 RandomizationId = t.Id,
                 VolunteerName = t.Initial,
                 IsRandomization = true,
                 SubjectNo = t.ScreeningNumber,
                 PatientStatusId = t.PatientStatusId,
                 PatientStatusName = t.PatientStatusId.GetDescription(),
                 RandomizationNumber = t.RandomizationNumber,
                 StudyVersion = t.StudyVersion,
                 TemplateCount = result.WorkFlowText.Select(x => new WorkFlowTemplateCount
                 {
                     LevelNo = x.LevelNo
                 }).ToList()
             }).ToListAsync();

            var templates = await _screeningTemplateRepository.All.
                Where(r => r.ScreeningVisit.ScreeningEntry.ProjectId == projectId && !r.IsDisable && r.DeletedDate == null).
                GroupBy(c => new
                {
                    c.ScreeningVisit.ScreeningEntryId,
                    c.ScreeningVisitId,
                    c.ReviewLevel,
                    c.Status
                }).Select(t => new
                {
                    t.Key.ScreeningEntryId,
                    t.Key.Status,
                    t.Key.ScreeningVisitId,
                    t.Key.ReviewLevel,
                    TotalTemplate = t.Count()
                }).ToListAsync();

            var queries = await _screeningTemplateValueRepository.All.Where(r =>
            r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId &&
            r.ProjectDesignVariable.DeletedDate == null && r.DeletedDate == null && r.QueryStatus != QueryStatus.Closed).
                GroupBy(c => new
                {
                    c.ScreeningTemplate.ScreeningVisit.ScreeningEntryId,
                    c.ScreeningTemplate.ScreeningVisitId,
                    c.AcknowledgeLevel,
                    c.UserRoleId,
                    c.ReviewLevel,
                    c.QueryStatus
                }).Select(t => new
                {
                    t.Key.ScreeningEntryId,
                    t.Key.AcknowledgeLevel,
                    t.Key.ScreeningVisitId,
                    t.Key.ReviewLevel,
                    t.Key.UserRoleId,
                    t.Key.QueryStatus,
                    TotalQuery = t.Count()
                }).ToListAsync();

            var closeQueries = await _screeningTemplateValueQueryRepository.All.Where(r =>
            r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId &&
            r.QueryStatus == QueryStatus.Closed &&
            r.ScreeningTemplateValue.ProjectDesignVariable.DeletedDate == null && r.ScreeningTemplateValue.DeletedDate == null).
                GroupBy(c => new
                {
                    c.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntryId,
                    c.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisitId,

                }).Select(t => new
                {
                    t.Key.ScreeningEntryId,
                    t.Key.ScreeningVisitId,
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
                PatientStatusName = x.RandomizationId != null ? x.Randomization.PatientStatusId.GetDescription() : "",
                RandomizationNumber = x.RandomizationId != null ? x.Randomization.RandomizationNumber : "",
                Visit = x.ScreeningVisit.Where(t => t.DeletedDate == null && (!t.IsSchedule || t.Status > ScreeningVisitStatus.NotStarted)).Select(a => new DataEntryVisitTemplateDto
                {
                    ScreeningVisitId = a.Id,
                    ProjectDesignVisitId = a.ProjectDesignVisitId,
                    VisitName = a.ProjectDesignVisit.DisplayName + Convert.ToString(a.ParentId != null ? "-" + a.RepeatedVisitNumber.ToString() : ""),
                    VisitStatus = a.Status.GetDescription(),
                    VisitStatusId = (int)a.Status,
                    ActualDate = (int)a.Status > 3 ? a.VisitStartDate : null,
                    ScheduleDate = a.ScheduleDate,
                    IsSchedule = a.IsSchedule,
                    DesignOrder = a.ProjectDesignVisit.DesignOrder,
                    IsLocked = a.ScreeningTemplates.All(x => x.IsLocked == true) ? true : false
                }).OrderBy(b => b.DesignOrder).ToList()

            }).ToListAsync();

            //await Task.WhenAll(projectDesignVisitTask, randomizationDataTask, templateTask, queryTask, screeningDataTask);

            //var projectDesignVisit = await projectDesignVisitTask;
            //var randomizationData = await randomizationDataTask;
            //var templates = await templateTask;
            //var queries = await queryTask;
            //var screeningData = await screeningDataTask;

            randomizationData.ForEach(r =>
            {
                r.Visit = projectDesignVisit.Where(t => (t.StudyVersion == null || t.StudyVersion <= r.StudyVersion) &&
                        (t.InActiveVersion == null || t.InActiveVersion > r.StudyVersion)).ToList();

            });


            randomizationData.ForEach(r => r.Visit = projectDesignVisit.Where(t => (t.StudyVersion == null || t.StudyVersion <= r.StudyVersion) &&
            (t.InActiveVersion == null || t.InActiveVersion > r.StudyVersion)).ToList());

            screeningData.ForEach(r =>
            {
                r.Visit.ForEach(v =>
                {
                    if (v.VisitStatusId > 3)
                    {
                        v.NotStarted = templates.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.Status == ScreeningTemplateStatus.Pending).Sum(t => t.TotalTemplate);
                        v.InProgress = templates.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.Status == ScreeningTemplateStatus.InProcess).Sum(t => t.TotalTemplate);

                        v.MyQuery = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && (
                        x.AcknowledgeLevel == workflowlevel.LevelNo ||
                        ((x.QueryStatus == QueryStatus.Open || x.QueryStatus == QueryStatus.Reopened) && workflowlevel.LevelNo == 0 && workflowlevel.IsStartTemplate) ||
                        ((x.QueryStatus == QueryStatus.Resolved || x.QueryStatus == QueryStatus.Answered) && workflowlevel.LevelNo == 0 && x.UserRoleId == _jwtTokenAccesser.RoleId)
                        )).Sum(t => t.TotalQuery);

                        v.ReOpen = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Reopened).Sum(t => t.TotalQuery);
                        v.Open = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Open).Sum(t => t.TotalQuery);
                        v.Answered = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Answered).Sum(t => t.TotalQuery);
                        v.Resolved = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Resolved).Sum(t => t.TotalQuery);
                        v.Closed = closeQueries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId).Sum(t => t.TotalQuery);
                        v.SelfCorrection = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.SelfCorrection).Sum(t => t.TotalQuery);
                        v.Acknowledge = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.AcknowledgeLevel != x.ReviewLevel && (x.QueryStatus == QueryStatus.Resolved || x.QueryStatus == QueryStatus.SelfCorrection)).Sum(t => t.TotalQuery);
                        v.TemplateCount = result.WorkFlowText.Select(x => new WorkFlowTemplateCount
                        {
                            LevelNo = x.LevelNo,
                            Count = templates.Where(a => a.ScreeningEntryId == r.ScreeningEntryId && a.ScreeningVisitId == v.ScreeningVisitId && a.ReviewLevel == x.LevelNo).Sum(t => t.TotalTemplate)
                        }).ToList();
                    }
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

            return result;

        }

        public List<DataEntryTemplateCountDisplayTree> GetTemplateForVisit(int screeningVisitId, ScreeningTemplateStatus templateStatus)
        {
            var result = _screeningTemplateRepository.All.Where(s => s.ScreeningVisitId == screeningVisitId && s.DeletedDate == null && s.Status == templateStatus)
                 .Select(t => new DataEntryTemplateCountDisplayTree
                 {
                     Id = t.Id,
                     ScreeningEntryId = t.ScreeningVisit.ScreeningEntryId,
                     ScreeningVisitId = t.ScreeningVisitId,
                     ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                     Status = t.Status,
                     ProjectDesignTemplateName = t.ProjectDesignTemplate.TemplateName,
                     DesignOrder = t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString()),
                     Progress = t.Progress ?? 0,
                     ReviewLevel = t.ReviewLevel,
                     IsLocked = t.IsLocked,
                     MyReview = false,
                     ParentId = t.ParentId,
                     ScheduleDate = t.ScheduleDate,
                     TemplateName = t.ProjectDesignTemplate.TemplateName,
                     VisitName = t.ScreeningVisit.ProjectDesignVisit.DisplayName,
                     SubjectName = t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                                         ? t.ScreeningVisit.ScreeningEntry.Randomization.Initial
                                         : t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName
                 }).ToList().OrderBy(t => t.DesignOrder).ToList();


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

            var result = tempResult.Select(t => new DataEntryTemplateCountDisplayTree
            {
                Id = t.Id,
                ScreeningEntryId = t.ScreeningVisit.ScreeningEntryId,
                ScreeningVisitId = t.ScreeningVisitId,
                ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                Status = t.Status,
                ProjectDesignTemplateName = t.ProjectDesignTemplate.TemplateName,
                DesignOrder = t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString()),
                Progress = t.Progress ?? 0,
                ReviewLevel = t.ReviewLevel,
                IsLocked = t.IsLocked,
                MyReview = false,
                ParentId = t.ParentId,
                ScheduleDate = t.ScheduleDate,
                TemplateName = t.ProjectDesignTemplate.TemplateName,
                VisitName = t.ScreeningVisit.ProjectDesignVisit.DisplayName,
                SubjectName = t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                                         ? t.ScreeningVisit.ScreeningEntry.Randomization.Initial
                                         : t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName
            }).ToList().OrderBy(t => t.DesignOrder).ToList();


            return result;

        }

        public List<DataEntryTemplateCountDisplayTree> GetTemplateVisitMyQuery(int screeningVisitId, int parentProjectId)
        {
            var projectDesignId = _projectDesignRepository.All.Where(r => r.ProjectId == parentProjectId).Select(t => t.Id).FirstOrDefault();

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesignId);

            var result = _screeningTemplateRepository.All.Where(s => s.ScreeningVisitId == screeningVisitId && s.DeletedDate == null
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
                     ProjectDesignTemplateName = t.ProjectDesignTemplate.TemplateName,
                     DesignOrder = t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString()),
                     Progress = t.Progress ?? 0,
                     ReviewLevel = t.ReviewLevel,
                     IsLocked = t.IsLocked,
                     MyReview = false,
                     ParentId = t.ParentId,
                     ScheduleDate = t.ScheduleDate,
                     TemplateName = t.ProjectDesignTemplate.TemplateName,
                     VisitName = t.ScreeningVisit.ProjectDesignVisit.DisplayName,
                     SubjectName = t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                                         ? t.ScreeningVisit.ScreeningEntry.Randomization.Initial
                                         : t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName
                 }).ToList().OrderBy(t => t.DesignOrder).ToList();


            return result;


        }


        public List<DataEntryTemplateCountDisplayTree> GetTemplateVisitWorkFlow(int screeningVisitId, short reviewLevel)
        {
            var result = _screeningTemplateRepository.All.Where(s => s.ScreeningVisitId == screeningVisitId && s.DeletedDate == null && s.ReviewLevel == reviewLevel)
                    .Select(t => new DataEntryTemplateCountDisplayTree
                    {
                        Id = t.Id,
                        ScreeningEntryId = t.ScreeningVisit.ScreeningEntryId,
                        ScreeningVisitId = t.ScreeningVisitId,
                        ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                        Status = t.Status,
                        ProjectDesignTemplateName = t.ProjectDesignTemplate.TemplateName,
                        DesignOrder = t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString()),
                        Progress = t.Progress ?? 0,
                        ReviewLevel = t.ReviewLevel,
                        IsLocked = t.IsLocked,
                        MyReview = false,
                        ParentId = t.ParentId,
                        ScheduleDate = t.ScheduleDate,
                        TemplateName = t.ProjectDesignTemplate.TemplateName,
                        VisitName = t.ScreeningVisit.ProjectDesignVisit.DisplayName,
                        SubjectName = t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                                            ? t.ScreeningVisit.ScreeningEntry.Randomization.Initial
                                            : t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName
                    }).ToList().OrderBy(t => t.DesignOrder).ToList();


            return result;


        }


        public List<DataEntryTemplateCountDisplayTree> GetMyTemplateView(int parentProjectId, int projectId)
        {
            var projectDesignId = _projectDesignRepository.All.Where(r => r.ProjectId == parentProjectId).Select(t => t.Id).FirstOrDefault();

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
                        ProjectDesignTemplateName = t.ProjectDesignTemplate.TemplateName,
                        DesignOrder = t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString()),
                        Progress = t.Progress ?? 0,
                        ReviewLevel = t.ReviewLevel,
                        IsLocked = t.IsLocked,
                        MyReview = false,
                        ParentId = t.ParentId,
                        ScheduleDate = t.ScheduleDate,
                        TemplateName = t.ProjectDesignTemplate.TemplateName,
                        VisitName = t.ScreeningVisit.ProjectDesignVisit.DisplayName + Convert.ToString(t.ScreeningVisit.RepeatedVisitNumber == null ? "" : "-" + t.ScreeningVisit.RepeatedVisitNumber),
                        SubjectName = t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                                            ? t.ScreeningVisit.ScreeningEntry.Randomization.Initial
                                            : t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName
                    }).ToList().OrderBy(t => t.DesignOrder).ToList();


            return result;


        }

        // Dashboard chart for data entry status
        public List<DashboardQueryStatusDto> GetDataEntriesStatus(int projectId)
        {
            // Formula % = (OpenVisitTemplate-NotStartedTemplate)/OpenVisitTemplate*100;
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
    }
}