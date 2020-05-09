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


        public IList<DataEntryDto> GetDataEntriesBySubject(int projectDesignPeriodId, int projectId)
        {
            var projectIds = _projectRightRepository.GetProjectRightIdList();
            if (!projectIds.Any()) return new List<DataEntryDto>();
            var attendances = Context.Attendance.Where(t =>
                    t.ProjectDesignPeriodId == projectDesignPeriodId
                    && t.DeletedDate == null
                    && t.ProjectId == projectId
                    && !t.IsProcessed
                    && t.AttendanceType != AttendanceType.Screening
                    && projectIds.Any(p => p == t.ProjectId))
                .Include(t => t.Volunteer)
                .Include(t => t.NoneRegister)
                .Include(t => t.ProjectSubject)
                .Include(t => t.ProjectDesignPeriod)
                .ToList();

            var dataEntries = new List<DataEntryDto>();
            attendances.ForEach(t =>
            {
                var dataEntry = new DataEntryDto
                {
                    AttendanceId = t.Id,
                    ProjectDesignId = t.ProjectDesignPeriod.ProjectDesignId,
                    ProjectId = t.ProjectId,
                    ProjectDesignPeriodId = t.ProjectDesignPeriodId,
                    VolunteerName = t.Volunteer == null ? t.NoneRegister.Initial : t.Volunteer.AliasName,
                    SubjectNo = t.Volunteer == null ? t.NoneRegister.ScreeningNumber : t.Volunteer.VolunteerNo,
                    RandomizationNumber = t.Volunteer == null
                        ? t.NoneRegister.RandomizationNumber
                        : t.ProjectSubject?.Number
                };

                dataEntries.Add(dataEntry);
            });

            var screenings = Context.ScreeningEntry.Where(t =>
                    t.ProjectDesignPeriodId == projectDesignPeriodId
                    && t.ProjectId == projectId
                    && t.DeletedDate == null
                    && t.EntryType != AttendanceType.Screening
                    && projectIds.Any(p => p == t.ProjectId))
                .Include(t => t.ProjectDesignPeriod)
                .ThenInclude(t => t.VisitList)
                .Include(t => t.Attendance)
                .ThenInclude(t => t.Volunteer)
                .Include(t => t.Attendance)
                .ThenInclude(t => t.NoneRegister)
                .Include(t => t.Attendance)
                .ToList();

            if (screenings.Count > 0)
                screenings.ForEach(screening =>
                {
                    var dataEntry = new DataEntryDto
                    {
                        AttendanceId = screening.Id,
                        ProjectDesignId = screening.ProjectDesignId,
                        ProjectId = screening.ProjectId,
                        ProjectDesignPeriodId = screening.ProjectDesignPeriodId,
                        VolunteerName = screening.Attendance.Volunteer == null
                            ? screening.Attendance.NoneRegister.Initial
                            : screening.Attendance.Volunteer.AliasName,
                        SubjectNo = screening.Attendance.Volunteer == null
                            ? screening.Attendance.NoneRegister.ScreeningNumber
                            : screening.Attendance.Volunteer.VolunteerNo,
                        RandomizationNumber = screening.Attendance.Volunteer == null
                            ? screening.Attendance.NoneRegister.RandomizationNumber
                            : screening.Attendance.ProjectSubject?.Number,
                        ScreeningEntryId = screening.Id
                    };

                    dataEntries.Add(dataEntry);
                });

            return dataEntries.OrderBy(t => t.SubjectNo).ToList();
        }

        public List<DataEntryVisitSummaryDto> GetVisitForDataEntry(int attendanceId, int screeningEntryId)
        {
            if (screeningEntryId == 0)
            {
                var projectDesignPeriodId = Context.Attendance.Find(attendanceId).ProjectDesignPeriodId;
                var attendances = Context.ProjectDesignVisit.Where(t =>
                        t.ProjectDesignPeriodId == projectDesignPeriodId
                        && t.DeletedDate == null)
                    .Include(t => t.Templates);
                return attendances.Select(x => new DataEntryVisitSummaryDto
                {
                    VisitName = x.DisplayName,
                    ProjectDesignVisitId = x.Id,
                    RecordId = attendanceId,
                    PendingCount = x.Templates.Where(v => v.DeletedDate == null).Count()
                }).OrderBy(x => x.ProjectDesignVisitId).ToList();
            }
            else
            {
                var attendances = Context.ScreeningTemplate.Where(t =>
                        t.ScreeningEntryId == screeningEntryId
                        && t.DeletedDate == null)
                    .Include(t => t.ProjectDesignVisit)
                    .Include(t => t.ScreeningTemplateValues)
                    .ToList();
                return attendances.GroupBy(r => new
                {
                    r.ProjectDesignVisit.DisplayName,
                    r.ProjectDesignVisitId
                }).Select(x => new DataEntryVisitSummaryDto
                {
                    VisitName = x.Key.DisplayName,
                    ProjectDesignVisitId = x.Key.ProjectDesignVisitId,
                    RecordId = screeningEntryId,
                    PendingCount = x.Count(r => r.Status == ScreeningStatus.Pending),
                    InProcess = x.Count(r => r.Status == ScreeningStatus.InProcess),
                    Submitted = x.Count(r => r.Status == ScreeningStatus.Submitted),
                    Reviewed = x.Count(r => r.Status == ScreeningStatus.Reviewed),
                    Completed = x.Count(r => r.Status == ScreeningStatus.Completed),
                    TotalQueries =
                        _screeningTemplateValueRepository.GetQueryCountByVisitId(x.Key.ProjectDesignVisitId,
                            screeningEntryId)
                }).OrderBy(x => x.ProjectDesignVisitId).ToList();
            }
        }

        public IList<DataEntryDto> GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int projectId)
        {
            var projectIds = _projectRightRepository.GetProjectRightIdList();
            if (!projectIds.Any()) return new List<DataEntryDto>();
            var attendances = Context.Attendance.Where(t =>
                    t.ProjectDesignPeriodId == projectDesignPeriodId
                    && t.DeletedDate == null
                    && t.ProjectId == projectId
                    && !t.IsProcessed
                    && t.AttendanceType != AttendanceType.Screening
                    && projectIds.Any(p => p == t.ProjectId))
                .Include(t => t.Volunteer)
                .Include(t => t.NoneRegister)
                .Include(t => t.ProjectSubject)
                .Include(t => t.ProjectDesignPeriod)
                .ThenInclude(t => t.VisitList)
                .ThenInclude(t => t.Templates)
                .ToList();

            var dataEntries = new List<DataEntryDto>();
            attendances.ForEach(t =>
            {
                var dataEntry = new DataEntryDto
                {
                    AttendanceId = t.Id,
                    ProjectDesignId = t.ProjectDesignPeriod.ProjectDesignId,
                    ProjectId = t.ProjectId,
                    ProjectDesignPeriodId = t.ProjectDesignPeriodId,
                    VolunteerName = t.Volunteer == null ? t.NoneRegister.Initial : t.Volunteer.AliasName,
                    SubjectNo = t.Volunteer == null ? t.NoneRegister.ScreeningNumber : t.Volunteer.VolunteerNo,
                    RandomizationNumber =
                        t.Volunteer == null ? t.NoneRegister.RandomizationNumber : t.ProjectSubject?.Number,
                    //VisitSummary = t.ProjectDesignPeriod.VisitList.Where(r => r.DeletedDate == null).Select(v => new DashboardStudyStatusDto
                    //{
                    //    NotStarted = v.Templates.Where(x => x.DeletedDate == null).Count(),
                    //    InProcess = 0,
                    //    Review1 = 0,
                    //    Review2 = 0,
                    //    Review3 = 0,
                    //    Review4 = 0,
                    //    Review5 = 0
                    //}).FirstOrDefault(),

                    VisitSummary = new DashboardStudyStatusDto
                    {
                        NotStarted = t.ProjectDesignPeriod.VisitList.Where(d => d.DeletedDate == null)
                            .Sum(b => b.Templates.Where(h => h.DeletedDate == null).Count()),
                        InProcess = 0,
                        Review1 = 0,
                        Review2 = 0,
                        Review3 = 0,
                        Review4 = 0,
                        Review5 = 0
                    },
                    QueryStatus = new DashboardQueryStatusDto
                    {
                        Open = 0,
                        Answered = 0,
                        Resolved = 0,
                        ReOpened = 0,
                        Closed = 0,
                        SelfCorrection = 0,
                        Acknowledge = 0,
                        MyQuery = 0
                    }
                };

                dataEntries.Add(dataEntry);
            });

            var screenings = Context.ScreeningEntry.Where(t =>
                    t.ProjectDesignPeriodId == projectDesignPeriodId
                    && t.ProjectId == projectId
                    && t.DeletedDate == null
                    && t.EntryType != AttendanceType.Screening
                    && projectIds.Any(p => p == t.ProjectId))
                .Include(t => t.ProjectDesignPeriod)
                .ThenInclude(t => t.VisitList)
                .Include(t => t.Attendance)
                .ThenInclude(t => t.Volunteer)
                .Include(t => t.Attendance)
                .ThenInclude(t => t.NoneRegister)
                .Include(t => t.Attendance)
                .ThenInclude(t => t.ProjectSubject)
                .Include(t => t.ScreeningTemplates)
                .ThenInclude(t => t.ProjectDesignTemplate)
                .Include(t => t.ScreeningTemplates)
                .ThenInclude(t => t.ScreeningTemplateValues)
                .ToList();

            if (screenings.Count > 0)
            {
                var queryList = _screeningTemplateValueRepository.GetQueryStatusByPeridId(projectDesignPeriodId);
                var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(screenings[0].ProjectDesignId);
                screenings.ForEach(screening =>
                {
                    var dataEntry = new DataEntryDto
                    {
                        WorkflowDetail = workflowlevel,
                        AttendanceId = screening.Id,
                        ProjectDesignId = screening.ProjectDesignId,
                        ProjectId = screening.ProjectId,
                        ProjectDesignPeriodId = screening.ProjectDesignPeriodId,
                        VolunteerName = screening.Attendance.Volunteer == null
                            ? screening.Attendance.NoneRegister.Initial
                            : screening.Attendance.Volunteer.AliasName,
                        SubjectNo = screening.Attendance.Volunteer == null
                            ? screening.Attendance.NoneRegister.ScreeningNumber
                            : screening.Attendance.Volunteer.VolunteerNo,
                        RandomizationNumber = screening.Attendance.Volunteer == null
                            ? screening.Attendance.NoneRegister.RandomizationNumber
                            : screening.Attendance.ProjectSubject?.Number,
                        ScreeningEntryId = screening.Id,
                        VisitSummary = screening.ScreeningTemplates.GroupBy(s => new
                        {
                            s.ScreeningEntryId
                        }).Select(x => new DashboardStudyStatusDto
                        {
                            InProcess = x.Count(r => r.Status == ScreeningStatus.InProcess),
                            NotStarted = x.Count(r => r.Status == ScreeningStatus.Pending),
                            Review1 = x.Count(r => (int?)r.ReviewLevel == 1),
                            Review2 = x.Count(r => (int?)r.ReviewLevel == 2),
                            Review3 = x.Count(r => (int?)r.ReviewLevel == 3),
                            Review4 = x.Count(r => (int?)r.ReviewLevel == 4),
                            Review5 = x.Count(r => (int?)r.ReviewLevel == 5)
                        }).FirstOrDefault(),
                        QueryStatus =
                            queryList.Where(r => r.ScreeningEntryId == screening.Id).FirstOrDefault() == null ||
                            queryList.Count() == 0
                                ? new DashboardQueryStatusDto
                                {
                                    Open = 0,
                                    Answered = 0,
                                    Resolved = 0,
                                    ReOpened = 0,
                                    Closed = 0,
                                    SelfCorrection = 0,
                                    Acknowledge = 0,
                                    MyQuery = 0
                                }
                                : new DashboardQueryStatusDto
                                {
                                    Open = queryList.Count(x =>
                                        x.ScreeningEntryId == screening.Id && x.QueryStatus == QueryStatus.Open),
                                    Answered = queryList.Count(x =>
                                        x.ScreeningEntryId == screening.Id && x.QueryStatus == QueryStatus.Answered),
                                    Resolved = queryList.Count(x =>
                                        x.ScreeningEntryId == screening.Id && x.QueryStatus == QueryStatus.Resolved),
                                    ReOpened = queryList.Count(x =>
                                        x.ScreeningEntryId == screening.Id && x.QueryStatus == QueryStatus.Reopened),
                                    Closed = queryList.Count(x =>
                                        x.ScreeningEntryId == screening.Id && x.QueryStatus == QueryStatus.Closed),
                                    SelfCorrection = queryList.Count(x =>
                                        x.ScreeningEntryId == screening.Id &&
                                        x.QueryStatus == QueryStatus.SelfCorrection),
                                    Acknowledge = queryList.Count(x =>
                                        x.ScreeningEntryId == screening.Id && x.QueryStatus == QueryStatus.Resolved),
                                    MyQuery = queryList.Count(x =>
                                        x.ScreeningEntryId == screening.Id &&
                                        x.AcknowledgeLevel == workflowlevel.LevelNo)
                                }

                                //Visits = screening.ScreeningTemplates.Where(r => r.DeletedDate == null).Select(visit => new DataEntryVisitDto
                                //{
                                //    Id = visit.Id,
                                //    TemplateCounts = visit. Where(x => x.ProjectDesignVisitId == visit.Id && x.DeletedDate == null)
                                //      .GroupBy(s => s.Status).Select(g => new DataEntryTemplateCountDto
                                //      {
                                //          Status = g.Key,
                                //          Count = g.Count(),
                                //          StatusName = g.Key.GetDescription(),
                                //          Templates = screening.ScreeningTemplates.Where(a => a.DeletedDate == null).Where(template => template.ProjectDesignVisitId == visit.Id && template.Status == g.Key).Select(m => new DataEntryTemplateDto
                                //          {
                                //              Id = m.ProjectDesignTemplate.Id,
                                //              TemplateName = m.ProjectDesignTemplate.TemplateName
                                //          }).ToList()
                                //      }).ToList(),
                                //    TotalQueries = queryList.Where(query => query.ProjectDesignVisitId == visit.Id && query.ScreeningEntryId == screening.Id).Count(),
                                //    TemplateQueries = queryList.Where(query => query.ProjectDesignVisitId == visit.Id && query.ScreeningEntryId == screening.Id)
                                //      .GroupBy(s => new { s.ProjectDesignTemplateId, s.TemplateName }).Select(g => new DataEntryTemplateQueryCountDto
                                //      {
                                //          Id = g.Key.ProjectDesignTemplateId,
                                //          TemplateName = g.Key.TemplateName,
                                //          Queries = g.GroupBy(q => q.QueryStatus).Select(q => new DataEntryQueryDto
                                //          {
                                //              Status = q.Key,
                                //              StatusName = q.Key == null ? "" : q.Key.GetDescription(),
                                //              Count = q.Count()
                                //          }).ToList()
                                //      }).ToList()
                                //}).ToList()
                    };

                    dataEntries.Add(dataEntry);
                });
            }

            return dataEntries;


            //var projectIds = _projectRightRepository.GetProjectRightIdList();
            //if (!projectIds.Any())
            //{
            //    return new List<DataEntryDto>();
            //}
            //var attendances = Context.Attendance.Where(t =>
            //                t.ProjectDesignPeriodId == projectDesignPeriodId
            //                && t.DeletedDate == null
            //                && t.ProjectId == projectId
            //                && !t.IsProcessed
            //                && t.AttendanceType != AttendanceType.Screening
            //                && projectIds.Any(p => p == t.ProjectId))
            //    .Include(t => t.Volunteer)
            //    .Include(t => t.NoneRegister)
            //    .Include(t => t.ProjectSubject)
            //    .Include(t => t.ProjectDesignPeriod)
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
            //        VolunteerName = t.Volunteer == null ? t.NoneRegister.Initial : t.Volunteer.AliasName,
            //        SubjectNo = t.Volunteer == null ? t.NoneRegister.ScreeningNumber : t.Volunteer.VolunteerNo,
            //        RandomizationNumber = t.Volunteer == null ? t.NoneRegister.RandomizationNumber : t.ProjectSubject?.Number,
            //    };

            //    dataEntries.Add(dataEntry);
            //});

            //var screenings = Context.ScreeningEntry.Where(t =>
            //t.ProjectDesignPeriodId == projectDesignPeriodId
            //&& t.ProjectId == projectId
            //&& t.DeletedDate == null
            //&& t.EntryType != AttendanceType.Screening
            //&& projectIds.Any(p => p == t.ProjectId))
            //                .Include(t => t.ProjectDesignPeriod)
            //                .ThenInclude(t => t.VisitList)
            //                .Include(t => t.Attendance)
            //                .ThenInclude(t => t.Volunteer)
            //                .Include(t => t.Attendance)
            //                .ThenInclude(t => t.NoneRegister)
            //                .Include(t => t.Attendance)
            //                .Include(t => t.ScreeningTemplates)
            //                .ThenInclude(t => t.ScreeningTemplateValues)
            //                .ToList();
            //QuerySearchDto qa = new QuerySearchDto();
            //qa.ProjectId = projectId;
            //if (screenings != null && screenings.Count > 0)
            //{
            //    var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(screenings[0].ProjectDesignId);
            //    var result = _screeningTemplateValueRepository.GetQueryByProjectDesignId(screenings[0].ProjectDesignId);
            //    screenings.ForEach(screening =>
            //    {
            //        //var result = _screeningTemplateValueRepository.GetQueryByProjectDesignId(screening.ProjectDesignId, screening.Id);
            //        var dataEntry = new DataEntryDto
            //        {
            //            WorkflowDetail = workflowlevel,
            //            AttendanceId = screening.Id,
            //            ProjectDesignId = screening.ProjectDesignId,
            //            ProjectId = screening.ProjectId,
            //            ProjectDesignPeriodId = screening.ProjectDesignPeriodId,
            //            VolunteerName = screening.Attendance.Volunteer == null ? screening.Attendance.NoneRegister.Initial : screening.Attendance.Volunteer.AliasName,
            //            SubjectNo = screening.Attendance.Volunteer == null ? screening.Attendance.NoneRegister.ScreeningNumber : screening.Attendance.Volunteer.VolunteerNo,
            //            RandomizationNumber = screening.Attendance.Volunteer == null ? screening.Attendance.NoneRegister.RandomizationNumber : screening.Attendance.ProjectSubject?.Number,
            //            ScreeningEntryId = screening.Id,
            //            VisitSummary = screening.ScreeningTemplates.GroupBy(s => new
            //            {
            //                s.ScreeningEntryId
            //            }).Select(x => new DashboardStudyStatusDto
            //            {
            //                InProcess = x.Count(r => r.Status == ScreeningStatus.InProcess),
            //                NotStarted = x.Count(r => r.Status == ScreeningStatus.Pending),
            //                Review1 = x.Count(r => (int?)r.ReviewLevel == 1),
            //                Review2 = x.Count(r => (int?)r.ReviewLevel == 2),
            //                Review3 = x.Count(r => (int?)r.ReviewLevel == 3),
            //                Review4 = x.Count(r => (int?)r.ReviewLevel == 4),
            //                Review5 = x.Count(r => (int?)r.ReviewLevel == 5)
            //            }).FirstOrDefault(),
            //            QueryStatus = (result.Where(r => r.ScreeningEntryId == screening.Id).FirstOrDefault() == null) || (result.Count() == 0) ? new DashboardQueryStatusDto
            //            {
            //                Open = 0,
            //                Answered = 0,
            //                Resolved = 0,
            //                ReOpened = 0,
            //                Closed = 0,
            //                SelfCorrection = 0,
            //                Acknowledge = 0,
            //                MyQuery = 0
            //            } :
            //            new DashboardQueryStatusDto
            //            {
            //                Open = result.Count(x => x.Status == QueryStatus.Open) == 0 ? 0 : result.FirstOrDefault(x => x.Status == QueryStatus.Open).Total,
            //                Answered = result.Count(x => x.Status == QueryStatus.Answered) == 0 ? 0 : result.FirstOrDefault(x => x.Status == QueryStatus.Answered).Total,
            //                Resolved = result.Count(x => x.Status == QueryStatus.Resolved) == 0 ? 0 : result.FirstOrDefault(x => x.Status == QueryStatus.Resolved).Total,
            //                ReOpened = result.Count(x => x.Status == QueryStatus.Reopened) == 0 ? 0 : result.FirstOrDefault(x => x.Status == QueryStatus.Reopened).Total,
            //                Closed = result.Count(x => x.Status == QueryStatus.Closed) == 0 ? 0 : result.FirstOrDefault(x => x.Status == QueryStatus.Closed).Total,
            //                SelfCorrection = result.Count(x => x.Status == QueryStatus.SelfCorrection) == 0 ? 0 : result.FirstOrDefault(x => x.Status == QueryStatus.SelfCorrection).Total,
            //                Acknowledge = result.Count(x => x.Status == QueryStatus.Acknowledge) == 0 ? 0 : result.FirstOrDefault(x => x.Status == QueryStatus.Acknowledge).Total,
            //                MyQuery = _screeningTemplateValueQueryRepository.GetQueryCountByVisitIdMyQuery(screening.ProjectDesignId, screening.Id),
            //            }
            //        };
            //        dataEntries.Add(dataEntry);
            //    });
            //}

            //return dataEntries.OrderBy(t => t.SubjectNo).ToList();
        }

        public List<DataEntryVisitTemplateDto> GetTemplateForVisit(int screeningEntryId, int projectDesignVisitId,
            ScreeningStatus screeningStatus, bool isQuery)
        {
            if (isQuery)
                return Context.ScreeningTemplate.Where(t =>
                    t.ScreeningEntryId == screeningEntryId
                    && t.ProjectDesignVisitId == projectDesignVisitId
                    && t.ScreeningTemplateValues.Any(c => c.QueryStatus != null && c.QueryStatus != QueryStatus.Closed)
                    && t.DeletedDate == null).Select(x => new DataEntryVisitTemplateDto
                    {
                        ScreeningEntryId = x.ScreeningEntryId,
                        ScreeningTemplateId = x.Id,
                        ProjectDesignTemplateId = x.ProjectDesignTemplateId,
                        TemplateName = x.ProjectDesignTemplate.TemplateName,
                        VisitName = x.ProjectDesignVisit.DisplayName,
                        SubjectName = x.ScreeningEntry.Attendance.Volunteer == null
                            ? x.ScreeningEntry.Attendance.NoneRegister.Initial
                            : x.ScreeningEntry.Attendance.Volunteer.AliasName
                    }
                ).ToList();
            return Context.ScreeningTemplate.Where(t =>
                t.ScreeningEntryId == screeningEntryId
                && t.ProjectDesignVisitId == projectDesignVisitId
                && t.Status == screeningStatus
                && t.DeletedDate == null).Select(x => new DataEntryVisitTemplateDto
                {
                    ScreeningEntryId = x.ScreeningEntryId,
                    ScreeningTemplateId = x.Id,
                    ProjectDesignTemplateId = x.ProjectDesignTemplateId,
                    TemplateName = x.ProjectDesignTemplate.TemplateName,
                    VisitName = x.ProjectDesignVisit.DisplayName,
                    SubjectName = x.ScreeningEntry.Attendance.Volunteer == null
                        ? x.ScreeningEntry.Attendance.NoneRegister.Initial
                        : x.ScreeningEntry.Attendance.Volunteer.AliasName
                }
            ).ToList();
        }
    }
}