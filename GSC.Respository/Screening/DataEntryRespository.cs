using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningEntryRepository _screeningEntryRepository;
        public DataEntryRespository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectRightRepository projectRightRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IRandomizationRepository randomizationRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IScreeningEntryRepository screeningEntryRepository,
            IProjectDesignRepository projectDesignRepository
        )
            : base(uow, jwtTokenAccesser)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectRightRepository = projectRightRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _randomizationRepository = randomizationRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _screeningEntryRepository = screeningEntryRepository;
            _projectDesignRepository = projectDesignRepository;
        }

        public async Task<DataCaptureGridDto> GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int parentProjectId, int projectId)
        {
            var result = new DataCaptureGridDto();

            var projectDesignId = _projectDesignRepository.All.Where(r => r.ProjectId == parentProjectId).Select(t => t.Id).FirstOrDefault();

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesignId);
            result.WorkFlowText = workflowlevel.WorkFlowText;

            var projectDesignVisit = await _projectDesignVisitRepository.All.
                Where(x => x.ProjectDesignPeriod.ProjectDesign.ProjectId == parentProjectId && x.IsSchedule != true).
            Select(t => new DataEntryVisitTemplateDto
            {
                ProjectDesignVisitId = t.Id,
                VisitName = t.DisplayName,
                VisitStatus = ScreeningVisitStatus.NotStarted.GetDescription(),
                VisitStatusId = (int)ScreeningVisitStatus.NotStarted
            }).ToListAsync();

            var randomizationData = await _randomizationRepository.All.Where(x => x.ProjectId == projectId && x.DeletedDate == null
             && x.PatientStatusId == ScreeningPatientStatus.Screening).Select(t => new DataCaptureGridData
             {
                 RandomizationId = t.Id,
                 VolunteerName = t.Initial,
                 IsRandomization = true,
                 SubjectNo = t.ScreeningNumber,
                 PatientStatus = t.PatientStatusId.GetDescription(),
                 RandomizationNumber = t.RandomizationNumber,
             }).ToListAsync();

            var templates = await _screeningTemplateRepository.All.Where(r => r.ScreeningVisit.ScreeningEntry.ProjectId == projectId && r.DeletedDate == null).
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

            var queries = await _screeningTemplateValueRepository.All.Where(r => r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId && r.DeletedDate == null).
                GroupBy(c => new
                {
                    c.ScreeningTemplate.ScreeningVisit.ScreeningEntryId,
                    c.ScreeningTemplate.ScreeningVisitId,
                    c.AcknowledgeLevel,
                    c.QueryStatus
                }).Select(t => new
                {
                    t.Key.ScreeningEntryId,
                    t.Key.AcknowledgeLevel,
                    t.Key.ScreeningVisitId,
                    t.Key.QueryStatus,
                    TotalQuery = t.Count()
                }).ToListAsync();

            var screeningData = await _screeningEntryRepository.All.Where(r => r.ProjectId == projectId && r.DeletedDate == null).Select(x => new DataCaptureGridData
            {
                ScreeningEntryId = x.Id,
                RandomizationId = x.RandomizationId,
                AttendanceId = x.AttendanceId,
                VolunteerName = x.RandomizationId != null ? x.Randomization.Initial : x.Attendance.Volunteer.AliasName,
                IsRandomization = x.RandomizationId != null,
                SubjectNo = x.RandomizationId != null ? x.Randomization.ScreeningNumber : x.Attendance.Volunteer.VolunteerNo,
                PatientStatus = x.RandomizationId != null ? x.Randomization.PatientStatusId.GetDescription() : "",
                RandomizationNumber = x.RandomizationId != null ? x.Randomization.RandomizationNumber : "",
                Visit = x.ScreeningVisit.Where(t => t.DeletedDate == null).Select(a => new DataEntryVisitTemplateDto
                {
                    ScreeningVisitId = a.Id,
                    ProjectDesignVisitId = a.ProjectDesignVisitId,
                    VisitName = a.ProjectDesignVisit.DisplayName,
                    VisitStatus = a.Status.GetDescription(),
                    VisitStatusId = (int)a.Status,
                    ActualDate = a.VisitStartDate,
                    ScheduleDate = a.ScheduleDate
                }).ToList()

            }).ToListAsync();

            //await Task.WhenAll(projectDesignVisitTask, randomizationDataTask, templateTask, queryTask, screeningDataTask);

            //var projectDesignVisit = await projectDesignVisitTask;
            //var randomizationData = await randomizationDataTask;
            //var templates = await templateTask;
            //var queries = await queryTask;
            //var screeningData = await screeningDataTask;

            randomizationData.ForEach(r => r.Visit = projectDesignVisit);

            screeningData.ForEach(r =>
            {
                r.Visit.ForEach(v =>
                {
                    if (v.VisitStatusId != 1)
                    {
                        v.NotStarted = templates.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.Status == ScreeningTemplateStatus.Pending).Sum(t => t.TotalTemplate);
                        v.InProgress = templates.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.Status == ScreeningTemplateStatus.InProcess).Sum(t => t.TotalTemplate);
                        v.MyQuery = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.AcknowledgeLevel == workflowlevel.LevelNo).Sum(t => t.TotalQuery);
                        v.ReOpen = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Reopened).Sum(t => t.TotalQuery);
                        v.Open = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Open).Sum(t => t.TotalQuery);
                        v.Answered = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Answered).Sum(t => t.TotalQuery);
                        v.Resolved = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Resolved).Sum(t => t.TotalQuery);
                        v.Closed = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Closed).Sum(t => t.TotalQuery);
                        v.SelfCorrection = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.SelfCorrection).Sum(t => t.TotalQuery);
                        v.Acknowledge = queries.Where(x => x.ScreeningEntryId == r.ScreeningEntryId && x.ScreeningVisitId == v.ScreeningVisitId && x.QueryStatus == QueryStatus.Acknowledge).Sum(t => t.TotalQuery);
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

            return result;

        }

        public List<DataEntryTemplateCountDisplayDto> GetTemplateForVisit(int screeningEntryId, int projectDesignVisitId,
            int screeningStatus, bool isQuery)
        {

            //return new List<DataEntryVisitTemplateDto>();

            if (isQuery)
                return Context.ScreeningTemplate.Where(t =>
                    t.ScreeningVisit.ScreeningEntryId == screeningEntryId
                    && t.ScreeningVisit.ProjectDesignVisitId == projectDesignVisitId
                    && t.ScreeningTemplateValues.Any(c => c.QueryStatus != null && c.QueryStatus != QueryStatus.Closed)
                    && t.DeletedDate == null).Select(x => new DataEntryTemplateCountDisplayDto
                    {
                        ScreeningEntryId = x.ScreeningVisit.ScreeningEntryId,
                        ScreeningTemplateId = x.Id,
                        ProjectDesignTemplateId = x.ProjectDesignTemplateId,
                        TemplateName = x.ProjectDesignTemplate.TemplateName,
                        VisitName = x.ScreeningVisit.ProjectDesignVisit.DisplayName,
                        SubjectName = x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                            ? x.ScreeningVisit.ScreeningEntry.Randomization.Initial
                            : x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName
                    }
                ).ToList();
            return Context.ScreeningTemplate.Where(t =>
                t.ScreeningVisit.ScreeningEntryId == screeningEntryId
                && t.ScreeningVisit.ProjectDesignVisitId == projectDesignVisitId
                && (int)t.ScreeningVisit.Status == screeningStatus
                && t.DeletedDate == null).Select(x => new DataEntryTemplateCountDisplayDto
                {
                    ScreeningEntryId = x.ScreeningVisit.ScreeningEntryId,
                    ScreeningTemplateId = x.Id,
                    ProjectDesignTemplateId = x.ProjectDesignTemplateId,
                    TemplateName = x.ProjectDesignTemplate.TemplateName,
                    VisitName = x.ScreeningVisit.ProjectDesignVisit.DisplayName,
                    SubjectName = x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                        ? x.ScreeningVisit.ScreeningEntry.Randomization.Initial
                        : x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName
                }
            ).ToList();
        }


    }
}