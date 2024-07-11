using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringReportRepository : GenericRespository<CtmsMonitoringReport>, ICtmsMonitoringReportRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly ICtmsMonitoringReportReviewRepository _ctmsMonitoringReportReviewRepository;
        private readonly ICtmsMonitoringReportVariableValueRepository _ctmsMonitoringReportVariableValueRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly ICtmsMonitoringReportVariableValueChildRepository _ctmsMonitoringReportVariableValueChildRepository;
        public CtmsMonitoringReportRepository(IGSCContext context,
            ICtmsMonitoringReportReviewRepository ctmsMonitoringReportReviewRepository,
            ICtmsMonitoringReportVariableValueRepository ctmsMonitoringReportVariableValueRepository,
            IUploadSettingRepository uploadSettingRepository,
            ICtmsMonitoringReportVariableValueChildRepository ctmsMonitoringReportVariableValueChildRepository, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _context = context;
            _ctmsMonitoringReportReviewRepository = ctmsMonitoringReportReviewRepository;
            _ctmsMonitoringReportVariableValueRepository = ctmsMonitoringReportVariableValueRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _ctmsMonitoringReportVariableValueChildRepository = ctmsMonitoringReportVariableValueChildRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        //Get ctms monitoring report variable value start

        public CtmsMonitoringReportFormDto GetCtmsMonitoringReportVariableValue(CtmsMonitoringReportFormDto designTemplateDto, int ctmsMonitoringReportId)
        {
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var ctmsMonitoringReport = All.FirstOrDefault(x => x.Id == ctmsMonitoringReportId && x.DeletedDate == null);
            var ctmsMonitoringReportFormBasic = GetFormBasic(ctmsMonitoringReportId);

            PopulateBasicInfo(designTemplateDto, ctmsMonitoringReportId, ctmsMonitoringReportFormBasic.ProjectId, ctmsMonitoringReport);

            var reviewPerson = _ctmsMonitoringReportReviewRepository.GetReview(ctmsMonitoringReportId);

            InitializeVariableLevels(designTemplateDto);

            var values = _ctmsMonitoringReportVariableValueRepository.GetVariableValues(ctmsMonitoringReportFormBasic.Id);

            values.ForEach(value => UpdateVariableValues(designTemplateDto, value, reviewPerson, documentUrl));

            return designTemplateDto;
        }

        private void PopulateBasicInfo(CtmsMonitoringReportFormDto designTemplateDto, int ctmsMonitoringReportId, int projectId, CtmsMonitoringReport ctmsMonitoringReport)
        {
            designTemplateDto.CtmsMonitoringReportId = ctmsMonitoringReportId;
            designTemplateDto.ProjectId = projectId;

            if (ctmsMonitoringReport != null)
            {
                designTemplateDto.IsSender = ctmsMonitoringReport.CreatedBy == _jwtTokenAccesser.UserId;
                designTemplateDto.ReportStatus = ctmsMonitoringReport.ReportStatus;
                designTemplateDto.VariableDisable = ctmsMonitoringReport.ReportStatus == MonitoringReportStatus.ReviewInProgress
                                                    || ctmsMonitoringReport.ReportStatus == MonitoringReportStatus.Approved;
            }
        }

        private void InitializeVariableLevels(CtmsMonitoringReportFormDto designTemplateDto)
        {
            designTemplateDto.Variables.Where(x => x.CollectionSource == CollectionSources.Table).ToList().ForEach(t =>
            {
                t.Values.Where(c => c.LevelNo == null).ToList().ForEach(v => v.LevelNo = 1);
            });
        }

        private void UpdateVariableValues(CtmsMonitoringReportFormDto designTemplateDto, CtmsMonitoringReportVariableValueBasic value, bool reviewPerson, string documentUrl)
        {
            var screeningTemplateValueChild = GetCTMSTemplateValueChild(value.Id);
            var maxLevel = screeningTemplateValueChild.Max(x => x.LevelNo);

            var variable = designTemplateDto.Variables.FirstOrDefault(v => v.StudyLevelFormVariableId == value.StudyLevelFormVariableId);
            if (variable != null)
            {
                UpdateVariable(variable, value, reviewPerson, documentUrl, maxLevel ?? 0);
            }
        }

        private void UpdateVariable(StudyLevelFormVariableDto variable, CtmsMonitoringReportVariableValueBasic value, bool reviewPerson, string documentUrl, short maxLevel)
        {
            variable.VariableValue = value.Value;
            variable.VariableValueOld = value.Value;
            variable.CtmsMonitoringReportVariableValueId = value.Id;
            variable.HasComments = value.IsComment;
            variable.IsReviewPerson = reviewPerson;
            variable.QueryStatus = value.QueryStatus;
            variable.IsNaValue = value.IsNa;
            variable.IsValid = !string.IsNullOrWhiteSpace(variable.VariableValue) || variable.IsNaValue;
            variable.DocPath = value.DocPath;
            variable.DocFullPath = value.DocPath != null ? $"{documentUrl}{value.DocPath}" : null;

            if (variable.Values != null)
            {
                if (variable.CollectionSource == CollectionSources.CheckBox || variable.CollectionSource == CollectionSources.MultiCheckBox)
                {
                    UpdateCheckBoxValues(variable, value);
                }
                else if (variable.CollectionSource == CollectionSources.Table)
                {
                    UpdateTableValues(variable, value, maxLevel);
                }
            }
        }

        private void UpdateCheckBoxValues(StudyLevelFormVariableDto variable, CtmsMonitoringReportVariableValueBasic value)
        {
            variable.Values.ToList().ForEach(val =>
            {
                var childValue = value.Children.FirstOrDefault(v => v.StudyLevelFormVariableValueId == val.Id);
                if (childValue != null)
                {
                    variable.IsValid = true;
                    val.VariableValue = childValue.Value;
                    val.VariableValueOld = childValue.Value;
                    val.CtmsMonitoringReportVariableValueChildId = childValue.Id;
                }
            });
        }

        private void UpdateTableValues(StudyLevelFormVariableDto variable, CtmsMonitoringReportVariableValueBasic value, int maxLevel)
        {
            var valuesList = new List<StudyLevelFormVariableValueDto>();

            variable.Values.ToList().ForEach(val =>
            {
                var notExistLevel = Enumerable.Range(1, maxLevel > 0 ? maxLevel : 1).ToArray();
                var childValue = GetChildValues(value, val);

                AddMissingLevels(value, val, notExistLevel, childValue);

                if (!childValue.Any() && !notExistLevel.Any())
                {
                    AddDefaultLevel(value, val, childValue);
                }

                valuesList.AddRange(childValue.Select(child => CreateValueDto(val, child)));
            });

            variable.Values = valuesList.Where(x => x.IsDeleted).ToList();
        }

        private List<CtmsMonitoringReportVariableValueChild> GetChildValues(CtmsMonitoringReportVariableValueBasic value, StudyLevelFormVariableValueDto val)
        {
            return value.Children
                .Where(v => v.StudyLevelFormVariableValueId == val.Id)
                .GroupBy(x => x.LevelNo)
                .Select(x => new CtmsMonitoringReportVariableValueChild
                {
                    Id = x.First().Id,
                    CtmsMonitoringReportVariableValueId = x.First().CtmsMonitoringReportVariableValueId,
                    StudyLevelFormVariableValueId = x.First().StudyLevelFormVariableValueId,
                    Value = x.First().Value,
                    LevelNo = x.First().LevelNo,
                    DeletedDate = x.First().DeletedDate
                }).ToList();
        }

        private void AddMissingLevels(CtmsMonitoringReportVariableValueBasic value, StudyLevelFormVariableValueDto val, int[] notExistLevel, List<CtmsMonitoringReportVariableValueChild> childValue)
        {
            var levels = notExistLevel.Where(x => !childValue.Select(y => (int)y.LevelNo).Contains(x)).ToList();

            levels.ForEach(level =>
            {
                childValue.Add(new CtmsMonitoringReportVariableValueChild
                {
                    Id = 0,
                    CtmsMonitoringReportVariableValueId = value.Id,
                    StudyLevelFormVariableValueId = val.Id,
                    Value = null,
                    LevelNo = (short)level
                });
            });
        }

        private void AddDefaultLevel(CtmsMonitoringReportVariableValueBasic value, StudyLevelFormVariableValueDto val, List<CtmsMonitoringReportVariableValueChild> childValue)
        {
            childValue.Add(new CtmsMonitoringReportVariableValueChild
            {
                Id = 0,
                CtmsMonitoringReportVariableValueId = value.Id,
                StudyLevelFormVariableValueId = val.Id,
                Value = null,
                LevelNo = 1
            });
        }

        private StudyLevelFormVariableValueDto CreateValueDto(StudyLevelFormVariableValueDto val, CtmsMonitoringReportVariableValueChild child)
        {
            return new StudyLevelFormVariableValueDto
            {
                Id = child.StudyLevelFormVariableValueId,
                VariableValue = child.Value,
                VariableValueOld = child.Value,
                CtmsMonitoringReportVariableValueChildId = child.Id,
                LevelNo = child.LevelNo,
                ValueName = val.ValueName,
                IsDeleted = child.DeletedDate != null,
                TableCollectionSource = val.TableCollectionSource
            };
        }


        //ctms monitoring report variable value code end

        public CtmsMonitoringReportBasic GetFormBasic(int ManageMonitoringReportId)
        {
            return All.Include(x => x.CtmsMonitoring).ThenInclude(x => x.StudyLevelForm).ThenInclude(x => x.VariableTemplate)
                .Where(r => r.Id == ManageMonitoringReportId).Select(
               c => new CtmsMonitoringReportBasic
               {
                   Id = c.Id,
                   StudyLevelFormId = c.CtmsMonitoring.StudyLevelFormId,
                   ProjectId = c.CtmsMonitoring.ProjectId,
                   VariableTemplateId = c.CtmsMonitoring.StudyLevelForm.VariableTemplateId,
               }).FirstOrDefault();
        }


        // Get monitoring form approved or not code start

        public string GetMonitoringFormApprovedOrNot(int projectId, int siteId, int tabNumber)
        {
            var appscreen = _context.AppScreen.FirstOrDefault(x => x.ScreenCode == "mnu_ctms");
            if (appscreen == null) return "App screen not found.";

            string activityCode = GetActivityCodeByTabNumber(tabNumber);

            var ctmsActivity = _context.CtmsActivity.FirstOrDefault(x => x.ActivityCode == activityCode && x.DeletedDate == null);
            if (ctmsActivity == null) return "Ctms activity not found.";

            var activity = _context.Activity.FirstOrDefault(x => x.CtmsActivityId == ctmsActivity.Id && x.AppScreenId == appscreen.Id && x.DeletedDate == null);
            if (activity == null) return "Activity not found.";

            var studyLevelForms = _context.StudyLevelForm
                .Include(x => x.Activity)
                .Where(x => x.ProjectId == projectId && x.ActivityId == activity.Id && x.AppScreenId == appscreen.Id && x.DeletedDate == null)
                .ToList();

            if (activityCode != "act_004" && activityCode != "act_001")
            {
                return CheckApprovalStatusForNonSpecialActivities(siteId, studyLevelForms, ctmsActivity);
            }

            if (activityCode == "act_001")
            {
                return CheckApprovalStatusForFeasibilityActivity(siteId, studyLevelForms, ctmsActivity);
            }

            return CheckApprovalStatusForSpecialActivity(siteId, studyLevelForms, ctmsActivity);
        }

        private string GetActivityCodeByTabNumber(int tabNumber)
        {
            return tabNumber switch
            {
                0 => "act_001",
                1 => "act_002",
                2 => "act_003",
                3 => "act_004",
                4 => "act_005",
                _ => "act_006",
            };
        }

        private string CheckApprovalStatusForNonSpecialActivities(int siteId, List<StudyLevelForm> studyLevelForms, CtmsActivity ctmsActivity)
        {
            var ctmsMonitoringStatuses = _context.CtmsMonitoringStatus
                .Where(x => x.CtmsMonitoring.ProjectId == siteId &&
                            studyLevelForms.Select(y => y.Id).Contains(x.CtmsMonitoring.StudyLevelFormId) &&
                            x.CtmsMonitoring.DeletedDate == null)
                .ToList();

            if (ctmsMonitoringStatuses.Any() &&
                ctmsMonitoringStatuses.OrderByDescending(c => c.Id).FirstOrDefault()?.Status == MonitoringSiteStatus.Approved)
            {
                return "";
            }

            return $"Please Approve {ctmsActivity.ActivityName} .";
        }

        private string CheckApprovalStatusForFeasibilityActivity(int siteId, List<StudyLevelForm> studyLevelForms, CtmsActivity ctmsActivity)
        {
            var ctmsMonitoringStatuses = _context.CtmsMonitoringStatus
                .Where(x => x.CtmsMonitoring.ProjectId == siteId &&
                            studyLevelForms.Select(y => y.Id).Contains(x.CtmsMonitoring.StudyLevelFormId) &&
                            x.CtmsMonitoring.DeletedDate == null)
                .ToList();

            var applicable = _context.CtmsMonitoring
                .Where(x => x.ProjectId == siteId &&
                            studyLevelForms.Select(y => y.Id).Contains(x.StudyLevelFormId) &&
                            x.DeletedDate == null)
                .ToList();

            if (applicable.Any() && !applicable.OrderByDescending(c => c.Id).FirstOrDefault()?.IfApplicable == true)
            {
                if (ctmsMonitoringStatuses.Any() &&
                    ctmsMonitoringStatuses.OrderByDescending(c => c.Id).FirstOrDefault()?.Status == MonitoringSiteStatus.Approved)
                {
                    return "";
                }

                return $"Please Approve {ctmsActivity.ActivityName} .";
            }

            return "";
        }

        private string CheckApprovalStatusForSpecialActivity(int siteId, List<StudyLevelForm> studyLevelForms, CtmsActivity ctmsActivity)
        {
            var ctmsMonitoringReports = _context.CtmsMonitoringReport
                .Where(x => x.CtmsMonitoring.ProjectId == siteId &&
                            studyLevelForms.Select(y => y.Id).Contains(x.CtmsMonitoring.StudyLevelFormId) &&
                            x.CtmsMonitoring.DeletedDate == null)
                .ToList();

            var openQueryExists = _context.CtmsActionPoint
                .Include(s => s.CtmsMonitoring)
                .ThenInclude(d => d.StudyLevelForm)
                .ThenInclude(x => x.Activity)
                .ThenInclude(r => r.CtmsActivity)
                .Any(x => x.CtmsMonitoring.ProjectId == siteId &&
                          x.Status == CtmsActionPointStatus.Open &&
                          x.CtmsMonitoring.StudyLevelForm.Activity.CtmsActivity.ActivityCode != "act_005");

            if (ctmsMonitoringReports.Any() &&
                ctmsMonitoringReports.OrderByDescending(c => c.Id).FirstOrDefault()?.ReportStatus == MonitoringReportStatus.Approved)
            {
                if (openQueryExists)
                {
                    return "Please Close All Open Query.";
                }

                return "";
            }

            return $"Please Approve {ctmsActivity.ActivityName} .";
        }

        // Get monitoring form approved or not code end

        private List<CtmsMonitoringReportVariableValueChild> GetCTMSTemplateValueChild(int CtmsMonitoringReportVariableValueId)
        {
            return _ctmsMonitoringReportVariableValueChildRepository.All.AsNoTracking().Where(t => t.CtmsMonitoringReportVariableValueId == CtmsMonitoringReportVariableValueId && t.DeletedDate == null).ToList();
        }
    }
}