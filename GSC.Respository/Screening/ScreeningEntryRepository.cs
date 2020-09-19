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
using GSC.Respository.Project.Design;
using GSC.Respository.Project.EditCheck;
using GSC.Respository.Project.Workflow;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;
using GSC.Respository.Volunteer;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class ScreeningEntryRepository : GenericRespository<ScreeningEntry, GscContext>, IScreeningEntryRepository
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly INumberFormatRepository _numberFormatRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        public ScreeningEntryRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IVolunteerRepository volunteerRepository,
            IProjectRightRepository projectRightRepository,
            IAttendanceRepository attendanceRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            INumberFormatRepository numberFormatRepository,
            IRolePermissionRepository rolePermissionRepository)
            : base(uow, jwtTokenAccesser)
        {
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _volunteerRepository = volunteerRepository;
            _projectRightRepository = projectRightRepository;
            _attendanceRepository = attendanceRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _numberFormatRepository = numberFormatRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _uow = uow;
        }

        public ScreeningEntryDto GetDetails(int id)
        {
            var screeningTemplateValue = _screeningTemplateValueRepository
                .FindBy(x => x.DeletedDate == null
                && x.ScreeningTemplate.ScreeningVisit.ScreeningEntryId == id).
                Select(r => new Data.Dto.Screening.ScreeningTemplateValueBasic
                {
                    ScreeningTemplateId = r.ScreeningTemplateId,
                    QueryStatus = r.QueryStatus
                }).ToList();

            var screeningEntryDto = Context.ScreeningEntry.Where(t => t.Id == id)
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
                    Progress = t.Progress,
                    ProjectDesignPeriodId = t.Attendance.ProjectDesignPeriodId,
                    ProjectId = t.ProjectId
                }).FirstOrDefault();

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(screeningEntryDto.ProjectDesignId);

            var templates = _screeningTemplateRepository.GetTemplateTree(screeningEntryDto.Id, screeningTemplateValue,
                    workflowlevel);

            screeningEntryDto.ScreeningTemplates = templates.Where(r => r.ParentId == null).ToList();

            screeningEntryDto.IsElectronicSignature = workflowlevel.IsElectricSignature;

            screeningEntryDto.ScreeningTemplates.ForEach(x =>
            {
                x.Children = templates.Where(c => c.ParentId == x.Id).ToList();
            });


            screeningEntryDto.IsMultipleVisits = screeningEntryDto.ScreeningTemplates
                                                     .Select(s => s.ProjectDesignVisitName).Distinct().Count() > 1;

            screeningEntryDto.MyReview = screeningEntryDto.ScreeningTemplates.Any(x => x.MyReview);

            if (workflowlevel != null && workflowlevel.WorkFlowText != null)
            {
                screeningEntryDto.WorkFlowText = new List<WorkFlowText>
                {
                    new WorkFlowText
                    {
                        LevelNo=-1,
                        RoleName="Operator"
                    }
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

        public void SaveScreening(ScreeningEntry screeningEntry, List<int> projectAttendanceTemplateIds)
        {
            var attendace = Context.Attendance.Find(screeningEntry.AttendanceId);

            screeningEntry.Id = 0;
            screeningEntry.ScreeningNo =
                _numberFormatRepository.GenerateNumber(attendace.IsTesting ? "TestingScreening" : "Screening");
            screeningEntry.EntryType = attendace.AttendanceType;
            screeningEntry.ProjectDesignPeriodId = attendace.ProjectDesignPeriodId;
            screeningEntry.ScreeningVisit = new List<ScreeningVisit>();

            var designVisits = _projectDesignVisitRepository.GetVisitAndTemplateByPeriordId(attendace.ProjectDesignPeriodId);

            designVisits.ForEach(r =>
            {
                var screeningVisit = new ScreeningVisit
                {

                    ProjectDesignVisitId = r.Id,
                    Status = ScreeningVisitStatus.NotStarted
                };

                r.Templates.ForEach(t =>
                {
                    screeningVisit.ScreeningTemplates.Add(new ScreeningTemplate
                    {
                        ProjectDesignTemplateId = t.Id,
                        Status = ScreeningTemplateStatus.Pending
                    });
                });

                screeningEntry.ScreeningVisit.Add(screeningVisit);
            });

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
        }

        public List<AttendanceScreeningGridDto> GetScreeningList(ScreeningSearhParamDto searchParam)
        {
            var status = 0;
            if (searchParam.ScreeningStatus != null) status = (int)searchParam.ScreeningStatus;

            var attendanceResult = new List<AttendanceScreeningGridDto>();
            if (searchParam.ScreeningStatus == ScreeningTemplateStatus.Pending || status == 0)
            {
                searchParam.AttendanceType = AttendanceType.Screening;
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

            screeningEntries = screeningEntries.Where(x => x.EntryType == AttendanceType.Screening);

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

            var query = Context.Volunteer.Where(x => volunterIds.Any(a => a.Id == x.Id)
                                                     && x.Attendances.Any(t =>
                                                         t.DeletedDate == null &&
                                                         t.AttendanceType == AttendanceType.Screening))
                .Select(t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.VolunteerNo + " " + t.FullName
                }).ToList();

            return query;
        }

        public IList<ScreeningAuditDto> GetAuditHistory(int id)
        {
            var auditDtos = (from screening in Context.ScreeningEntry.Where(t => t.Id == id)
                             join template in Context.ScreeningTemplate on screening.Id equals template.ScreeningVisit.ScreeningEntryId
                             join value in Context.ScreeningTemplateValue on template.Id equals value.ScreeningTemplateId
                             join audit in Context.ScreeningTemplateValueAudit on value.Id equals audit.ScreeningTemplateValueId
                             join reasonTemp in Context.AuditReason on audit.ReasonId equals reasonTemp.Id into reasonDt
                             from reason in reasonDt.DefaultIfEmpty()
                             join designVerialbe in Context.ProjectDesignVariable on value.ProjectDesignVariableId equals
                                 designVerialbe.Id
                             join designTemplate in Context.ProjectDesignTemplate on template.ProjectDesignTemplateId equals
                                 designTemplate.Id
                             join designVisit in Context.ProjectDesignVisit on designTemplate.ProjectDesignVisitId equals
                                 designVisit
                                     .Id
                             join userTemp in Context.Users on audit.UserId equals userTemp.Id into userDto
                             from user in userDto.DefaultIfEmpty()
                             join roleTemp in Context.SecurityRole on audit.UserRoleId equals roleTemp.Id into roleDto
                             from role in roleDto.DefaultIfEmpty()
                             select new ScreeningAuditDto
                             {
                                 CreatedDate = audit.CreatedDate,
                                 IpAddress = audit.IpAddress,
                                 NewValue = audit.Value,
                                 Note = audit.Note,
                                 OldValue = !string.IsNullOrEmpty(audit.Value) && string.IsNullOrEmpty(audit.OldValue)
                                     ? "Default"
                                     : audit.OldValue,
                                 Reason = reason.ReasonName,
                                 Role = role.RoleName,
                                 Template = designTemplate.TemplateName,
                                 TimeZone = audit.TimeZone,
                                 User = user.UserName,
                                 Variable = designVerialbe.VariableName,
                                 Visit = designVisit.DisplayName
                             }).OrderBy(t => t.Visit).ThenBy(t => t.Template).ThenBy(t => t.Variable)
                .ThenByDescending(t => t.CreatedDate).ToList();

            return auditDtos;
        }

        public ScreeningSummaryDto GetSummary(int id)
        {
            var values = (from screening in Context.ScreeningEntry.Where(t => t.Id == id)
                          join template in Context.ScreeningTemplate on screening.Id equals template.ScreeningVisit.ScreeningEntryId
                          join value in Context.ScreeningTemplateValue on template.Id equals value.ScreeningTemplateId
                          select new ScreeningSummaryValue
                          {
                              Id = value.Id,
                              Value = value.Value,
                              ProjectDesignVariableId = value.ProjectDesignVariableId
                          }).ToList();

            var summary = Context.ScreeningEntry.Where(t => t.Id == id)
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

            summary.Visits.ForEach(visit =>
            {
                visit.Templates.ForEach(template =>
                {
                    template.Variables.ForEach(variable =>
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
                                        var childValues = Context.ScreeningTemplateValueChild
                                            .Where(c => c.ScreeningTemplateValueId == screeningValue.Id).ToList();
                                        variable.Values.ForEach(value =>
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

            var projectDesign = Context.ProjectDesign.FirstOrDefault(x => x.ProjectId == parentProjectId);

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
    }
}