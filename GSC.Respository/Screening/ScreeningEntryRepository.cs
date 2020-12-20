using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
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
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IGSCContext _context;
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
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
        IRolePermissionRepository rolePermissionRepository)
            : base(context)
        {
            _volunteerRepository = volunteerRepository;
            _projectRightRepository = projectRightRepository;
            _attendanceRepository = attendanceRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _randomizationRepository = randomizationRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _numberFormatRepository = numberFormatRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _screeningVisitRepository = screeningVisitRepository;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectDesignRepository = projectDesignRepository;
            _projectRepository = projectRepository;
            _context = context;
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
                    ProjectNo = t.ProjectNo,
                    ProjectDesignId = t.ProjectDesignId,
                    FitnessReason = t.FitnessReason,
                    FitnessNotes = t.FitnessNotes,
                    PatientStatusName = t.RandomizationId != null ? t.Randomization.PatientStatusId.GetDescription() : "",
                    Progress = t.Progress,
                    ProjectDesignPeriodId = t.ProjectDesignPeriodId,
                    ProjectId = t.ProjectId
                }).FirstOrDefault();

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(screeningEntryDto.ProjectDesignId);

            bool myReview = false;
            screeningEntryDto.ScreeningVisits = VisitTemplateProcess(screeningEntryDto.Id, workflowlevel, ref myReview);

            screeningEntryDto.IsElectronicSignature = workflowlevel.IsElectricSignature;


            screeningEntryDto.IsMultipleVisits = screeningEntryDto.ScreeningVisits.Select(s => s.ProjectDesignVisitId).Distinct().Count() > 1;

            screeningEntryDto.MyReview = myReview;

            if (workflowlevel != null && workflowlevel.WorkFlowText != null)
            {
                screeningEntryDto.WorkFlowText = new List<WorkFlowText>
                {
                    new WorkFlowText { LevelNo=-1,RoleName="Operator"}
                };
                screeningEntryDto.WorkFlowText.AddRange(workflowlevel.WorkFlowText);
                screeningEntryDto.WorkFlowText.Add(new WorkFlowText
                {
                    LevelNo = 0,
                    RoleName = "Independent"
                });
                screeningEntryDto.LevelName1 = workflowlevel.WorkFlowText.Where(r => r.LevelNo == 1).Select(t => t.RoleName).FirstOrDefault();
                screeningEntryDto.IsSystemQueryUpdate = workflowlevel.IsStartTemplate;
            }

            return screeningEntryDto;
        }

        private List<ScreeningVisitTree> VisitTemplateProcess(int screeningEntryId, WorkFlowLevelDto workflowlevel, ref bool myReview)
        {
            var screeningTemplateValue = _screeningTemplateValueRepository
                .FindBy(x => x.DeletedDate == null
                && x.ProjectDesignVariable.DeletedDate == null
                && x.ScreeningTemplate.ScreeningVisit.ScreeningEntryId == screeningEntryId).
                Select(r => new Data.Dto.Screening.ScreeningTemplateValueBasic
                {
                    ScreeningTemplateId = r.ScreeningTemplateId,
                    QueryStatus = r.QueryStatus
                }).ToList();


            var visits = _screeningVisitRepository.GetVisitTree(screeningEntryId);
            var templates = _screeningTemplateRepository.GetTemplateTree(screeningEntryId, screeningTemplateValue, workflowlevel);

            visits.ForEach(x =>
            {
                x.ScreeningTemplates = templates.Where(a => a.ScreeningVisitId == x.ScreeningVisitId && a.ParentId == null).OrderBy(c => c.DesignOrder).ToList();
                x.ScreeningTemplates.ForEach(v => v.Children = templates.Where(a => a.ParentId == v.Id).OrderBy(c => c.DesignOrder).ToList());

                x.IsVisitRepeated = x.ParentScreeningVisitId != null ? false :
                    workflowlevel.IsStartTemplate &&
                    templates.Any(t => t.Status > ScreeningTemplateStatus.Pending && t.ScreeningVisitId == x.ScreeningVisitId) && x.IsVisitRepeated ? true : false;

                if (x.VisitStatus == ScreeningVisitStatus.Missed || x.VisitStatus == ScreeningVisitStatus.Missed)
                    x.IsVisitRepeated = false;
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

            _screeningVisitRepository.ScreeningVisitSave(screeningEntry, attendace.ProjectDesignPeriodId, 0, System.DateTime.Now);

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

            var parentProjectId = _projectRepository.All.Where(r => r.Id == randomization.ProjectId).Select(t => t.ParentProjectId).FirstOrDefault();
            var projectDesign = _projectDesignRepository.All.Where(r => r.ProjectId == parentProjectId && r.DeletedDate == null).
                 Select(t => new
                 {
                     t.IsUnderTesting,
                     ProjectDesignId = t.Id,
                     ProjectDesignPeriodId = t.ProjectDesignPeriods.Where(x => x.DeletedDate == null).Select(a => a.Id).OrderByDescending(t => t).FirstOrDefault()
                 }).FirstOrDefault();
            randomization.PatientStatusId = ScreeningPatientStatus.OnTrial;
            screeningEntry.ProjectId = randomization.ProjectId;
            screeningEntry.ScreeningNo = _numberFormatRepository.GenerateNumber(projectDesign.IsUnderTesting ? "TestingScreening" : "Screening");
            screeningEntry.EntryType = DataEntryType.Randomization;
            screeningEntry.RandomizationId = saveRandomizationDto.RandomizationId;
            screeningEntry.ScreeningDate = saveRandomizationDto.VisitDate;
            screeningEntry.ProjectDesignId = projectDesign.ProjectDesignId;
            screeningEntry.ProjectDesignPeriodId = projectDesign.ProjectDesignPeriodId;
            screeningEntry.ScreeningVisit = new List<ScreeningVisit>();

            _screeningVisitRepository.ScreeningVisitSave(screeningEntry, projectDesign.ProjectDesignPeriodId, saveRandomizationDto.ProjectDesignVisitId, saveRandomizationDto.VisitDate);

            screeningEntry.IsTesting = projectDesign.IsUnderTesting;
            screeningEntry.ScreeningHistory = new ScreeningHistory();
            _randomizationRepository.Update(randomization);
            Add(screeningEntry);

            _context.Save();

            _context.DetachAllEntities();

            _screeningVisitRepository.PatientStatus(screeningEntry.Id);

            var screningVisit = screeningEntry.ScreeningVisit.Where(x => x.ProjectDesignVisitId == saveRandomizationDto.ProjectDesignVisitId).FirstOrDefault();

            if (screningVisit != null)
            {
                _screeningVisitRepository.FindOpenVisitVarible(screningVisit.ProjectDesignVisitId, screningVisit.Id, saveRandomizationDto.VisitDate, screningVisit.ScreeningEntryId);
                _context.Save();
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

            var screeningEntries = All.Where(t => t.DeletedDate == null
                                                  && projectList.Any(c => c == t.ProjectId))
                .AsQueryable();

            if (searchParam.Id > 0)
            {
                screeningEntries = screeningEntries.Where(x => x.Attendance.VolunteerId == searchParam.Id);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(searchParam.TextSearch))
                {
                    var volunterIds = _volunteerRepository.AutoCompleteSearch(searchParam.TextSearch.Trim());
                    screeningEntries =
                        screeningEntries.Where(x => volunterIds.Any(a => a.Id == x.Attendance.VolunteerId));
                }

                if (searchParam.FromDate.HasValue && searchParam.ToDate.HasValue)
                    screeningEntries = screeningEntries.Where(x =>
                        x.ScreeningDate.Date >= searchParam.FromDate.Value.Date &&
                        x.ScreeningDate.Date <= searchParam.ToDate.Value.Date);

                if (searchParam.FromDate.HasValue && searchParam.ToDate == null)
                    screeningEntries =
                        screeningEntries.Where(x => x.ScreeningDate.Date == searchParam.FromDate.Value.Date);

                //if (status > 0)
                //    screeningEntries = screeningEntries.Where(x => x.Status == searchParam.ScreeningStatus);

                if (searchParam.IsFitnessFit.HasValue)
                    screeningEntries = screeningEntries.Where(x => x.IsFitnessFit == searchParam.IsFitnessFit);
            }

            var role = _rolePermissionRepository.GetRolePermissionByScreenCode("mnu_underTesting");

            if (!role.IsView)
                screeningEntries = screeningEntries.Where(x => !x.IsTesting);

            screeningEntries = screeningEntries.Where(x => x.EntryType == DataEntryType.Screening);

            var items = screeningEntries.Select(x => new AttendanceScreeningGridDto
            {
                ScreeningEntryId = x.Id,
                VolunteerId = x.Attendance.VolunteerId,
                VolunteerName = x.Attendance.Volunteer.FullName,
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
                IsFitnessFit = x.IsFitnessFit == null ? "" : x.IsFitnessFit == true ? "Yes" : "No"
            }).ToList();

            items.AddRange(attendanceResult);
            return items.OrderByDescending(x => x.ScreeningDate).ToList();
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

      

        public ScreeningSummaryDto GetSummary(int id)
        {
            var values = (from screening in _context.ScreeningEntry.Where(t => t.Id == id)
                          join template in _context.ScreeningTemplate on screening.Id equals template.ScreeningVisit.ScreeningEntryId
                          join value in _context.ScreeningTemplateValue on template.Id equals value.ScreeningTemplateId
                          select new ScreeningSummaryValue
                          {
                              Id = value.Id,
                              Value = value.Value,
                              ProjectDesignVariableId = value.ProjectDesignVariableId
                          }).ToList();

            var summary = _context.ScreeningEntry.Where(t => t.Id == id)
                .Include(t => t.Attendance)
                .ThenInclude(t => t.ProjectDesignPeriod)
                .ThenInclude(t => t.VisitList)
                .ThenInclude(t => t.Templates)
                .ThenInclude(t => t.Variables)
                .ThenInclude(t => t.VariableCategory)
                .Include(t => t.Attendance)
                .ThenInclude(t => t.ProjectDesignPeriod)
                .ThenInclude(t => t.VisitList)
                .ThenInclude(t => t.Templates)
                .ThenInclude(t => t.Variables)
                .ThenInclude(t => t.Values)
                .Select(screening => new ScreeningSummaryDto
                {
                    Visits = screening.Attendance.ProjectDesignPeriod.VisitList.Where(d => d.DeletedDate == null)
                        .Select(visit => new ScreeningSummaryVisit
                        {
                            Name = visit.DisplayName,
                            Templates = visit.Templates.Where(d => d.DeletedDate == null).Select(template =>
                                new ScreeningSummaryTemplate
                                {
                                    Name = template.TemplateName,
                                    DesignOrder = template.DesignOrder,
                                    Variables = template.Variables.Where(d => d.DeletedDate == null).Select(variable =>
                                        new ScreeningSummaryVariable
                                        {
                                            ProjectDesignVariableId = variable.Id,
                                            DesignOrder = variable.DesignOrder,
                                            VariableCategoryName = variable.VariableCategoryId == null
                                                ? ""
                                                : variable.VariableCategory.CategoryName,
                                            Name = variable.VariableName,
                                            Values = variable.Values,
                                            CollectionSource = variable.CollectionSource
                                        }).ToList()
                                }).OrderBy(a => a.DesignOrder).ToList()
                        }).ToList()
                }).FirstOrDefault();

            summary.Visits.ToList().ForEach(visit =>
            {
                visit.Templates.ToList().ForEach(template =>
                {
                    template.Variables.ToList().ForEach(variable =>
                    {
                        var screeningValue = values.FirstOrDefault(t =>
                            t.ProjectDesignVariableId == variable.ProjectDesignVariableId);
                        if (screeningValue != null)
                        {
                            variable.Value = screeningValue.Value;
                            if (variable.CollectionSource == CollectionSources.CheckBox ||
                                variable.CollectionSource == CollectionSources.ComboBox ||
                                variable.CollectionSource == CollectionSources.MultiCheckBox ||
                                variable.CollectionSource == CollectionSources.RadioButton)
                                if (variable.Values != null && variable.Values.Any())
                                {
                                    if (variable.CollectionSource == CollectionSources.ComboBox ||
                                        variable.CollectionSource == CollectionSources.RadioButton)
                                    {
                                        var value = variable.Values.FirstOrDefault(d =>
                                            d.Id == Convert.ToInt32(screeningValue.Value));
                                        if (value != null) variable.Value = value.ValueName;
                                    }
                                    else if (variable.CollectionSource == CollectionSources.CheckBox)
                                    {
                                        var value = variable.Values.FirstOrDefault();
                                        variable.Items = new List<ScreeningSummaryValueItem>
                                        {
                                            new ScreeningSummaryValueItem
                                            {
                                                Name = value.ValueName,
                                                Selected = Convert.ToBoolean(screeningValue.Value)
                                            }
                                        };
                                    }
                                    else if (variable.CollectionSource == CollectionSources.MultiCheckBox)
                                    {
                                        variable.Items = new List<ScreeningSummaryValueItem>();
                                        var childValues = _context.ScreeningTemplateValueChild
                                            .Where(c => c.ScreeningTemplateValueId == screeningValue.Id).ToList();
                                        variable.Values.ToList().ForEach(value =>
                                        {
                                            var childValue = childValues.FirstOrDefault(t =>
                                                t.ProjectDesignVariableValueId == value.Id);
                                            var selected = false;
                                            if (childValue != null) selected = Convert.ToBoolean(childValue.Value);

                                            variable.Items.Add(
                                                new ScreeningSummaryValueItem
                                                {
                                                    Name = value.ValueName,
                                                    Selected = selected
                                                });
                                        });
                                    }
                                }
                        }

                        variable.Values = null;
                    });
                });
            });

            return summary;
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

        public IList<DropDownDto> GetSubjectByProjecIdLocked(int projectId, bool isLock)
        {
            var ParentProject = _context.Project.FirstOrDefault(x => x.Id == projectId).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == projectId).ToList().Select(x => x.Id).ToList();

            var subject = All.Include(a => a.Randomization).Include(a => a.Attendance).Include(a => a.ScreeningVisit)
                .ThenInclude(x => x.ScreeningTemplates).
                Where(a => a.DeletedDate == null && ParentProject != null ? a.ProjectId == projectId : sites.Contains(a.ProjectId)
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
            var ParentProject = _context.Project.FirstOrDefault(x => x.Id == lockUnlockDDDto.ChildProjectId).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == lockUnlockDDDto.ChildProjectId).ToList().Select(x => x.Id).ToList();

            var Period = All.Include(a => a.ProjectDesignPeriod).Include(a => a.ScreeningVisit).ThenInclude(a => a.ScreeningTemplates)
                .Where(a => a.DeletedDate == null && ParentProject != null ? a.ProjectId == lockUnlockDDDto.ChildProjectId : sites.Contains(a.ProjectId)
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
    }
}