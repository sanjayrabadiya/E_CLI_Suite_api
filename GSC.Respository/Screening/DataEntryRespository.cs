using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Workflow;
using GSC.Respository.ProjectRight;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class DataEntryRespository : GenericRespository<ScreeningEntry, GscContext>, IDataEntryRespository
    {
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IProjectWorkflowLevelRepository _projectWorkflowLevelRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        private readonly IScreeningEntryRepository _screeningEntryRepository;
        public DataEntryRespository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectRightRepository projectRightRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IProjectWorkflowLevelRepository projectWorkflowLevelRepository,
            IRandomizationRepository randomizationRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IScreeningVisitRepository screeningVisitRepository,
            IScreeningEntryRepository screeningEntryRepository
        )
            : base(uow, jwtTokenAccesser)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectRightRepository = projectRightRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _projectWorkflowLevelRepository = projectWorkflowLevelRepository;
            _randomizationRepository = randomizationRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _screeningVisitRepository = screeningVisitRepository;
            _screeningEntryRepository = screeningEntryRepository;
        }



        public DataCaptureGridDto GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int parentProjectId, int projectId)
        {
            var result = new DataCaptureGridDto();
            result.WorkFlowText = _projectWorkflowLevelRepository.All.
                Where(x => x.ProjectWorkflow.ProjectDesign.ProjectId == parentProjectId
                        && x.DeletedDate == null && x.ProjectWorkflow.DeletedDate == null).Select(r => new WorkFlowText
                        {
                            LevelNo = r.LevelNo,
                            RoleName = r.SecurityRole.RoleShortName
                        }).ToList();

            var projectDesignVisit = _projectDesignVisitRepository.All.
                Where(x => x.ProjectDesignPeriod.ProjectDesign.ProjectId == parentProjectId && x.IsSchedule != true).
            Select(t => new DataEntryVisitTemplateDto
            {
                ProjectDesignVisitId= t.Id,
                VisitName = t.DisplayName,
                VisitStatus = ScreeningVisitStatus.NotStarted.GetDescription(),
                VisitStatusId = (int)ScreeningVisitStatus.NotStarted
            }).ToList();

            var queryList = _screeningTemplateValueRepository.GetQueryStatusByPeridId(projectDesignPeriodId);
            var test = new List<WorkFlowTemplateCount>();
            test.Add(new WorkFlowTemplateCount { Count = 1, LevelNo = 1 });

            var randomizationData = _randomizationRepository.All.Where(x => x.ProjectId == projectId && x.DeletedDate == null
             && x.PatientStatusId == ScreeningPatientStatus.Screening).Select(t => new DataCaptureGridData
             {
                 RandomizationId = t.Id,
                 VolunteerName = t.Initial,
                 IsRandomization = true,
                 SubjectNo = t.ScreeningNumber,
                 PatientStatus = t.PatientStatusId.GetDescription(),
                 RandomizationNumber = t.RandomizationNumber,
                 TemplateCount = test,
                 Visit = projectDesignVisit,
             }).ToList();

            var screeningData = _screeningEntryRepository.All.Where(r => r.ProjectId == projectId && r.DeletedDate == null).Select(x => new DataCaptureGridData
            {
                ScreeningEntryId= x.Id,
                RandomizationId = x.RandomizationId,
                AttendanceId= x.AttendanceId,
                VolunteerName = x.RandomizationId != null ? x.Randomization.Initial : x.Attendance.Volunteer.AliasName,
                IsRandomization = x.RandomizationId != null,
                SubjectNo = x.RandomizationId != null ? x.Randomization.ScreeningNumber : x.Attendance.Volunteer.VolunteerNo,
                PatientStatus = x.RandomizationId != null ? x.Randomization.PatientStatusId.GetDescription() : "",
                RandomizationNumber = x.RandomizationId != null ?  x.Randomization.RandomizationNumber : "",
                TemplateCount = test,
                Visit = x.ScreeningVisit.Where(t => t.DeletedDate == null).Select(a => new DataEntryVisitTemplateDto
                {
                    ScreeningVisitId= a.Id,
                    ProjectDesignVisitId= a.ProjectDesignVisitId,
                    VisitName =a.ProjectDesignVisit.DisplayName,
                    VisitStatus = a.Status.GetDescription(),
                    VisitStatusId = (int)a.Status,
                    ActualDate=a.VisitStartDate,
                    ScheduleDate=a.ScheduleDate,
                    NotStarted = a.ScreeningTemplates.Count(c => c.DeletedDate == null && c.Status == ScreeningTemplateStatus.Pending),
                    InProgress = a.ScreeningTemplates.Count(c => c.DeletedDate == null && c.Status == ScreeningTemplateStatus.InProcess)
                }).ToList()

            }).ToList();

            #region comment old code
            //var projectIds = _projectRightRepository.GetProjectRightIdList();
            //if (!projectIds.Any()) return new List<DataEntryDto>();
            //var attendances = Context.Attendance.Where(t =>
            //        t.ProjectDesignPeriodId == projectDesignPeriodId
            //        && t.DeletedDate == null
            //        && t.ProjectId == projectId
            //        && !t.IsProcessed
            //        && t.AttendanceType != AttendanceType.Screening
            //        && projectIds.Any(p => p == t.ProjectId))
            //    .Include(t => t.Volunteer)
            //    .Include(t => t.Randomization)
            //    .Include(t => t.ProjectSubject)
            //    .Include(t => t.ProjectDesignPeriod)
            //    .ThenInclude(t => t.VisitList)
            //    .ThenInclude(t => t.Templates)
            //    .ToList();
            //var dataEntries = new List<DataEntryDto>();
            //attendances.ForEach(t =>
            //{
            //    var dataEntry = new DataEntryDto
            //    {
            //        AttendanceId = t.Id,
            //        ProjectDesignId = t.ProjectDesignPeriod.ProjectDesignId,
            //        ProjectId = t.ProjectId,
            //        ProjectDesignPeriodId = t.ProjectDesignPeriodId,
            //        VolunteerName = t.Volunteer == null ? t.Randomization.Initial : t.Volunteer.AliasName,
            //        SubjectNo = t.Volunteer == null ? t.Randomization.ScreeningNumber : t.Volunteer.VolunteerNo,
            //        RandomizationNumber =
            //            t.Volunteer == null ? t.Randomization.RandomizationNumber : t.ProjectSubject?.Number,
            //        VisitSummary = new DashboardStudyStatusDto
            //        {
            //            NotStarted = t.ProjectDesignPeriod.VisitList.Where(d => d.DeletedDate == null)
            //                .Sum(b => b.Templates.Where(h => h.DeletedDate == null).Count()),
            //            InProcess = 0,
            //            Review1 = 0,
            //            Review2 = 0,
            //            Review3 = 0,
            //            Review4 = 0,
            //            Review5 = 0
            //        },
            //        QueryStatus = new DashboardQueryStatusDto
            //        {
            //            Open = 0,
            //            Answered = 0,
            //            Resolved = 0,
            //            ReOpened = 0,
            //            Closed = 0,
            //            SelfCorrection = 0,
            //            Acknowledge = 0,
            //            MyQuery = 0
            //        }
            //    };
            //    dataEntries.Add(dataEntry);
            //});
            //var screenings = Context.ScreeningEntry.Where(t =>
            //        t.ProjectDesignPeriodId == projectDesignPeriodId
            //        && t.ProjectId == projectId
            //        && t.DeletedDate == null
            //        && t.EntryType != AttendanceType.Screening
            //        && projectIds.Any(p => p == t.ProjectId))
            //    .Include(t => t.ProjectDesignPeriod)
            //    .ThenInclude(t => t.VisitList)
            //    .Include(t => t.Attendance)
            //    .ThenInclude(t => t.Volunteer)
            //    .Include(t => t.Attendance)
            //    .ThenInclude(t => t.Randomization)
            //    .Include(t => t.Attendance)
            //    .ThenInclude(t => t.ProjectSubject)
            //    .Include(t => t.ScreeningVisit)
            //    .ThenInclude(t => t.ScreeningTemplates)
            //    .ThenInclude(t => t.ProjectDesignTemplate)
            //     .Include(t => t.ScreeningVisit)
            //    .ThenInclude(t => t.ScreeningTemplates)
            //    .ThenInclude(t => t.ScreeningTemplateValues)
            //    .ToList();
            //if (screenings.Count > 0)
            //{
            //    var queryList = _screeningTemplateValueRepository.GetQueryStatusByPeridId(projectDesignPeriodId);
            //    var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(screenings[0].ProjectDesignId);
            //    screenings.ForEach(screening =>
            //    {
            //        var dataEntry = new DataEntryDto
            //        {
            //            WorkflowDetail = workflowlevel,
            //            AttendanceId = screening.Id,
            //            ProjectDesignId = screening.ProjectDesignId,
            //            ProjectId = screening.ProjectId,
            //            ProjectDesignPeriodId = screening.ProjectDesignPeriodId,
            //            VolunteerName = screening.Attendance.Volunteer == null
            //                ? screening.Attendance.Randomization.Initial
            //                : screening.Attendance.Volunteer.AliasName,
            //            SubjectNo = screening.Attendance.Volunteer == null
            //                ? screening.Attendance.Randomization.ScreeningNumber
            //                : screening.Attendance.Volunteer.VolunteerNo,
            //            RandomizationNumber = screening.Attendance.Volunteer == null
            //                ? screening.Attendance.Randomization.RandomizationNumber
            //                : screening.Attendance.ProjectSubject?.Number,
            //            ScreeningEntryId = screening.Id,
            //            //VisitSummary = screening.ScreeningTemplates.GroupBy(s => new
            //            //{
            //            //    s.ScreeningEntryId
            //            //}).Select(x => new DashboardStudyStatusDto
            //            //{
            //            //    Review1 = x.Count(r => (int?)r.ReviewLevel == 1),
            //            //    Review2 = x.Count(r => (int?)r.ReviewLevel == 2),
            //            //    Review3 = x.Count(r => (int?)r.ReviewLevel == 3),
            //            //    Review4 = x.Count(r => (int?)r.ReviewLevel == 4),
            //            //    Review5 = x.Count(r => (int?)r.ReviewLevel == 5)
            //            //}).FirstOrDefault(),
            //            QueryStatus =
            //                queryList.Where(r => r.ScreeningEntryId == screening.Id).FirstOrDefault() == null ||
            //                queryList.Count() == 0
            //                    ? new DashboardQueryStatusDto
            //                    {
            //                        Open = 0,
            //                        Answered = 0,
            //                        Resolved = 0,
            //                        ReOpened = 0,
            //                        Closed = 0,
            //                        SelfCorrection = 0,
            //                        Acknowledge = 0,
            //                        MyQuery = 0
            //                    }
            //                    : new DashboardQueryStatusDto
            //                    {
            //                        Open = queryList.Count(x =>
            //                            x.ScreeningEntryId == screening.Id && x.QueryStatus == QueryStatus.Open),
            //                        Answered = queryList.Count(x =>
            //                            x.ScreeningEntryId == screening.Id && x.QueryStatus == QueryStatus.Answered),
            //                        Resolved = queryList.Count(x =>
            //                            x.ScreeningEntryId == screening.Id && x.QueryStatus == QueryStatus.Resolved),
            //                        ReOpened = queryList.Count(x =>
            //                            x.ScreeningEntryId == screening.Id && x.QueryStatus == QueryStatus.Reopened),
            //                        Closed = queryList.Count(x =>
            //                            x.ScreeningEntryId == screening.Id && x.QueryStatus == QueryStatus.Closed),
            //                        SelfCorrection = queryList.Count(x =>
            //                            x.ScreeningEntryId == screening.Id &&
            //                            x.QueryStatus == QueryStatus.SelfCorrection),
            //                        Acknowledge = queryList.Count(x =>
            //                            x.ScreeningEntryId == screening.Id && x.QueryStatus == QueryStatus.Resolved),
            //                        MyQuery = queryList.Count(x =>
            //                            x.ScreeningEntryId == screening.Id &&
            //                            x.AcknowledgeLevel == workflowlevel.LevelNo)
            //                    }
            //        };
            //        dataEntries.Add(dataEntry);
            //    });
            //}
            #endregion

            result.Data.AddRange(randomizationData);
            result.Data.AddRange(screeningData);

            return result;

        }

        public List<DataEntryVisitTemplateDto> GetTemplateForVisit(int screeningEntryId, int projectDesignVisitId,
            int screeningStatus, bool isQuery)
        {

            return new List<DataEntryVisitTemplateDto>();

            //if (isQuery)
            //    return Context.ScreeningTemplate.Where(t =>
            //        t.ScreeningEntryId == screeningEntryId
            //        && t.ProjectDesignVisitId == projectDesignVisitId
            //        && t.ScreeningTemplateValues.Any(c => c.QueryStatus != null && c.QueryStatus != QueryStatus.Closed)
            //        && t.DeletedDate == null).Select(x => new DataEntryVisitTemplateDto
            //        {
            //            ScreeningEntryId = x.ScreeningEntryId,
            //            ScreeningTemplateId = x.Id,
            //            ProjectDesignTemplateId = x.ProjectDesignTemplateId,
            //            TemplateName = x.ProjectDesignTemplate.TemplateName,
            //            VisitName = x.ProjectDesignVisit.DisplayName,
            //            SubjectName = x.ScreeningEntry.Attendance.Volunteer == null
            //                ? x.ScreeningEntry.Attendance.NoneRegister.Initial
            //                : x.ScreeningEntry.Attendance.Volunteer.AliasName
            //        }
            //    ).ToList();
            //return Context.ScreeningTemplate.Where(t =>
            //    t.ScreeningEntryId == screeningEntryId
            //    && t.ProjectDesignVisitId == projectDesignVisitId
            //    && t.Status == screeningStatus
            //    && t.DeletedDate == null).Select(x => new DataEntryVisitTemplateDto
            //    {
            //        ScreeningEntryId = x.ScreeningEntryId,
            //        ScreeningTemplateId = x.Id,
            //        ProjectDesignTemplateId = x.ProjectDesignTemplateId,
            //        TemplateName = x.ProjectDesignTemplate.TemplateName,
            //        VisitName = x.ProjectDesignVisit.DisplayName,
            //        SubjectName = x.ScreeningEntry.Attendance.Volunteer == null
            //            ? x.ScreeningEntry.Attendance.NoneRegister.Initial
            //            : x.ScreeningEntry.Attendance.Volunteer.AliasName
            //    }
            //).ToList();
        }


    }
}