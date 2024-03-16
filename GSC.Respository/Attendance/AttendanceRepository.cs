using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Dto.Screening;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Barcode;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.ProjectRight;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using GSC.Respository.Volunteer;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Attendance
{
    public class AttendanceRepository : GenericRespository<Data.Entities.Attendance.Attendance>,
        IAttendanceRepository
    {
        private readonly IGSCContext _context;
        private readonly IAttendanceHistoryRepository _attendanceHistoryRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IAttendanceBarcodeGenerateRepository _attendanceBarcodeGenerateRepository;

        public AttendanceRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository,
            IVolunteerRepository volunteerRepository,
            IAttendanceHistoryRepository attendanceHistoryRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IAttendanceBarcodeGenerateRepository attendanceBarcodeGenerateRepository)
            : base(context)
        {
            _context = context;
            _projectRightRepository = projectRightRepository;
            _volunteerRepository = volunteerRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _attendanceHistoryRepository = attendanceHistoryRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _attendanceBarcodeGenerateRepository = attendanceBarcodeGenerateRepository;
        }

        public void SaveAttendance(Data.Entities.Attendance.Attendance attendance)
        {
            attendance.RoleId = _jwtTokenAccesser.RoleId;
            attendance.UserId = _jwtTokenAccesser.UserId;
            attendance.IsTesting = _context.Project.Any(x =>
                x.IsTestSite && x.Id == attendance.ProjectId);
            attendance.AttendanceHistory =
                _attendanceHistoryRepository.SaveHistory("Added attendance", 0, attendance.AuditReasonId);
            Add(attendance);
        }

        public List<AttendanceScreeningGridDto> GetAttendaceList(ScreeningSearhParamDto attendanceSearch)
        {
            if (attendanceSearch.ProjectId == 0) return new List<AttendanceScreeningGridDto>();

            var projectList = _projectRightRepository.GetProjectRightIdList();

            if (projectList == null || projectList.Count == 0) return new List<AttendanceScreeningGridDto>();

            var result = _context.Attendance.Where(t => t.DeletedDate == null
                                                       && projectList.Any(c => c == t.ProjectId));
            if (attendanceSearch.Id > 0)
            {
                result = result.Where(x => x.Id == attendanceSearch.Id);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(attendanceSearch.TextSearch))
                {
                    var volunterIds = _volunteerRepository.AutoCompleteSearch(attendanceSearch.TextSearch.Trim());
                    result = result.Where(x => volunterIds.Any(a => a.Id == x.VolunteerId));
                }

                result = result.Where(x => x.AttendanceType == attendanceSearch.AttendanceType);

                if (attendanceSearch.IsFromScreening)
                    result = result.Where(x => !x.IsProcessed);

                if (attendanceSearch.StudyId > 0)
                    result = result.Where(x => x.ProjectId == attendanceSearch.StudyId);

                if (attendanceSearch.ProjectId > 0)
                    result = result.Where(x => x.ProjectId == attendanceSearch.ProjectId);

                if (attendanceSearch.SiteId > 0)
                    result = result.Where(x => x.SiteId == attendanceSearch.SiteId);

                if (attendanceSearch.PeriodNo > 0)
                    result = result.Where(x => x.PeriodNo == attendanceSearch.PeriodNo);

                if (attendanceSearch.FromDate.HasValue && attendanceSearch.ToDate.HasValue)
                    result = result.Where(x =>
                        x.AttendanceDate.Date >= attendanceSearch.FromDate.Value.Date &&
                        x.AttendanceDate.Date <= attendanceSearch.ToDate.Value.Date);

                if (attendanceSearch.FromDate.HasValue && attendanceSearch.ToDate == null)
                    result = result.Where(x => x.AttendanceDate.Date == attendanceSearch.FromDate.Value.Date);
            }

            var items = result.Select(x => new AttendanceScreeningGridDto
            {
                Id = x.Id,
                AttendanceId = x.Id,
                VolunteerId = x.VolunteerId,
                ProjectDesignId = x.ProjectDesignPeriod.ProjectDesignId,
                AliasName = x.Volunteer.AliasName,
                VolunteerName = x.Volunteer.FirstName + " " + x.Volunteer.MiddleName + " " + x.Volunteer.LastName,
                VolunteerNumber = x.Volunteer.VolunteerNo,
                Gender = x.Volunteer == null || x.Volunteer.GenderId == null ? "" : x.Volunteer.GenderId.ToString(),
                ProjectCode = x.Project.ProjectCode,
                ProjectId = x.ProjectId,
                ProjectName = x.Project.ProjectName,
                AttendanceDate = x.AttendanceDate,
                IsFingerPrint = x.IsFingerPrint,
                AttendedBy = x.User.UserName,
                AuditReasonName = x.AuditReason.ReasonName,
                Note = x.Note,
                AttendanceTemplateId = x.ProjectDesignPeriod.AttendanceTemplateId,
                DiscontinuedTemplateId = x.ProjectDesignPeriod.DiscontinuedTemplateId,
                ProjectSubjectId = x.ProjectSubjectId,
                PeriodNo = x.PeriodNo,
                IsStandby = x.IsStandby ? "Yes" : "No",
                SubjectNumber = x.ProjectSubject != null ? x.ProjectSubject.Number : "",
                RandomizationNumber = x.Volunteer.RandomizationNumber,
                AttendanceType = x.AttendanceType,
                Status = x.Status,
                AttendaceStatusName = x.Status != null ? x.Status.GetDescription() : "",
                IsReplaced = x.ProjectSubject != null && x.ProjectSubject.IsRepaced,
                AttendanceScreeningEntryId = x.ScreeningEntry.Id,
                IsScreeningStarted = x.ScreeningEntry != null,
                CreatedByUser = x.CreatedByUser.UserName,
                ModifiedByUser = x.ModifiedByUser.UserName,
                DeletedByUser = x.DeletedByUser.UserName,
                CreatedDate = x.CreatedDate,
                ModifiedDate = x.ModifiedDate,
                DeletedDate = x.DeletedDate,
                IsLocked = !_screeningTemplateRepository.All.Any(t => t.ScreeningVisit.ScreeningEntryId == x.ScreeningEntry.Id && !t.IsLocked),
                IsBarcodeGenerated = _attendanceBarcodeGenerateRepository.All.Any(t => t.AttendanceId == x.Id && t.DeletedBy == null),//x.AttendanceBarcodeGenerate != null ? true : false,
                AttendanceBarcodeGenerateId = _attendanceBarcodeGenerateRepository.All.Where(t => t.AttendanceId == x.Id && t.DeletedBy == null).FirstOrDefault().Id//x.AttendanceBarcodeGenerate.Id
            }).AsEnumerable().OrderByDescending(x => x.Id).ToList();

            return items;
        }

        public List<AttendanceScreeningGridDto> GetAttendaceForProjectRightList(ScreeningSearhParamDto attendanceSearch)
        {

            return new List<AttendanceScreeningGridDto>();
        }

        public List<AttendanceScreeningGridDto> GetAttendaceListByLock(ScreeningSearhParamDto attendanceSearch, bool isLock)
        {
            var items = new List<AttendanceScreeningGridDto>();
            return items;
        }

        public string CheckVolunteer(AttendanceDto attendanceDto)
        {
            if (attendanceDto.AttendanceType == DataEntryType.Screening && All.Any(x =>
                    x.VolunteerId == attendanceDto.VolunteerId && x.AttendanceType == DataEntryType.Screening && x.ProjectId == attendanceDto.ProjectId &&
                    x.AttendanceDate.Day.CompareTo(attendanceDto.AttendanceDate.Day) == 0 &&
                    x.DeletedDate == null))
                return "Volunteer already present today";
            if (attendanceDto.AttendanceType == DataEntryType.Project && All.Any(x =>
                    x.VolunteerId == attendanceDto.VolunteerId && x.AttendanceType == DataEntryType.Project &&
                    x.ProjectId == attendanceDto.ProjectId && x.PeriodNo == attendanceDto.PeriodNo &&
                    x.Status != AttendaceStatus.Suspended && x.DeletedDate == null))
                return "Volunteer already attendanced";
            return "";
        }

        public IList<DropDownDto> GetVolunteersByProjectId(int projectId)
        {
            var volunteers = _context.Volunteer.Where(t => All.Where(x => x.DeletedDate == null
                                                                         && x.ProjectId == projectId)
                .Select(s => s.VolunteerId).Contains(t.Id)).Select(t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.FullName
                }).ToList();

            return volunteers;
        }

        public IList<DropDownDto> GetVolunteersForReplacement(int projectId)
        {
            var volunteers = All.Where(x => x.DeletedDate == null
                                            && x.ProjectId == projectId &&
                                            x.ProjectSubject.NumberType == SubjectNumberType.StandBy
                                            && x.PeriodNo == 1).Select(t => new DropDownDto
                                            {
                                                Id = t.Id,
                                                Value = t.Volunteer.FullName
                                            }).ToList();

            return volunteers;
        }

        public string ProjectSuspended(int projectId)
        {
            var validateMessage = "";

            if (All.Any(x => x.ProjectId == projectId && x.AttendanceType == DataEntryType.Project
                                                      && x.PeriodNo == 1 &&
                                                      (x.ProjectSubject != null || x.IsProcessed) &&
                                                      x.DeletedDate == null))
                return "This project in under process!";

            var result = All.Where(x => x.ProjectId == projectId && x.AttendanceType == DataEntryType.Project
                                                                 && x.Status != AttendaceStatus.Suspended &&
                                                                 x.PeriodNo == 1 &&
                                                                 x.DeletedDate == null).ToList();

            if (result.Count == 0) return "Not found record!";

            result.ForEach(x =>
            {
                _attendanceHistoryRepository.SaveHistory("Suspended attendance", x.Id, null);
                x.Status = AttendaceStatus.Suspended;
                Update(x);
            });
            return validateMessage;
        }


        public IList<VolunteerAttendaceDto> GetAttendanceAnotherPeriod(VolunteerSearchDto search)
        {
            var projectId = _context.ProjectDesignPeriod.Where(t => t.Id == search.ProjectDesignPeriodId)
                .Select(x => x.ProjectDesign.ProjectId).FirstOrDefault();
            var query = All.Where(t => t.DeletedDate == null && t.Status == AttendaceStatus.FitnessPass
                                                             && t.PeriodNo == 1 && t.ProjectId == projectId);

            if (!string.IsNullOrEmpty(search.TextSearch))
            {
                var volunterIds = _volunteerRepository.AutoCompleteSearch(search.TextSearch.Trim());
                query = query.Where(x => volunterIds.Any(a => a.Id == x.VolunteerId));
            }

            query = query.Where(x => !All.Any(t => t.DeletedDate == null
                                                   && (t.Status == null || t.Status != AttendaceStatus.Suspended)
                                                   && t.PeriodNo == search.PeriodNo && t.ProjectId == projectId &&
                                                   t.VolunteerId == x.VolunteerId));

            return query.Select(x => new VolunteerAttendaceDto
            {
                Id = x.Volunteer.Id,
                VolunteerNo = x.Volunteer.VolunteerNo,
                RefNo = x.ProjectSubject.Number,
                LastName = x.Volunteer.LastName,
                FirstName = x.Volunteer.FirstName,
                MiddleName = x.Volunteer.MiddleName,
                AliasName = x.Volunteer.AliasName,
                ScreeningEntryId = x.ScreeningEntryId,
                ProjectSubjectId = x.ProjectSubjectId,
                DateOfBirth = x.Volunteer.DateOfBirth,
                FullName = x.Volunteer.FullName,
                FromAge = x.Volunteer.FromAge,
                ToAge = x.Volunteer.ToAge,
                Gender = x.Volunteer.GenderId == null ? "" : ((Gender)x.Volunteer.GenderId).GetDescription(),
                Race = x.Volunteer.Race.RaceName,
                IsDeleted = x.Volunteer.DeletedDate != null,
                Blocked = x.Volunteer.IsBlocked ?? false
            }).OrderByDescending(x => x.FullName).ToList();
        }

        public List<DropDownDto> GetAttendanceForMeddraCodingDropDown(MeddraCodingSearchDto filters)
        {
            return new List<DropDownDto>();
        }

        public List<int> GetProjectList(int ProjectId)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            return _context.Project.Where(c => c.DeletedDate == null && (c.Id == ProjectId || c.ParentProjectId == ProjectId) && projectList.Any(t => t == c.Id)).Select(x => x.Id).ToList();
        }
    }
}