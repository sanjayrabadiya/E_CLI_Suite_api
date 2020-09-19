using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Project.Workflow;
using GSC.Respository.ProjectRight;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class DataEntryRespository : GenericRespository<ScreeningEntry, GscContext>, IDataEntryRespository
    {
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;

        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;

        public DataEntryRespository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectRightRepository projectRightRepository,
            IProjectWorkflowRepository projectWorkflowRepository
        )
            : base(uow, jwtTokenAccesser)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectRightRepository = projectRightRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
        }



        public IList<DataCaptureGridDto> GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int projectId)
        {
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

            return null;



        }

        public List<DataEntryVisitSummaryDto> GetVisitForDataEntry(int attendanceId, int screeningEntryId)
        {
            if (screeningEntryId == 0)
            {
                var projectDesignPeriodId = Context.Attendance.Where(x => x.Id == attendanceId).Select(r => r.ProjectDesignPeriodId).FirstOrDefault();
                return Context.ProjectDesignVisit.Where(t =>
                        t.ProjectDesignPeriodId == projectDesignPeriodId
                        && t.DeletedDate == null).Select(x => new DataEntryVisitSummaryDto
                        {
                            VisitName = x.DisplayName,
                            ScreeningVisitId = x.Id,
                            RecordId = attendanceId,
                            PendingCount = x.Templates.Where(v => v.DeletedDate == null).Count()
                        }).OrderBy(x => x.ScreeningVisitId).ToList();
            }
            else
            {
                return Context.ScreeningTemplate.Where(t =>
                        t.ScreeningVisit.ScreeningEntryId == screeningEntryId
                        && t.DeletedDate == null).GroupBy(r => new
                        {
                            r.ScreeningVisit.ProjectDesignVisit.DisplayName,
                            ScreeningVisitId = r.Id,
                            r.ScreeningVisit.RepeatedVisitNumber
                        }).Select(x => new DataEntryVisitSummaryDto
                        {
                            VisitName = x.Key.DisplayName + Convert.ToString(x.Key.RepeatedVisitNumber == null ? "" : "_" + x.Key.RepeatedVisitNumber),
                            ScreeningVisitId = x.Key.ScreeningVisitId,
                            RecordId = screeningEntryId,
                            PendingCount = x.Count(r => r.Status == ScreeningTemplateStatus.Pending),
                            InProcess = x.Count(r => r.Status == ScreeningTemplateStatus.InProcess),
                            Submitted = x.Count(r => r.Status == ScreeningTemplateStatus.Submitted),
                            Reviewed = x.Count(r => r.Status == ScreeningTemplateStatus.Reviewed),
                            Completed = x.Count(r => r.Status == ScreeningTemplateStatus.Completed),
                            TotalQueries = _screeningTemplateValueRepository.GetQueryCountByVisitId(x.Key.ScreeningVisitId)
                        }).OrderBy(x => x.VisitName).ToList();
            }
        }

    }
}