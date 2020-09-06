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



        public List<DataEntryVisitTemplateDto> GetTemplateForVisit(int screeningEntryId, int projectDesignVisitId,
            ScreeningTemplateStatus screeningStatus, bool isQuery)
        {
            if (isQuery)
                return Context.ScreeningTemplate.Where(t =>
                    t.ScreeningVisit.ScreeningEntryId == screeningEntryId
                    && t.ScreeningVisitId == projectDesignVisitId
                    && t.ScreeningTemplateValues.Any(c => c.QueryStatus != null && c.QueryStatus != QueryStatus.Closed)
                    && t.DeletedDate == null).Select(x => new DataEntryVisitTemplateDto
                    {
                        ScreeningEntryId = x.ScreeningVisit.ScreeningEntryId,
                        ScreeningTemplateId = x.Id,
                        ProjectDesignTemplateId = x.ProjectDesignTemplateId,
                        TemplateName = x.ProjectDesignTemplate.TemplateName,
                        VisitName = x.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                        SubjectName = x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                            ? x.ScreeningVisit.ScreeningEntry.Attendance.NoneRegister.Initial
                            : x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName
                    }
                ).ToList();
            return Context.ScreeningTemplate.Where(t =>
                t.ScreeningVisit.ScreeningEntryId == screeningEntryId
                && t.ScreeningVisitId == projectDesignVisitId
                && t.Status == screeningStatus
                && t.DeletedDate == null).Select(x => new DataEntryVisitTemplateDto
                {
                    ScreeningEntryId = x.ScreeningVisit.ScreeningEntryId,
                    ScreeningTemplateId = x.Id,
                    ProjectDesignTemplateId = x.ProjectDesignTemplateId,
                    TemplateName = x.ProjectDesignTemplate.TemplateName,
                    VisitName = x.ScreeningVisit.ProjectDesignVisit.DisplayName,
                    SubjectName = x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                        ? x.ScreeningVisit.ScreeningEntry.Attendance.NoneRegister.Initial
                        : x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName
                }
            ).ToList();
        }
    }
}