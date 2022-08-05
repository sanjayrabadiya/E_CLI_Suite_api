using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Barcode;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Schedule;
using GSC.Respository.Project.Workflow;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;
using GSC.Respository.Volunteer;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class ScreeningEntryRepository : GenericRespository<ScreeningEntry>, IScreeningEntryRepository
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly INumberFormatRepository _numberFormatRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectScheduleRepository _projectScheduleRepository;
        private readonly IAttendanceBarcodeGenerateRepository _attendanceBarcodeGenerateRepository;
        private readonly IAppSettingRepository _appSettingRepository;
        public ScreeningEntryRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IVolunteerRepository volunteerRepository,
            IProjectRightRepository projectRightRepository,
            IAttendanceRepository attendanceRepository,
            IProjectDesignRepository projectDesignRepository,
            IScreeningVisitRepository screeningVisitRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IProjectRepository projectRepository,
            IRandomizationRepository randomizationRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            INumberFormatRepository numberFormatRepository,
            IRolePermissionRepository rolePermissionRepository,
             IProjectScheduleRepository projectScheduleRepository,
             IAttendanceBarcodeGenerateRepository attendanceBarcodeGenerateRepository,
             IAppSettingRepository appSettingRepository)
            : base(context)
        {
            _volunteerRepository = volunteerRepository;
            _projectRightRepository = projectRightRepository;
            _attendanceRepository = attendanceRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectWorkflowRepository = projectWorkflowRepository;
            _randomizationRepository = randomizationRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _numberFormatRepository = numberFormatRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _screeningVisitRepository = screeningVisitRepository;
            _projectDesignRepository = projectDesignRepository;
            _projectRepository = projectRepository;
            _context = context;
            _projectScheduleRepository = projectScheduleRepository;
            _attendanceBarcodeGenerateRepository = attendanceBarcodeGenerateRepository;
            _appSettingRepository = appSettingRepository;
        }

        public ScreeningEntryDto GetDetails(int id)
        {

            var screeningEntryDto = _context.ScreeningEntry.Where(t => t.Id == id)
                .Select(t => new ScreeningEntryDto
                {
                    Id = t.Id,
                    AttendanceId = t.AttendanceId,
                    RandomizationId = t.RandomizationId,
                    ScreeningNo = t.ScreeningNo,
                    ScreeningDate = t.ScreeningDate,
                    VolunteerName = t.RandomizationId != null
                        ? t.Randomization.Initial
                        : t.Attendance.Volunteer.AliasName,
                    IsFitnessFit = t.IsFitnessFit,
                    IsEnrolled = t.IsEnrolled,
                    ProjectNo = t.Project.ProjectCode,
                    ProjectDesignId = t.ProjectDesignId,
                    FitnessReason = t.FitnessReason,
                    FitnessNotes = t.FitnessNotes,
                    PatientStatusName = t.RandomizationId != null ? t.Randomization.PatientStatusId.GetDescription() : "",
                    PatientStatusId = t.RandomizationId != null ? t.Randomization.PatientStatusId : 0,
                    Progress = t.Progress,
                    ProjectDesignPeriodId = t.ProjectDesignPeriodId,
                    ProjectId = t.ProjectId,
                    StudyVersion = t.StudyVersion ?? 1

                }).FirstOrDefault();

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(screeningEntryDto.ProjectDesignId);

            bool myReview = false;
            screeningEntryDto.ScreeningVisits = VisitTemplateProcess(screeningEntryDto.Id, workflowlevel, ref myReview);

            if (screeningEntryDto.AttendanceId != null)
            {
                var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
                GeneralSettings.TimeFormat = GeneralSettings.TimeFormat.Replace("a", "tt");

                var attendance = _attendanceRepository.Find((int)screeningEntryDto.AttendanceId);

                var attendanceListByVolunteer = _attendanceRepository.All.Where(x => x.VolunteerId == (int)attendance.VolunteerId && x.DeletedDate == null).ToList();

                screeningEntryDto.AttendanceList = All.Where(x => attendanceListByVolunteer.Select(y => y.Id).ToList().Contains((int)x.AttendanceId))
                    .Select(z => new VolunteerAttendanceDto
                    {
                        ScreeningEntryId = z.Id,
                        AttendanceId = (int)z.AttendanceId,
                        ScreeningNo = z.ScreeningNo,
                        ScreeningDate = DateTime.Parse(z.ScreeningDate.ToString()).ToString(GeneralSettings.DateFormat, CultureInfo.InvariantCulture)
                    }).ToList();
            }

            screeningEntryDto.IsElectronicSignature = workflowlevel.IsElectricSignature;


            screeningEntryDto.IsMultipleVisits = screeningEntryDto.ScreeningVisits.Select(s => s.ProjectDesignVisitId).Distinct().Count() > 1;

            screeningEntryDto.MyReview = myReview;

            if (workflowlevel != null && workflowlevel.WorkFlowText != null)
            {
                screeningEntryDto.LevelName1 = workflowlevel.WorkFlowText.Where(r => r.LevelNo == 1).Select(t => t.RoleName).FirstOrDefault();
                screeningEntryDto.IsSystemQueryUpdate = workflowlevel.IsStartTemplate;
            }

            return screeningEntryDto;
        }

        private List<ScreeningVisitTree> VisitTemplateProcess(int screeningEntryId, WorkFlowLevelDto workflowlevel, ref bool myReview)
        {
            var visits = _screeningVisitRepository.GetVisitTree(screeningEntryId);
            var templates = _screeningTemplateRepository.GetTemplateTree(screeningEntryId, workflowlevel);

            visits.ForEach(x =>
            {
                x.ScreeningTemplates = templates.Where(a => a.ScreeningVisitId == x.ScreeningVisitId && a.ParentId == null).OrderBy(c => Convert.ToDecimal(c.DesignOrderForOrderBy)).ToList();
                x.ScreeningTemplates.ForEach(v => v.Children = templates.Where(a => a.ParentId == v.Id).OrderBy(x => x.Id).ThenBy(c => c.DesignOrder).ToList());

                x.IsVisitRepeated = x.ParentScreeningVisitId != null ? false :
                    workflowlevel.IsStartTemplate && x.IsVisitRepeated ? true : false;

                if (x.VisitStatus == ScreeningVisitStatus.Missed || x.VisitStatus == ScreeningVisitStatus.Missed)
                    x.IsVisitRepeated = false;
                x.IsLocked = x.ScreeningTemplates.All(x => x.IsLocked);
            });

            myReview = templates.Any(x => x.MyReview);

            return visits;
        }

        public void SaveScreeningAttendance(ScreeningEntry screeningEntry, List<int> projectAttendanceTemplateIds)
        {
            var attendace = _attendanceRepository.Find(screeningEntry.AttendanceId ?? 0);

            screeningEntry.Id = 0;
            screeningEntry.ScreeningNo =
                _numberFormatRepository.GenerateNumber(attendace.IsTesting ? "TestingScreening" : "Screening");
            screeningEntry.EntryType = attendace.AttendanceType;
            screeningEntry.ProjectDesignPeriodId = attendace.ProjectDesignPeriodId;

            _screeningVisitRepository.ScreeningVisitSave(screeningEntry, attendace.ProjectDesignPeriodId, 0, _jwtTokenAccesser.GetClientDate(), attendace.StudyVersion);

            attendace.IsProcessed = true;
            _attendanceRepository.Update(attendace);
            screeningEntry.IsTesting = attendace.IsTesting;
            screeningEntry.ScreeningHistory = new ScreeningHistory();
            Add(screeningEntry);

            if (attendace.VolunteerId.HasValue)
            {
                var volunteer = _volunteerRepository.Find((int)attendace.VolunteerId);
                volunteer.IsScreening = true;
                _volunteerRepository.Update(volunteer);
            }
            _context.Save();


            _screeningVisitRepository.PatientStatus(screeningEntry.Id);
        }


        public ScreeningEntry SaveScreeningRandomization(SaveRandomizationDto saveRandomizationDto)
        {
            var screeningEntry = new ScreeningEntry();
            screeningEntry.Id = 0;

            var randomization = _randomizationRepository.Find(saveRandomizationDto.RandomizationId);

            var projectDetail = _projectRepository.All.Where(r => r.Id == randomization.ProjectId).Select(t => new { t.ParentProjectId, t.IsTestSite }).FirstOrDefault();

            var projectDesign = _projectDesignRepository.All.Where(r => r.ProjectId == projectDetail.ParentProjectId && r.DeletedDate == null).
                 Select(t => new
                 {
                     ProjectDesignId = t.Id,
                     ProjectDesignPeriodId = t.ProjectDesignPeriods.Where(x => x.DeletedDate == null).Select(a => a.Id).OrderByDescending(t => t).FirstOrDefault()
                 }).FirstOrDefault();
            randomization.PatientStatusId = ScreeningPatientStatus.OnTrial;
            screeningEntry.ProjectId = randomization.ProjectId;
            screeningEntry.ScreeningNo = _numberFormatRepository.GenerateNumber(projectDetail.IsTestSite ? "TestingScreening" : "Screening");
            screeningEntry.EntryType = DataEntryType.Randomization;
            screeningEntry.RandomizationId = saveRandomizationDto.RandomizationId;
            screeningEntry.ScreeningDate = saveRandomizationDto.VisitDate;
            screeningEntry.ProjectDesignId = projectDesign.ProjectDesignId;
            screeningEntry.ProjectDesignPeriodId = projectDesign.ProjectDesignPeriodId;
            screeningEntry.StudyVersion = randomization.StudyVersion;
            screeningEntry.ScreeningVisit = new List<ScreeningVisit>();

            _screeningVisitRepository.ScreeningVisitSave(screeningEntry, projectDesign.ProjectDesignPeriodId, saveRandomizationDto.ProjectDesignVisitId, saveRandomizationDto.VisitDate, randomization.StudyVersion);

            screeningEntry.IsTesting = projectDetail.IsTestSite;
            screeningEntry.ScreeningHistory = new ScreeningHistory();
            _randomizationRepository.Update(randomization);
            Add(screeningEntry);

            _context.Save();

            _context.DetachAllEntities();

            _screeningVisitRepository.PatientStatus(screeningEntry.Id);

            var screningVisit = screeningEntry.ScreeningVisit.Where(x => x.ProjectDesignVisitId == saveRandomizationDto.ProjectDesignVisitId).FirstOrDefault();

            _screeningVisitRepository.FindOpenVisitVarible(screningVisit.ProjectDesignVisitId, screningVisit.Id, saveRandomizationDto.VisitDate, screningVisit.ScreeningEntryId);
            _context.Save();

            var isScheduleReference = _projectScheduleRepository.All.Any(x => x.ProjectDesignVisitId == screningVisit.ProjectDesignVisitId && x.DeletedDate == null);
            if (screningVisit != null && isScheduleReference)
            {
                _screeningVisitRepository.ScheduleVisitUpdate(screeningEntry.Id);
            }

            return screeningEntry;
        }

        public List<AttendanceScreeningGridDto> GetScreeningList(ScreeningSearhParamDto searchParam)
        {
            var status = 0;
            if (searchParam.ScreeningStatus != null) status = (int)searchParam.ScreeningStatus;

            var attendanceResult = new List<AttendanceScreeningGridDto>();
            if (searchParam.ScreeningStatus == ScreeningTemplateStatus.Pending || status == 0)
            {
                searchParam.AttendanceType = DataEntryType.Screening;
                searchParam.IsFromScreening = true;
                attendanceResult.AddRange(_attendanceRepository.GetAttendaceList(searchParam));
            }

            if (searchParam.ScreeningStatus == ScreeningTemplateStatus.Pending) return attendanceResult;

            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<AttendanceScreeningGridDto>();

            var screeningEntries = All.Where(t => t.DeletedDate == null && projectList.Any(c => c == t.ProjectId))
                .AsQueryable();

            if (searchParam.Id > 0)
            {
                attendanceResult = attendanceResult.Where(x => x.Id == searchParam.Id).ToList();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(searchParam.TextSearch))
                {
                    //var volunterIds = _volunteerRepository.AutoCompleteSearch(searchParam.TextSearch.Trim());
                    //screeningEntries =
                    //    screeningEntries.Where(x => volunterIds.Any(a => a.Id == x.Attendance.VolunteerId));
                    var volunterIds = BarcodeSearch(searchParam.TextSearch.Trim());
                    screeningEntries = screeningEntries.Where(x => volunterIds.Any(a => a.Id == x.Attendance.VolunteerId));
                }

                //if (searchParam.FromDate.HasValue && searchParam.ToDate.HasValue)
                //    screeningEntries = screeningEntries.Where(x =>
                //        x.ScreeningDate.Date >= searchParam.FromDate.Value.Date &&
                //        x.ScreeningDate.Date <= searchParam.ToDate.Value.Date);

                //if (searchParam.FromDate.HasValue && searchParam.ToDate == null)
                //    screeningEntries =
                //        screeningEntries.Where(x => x.ScreeningDate.Date == searchParam.FromDate.Value.Date);

                //if (status > 0)
                //    screeningEntries = screeningEntries.Where(x => x.Status == searchParam.ScreeningStatus);

                //if (searchParam.IsFitnessFit.HasValue)
                //    screeningEntries = screeningEntries.Where(x => x.IsFitnessFit == searchParam.IsFitnessFit);
            }

            var role = _rolePermissionRepository.GetRolePermissionByScreenCode("mnu_underTesting");

            if (!role.IsView)
                screeningEntries = screeningEntries.Where(x => !x.IsTesting);

            screeningEntries = screeningEntries.Where(x => x.EntryType == DataEntryType.Screening);

            var items = screeningEntries.Select(x => new AttendanceScreeningGridDto
            {
                ScreeningEntryId = x.Id,
                VolunteerId = x.Attendance.VolunteerId,
                VolunteerName = string.IsNullOrEmpty(x.Attendance.Volunteer.FullName) ? $"{ x.Attendance.Volunteer.FirstName} {x.Attendance.Volunteer.MiddleName } {x.Attendance.Volunteer.LastName }" : x.Attendance.Volunteer.FullName, // Change by Tinku Mahato for null full name (28/06/2022)
                ProjectDesignId = x.ProjectDesignId,
                ScreeningNo = x.ScreeningNo,
                VolunteerNumber = x.Attendance.Volunteer.VolunteerNo,
                Gender = x.Attendance.Volunteer.GenderId.ToString(),
                ProjectCode = x.Project.ProjectCode,
                ProjectId = x.ProjectId,
                ProjectName = x.Project.ProjectName,
                ScreeningDate = x.ScreeningDate,
                AttendanceDate = x.Attendance.AttendanceDate,
                AttendedBy = x.Attendance.User.UserName,
                IsFitnessFit = x.IsFitnessFit == null ? "No" : x.IsFitnessFit == true ? "Yes" : "No",
            }).ToList();

            if (searchParam.Id > 0)
            {
                items = items.Where(x => x.VolunteerId == searchParam.Id).ToList();
            }

            //items.ForEach(data =>
            //{
            //    data.TemplateList = _screeningTemplateRepository.GetTemplateData(data.ScreeningEntryId, );
            //});

            items.AddRange(attendanceResult);



            if (searchParam.IsFitnessFit.HasValue)
                items = items.Where(x => x.IsFitnessFit == (searchParam.IsFitnessFit == true && searchParam.IsFitnessFit != null ? "Yes" : "No")).ToList();

            if (searchParam.FromDate.HasValue && searchParam.ToDate.HasValue)
                items = items.Where(x => x.ScreeningDate != null && x.ScreeningDate.Value.Date >= searchParam.FromDate.Value.Date &&
                    x.ScreeningDate.Value.Date <= searchParam.ToDate.Value.Date).ToList();

            if (searchParam.FromDate.HasValue && searchParam.ToDate == null)
                items = items.Where(x => x.ScreeningDate != null && x.ScreeningDate.Value.Date >= searchParam.FromDate.Value.Date).ToList();

            if (searchParam.ToDate.HasValue && searchParam.FromDate == null)
                items = items.Where(x => x.ScreeningDate != null && x.ScreeningDate.Value.Date <= searchParam.ToDate.Value.Date).ToList();

            if (searchParam.ProjectId > 0)
                items = items.Where(x => x.ProjectId == searchParam.ProjectId).ToList();

            //if (searchParam.VisitId > 0)
            //    items = items.Where(x => x.ProjectId == searchParam.VisitId).ToList();

            return items.OrderByDescending(x => x.ScreeningDate ?? DateTime.MaxValue).ToList();
        }


        public ScreeningGridDto GetScreeningDataList(ScreeningSearhParamDto searchParam)
        {
            var result = new ScreeningGridDto();
            var status = 0;
            if (searchParam.ScreeningStatus != null) status = (int)searchParam.ScreeningStatus;

            var attendanceResult = new List<AttendanceScreeningGridDto>();
            if (searchParam.ScreeningStatus == ScreeningTemplateStatus.Pending || status == 0)
            {
                searchParam.AttendanceType = DataEntryType.Screening;
                searchParam.IsFromScreening = true;
                attendanceResult.AddRange(_attendanceRepository.GetAttendaceList(searchParam));
            }

            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new ScreeningGridDto();

            var screeningEntries = All.Where(t => t.DeletedDate == null && projectList.Any(c => c == t.ProjectId))
                .AsQueryable();

            if (searchParam.Id > 0)
            {
                attendanceResult = attendanceResult.Where(x => x.Id == searchParam.Id).ToList();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(searchParam.TextSearch))
                {
                    var volunterIds = BarcodeSearch(searchParam.TextSearch.Trim());
                    screeningEntries = screeningEntries.Where(x => volunterIds.Any(a => a.Id == x.Attendance.VolunteerId));
                }

            }

            var role = _rolePermissionRepository.GetRolePermissionByScreenCode("mnu_underTesting");

            if (!role.IsView)
                screeningEntries = screeningEntries.Where(x => !x.IsTesting);

            screeningEntries = screeningEntries.Where(x => x.EntryType == DataEntryType.Screening);

            var items = screeningEntries.Select(x => new AttendanceScreeningGridDto
            {
                ScreeningEntryId = x.Id,
                VolunteerId = x.Attendance.VolunteerId,
                VolunteerName = string.IsNullOrEmpty(x.Attendance.Volunteer.FullName) ? $"{ x.Attendance.Volunteer.FirstName} {x.Attendance.Volunteer.MiddleName } {x.Attendance.Volunteer.LastName }" : x.Attendance.Volunteer.FullName, // Change by Tinku Mahato for null full name (28/06/2022)
                ProjectDesignId = x.ProjectDesignId,
                ScreeningNo = x.ScreeningNo,
                VolunteerNumber = x.Attendance.Volunteer.VolunteerNo,
                Gender = x.Attendance.Volunteer.GenderId.ToString(),
                ProjectCode = x.Project.ProjectCode,
                ProjectId = x.ProjectId,
                ProjectName = x.Project.ProjectName,
                ScreeningDate = x.ScreeningDate,
                AttendanceDate = x.Attendance.AttendanceDate,
                AttendedBy = x.Attendance.User.UserName,
                IsFitnessFit = x.IsFitnessFit == null ? "No" : x.IsFitnessFit == true ? "Yes" : "No",
                StudyId = x.StudyId,
                StudyCode = x.Study.ProjectCode,
                Notes = x.Notes
            }).ToList();

            if (searchParam.Id > 0)
            {
                items = items.Where(x => x.VolunteerId == searchParam.Id).ToList();
            }
            items.AddRange(attendanceResult);


            if (searchParam.IsFitnessFit.HasValue)
                items = items.Where(x => x.IsFitnessFit == (searchParam.IsFitnessFit == true && searchParam.IsFitnessFit != null ? "Yes" : "No")).ToList();

            //if (searchParam.FromDate.HasValue && searchParam.ToDate.HasValue)
            //    items = items.Where(x => x.ScreeningDate != null && x.ScreeningDate.Value.Date >= searchParam.FromDate.Value.Date &&
            //        x.ScreeningDate.Value.Date <= searchParam.ToDate.Value.Date).ToList();

            if (searchParam.FromDate.HasValue)
            {
                items = items.Where(x => x.ScreeningDate != null && x.ScreeningDate.Value.Date == searchParam.FromDate.Value.Date).ToList();
            }

            if (searchParam.AttendanceDate.HasValue)
            {
                items = items.Where(x => x.AttendanceDate.Date == searchParam.AttendanceDate.Value.Date).ToList();
            }

            //if (searchParam.ToDate.HasValue && searchParam.FromDate == null)
            //    items = items.Where(x => x.ScreeningDate != null && x.ScreeningDate.Value.Date <= searchParam.ToDate.Value.Date).ToList();

            if (searchParam.ProjectId > 0)
                items = items.Where(x => x.ProjectId == searchParam.ProjectId).ToList();

            items.ForEach(data =>
            {
                if (data.ScreeningEntryId > 0)
                    data.TemplateStatusList = _screeningTemplateRepository.GetTemplateStatus(searchParam.ProjectId, searchParam.VisitId, data.ScreeningEntryId);
            });

            items = items.OrderByDescending(x => x.ScreeningDate ?? DateTime.MaxValue).ToList();

            result.TemplateText = _screeningTemplateRepository.GetTemplateData(searchParam.ProjectId, searchParam.VisitId);
            result.Data = items;
            return result;
        }


        public IList<DropDownDto> AutoCompleteSearch(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return new List<DropDownDto>();
            searchText = searchText.Trim();
            var volunterIds = _volunteerRepository.AutoCompleteSearch(searchText, true);
            if (volunterIds == null || volunterIds.Count == 0) return new List<DropDownDto>();

            var query = _context.Volunteer.Where(x => volunterIds.Any(a => a.Id == x.Id)
                                                     && x.Attendances.Any(t =>
                                                         t.DeletedDate == null &&
                                                         t.AttendanceType == DataEntryType.Screening))
                .Select(t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.VolunteerNo + " " + t.FullName
                }).ToList();

            return query;
        }

        public IList<DropDownDto> BarcodeSearch(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return new List<DropDownDto>();
            searchText = searchText.Trim();
            var attendanceIds = _attendanceBarcodeGenerateRepository.FindByInclude(x => x.BarcodeString == searchText && x.DeletedBy == null).Select(t => new DropDownDto
            {
                Id = t.AttendanceId,
                Value = t.BarcodeString
            }).ToList();
            if (attendanceIds == null || attendanceIds.Count == 0) return new List<DropDownDto>();

            return attendanceIds;
        }

        public IList<DropDownDto> VolunteerSearch(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return new List<DropDownDto>();
            searchText = searchText.Trim();
            var volunterIds = _volunteerRepository.AutoCompleteSearch(searchText, true);
            if (volunterIds == null || volunterIds.Count == 0) return new List<DropDownDto>();

            var query = _context.Volunteer.Where(x => volunterIds.Select(z => z.Id).Contains(x.Id)
                        && x.Attendances.Any(t => t.DeletedDate == null && t.AttendanceType == DataEntryType.Screening))
                        .Select(t => new DropDownDto
                        {
                            Id = t.Id,
                            Value = t.VolunteerNo + " " + t.FirstName + " " + t.MiddleName + " " + t.LastName
                        }).ToList();

            return query;
        }


        public List<DropDownDto> GetProjectStatusAndLevelDropDown(int parentProjectId)
        {
            var result = new List<DropDownDto>();

            result.Add(new DropDownDto { Id = (int)ScreeningTemplateStatus.Pending, Value = ScreeningTemplateStatus.Pending.GetDescription(), ExtraData = false });
            result.Add(new DropDownDto { Id = (int)ScreeningTemplateStatus.InProcess, Value = ScreeningTemplateStatus.InProcess.GetDescription(), ExtraData = false });
            //result.Add(new DropDownDto { Id = (int)ScreeningStatus.Pending, Value = ScreeningStatus.Pending.GetDescription(), ExtraData = false });

            var projectDesign = _context.ProjectDesign.FirstOrDefault(x => x.ProjectId == parentProjectId);

            if (projectDesign != null)
            {
                var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesign.Id);
                workflowlevel.WorkFlowText.ForEach(x =>
                {
                    result.Add(new DropDownDto { Id = (int)x.LevelNo, Value = x.RoleName, ExtraData = true });
                });
            }
            result.Add(new DropDownDto { Id = (int)ScreeningTemplateStatus.Completed, Value = ScreeningTemplateStatus.Completed.GetDescription(), ExtraData = false });

            return result;
        }


        public IList<DropDownDto> GetSubjectByProjecId(int projectId)
        {
            var ParentProject = _context.Project.FirstOrDefault(x => x.Id == projectId).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == projectId).ToList().Select(x => x.Id).ToList();

            return All.Where(a => a.DeletedDate == null && ParentProject != null ? a.ProjectId == projectId : sites.Contains(a.ProjectId))
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.RandomizationId != null
                        ? Convert.ToString(x.Randomization.ScreeningNumber + " - " +
                                           x.Randomization.Initial +
                                           (x.Randomization.RandomizationNumber == null
                                               ? ""
                                               : " - " + x.Randomization.RandomizationNumber))
                        : Convert.ToString(
                            Convert.ToString(x.Attendance.ProjectSubject != null
                                ? x.Attendance.ProjectSubject.Number
                                : "") + " - " + x.Attendance.Volunteer.FullName),
                    Code = "Screening"
                }).Distinct().ToList();

        }

        public IList<DropDownDto> GetSubjectByProjecIdLocked(int projectId, bool isLock, bool isParent)
        {
            //var ParentProject = _context.Project.FirstOrDefault(x => x.Id == projectId).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == projectId).ToList().Select(x => x.Id).ToList();

            var subject = All.Include(a => a.Randomization).Include(a => a.Attendance).Include(a => a.ScreeningVisit)
                .ThenInclude(x => x.ScreeningTemplates).
                Where(a => a.DeletedDate == null && !isParent ? a.ProjectId == projectId : sites.Contains(a.ProjectId) // Change by Tinku for add separate dropdown for parent project (24/06/2022) 
               ).ToList();

            subject = subject.Where(x => x.ScreeningVisit.Where(z => z.ScreeningTemplates.Where(t => t.IsLocked == !isLock).Count() > 0 && z.ScreeningTemplates != null).Count() > 0
                        && x.ScreeningVisit != null).ToList();

            return subject.Select(x => new DropDownDto
            {
                Id = x.Id,
                Value = x.RandomizationId != null
                        ? Convert.ToString(x.Randomization.ScreeningNumber + " - " +
                                           x.Randomization.Initial +
                                           (x.Randomization.RandomizationNumber == null
                                               ? ""
                                               : " - " + x.Randomization.RandomizationNumber))
                        : Convert.ToString(
                            Convert.ToString(x.Attendance.ProjectSubject != null
                                ? x.Attendance.ProjectSubject.Number
                                : "") + " - " + x.Attendance.Volunteer.FullName),
                Code = "Screening",
            }).Distinct().ToList();
        }

        public IList<DropDownDto> GetPeriodByProjectIdIsLockedDropDown(LockUnlockDDDto lockUnlockDDDto)
        {
            // var ParentProject = _context.Project.FirstOrDefault(x => x.Id == lockUnlockDDDto.ChildProjectId).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == lockUnlockDDDto.ProjectId).ToList().Select(x => x.Id).ToList(); // Change by Tinku for add separate dropdown for parent project (24/06/2022) 

            var Period = All.Include(a => a.ProjectDesignPeriod).Include(a => a.ScreeningVisit).ThenInclude(a => a.ScreeningTemplates)
                 .Where(a => a.DeletedDate == null && lockUnlockDDDto.ChildProjectId > 0 ? a.ProjectId == lockUnlockDDDto.ChildProjectId : sites.Contains(a.ProjectId) // Change by Tinku for add separate dropdown for parent project (24/06/2022) 
                && (lockUnlockDDDto.SubjectIds == null || lockUnlockDDDto.SubjectIds.Contains(a.Id))).ToList();

            Period = Period.Where(a => a.ScreeningVisit.Where(z => z.ScreeningTemplates.Where(t => t.IsLocked == !lockUnlockDDDto.IsLock).Count() > 0
                  && z.ScreeningTemplates != null).Count() > 0
                && a.ScreeningVisit != null).ToList();

            return Period.GroupBy(x => x.ProjectDesignPeriodId).Select(x => new DropDownDto
            {
                Id = x.Key,
                Value = x.FirstOrDefault().ProjectDesignPeriod.DisplayName
            }).Distinct().ToList();
        }


        // Change by Tinku for add separate dropdown for parent project (24/06/2022) 
        public List<ProjectDropDown> GetSiteByLockUnlock(int parentProjectId, bool isLock)
        {
            var sites = _projectRepository.GetChildProjectDropDown(parentProjectId).Select(s => s.Id).ToList();
            var siteIds = _context.ScreeningTemplate.Where(q => sites.Contains(q.ScreeningVisit.ScreeningEntry.ProjectId) && q.IsLocked == (!isLock))
                .Select(s => s.ScreeningVisit.ScreeningEntry.ProjectId).Distinct().ToList();

            var resultSites = _projectRepository.GetChildProjectDropDown(parentProjectId).Where(q => siteIds.Contains(q.Id)).ToList();
            return resultSites;
        }

        public void SetFitnessValue(ScreeningTemplateValue screeningTemplateValueDto)
        {
            var ScreeningTemplate = _screeningTemplateRepository.Find(screeningTemplateValueDto.ScreeningTemplateId);
            var ScreeningVisit = _screeningVisitRepository.Find(ScreeningTemplate.ScreeningVisitId);

            var screeningEntry = All.Where(x => x.Id == ScreeningVisit.ScreeningEntryId).FirstOrDefault();
            var ScreeningHistory = _context.ScreeningHistory.Where(x => x.ScreeningEntryId == ScreeningVisit.ScreeningEntryId).FirstOrDefault();

            var projectDesignVariable = _context.ProjectDesignVariable.Include(x => x.Domain).Where(x => x.Id == screeningTemplateValueDto.ProjectDesignVariableId).FirstOrDefault();

            if (projectDesignVariable.Domain.DomainCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                if (projectDesignVariable.DesignOrder == 3 || projectDesignVariable.DesignOrder == 4 || projectDesignVariable.DesignOrder == 5)
                {
                    if (projectDesignVariable.VariableCode == ScreeningFitnessFitVariable.Note.GetDescription())
                    {
                        screeningEntry.FitnessNotes = screeningTemplateValueDto.Value;
                    }
                    else if (projectDesignVariable.VariableCode == ScreeningFitnessFitVariable.ProjectNumber.GetDescription())
                    {
                        screeningEntry.ProjectNo = screeningTemplateValueDto.Value;
                        ScreeningHistory.ProjectNumber = screeningTemplateValueDto.Value;
                    }
                    else if (projectDesignVariable.VariableCode == ScreeningFitnessFitVariable.Reason.GetDescription())
                    {
                        screeningEntry.FitnessReason = screeningTemplateValueDto.Value;
                        //ScreeningHistory.Reason = screeningTemplateValueDto.Value;
                    }
                }

                if (projectDesignVariable.DesignOrder == 1 || projectDesignVariable.DesignOrder == 2)
                {
                    var ProjectDesignVariableValue = _context.ProjectDesignVariableValue.Where(x => x.Id == Convert.ToInt32(screeningTemplateValueDto.Value)).FirstOrDefault();
                    if (projectDesignVariable.VariableCode == ScreeningFitnessFitVariable.FitnessFit.GetDescription())
                    {
                        screeningEntry.IsFitnessFit = ProjectDesignVariableValue.ValueName == "Yes" ? true : false;
                    }
                    else if (projectDesignVariable.VariableCode == ScreeningFitnessFitVariable.Enrolled.GetDescription())
                    {
                        screeningEntry.IsEnrolled = ProjectDesignVariableValue.ValueName == "Yes" ? true : false;
                        ScreeningHistory.Enrolled = ProjectDesignVariableValue.ValueName == "Yes" ? true : false;
                    }
                }

                Update(screeningEntry);
                _context.ScreeningHistory.Update(ScreeningHistory);
            }

        }
        public List<ScreeningEntryStudyHistoryDto> GetVolunteerProjectHistory(int ScreeningEntryId)
        {
            var data = _context.ScreeningEntryStudyHistory.Where(x => x.ScreeningEntryId == ScreeningEntryId).Select(x => new ScreeningEntryStudyHistoryDto
            {
                Id = x.Id,
                VolunteerNo = x.ScreeningEntry.Attendance.Volunteer.VolunteerNo,
                ProjectNo = x.Study.ProjectCode,
                Notes = x.Notes,
                CreatedDate = x.CreatedDate,
                RoleName = _context.SecurityRole.Where(z => z.Id == x.RoleId).FirstOrDefault() != null ?
                 _context.SecurityRole.Where(z => z.Id == x.RoleId).FirstOrDefault().RoleName : "",
                CreatedByName = x.CreatedByUser.UserName
            }).OrderByDescending(x => x.Id).ToList();
            return data;
        }
    }
}