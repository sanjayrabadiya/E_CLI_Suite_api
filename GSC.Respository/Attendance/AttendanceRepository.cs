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
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.ProjectRight;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using GSC.Respository.Volunteer;
using GSC.Shared;
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
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IProjectDesignPeriodRepository _projectDesignPeriodRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;

        public AttendanceRepository(IGSCContext context,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository,
            IVolunteerRepository volunteerRepository,
            IAttendanceHistoryRepository attendanceHistoryRepository,
            IRolePermissionRepository rolePermissionRepository,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IScreeningTemplateRepository screeningTemplateRepository)
            : base(context)
        {
           _context = context;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectRightRepository = projectRightRepository;
            _volunteerRepository = volunteerRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _attendanceHistoryRepository = attendanceHistoryRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
        }

        public void SaveAttendance(Data.Entities.Attendance.Attendance attendance)
        {
            attendance.RoleId = _jwtTokenAccesser.RoleId;
            attendance.UserId = _jwtTokenAccesser.UserId;
            attendance.AttendanceDate = DateTime.Now.UtcDateTime();
            attendance.IsTesting = _context.ProjectDesignPeriod.Any(x =>
                x.ProjectDesign.IsUnderTesting && x.Id == attendance.ProjectDesignPeriodId);
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


            var role = _rolePermissionRepository.GetRolePermissionByScreenCode("mnu_underTesting");

            if (!role.IsView)
                result = result.Where(x => !x.IsTesting);

            if (attendanceSearch.Id > 0)
            {
                result = result.Where(x => x.VolunteerId == attendanceSearch.Id);
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

                if (attendanceSearch.ProjectId > 0)
                    result = result.Where(x => x.ProjectId == attendanceSearch.ProjectId);

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
                VolunteerName = x.Volunteer.FullName,
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
                AttendanceType = x.AttendanceType,
                Status = x.Status,
                AttendaceStatusName = x.Status != null ? x.Status.GetDescription() : "",
                IsReplaced = x.ProjectSubject != null && x.ProjectSubject.IsRepaced,
                AttendanceScreeningEntryId = x.ScreeningEntry.Id,
                IsScreeningStarted = x.ScreeningEntry != null ? true : false,
                CreatedDate = x.CreatedDate,
                ModifiedDate = x.ModifiedDate,
                DeletedDate = x.DeletedDate,
                IsLocked = !_screeningTemplateRepository.All.Any(t => t.ScreeningVisit.ScreeningEntryId == x.ScreeningEntry.Id && !t.IsLocked)

            }).ToList().OrderBy(x => x.Id).ToList();

            return items;
        }

        public List<AttendanceScreeningGridDto> GetAttendaceForProjectRightList(ScreeningSearhParamDto attendanceSearch)
        {
            //var projectList = _projectRightRepository.GetProjectRightIdList();
            //if (projectList == null || projectList.Count == 0) return new List<AttendanceScreeningGridDto>();

            //var result = _context.Attendance.Where(t => t.DeletedDate == null
            //                                           && projectList.Any(c => c == t.ProjectId))
            //    .AsQueryable();

            //var role = _rolePermissionRepository.GetRolePermissionByScreenCode("mnu_underTesting");

            //if (!role.IsView)
            //    result = result.Where(x => !x.IsTesting);

            //if (attendanceSearch.Id > 0)
            //{
            //    result = result.Where(x => x.VolunteerId == attendanceSearch.Id);
            //}
            //else
            //{
            //    result = result.Where(x => x.AttendanceType == attendanceSearch.AttendanceType);

            //    if (attendanceSearch.IsFromScreening)
            //        result = result.Where(x => !x.IsProcessed);

            //    if (attendanceSearch.ProjectId > 0)
            //        result = result.Where(x => x.ProjectId == attendanceSearch.ProjectId);

            //    if (attendanceSearch.PeriodNo > 0)
            //        result = result.Where(x => x.PeriodNo == attendanceSearch.PeriodNo);
            //}

            //var items = result.Select(x => new AttendanceScreeningGridDto
            //{
            //    Id = x.Id,
            //    AttendanceId = x.Id,
            //    VolunteerId = x.VolunteerId,
            //    ProjectDesignId = x.ProjectDesignPeriod.ProjectDesignId,
            //    VolunteerName = x.Volunteer == null ? x.Randomization.Initial : x.Volunteer.FullName,
            //    VolunteerNumber = x.Volunteer == null ? x.Randomization.RandomizationNumber : x.Volunteer.VolunteerNo,
            //    Gender = x.Volunteer == null || x.Volunteer.GenderId == null ? "" : x.Volunteer.GenderId.ToString(),
            //    ProjectCode = x.Project.ProjectCode,
            //    ProjectId = x.ProjectId,
            //    ProjectName = x.Project.ProjectName,
            //    AttendanceDate = x.AttendanceDate,
            //    IsFingerPrint = x.IsFingerPrint,
            //    AttendedBy = x.User.UserName,
            //    AuditReasonName = x.AuditReason.ReasonName,
            //    Note = x.Note,
            //    AttendanceTemplateId = x.ProjectDesignPeriod.AttendanceTemplateId,
            //    DiscontinuedTemplateId = x.ProjectDesignPeriod.DiscontinuedTemplateId,
            //    ProjectSubjectId = x.ProjectSubjectId,
            //    PeriodNo = x.PeriodNo,
            //    IsStandby = x.IsStandby ? "Yes" : "No",
            //    SubjectNumber = x.ProjectSubject != null ? x.ProjectSubject.Number : "",
            //    AttendanceType = x.AttendanceType,
            //    Status = x.Status,
            //    AttendaceStatusName = x.Status != null ? x.Status.GetDescription() : "",
            //    IsReplaced = x.ProjectSubject != null && x.ProjectSubject.IsRepaced,
            //    IsLocked = !_screeningTemplateRepository.All.Any(t => t.ScreeningVisit.ScreeningEntryId == x.ScreeningEntry.Id && !t.IsLocked)
            //}).ToList().OrderBy(x => x.Id).ToList();

            //if (attendanceSearch.AttendanceType == AttendanceType.Project && items.Count > 0)
            //{
            //    var firstRecord = items.FirstOrDefault(x => x.Status == null);
            //    items.ForEach(t =>
            //    {
            //        if (firstRecord != null && t.AttendanceId > firstRecord.AttendanceId)
            //        {
            //            t.AttendanceTemplateId = null;
            //            t.DiscontinuedTemplateId = null;
            //        }

            //        var screening = _context.ScreeningEntry.Where(x => x.AttendanceId == t.AttendanceId).FirstOrDefault();
            //        if (screening != null)
            //        {
            //            t.AttendanceScreeningEntryId = screening.Id;
            //            t.IsScreeningStarted = true;
            //        }
            //    });
            //}


            //return items;

            return null;
        }

        public List<AttendanceScreeningGridDto> GetAttendaceListByLock(ScreeningSearhParamDto attendanceSearch, bool isLock)
        {
            var items = new List<AttendanceScreeningGridDto>();




            //if (isLock)
            //{
            //    items = (from attendance in _context.Attendance.Where(t => t.DeletedDate == null && t.ProjectId == attendanceSearch.ProjectId && t.AttendanceType == attendanceSearch.AttendanceType)
            //             select new AttendanceScreeningGridDto
            //             {
            //                 Id = attendance.Id,
            //                 ProjectId = attendance.ProjectId,
            //                 VolunteerName = attendance.Volunteer == null ? attendance.Randomization.Initial : attendance.Volunteer.FullName,
            //                 VolunteerNumber = attendance.Volunteer == null ? attendance.Randomization.RandomizationNumber : attendance.Volunteer.VolunteerNo,
            //                 SubjectNumber = attendance.ProjectSubject != null ? attendance.ProjectSubject.Number : "",
            //                 AttendanceId = attendance.Id,
            //             }).ToList();

            //    var lstsubject = (from attendance in _context.Attendance.Where(t => t.DeletedDate == null && t.ProjectId == attendanceSearch.ProjectId && t.AttendanceType == attendanceSearch.AttendanceType)
            //                      select new AttendanceScreeningGridDto
            //                      {
            //                          Id = attendance.Id,
            //                          ProjectId = attendance.ProjectId,
            //                          VolunteerName = attendance.Volunteer == null ? attendance.Randomization.Initial : attendance.Volunteer.FullName,
            //                          VolunteerNumber = attendance.Volunteer == null ? attendance.Randomization.RandomizationNumber : attendance.Volunteer.VolunteerNo,
            //                          SubjectNumber = attendance.ProjectSubject != null ? attendance.ProjectSubject.Number : "",
            //                          AttendanceId = attendance.Id,
            //                      }).ToList();

            //    foreach (var item in lstsubject)
            //    {
            //        var screeningTemplate = _screeningTemplateRepository.FindByInclude(x => x.ScreeningVisit.ScreeningEntry.AttendanceId == item.AttendanceId && x.DeletedDate == null).ToList();
            //        if (screeningTemplate.Count() <= 0 || screeningTemplate.Any(y => y.IsLocked == false))
            //        {
            //            var itemexist = items.Where(x => x.Id == item.Id).FirstOrDefault();
            //            if (itemexist == null)
            //            {
            //                items.Add(item);
            //            }
            //        }
            //        else
            //        {
            //            items.RemoveAll(x => x.Id == item.Id);
            //        }
            //    }
            //}
            //else
            //{


            //    var result = (from attendance in _context.Attendance.Where(t => t.DeletedDate == null && t.ProjectId == attendanceSearch.ProjectId && t.AttendanceType == attendanceSearch.AttendanceType)
            //                  join locktemplate in _context.ScreeningTemplate.Where(x => x.IsLocked)
            //                  on attendance.Id equals locktemplate.ScreeningVisit.ScreeningEntry.AttendanceId
            //                  select new AttendanceScreeningGridDto
            //                  {
            //                      Id = attendance.Id,
            //                      ProjectId = attendance.ProjectId,
            //                      VolunteerName = attendance.Volunteer == null ? attendance.Randomization.Initial : attendance.Volunteer.FullName,
            //                      VolunteerNumber = attendance.Volunteer == null ? attendance.Randomization.RandomizationNumber : attendance.Volunteer.VolunteerNo,
            //                      SubjectNumber = attendance.ProjectSubject != null ? attendance.ProjectSubject.Number : "",
            //                      AttendanceId = attendance.Id,
            //                      IsLocked = locktemplate.IsLocked,
            //                  }).Distinct().ToList();

            //    items = result.Where(r => r.IsLocked).GroupBy(x => x.VolunteerName).Select(z => new AttendanceScreeningGridDto
            //    {
            //        Id = z.FirstOrDefault().Id,
            //        ProjectId = z.FirstOrDefault().ProjectId,
            //        VolunteerName = z.FirstOrDefault().VolunteerName,
            //        VolunteerNumber = z.FirstOrDefault().VolunteerNumber,
            //        SubjectNumber = z.FirstOrDefault().SubjectNumber,
            //        AttendanceId = z.FirstOrDefault().AttendanceId,
            //    }).ToList();

            //}
            return null;
        }

        public string CheckVolunteer(AttendanceDto attendanceDto)
        {
            if (attendanceDto.AttendanceType == DataEntryType.Screening)
                if (All.Any(x =>
                    x.VolunteerId == attendanceDto.VolunteerId && x.AttendanceType == DataEntryType.Screening &&
                    x.AttendanceDate.ToShortDateString() == DateTime.Now.UtcDate().ToShortDateString() &&
                    x.DeletedDate == null))
                    return "Volunteer already present today";
            if (attendanceDto.AttendanceType == DataEntryType.Project)
                if (All.Any(x =>
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
            //var projectList = new List<int>();
            //int ProjectId = 0;
            //if (filters.ProjectId == 0)
            //{
            //    ProjectId = _context.ProjectDesign.Find(filters.ProjectDesignId).ProjectId;
            //}
            //else
            //{
            //    ProjectId = (int)filters.ProjectId;
            //}
            //projectList = GetProjectList(ProjectId);
            //if (projectList == null || projectList.Count == 0)
            //    return new List<DropDownDto>();

            //var Isstatic = _context.Project.Where(x => x.Id == ProjectId).FirstOrDefault().IsStatic;

            //if (Isstatic)
            //{
            //    return All.Where(t => (t.CompanyId == null
            //                   || t.CompanyId == _jwtTokenAccesser.CompanyId)
            //                  && projectList.Any(c => c == t.ProjectId)
            //                  && t.Randomization.RandomizationNumber != null
            //                   ).Select(r => new DropDownDto
            //                   {
            //                       Id = r.Id,
            //                       Value = r.Randomization.ScreeningNumber + "-" + r.Randomization.Initial + "-" + r.Randomization.RandomizationNumber
            //                   }).ToList();
            //}
            //else
            //{
            //    return All.Where(t => (t.CompanyId == null
            //                   || t.CompanyId == _jwtTokenAccesser.CompanyId)
            //                  && projectList.Any(c => c == t.ProjectId)
            //                  && t.VolunteerId != null
            //                  && t.ProjectSubject.Number != null
            //                   ).Select(r => new DropDownDto
            //                   {
            //                       Id = r.Id,
            //                       Value = r.Volunteer.VolunteerNo + "-" + r.Volunteer.AliasName + "-" + r.ProjectSubject.Number
            //                   }).ToList();
            //}

            return null;
        }

        public List<int> GetProjectList(int ProjectId)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            return _context.Project.Where(c => c.DeletedDate == null && (c.Id == ProjectId || c.ParentProjectId == ProjectId) && projectList.Any(t => t == c.Id)).Select(x => x.Id).ToList();
        }
    }
}