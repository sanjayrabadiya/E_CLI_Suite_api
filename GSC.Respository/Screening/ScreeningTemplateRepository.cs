using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EditCheckImpact;
using GSC.Respository.EmailSender;
using GSC.Respository.LabManagement;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.GeneralConfig;
using GSC.Respository.Project.Workflow;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateRepository : GenericRespository<ScreeningTemplate>,
        IScreeningTemplateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateValueChildRepository _screeningTemplateValueChildRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IEditCheckImpactRepository _editCheckImpactRepository;
        private readonly IMapper _mapper;
        private readonly IScheduleRuleRespository _scheduleRuleRespository;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IGSCContext _context;
        private readonly IProjectDesingTemplateRestrictionRepository _projectDesingTemplateRestrictionRepository;
        private readonly ILabManagementVariableMappingRepository _labManagementVariableMappingRepository;
        private readonly IProjectDesignVariableValueRepository _projectDesignVariableValueRepository;

        private readonly ITemplateVariableSequenceNoSettingRepository _templateVariableSequenceNoSettingRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IEmailConfigurationEditCheckRepository _emailConfigurationEditCheckRepository;
        public ScreeningTemplateRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IUploadSettingRepository uploadSettingRepository, IMapper mapper,
            IProjectWorkflowRepository projectWorkflowRepository,
            IEditCheckImpactRepository editCheckImpactRepository,
            IScheduleRuleRespository scheduleRuleRespository,
            IScreeningTemplateValueChildRepository screeningTemplateValueChildRepository,
            IProjectDesingTemplateRestrictionRepository projectDesingTemplateRestrictionRepository,
            ILabManagementVariableMappingRepository labManagementVariableMappingRepository,
                        IProjectDesignVariableValueRepository projectDesignVariableValueRepository,
            IAppSettingRepository appSettingRepository,
             ITemplateVariableSequenceNoSettingRepository templateVariableSequenceNoSettingRepository,
            IEmailSenderRespository emailSenderRespository,
            IEmailConfigurationEditCheckRepository emailConfigurationEditCheckRepository)
            : base(context)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _scheduleRuleRespository = scheduleRuleRespository;
            _screeningTemplateValueChildRepository = screeningTemplateValueChildRepository;
            _editCheckImpactRepository = editCheckImpactRepository;
            _context = context;
            _appSettingRepository = appSettingRepository;
            _projectDesingTemplateRestrictionRepository = projectDesingTemplateRestrictionRepository;
            _labManagementVariableMappingRepository = labManagementVariableMappingRepository;
            _projectDesignVariableValueRepository = projectDesignVariableValueRepository;
            _templateVariableSequenceNoSettingRepository = templateVariableSequenceNoSettingRepository;
            _emailSenderRespository = emailSenderRespository;
            _emailConfigurationEditCheckRepository = emailConfigurationEditCheckRepository;
        }

        private ScreeningTemplateBasic GetScreeningTemplateBasic(int screeningTemplateId)
        {
            return All.AsNoTracking().Where(r => r.Id == screeningTemplateId).Select(
               c => new ScreeningTemplateBasic
               {
                   Id = c.Id,
                   ScreeningEntryId = c.ScreeningVisit.ScreeningEntryId,
                   ProjectDesignId = c.ScreeningVisit.ScreeningEntry.ProjectDesignId,
                   ProjectDesignTemplateId = c.ProjectDesignTemplateId,
                   StudyVersion = c.ScreeningVisit.ScreeningEntry.StudyVersion,
                   Status = c.Status,
                   DomainId = c.ProjectDesignTemplate.DomainId,
                   ReviewLevel = c.ReviewLevel,
                   ScreeningVisitId = c.ScreeningVisitId,
                   ProjectDesignVisitId = c.ScreeningVisit.ProjectDesignVisitId,
                   IsNoCRF = c.ScreeningVisit.ProjectDesignVisit.IsNonCRF,
                   IsLocked = c.IsLocked,
                   IsDisable = c.IsDisable,
                   PatientStatus = c.ScreeningVisit.ScreeningEntry.Randomization.PatientStatusId,
                   VisitStatus = c.ScreeningVisit.Status,
                   ParentId = c.ParentId,
                   LastReviewLevel = c.LastReviewLevel,
                   ProjectId = c.ScreeningVisit.ScreeningEntry.ProjectId,
                   Gender = c.ScreeningVisit.ScreeningEntry.Randomization.Gender ?? Gender.Male,
                   IsCompleteReview = c.IsCompleteReview,
                   IsNA = c.IsNA
               }).FirstOrDefault();
        }

        private List<ScreeningTemplateValueBasic> GetScreeningValues(int screeningTemplateId)
        {
            return _screeningTemplateValueRepository.All.AsNoTracking().Where(t => t.ScreeningTemplateId == screeningTemplateId)
                    .ProjectTo<ScreeningTemplateValueBasic>(_mapper.ConfigurationProvider).ToList();
        }

        private List<ScreeningTemplateValueChild> GetScreeningTemplateValueChild(int ScreeningTemplateValueId)
        {
            return _screeningTemplateValueChildRepository.All.AsNoTracking().Where(t => t.ScreeningTemplateValueId == ScreeningTemplateValueId && t.DeletedDate == null).ToList();
        }

        private List<ProjectDesignVariableValue> GetProjectDesignVariableValue(int ProjectDesignVariableId)
        {
            return _projectDesignVariableValueRepository.All.AsNoTracking().Where(t => t.ProjectDesignVariableId == ProjectDesignVariableId && t.DeletedDate == null).ToList();
        }

        public DesignScreeningTemplateDto GetScreeningTemplate(DesignScreeningTemplateDto designTemplateDto, int screeningTemplateId)
        {
            var screeningTemplateBasic = GetScreeningTemplateBasic(screeningTemplateId);

            var statusId = (int)screeningTemplateBasic.Status;

            designTemplateDto.ScreeningVisitName = _context.ScreeningVisit.Find(screeningTemplateBasic.ScreeningVisitId).ScreeningVisitName;

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(screeningTemplateBasic.ProjectDesignId);

            designTemplateDto.ScreeningTemplateId = screeningTemplateId;
            designTemplateDto.IsSubmittedButton = statusId < 3 && workflowlevel.IsStartTemplate;
            designTemplateDto.IsUnSubmittedButton = screeningTemplateBasic.Status == ScreeningTemplateStatus.Submitted && workflowlevel.IsStartTemplate;

            if (screeningTemplateBasic.Status == ScreeningTemplateStatus.Reviewed)
                designTemplateDto.IsUnReviewedButton = workflowlevel.LevelNo == screeningTemplateBasic.LastReviewLevel;

            if (workflowlevel.LevelNo >= 0 && designTemplateDto.IsRepeated)
                designTemplateDto.IsRepeated = workflowlevel.IsStartTemplate;

            designTemplateDto.MyReview = workflowlevel.LevelNo == screeningTemplateBasic.ReviewLevel;
            designTemplateDto.ScreeningTemplateId = screeningTemplateBasic.Id;
            designTemplateDto.ParentId = screeningTemplateBasic.ParentId;
            designTemplateDto.IsLocked = screeningTemplateBasic.IsLocked;
            designTemplateDto.Status = screeningTemplateBasic.Status;
            designTemplateDto.StatusName = GetStatusName(screeningTemplateBasic, workflowlevel.LevelNo == screeningTemplateBasic.ReviewLevel, workflowlevel);
            designTemplateDto.IsNA = screeningTemplateBasic.IsNA;

            if (screeningTemplateBasic.Status == ScreeningTemplateStatus.Pending)
                _screeningTemplateValueRepository.UpdateDefaultValue(designTemplateDto.Variables, screeningTemplateId);

            if (screeningTemplateBasic.Status == ScreeningTemplateStatus.Pending || screeningTemplateBasic.Status == ScreeningTemplateStatus.InProcess)
                _screeningTemplateValueRepository.UpdateTemplateConfigurationUploadRandomizationValue(designTemplateDto, screeningTemplateId);

            SetScreeningValue(designTemplateDto, screeningTemplateBasic, workflowlevel);

            if (designTemplateDto.Status == ScreeningTemplateStatus.Pending && designTemplateDto.IsSchedule &&
                (screeningTemplateBasic.PatientStatus == ScreeningPatientStatus.ScreeningFailure ||
               screeningTemplateBasic.VisitStatus == ScreeningVisitStatus.Withdrawal ||
               screeningTemplateBasic.VisitStatus == ScreeningVisitStatus.Missed ||
               screeningTemplateBasic.VisitStatus == ScreeningVisitStatus.OnHold))
                designTemplateDto.IsSubmittedButton = false;

            designTemplateDto.Variables.Where(x => x.IsEncrypt == true).ToList().ForEach(c =>
            {
                c.ScreeningValueOld = null;
                c.ScreeningValue = null;
                c.HasQueries = false;
                c.WorkFlowButton = null;
                c.EditCheckValidation = null;
            });

            return designTemplateDto;
        }

        private void SetLabVariable(List<DesignScreeningVariableDto> variables, ScreeningTemplateBasic screeningTemplateBasic)
        {
            var labVariable = _labManagementVariableMappingRepository.All.Where(x => x.LabManagementConfiguration.ProjectId == screeningTemplateBasic.ProjectId
            && x.LabManagementConfiguration.ProjectDesignTemplateId == screeningTemplateBasic.ProjectDesignTemplateId).Select(t => new
            {
                t.ProjectDesignVariableId,
                t.FemaleHighRange,
                t.FemaleLowRange,
                t.MaleHighRange,
                t.MaleLowRange,
                t.Unit
            }).ToList();

            labVariable.ForEach(x =>
            {
                var screeningVariable = variables.FirstOrDefault(t => t.ProjectDesignVariableId == x.ProjectDesignVariableId);
                if (screeningVariable != null)
                {
                    if (screeningTemplateBasic.Gender == Gender.Female)
                    {
                        screeningVariable.HighRangeValue = x.FemaleHighRange != null ? x.FemaleHighRange.ToString() : "";
                        screeningVariable.LowRangeValue = x.FemaleLowRange != null ? x.FemaleLowRange.ToString() : "";
                    }
                    else
                    {
                        screeningVariable.HighRangeValue = x.MaleHighRange != null ? x.MaleHighRange.ToString() : "";
                        screeningVariable.LowRangeValue = x.MaleLowRange != null ? x.MaleLowRange.ToString() : "";
                    }
                    screeningVariable.UnitName = x.Unit;

                }
            });


        }

        private void SetScreeningValue(DesignScreeningTemplateDto designTemplateDto, ScreeningTemplateBasic screeningTemplateBasic, WorkFlowLevelDto workflowlevel)
        {
            var values = GetScreeningValues(screeningTemplateBasic.Id);
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();

            SetLabVariable(designTemplateDto.Variables.ToList(), screeningTemplateBasic);

            var isRestriction = false;

            var templateRestriction = _projectDesingTemplateRestrictionRepository.All.Where(x => x.ProjectDesignTemplateId == designTemplateDto.ProjectDesignTemplateId
            && x.SecurityRoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null).Select(r => new { r.ProjectDesignTemplateId, r.IsHide }).FirstOrDefault();

            if (templateRestriction != null)
            {
                designTemplateDto.IsSubmittedButton = false;
                designTemplateDto.IsRepeated = false;
                isRestriction = true;
                designTemplateDto.IsUnSubmittedButton = false;


                if (templateRestriction.IsHide)
                    throw new Exception("You can't view the template!");
            }



            designTemplateDto.Variables = designTemplateDto.Variables.Where(t => (t.StudyVersion == null || t.StudyVersion <= screeningTemplateBasic.StudyVersion)
            && (t.InActiveVersion == null || t.InActiveVersion > screeningTemplateBasic.StudyVersion)).ToList();

            designTemplateDto.Variables.ToList().ForEach(r =>
            {
                if (r.Values != null)
                    r.Values = r.Values.Where(t => (t.StudyVersion == null || t.StudyVersion <= screeningTemplateBasic.StudyVersion)
                    && (t.InActiveVersion == null || t.InActiveVersion > screeningTemplateBasic.StudyVersion)).ToList();
            });

            designTemplateDto.Variables.Where(x => x.CollectionSource == CollectionSources.Relation && x.RelationProjectDesignVariableId > 0).ToList().
                ForEach(t => t.Values = _screeningTemplateValueRepository.GetScreeningRelation(t.RelationProjectDesignVariableId ?? 0, screeningTemplateBasic.ScreeningEntryId));

            designTemplateDto.Variables.Where(x => x.CollectionSource == CollectionSources.Table).ToList().ForEach(t =>
            {
                t.Values.Where(c => c.LevelNo == null).ToList().ForEach(v =>
                {
                    v.LevelNo = 1;
                });
            });

            values.ForEach(t =>
            {
                var ScreeningTemplateValueChild = GetScreeningTemplateValueChild(t.Id);

                var MaxLevel = ScreeningTemplateValueChild.Max(x => x.LevelNo);
                var variable = designTemplateDto.Variables.FirstOrDefault(v => v.ProjectDesignVariableId == t.ProjectDesignVariableId);
                if (variable != null)
                {
                    variable.ScreeningValue = t.Value;
                    variable.ScreeningValueOld = t.IsNa ? "N/A" : t.Value;
                    variable.ScreeningTemplateValueId = t.Id;
                    variable.ScheduleDate = t.ScheduleDate;
                    variable.QueryStatus = t.QueryStatus;
                    variable.HasComments = t.IsComment;
                    variable.HasQueries = t.QueryStatus != null ? true : false;
                    variable.IsNaValue = t.IsNa;
                    variable.IsSystem = t.QueryStatus == QueryStatus.Closed ? false : t.IsSystem;

                    if (!isRestriction)
                        variable.WorkFlowButton = SetWorkFlowButton(t, workflowlevel, designTemplateDto, screeningTemplateBasic);

                    variable.DocPath = t.DocPath != null ? t.DocPath : null;
                    variable.DocFullPath = t.DocPath != null ? documentUrl + t.DocPath : null;
                    if (!string.IsNullOrWhiteSpace(variable.ScreeningValue) || variable.IsNaValue)
                        variable.IsValid = true;

                    if (variable.Values != null && (variable.CollectionSource == CollectionSources.CheckBox || variable.CollectionSource == CollectionSources.MultiCheckBox))
                        variable.Values.ToList().ForEach(val =>
                        {

                            var childValue = t.Children.FirstOrDefault(v => v.ProjectDesignVariableValueId == val.Id);
                            if (childValue != null)
                            {
                                variable.IsValid = true;
                                val.ScreeningValue = childValue.Value;
                                val.ScreeningValueOld = childValue.Value;
                                val.ScreeningTemplateValueChildId = childValue.Id;
                                val.LevelNo = childValue.LevelNo;
                            }
                        });

                    if (variable.Values != null && variable.CollectionSource == CollectionSources.Table)
                    {
                        var ValuesList = new List<ScreeningVariableValueDto>();

                        variable.Values.ToList().ForEach(val =>
                        {
                            MaxLevel = MaxLevel > 0 ? MaxLevel : 0;
                            var notExistLevel = Enumerable.Range(1, (int)MaxLevel).ToArray();

                            var childValue = t.Children.Where(v => v.ProjectDesignVariableValueId == val.Id).GroupBy(x => x.LevelNo)
                            .Select(x => new ScreeningTemplateValueChild
                            {
                                Id = x.FirstOrDefault().Id,
                                ScreeningTemplateValueId = x.FirstOrDefault().ScreeningTemplateValueId,
                                ProjectDesignVariableValueId = x.FirstOrDefault().ProjectDesignVariableValueId,
                                Value = x.FirstOrDefault().Value,
                                LevelNo = x.FirstOrDefault().LevelNo,
                                DeletedDate = x.FirstOrDefault().DeletedDate
                            }).ToList();


                            var Levels = notExistLevel.Where(x => !childValue.Select(y => (int)y.LevelNo).Contains(x)).ToList();

                            Levels.ForEach(x =>
                            {
                                ScreeningTemplateValueChild obj = new ScreeningTemplateValueChild();
                                obj.Id = 0;
                                obj.ScreeningTemplateValueId = t.Id;
                                obj.ProjectDesignVariableValueId = val.Id;
                                obj.Value = null;
                                obj.LevelNo = (short)x;
                                childValue.Add(obj);
                            });

                            if (childValue.Count() == 0 && Levels.Count() == 0)
                            {
                                ScreeningTemplateValueChild obj = new ScreeningTemplateValueChild();
                                obj.Id = 0;
                                obj.ScreeningTemplateValueId = t.Id;
                                obj.ProjectDesignVariableValueId = val.Id;
                                obj.Value = null;
                                obj.LevelNo = 1;
                                childValue.Add(obj);
                            }

                            childValue.ForEach(child =>
                            {
                                ScreeningVariableValueDto obj = new ScreeningVariableValueDto();
                                variable.IsValid = true;
                                obj.Id = child.ProjectDesignVariableValueId;
                                obj.ScreeningValue = child.Value;
                                obj.ScreeningValueOld = child.Value;
                                obj.ScreeningTemplateValueChildId = child.Id;
                                obj.LevelNo = child.LevelNo;
                                obj.ValueName = val.ValueName;
                                obj.IsDeleted = child.DeletedDate == null ? false : true;
                                obj.TableCollectionSource = val.TableCollectionSource;
                                ValuesList.Add(obj);
                            });
                        });
                        variable.Values = ValuesList.Where(x => x.IsDeleted == false).ToList();
                    }

                    variable.IsSaved = variable.IsValid;
                }
            });

            if (!screeningTemplateBasic.IsLocked)
            {
                EditCheckProcess(designTemplateDto, values, screeningTemplateBasic);
                CheckSchedule(designTemplateDto, values, screeningTemplateBasic);
            }
        }


        public bool IsRepated(int screeningTemplateId)
        {
            return All.Any(x => x.DeletedDate == null &&
                                x.Id == screeningTemplateId && (x.ParentId != null || x.ScreeningVisit.ParentId != null));
        }

        void EditCheckProcess(DesignScreeningTemplateDto projectDesignTemplateDto, List<ScreeningTemplateValueBasic> values, ScreeningTemplateBasic screeningTemplateBasic)
        {
            var result = _editCheckImpactRepository.CheckValidation(projectDesignTemplateDto, values, screeningTemplateBasic, false);


            result.Where(t => t.IsTarget && t.ProjectDesignTemplateId == projectDesignTemplateDto.ProjectDesignTemplateId &&
            (t.CheckBy == EditCheckRuleBy.ByTemplate || t.CheckBy == EditCheckRuleBy.ByTemplateAnnotation)).ToList().ForEach(r =>
            {
                if (r.Operator == Operator.Warning && r.ValidateType == EditCheckValidateType.Passed)
                {
                    projectDesignTemplateDto.IsWarning = true;
                    projectDesignTemplateDto.EditCheckMessage = $"{r.AutoNumber} {r.Message}";
                }

                if (r.Operator == Operator.Enable && r.ValidateType != EditCheckValidateType.Passed)
                {
                    projectDesignTemplateDto.IsRepeated = false;
                    projectDesignTemplateDto.IsSubmittedButton = false;
                    projectDesignTemplateDto.IsLocked = true;
                    projectDesignTemplateDto.EditCheckMessage = $"{r.AutoNumber} {r.Message}";
                }
            });

            var variableTargetResult = _editCheckImpactRepository.UpdateVariale(result.Where(x => x.IsTarget).ToList(), screeningTemplateBasic.ScreeningEntryId, screeningTemplateBasic.ScreeningVisitId, false, false);
            projectDesignTemplateDto.Variables.ToList().ForEach(r =>
            {
                var singleResult = variableTargetResult.Where(x => x.ProjectDesignVariableId == r.ProjectDesignVariableId).FirstOrDefault();
                if (singleResult != null)
                {
                    r.EditCheckValidation = new EditCheckTargetValidation();

                    if (singleResult.IsValueSet || (singleResult.IsSoftFetch && (string.IsNullOrEmpty(r.ScreeningValue))))
                    {
                        if (Convert.ToString(r.ScreeningValue ?? "") != Convert.ToString(singleResult.Value ?? ""))
                        {
                            var newValueId = _editCheckImpactRepository.InsertScreeningValue(projectDesignTemplateDto.ScreeningTemplateId,
                                                           (int)r.ProjectDesignVariableId, singleResult.Value, singleResult.Note, singleResult.IsSoftFetch, r.CollectionSource, singleResult.EditCheckDisable);

                            if (newValueId > 0 && r.ScreeningTemplateValueId == 0)
                                r.ScreeningTemplateValueId = newValueId;
                        }

                        r.ScreeningValue = singleResult.Value;
                        r.ScreeningValueOld = singleResult.Value;
                    }

                    if (singleResult.OriginalValidationType != null)
                        r.ValidationType = (ValidationType)singleResult.OriginalValidationType;

                    r.EditCheckValidation.EditCheckMsg = singleResult.EditCheckMsg;
                    r.EditCheckValidation.InfoType = singleResult.InfoType;
                    r.EditCheckValidation.EditCheckDisable = singleResult.EditCheckDisable;
                    r.EditCheckValidation.IsHide = singleResult.IsHide;
                }
                r.editCheckIds = GetEditCheckIds(result, (int)r.ProjectDesignVariableId, screeningTemplateBasic.ProjectDesignTemplateId);
            });


        }

        void CheckSchedule(DesignScreeningTemplateDto projectDesignTemplateDto, List<Data.Dto.Screening.ScreeningTemplateValueBasic> values, ScreeningTemplateBasic screeningTemplateBasic)
        {
            if (screeningTemplateBasic.ParentId == null)
            {
                var scheduleResult = _scheduleRuleRespository.ValidateByTemplate(values, screeningTemplateBasic, false);
                if (scheduleResult != null && scheduleResult.Count > 0)
                {
                    projectDesignTemplateDto.Variables.ToList().ForEach(r =>
                    {
                        var scheduleVariable = scheduleResult.Where(x => x.ProjectDesignVariableId == r.ProjectDesignVariableId).ToList();
                        if (scheduleVariable != null && scheduleVariable.Count > 0)
                        {
                            if (r.EditCheckValidation == null)
                            {
                                r.EditCheckValidation = new EditCheckTargetValidation();
                                r.EditCheckValidation.InfoType = EditCheckInfoType.Info;
                            }

                            if (scheduleVariable.Any(x => x.ValidateType == EditCheckValidateType.Failed))
                                r.EditCheckValidation.InfoType = EditCheckInfoType.Failed;

                            if (scheduleVariable.Any(x => x.HasQueries))
                                r.EditCheckValidation.HasQueries = true;

                            var schMessage = scheduleResult.Select(t => new EditCheckMessage
                            {
                                AutoNumber = t.AutoNumber,
                                Message = t.Message,
                                ValidateType = t.ValidateType.GetDescription()
                            }).ToList();

                            r.EditCheckValidation.EditCheckMsg.AddRange(schMessage);
                        }
                    });
                }
            }
        }

        List<EditCheckIds> GetEditCheckIds(List<EditCheckValidateDto> editCheckValidateDtos, int projectDesignVariableId, int projectDesignTemplateId)
        {
            var editCheckIds = editCheckValidateDtos.
                Where(x => (x.ProjectDesignVariableId == projectDesignVariableId || (x.FetchingProjectDesignVariableId == projectDesignVariableId && x.FetchingProjectDesignTemplateId == projectDesignTemplateId))).
                GroupBy(t => t.EditCheckId).Select(r => r.Key).ToList();

            if (editCheckIds.Count > 0)
                RecuranceEditCheck(editCheckValidateDtos, editCheckIds, ref editCheckIds, projectDesignTemplateId);

            TargetVaribaleAndReferenceEditCheck(editCheckValidateDtos, editCheckIds, ref editCheckIds, projectDesignTemplateId);

            return editCheckIds.GroupBy(t => t).Select(r => new EditCheckIds { EditCheckId = r.Key }).ToList();
        }

        void RecuranceEditCheck(List<EditCheckValidateDto> editCheckValidateDtos, List<int> editCheckIds, ref List<int> result, int projectDesignTemplateId)
        {
            var variable = GetVariableIdEditCheckDetail(editCheckValidateDtos, editCheckIds, result);
            foreach (var item in variable)
            {
                var subEditCheckIds = GetEditCheckIdTarget(editCheckValidateDtos, item, result, projectDesignTemplateId);
                result.AddRange(subEditCheckIds);
                RecuranceEditCheck(editCheckValidateDtos, subEditCheckIds, ref result, projectDesignTemplateId);
            }
        }

        void TargetVaribaleAndReferenceEditCheck(List<EditCheckValidateDto> editCheckValidateDtos, List<int> editCheckIds, ref List<int> result, int projectDesignTemplateId)
        {
            var variable = editCheckValidateDtos.
              Where(x => editCheckIds.Contains(x.EditCheckId) && x.IsTarget &&
              (x.IsFormula || x.Operator == Operator.Enable || x.Operator == Operator.HardFetch || x.Operator == Operator.SoftFetch || x.Operator == Operator.Default)).
             Select(t => t.ProjectDesignVariableId).Distinct().ToList();

            foreach (var item in variable)
            {
                var subEditCheckIds = editCheckValidateDtos.Where(x =>
                (x.ProjectDesignVariableId == item || (x.FetchingProjectDesignVariableId == item) && !editCheckIds.Contains(x.EditCheckId) && x.IsTarget &&
                (x.IsFormula || x.Operator == Operator.Enable || x.Operator == Operator.HardFetch || x.Operator == Operator.SoftFetch || x.Operator == Operator.Default))).
                GroupBy(t => t.EditCheckId).Select(r => r.Key).ToList();
                result.AddRange(subEditCheckIds);
            }
        }

        List<int> GetVariableIdEditCheckDetail(List<EditCheckValidateDto> editCheckValidateDtos, List<int> editCheckIds, List<int> alreadyUsed)
        {
            return editCheckValidateDtos.
               Where(x => editCheckIds.Contains(x.EditCheckId) && !alreadyUsed.Contains(x.EditCheckId) &&
               (x.IsFormula || x.Operator == Operator.Enable || x.Operator == Operator.HardFetch || x.Operator == Operator.SoftFetch || x.Operator == Operator.Default)).
              Select(t => t.ProjectDesignVariableId).Distinct().ToList();
        }


        List<int> GetEditCheckIdTarget(List<EditCheckValidateDto> editCheckValidateDtos, int projectDesignVariableId, List<int> alreadyUsed, int projectDesignTemplateId)
        {
            return editCheckValidateDtos.
             Where(x => (x.ProjectDesignVariableId == projectDesignVariableId || (x.FetchingProjectDesignVariableId == projectDesignVariableId && x.FetchingProjectDesignTemplateId == projectDesignTemplateId)) && !alreadyUsed.Contains(x.EditCheckId) && x.IsTarget &&
             (x.IsFormula || x.Operator == Operator.Enable || x.Operator == Operator.HardFetch || x.Operator == Operator.SoftFetch || x.Operator == Operator.Default)).
             GroupBy(t => t.EditCheckId).Select(r => r.Key).ToList();
        }

        public void SubmitReviewTemplate(int screeningTemplateId, bool isFromLockUnLock)
        {
            _context.DetachAllEntities();

            var screeningTemplateBasic = GetScreeningTemplateBasic(screeningTemplateId);

            if (screeningTemplateBasic.Status < ScreeningTemplateStatus.Submitted)
                return;

            var values = GetScreeningValues(screeningTemplateBasic.Id);
            var result = _editCheckImpactRepository.CheckValidation(null, values, screeningTemplateBasic, !isFromLockUnLock);
            _editCheckImpactRepository.UpdateVariale(result.Where(x => x.IsTarget).ToList(), screeningTemplateBasic.ScreeningEntryId, screeningTemplateBasic.ScreeningVisitId, true, true);
            _editCheckImpactRepository.HideDisableVisit(result.Where(x => x.IsTarget).ToList(), screeningTemplateBasic.ScreeningEntryId);
            _scheduleRuleRespository.ValidateByTemplate(values, screeningTemplateBasic, true);

        }

        public BasicProjectDesignVisit GetProjectDesignId(int screeningTemplateId)
        {
            return All.Where(x => x.Id == screeningTemplateId).Select(r => new BasicProjectDesignVisit
            {
                ProjectDesignId = r.ScreeningVisit.ScreeningEntry.ProjectDesignId,
                ProjectDesignVisitId = r.ScreeningVisit.ProjectDesignVisitId,
                IsNonCRF = r.ScreeningVisit.ProjectDesignVisit.IsNonCRF
            }).FirstOrDefault();
        }

        public int GeScreeningEntryId(int screeningTemplateId)
        {
            return All.Where(x => x.Id == screeningTemplateId).Select(r => r.ScreeningVisit.ScreeningEntryId).FirstOrDefault();
        }

        public List<TemplateText> GetTemplateData(int ProjectId, int VisitId)
        {
            var Templates = All.Where(s => s.ScreeningVisit.ScreeningEntry.ProjectId == ProjectId
            && s.ScreeningVisit.ProjectDesignVisit.Id == VisitId && s.DeletedDate == null
            && s.ScreeningVisit.Status >= ScreeningVisitStatus.Open)
            .GroupBy(x => new { x.ProjectDesignTemplateId, x.ProjectDesignTemplate.TemplateName })
            .Select(t => new TemplateText
            {
                ProjectDesignTemplateId = t.Key.ProjectDesignTemplateId,
                ProjectDesignTemplateName = t.Key.TemplateName,
            }).OrderBy(c => c.ProjectDesignTemplateId).ToList();


            var result = _context.ProjectDesignTemplate.Where(z => Templates.Select(x => x.ProjectDesignTemplateId).Contains(z.Id))
                .Select(t => new TemplateText
                {
                    ProjectDesignTemplateId = t.Id,
                    ProjectDesignTemplateName = t.TemplateName,
                    DesignOrder = t.DesignOrder
                }).OrderBy(x => x.DesignOrder).ToList();

            return result;
        }

        public List<TemplateStatusList> GetTemplateStatus(int ProjectId, int VisitId, int ScreeningEntryId)
        {
            var Templates = All.Where(s => s.ScreeningVisit.ScreeningEntry.ProjectId == ProjectId
            && s.ScreeningVisit.ScreeningEntryId == ScreeningEntryId
            && s.ScreeningVisit.ProjectDesignVisit.Id == VisitId && s.DeletedDate == null)
            .Select(t => new TemplateStatusList
            {
                ScreeningEntryId = t.ScreeningVisit.ScreeningEntryId,
                ScreeningTemplateId = t.Id,
                ProjectDesignTemplateId = t.ProjectDesignTemplateId,
            }).OrderBy(c => c.ProjectDesignTemplateId).ToList();

            var result = All.Where(z => Templates.Select(x => x.ScreeningTemplateId).Contains(z.Id))
                .Select(t => new TemplateStatusList
                {
                    ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                    DesignOrder = t.ProjectDesignTemplate.DesignOrder,
                    Status = t.Status.GetDescription(),
                    StatusId = (int)t.Status
                }).OrderBy(x => x.DesignOrder).ToList();

            return result;
        }

        public List<ScreeningTemplateTree> GetTemplateTree(int screeningEntryId, WorkFlowLevelDto workFlowLevel)
        {
            var projectDesignId = _context.ScreeningEntry.Find(screeningEntryId).ProjectDesignId;
            var sequenseDeatils = _templateVariableSequenceNoSettingRepository.All.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).FirstOrDefault();

            var result = All.Where(s => s.ScreeningVisit.ScreeningEntryId == screeningEntryId && s.DeletedDate == null
            && s.ScreeningVisit.Status >= ScreeningVisitStatus.Open).Select(t => new ScreeningTemplateTree
            {
                Id = t.Id,
                ScreeningVisitId = t.ScreeningVisitId,
                ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                Status = t.Status,
                // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                ScreeningTemplateName = t.ScreeningTemplateName,
                //ProjectDesignTemplateName = t.ProjectDesignTemplate.TemplateName,
                DesignOrder = sequenseDeatils.IsTemplateSeqNo == true ? t.RepeatSeqNo == null ? Convert.ToString(t.ProjectDesignTemplate.DesignOrder) : t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString() : "",
                DesignOrderForOrderBy = t.RepeatSeqNo == null ? Convert.ToString(t.ProjectDesignTemplate.DesignOrder) : t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString(),
                Progress = t.Progress ?? 0,
                ReviewLevel = t.ReviewLevel,
                IsLocked = t.IsLocked,
                IsHide = t.IsHide ?? false,
                MyReview = workFlowLevel.LevelNo == t.ReviewLevel,
                ParentId = t.ParentId,
                IsTemplateSeqNo = sequenseDeatils.IsTemplateSeqNo,
                IsVariableSeqNo = sequenseDeatils.IsVariableSeqNo,
                Label = t.ProjectDesignTemplate.Label,
                PreLabel = PreLabelSetting(t, t.ProjectDesignTemplate, sequenseDeatils),
                IsNA = t.IsNA
            }).ToList();

            var templateValues = _screeningTemplateValueRepository.GetQueryStatusBySubject(screeningEntryId);


            result.ForEach(a =>
            {
                if ((bool)a.IsNA)
                    a.StatusName = "Not Applicable";
                else
                    a.StatusName = GetStatusName(new ScreeningTemplateBasic { ReviewLevel = a.ReviewLevel, Status = a.Status }, workFlowLevel.LevelNo == a.ReviewLevel, workFlowLevel);
                a.TotalQueries = templateValues.Where(t => t.ScreeningTemplateId == a.Id).Select(c => c.Total).FirstOrDefault();
            });

            return result;
        }

        public List<MyReviewDto> GetScreeningTemplateReview()
        {
            var result = All.Where(x => x.DeletedDate == null
                                        && x.ReviewLevel != null && x.ReviewLevel > 0
                                        && (_context.ProjectWorkflowIndependent.Any(r => r.DeletedDate == null &&
                                                                                        r.ProjectWorkflow
                                                                                            .ProjectDesignId ==
                                                                                        x.ScreeningVisit.ScreeningEntry.ProjectDesignId
                                                                                        && r.SecurityRoleId ==
                                                                                        _jwtTokenAccesser.RoleId) ||
                                            _context.ProjectWorkflowLevel.Any(r => r.DeletedDate == null &&
                                                                                  r.ProjectWorkflow.ProjectDesignId ==
                                                                                  x.ScreeningVisit.ScreeningEntry.ProjectDesignId
                                                                                  && r.SecurityRoleId ==
                                                                                  _jwtTokenAccesser.RoleId
                                                                                  && r.LevelNo == x.ReviewLevel))
            ).Select(a => new MyReviewDto
            {
                ScreeningEntryId = a.ScreeningVisit.ScreeningEntryId,
                ProjectDesignTemplateId = a.ProjectDesignTemplateId,
                ScreeningDate = a.ScreeningVisit.ScreeningEntry.ScreeningDate,
                ScreeningNo = a.ScreeningVisit.ScreeningEntry.ScreeningNo,
                ProjectName = a.ScreeningVisit.ScreeningEntry.Project.ProjectName,
                VolunteerName = a.ScreeningVisit.ScreeningEntry.RandomizationId != null
                    ? a.ScreeningVisit.ScreeningEntry.Randomization.Initial
                    : a.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,

                // changes on 13/06/2023 for add template name in screeningtecmplate table change by vipul rokad
                TemplateName = a.ScreeningTemplateName,
                //TemplateName = a.ProjectDesignTemplate.TemplateName,

                // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                VistName = a.ScreeningVisit.ScreeningVisitName,
                //VistName = a.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                ReviewedLevel = a.ReviewLevel.ToString(),
                SubmittedDate = a.ScreeningTemplateReview.FirstOrDefault(c => c.Status == ScreeningTemplateStatus.Submitted)
                    .CreatedDate,
                SubmittedBy = a.ScreeningTemplateReview.FirstOrDefault(c => c.Status == ScreeningTemplateStatus.Submitted)
                    .CreatedByUser.UserName,
                LastReviewedDate = a.ScreeningTemplateReview
                    .FirstOrDefault(c => c.Status == ScreeningTemplateStatus.Reviewed && c.ReviewLevel == a.ReviewLevel - 1)
                    .CreatedDate,
                LastReviewedBy = a.ScreeningTemplateReview
                    .FirstOrDefault(c => c.Status == ScreeningTemplateStatus.Reviewed && c.ReviewLevel == a.ReviewLevel - 1)
                    .CreatedByUser.UserName
            }).ToList();

            return result;
        }

        public ScreeningTemplate TemplateRepeat(ScreeningTemplateRepeat screeningTemplateRepeat)
        {
            var screeningTemplate = new ScreeningTemplate();
            var originalTemplate = All.AsNoTracking().Where(x => x.Id == screeningTemplateRepeat.ScreeningTemplateId).FirstOrDefault();
            if (originalTemplate != null)
            {
                screeningTemplate.ScreeningTemplateName = screeningTemplateRepeat.ScreeningTemplateName;
                screeningTemplate.ParentId = originalTemplate.Id;
                screeningTemplate.Id = 0;
                screeningTemplate.RepeatSeqNo = All.Count(x => x.ParentId == originalTemplate.Id) + 1;
                screeningTemplate.ScreeningVisitId = originalTemplate.ScreeningVisitId;
                screeningTemplate.ProjectDesignTemplateId = originalTemplate.ProjectDesignTemplateId;
                screeningTemplate.Status = ScreeningTemplateStatus.Pending;
                screeningTemplate.Children = null;
                screeningTemplate.IsDisable = false;
                screeningTemplate.IsHide = false;
            }
            Add(screeningTemplate);

            return screeningTemplate;
        }

        // changes for dynamic column 04/06/2023
        public List<ReviewDto> GetReviewReportList(ReviewSearchDto filters)
        {
            int parentprojectid = filters.ProjectId;
            int? siteId = filters.SiteId;
            var parentIds = new List<int>();
            if (siteId == null)
            {
                parentIds = _context.Project.Where(x => x.ParentProjectId == filters.ProjectId && !x.IsTestSite).Select(y => y.Id).ToList();
            }
            else
            {
                parentIds.Add((int)filters.SiteId);
            }
            // added for dynamic column 04/06/2023
            var ID = _context.ProjectDesign.Where(x => x.ProjectId == parentprojectid).Select(s => s.Id).FirstOrDefault();
            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(ID);

            var result = All.Where(x => x.DeletedDate == null && x.ScreeningVisit.Status != ScreeningVisitStatus.NotStarted);
            if (filters.ReviewStatus != null)
            {
                result = result.Where(y => y.Status != ScreeningTemplateStatus.Pending && y.Status != ScreeningTemplateStatus.InProcess);
            }

            if (filters.ReviewStatus != null) result = result.Where(x => x.ReviewLevel != null);
            if (parentIds != null) result = result.Where(x => parentIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectId));
            if (filters.PeriodIds != null) result = result.Where(x => filters.PeriodIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectDesignPeriodId));
            if (filters.VisitIds != null) result = result.Where(x => filters.VisitIds.Contains(x.ScreeningVisit.ProjectDesignVisitId));
            if (filters.SubjectIds != null) result = result.Where(x => filters.SubjectIds.Contains(x.ScreeningVisit.ScreeningEntry.Id));
            if (filters.TemplateIds != null) result = result.Where(x => filters.TemplateIds.Contains(x.ProjectDesignTemplateId));
            if (filters.StatusIds != null) result = result.Where(x => filters.StatusIds.Contains((int)x.Status));
            if (filters.ReviewStatus != null) result = result.Where(x => filters.ReviewStatus.Contains(x.ReviewLevel));
            result = result.Where(x => x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.DeletedDate == null);

            return result.Select(r => new ReviewDto
            {
                Id = r.Id,
                SiteName = string.IsNullOrEmpty(r.ScreeningVisit.ScreeningEntry.Project.SiteName) ? r.ScreeningVisit.ScreeningEntry.Project.ProjectName : r.ScreeningVisit.ScreeningEntry.Project.SiteName,
                SiteCode = r.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                ScreeningEntryId = r.ScreeningVisit.ScreeningEntryId,
                ScreeningNo = r.ScreeningVisit.ScreeningEntry.ScreeningNo,
                ReviewLevel = r.ReviewLevel,
                StatusName = r.Status.GetDescription(),
                ScreeningTemplateId = r.Id,
                ProjectCode = r.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                ScreeningTemplateValue = r.ScreeningTemplateName,
                // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                Visit = r.ScreeningVisit.ScreeningVisitName,
                VolunteerName = r.ScreeningVisit.ScreeningEntry.AttendanceId != null ? r.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName : r.ScreeningVisit.ScreeningEntry.Randomization.Initial,
                SubjectNo = r.ScreeningVisit.ScreeningEntry.AttendanceId != null ? r.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.VolunteerNo : r.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber,
                RandomizationNumber = r.ScreeningVisit.ScreeningEntry.AttendanceId != null ? r.ScreeningVisit.ScreeningEntry.Attendance.ProjectSubject.Number : r.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber,
                ReviewLevelName = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflow.ProjectDesignId == r.ScreeningVisit.ScreeningEntry.ProjectDesignId
                && x.LevelNo == r.ReviewLevel && x.DeletedDate == null).Select(t => t.SecurityRole.RoleShortName).FirstOrDefault(),
                // added for dynamic column 04/06/2023
                WorkFlowReviewList = GetList(r, workflowlevel.WorkFlowText)
            }).ToList();
        }
        // added for dynamic column 04/06/2023
        public static List<WorkFlowReview> GetList(ScreeningTemplate r, List<WorkFlowText> WorkFlowText)
        {
            var result = new List<WorkFlowReview>();

            foreach (var item in WorkFlowText)
            {
                var rs = new WorkFlowReview();
                rs.ReviewerRole = item.RoleName;
                rs.LevelNo = item.LevelNo;
                if (r.ScreeningTemplateReview != null)
                {
                    rs.ReviewerName = r.ScreeningTemplateReview.Where(s => s.ScreeningTemplateId == r.Id && !s.IsRepeat && s.ReviewLevel == item.LevelNo).Select(x => x.CreatedByUser.UserName).FirstOrDefault();
                    rs.ReviewedDate = r.ScreeningTemplateReview.Where(s => s.ScreeningTemplateId == r.Id && !s.IsRepeat && s.ReviewLevel == item.LevelNo).Select(x => x.CreatedDate).FirstOrDefault();
                }
                result.Add(rs);
            }
            return result;
        }

        public IList<ReviewDto> GetScreeningReviewReportList(ScreeningQuerySearchDto filters)
        {
            var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            var result = All.Where(x => x.DeletedDate == null && x.ScreeningVisit.Status != ScreeningVisitStatus.NotStarted);

            if (filters.ProjectId != null) result = result.Where(x => x.ScreeningVisit.ScreeningEntry.ProjectId == filters.ProjectId);
            if (filters.StudyId != null) result = result.Where(x => x.ScreeningVisit.ScreeningEntry.StudyId == filters.StudyId);
            if (filters.VolunteerId != null) result = result.Where(x => x.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId == filters.VolunteerId);
            if (filters.ScreeningDate != null) result = result.Where(x => x.ScreeningVisit.ScreeningEntry.ScreeningDate.Date == Convert.ToDateTime(filters.ScreeningDate).Date);

            result = result.Where(x => x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.DeletedDate == null);

            return result.Select(r => new ReviewDto
            {
                Id = r.Id,
                SiteName = string.IsNullOrEmpty(r.ScreeningVisit.ScreeningEntry.Project.SiteName) ? r.ScreeningVisit.ScreeningEntry.Project.ProjectName : r.ScreeningVisit.ScreeningEntry.Project.SiteName,
                SiteCode = r.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                ScreeningEntryId = r.ScreeningVisit.ScreeningEntryId,
                ScreeningNo = r.ScreeningVisit.ScreeningEntry.ScreeningNo,
                ReviewLevel = r.ReviewLevel,
                StatusName = r.Status.GetDescription(),
                ScreeningTemplateId = r.Id,
                ProjectCode = r.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                ScreeningTemplateValue = r.ScreeningTemplateName,
                // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                Visit = r.ScreeningVisit.ScreeningVisitName,
                VolunteerName = r.ScreeningVisit.ScreeningEntry.AttendanceId != null ? r.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName : r.ScreeningVisit.ScreeningEntry.Randomization.Initial,
                SubjectNo = r.ScreeningVisit.ScreeningEntry.AttendanceId != null ? r.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.VolunteerNo : r.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber,
                RandomizationNumber = r.ScreeningVisit.ScreeningEntry.AttendanceId != null ? r.ScreeningVisit.ScreeningEntry.Attendance.ProjectSubject.Number : r.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber,
                ReviewLevelName = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflow.ProjectDesignId == r.ScreeningVisit.ScreeningEntry.ProjectDesignId
                && x.LevelNo == r.ReviewLevel && x.DeletedDate == null).Select(t => t.SecurityRole.RoleShortName).FirstOrDefault(),
                AttendanceDate = DateTime.Parse(r.ScreeningVisit.ScreeningEntry.Attendance.AttendanceDate.ToString()).ToString(GeneralSettings.DateFormat),
                ScreeningDate = DateTime.Parse(r.ScreeningVisit.ScreeningEntry.ScreeningDate.ToString()).ToString(GeneralSettings.DateFormat)
            }).ToList();
        }

        private WorkFlowButton SetWorkFlowButton(ScreeningTemplateValueBasic screeningValue,
            WorkFlowLevelDto workflowlevel, DesignScreeningTemplateDto designTemplateDto,
            ScreeningTemplateBasic templateBasic)
        {
            var workFlowButton = new WorkFlowButton();
            var statusId = (int)templateBasic.Status;

            if (templateBasic.IsLocked)
            {
                designTemplateDto.MyReview = false;
                designTemplateDto.IsSubmittedButton = false;
                return workFlowButton;
            }

            if (statusId > 2)
            {
                if (workflowlevel.LevelNo <= templateBasic.ReviewLevel || workflowlevel.LevelNo == 0)
                    workFlowButton.SelfCorrection = workflowlevel.SelfCorrection && screeningValue.QueryStatus != QueryStatus.SelfCorrection;

                if (workflowlevel.LevelNo == screeningValue.AcknowledgeLevel || (workflowlevel.LevelNo == 0 && workflowlevel.IsStartTemplate))
                    workFlowButton.Update = screeningValue.QueryStatus == QueryStatus.Open || screeningValue.QueryStatus == QueryStatus.Reopened;

                if (templateBasic.Status != ScreeningTemplateStatus.Completed && workflowlevel.IsGenerateQuery && (designTemplateDto.MyReview || workflowlevel.LevelNo == 0))
                    workFlowButton.Generate = screeningValue.QueryStatus == null || screeningValue.QueryStatus == QueryStatus.Closed;

                if (workflowlevel.LevelNo == screeningValue.ReviewLevel)
                    workFlowButton.Review = screeningValue.QueryStatus == QueryStatus.Answered || screeningValue.QueryStatus == QueryStatus.Resolved;

                if (workflowlevel.LevelNo == 0 && workFlowButton.Review)
                    workFlowButton.Review = screeningValue.UserRoleId == _jwtTokenAccesser.RoleId;

                if (screeningValue.IsSystem && screeningValue.QueryStatus == QueryStatus.Open && workflowlevel.IsStartTemplate)
                    workFlowButton.Update = screeningValue.QueryStatus == QueryStatus.Open;


                if (!screeningValue.IsSystem && screeningValue.QueryStatus == QueryStatus.Open && screeningValue.UserRoleId == _jwtTokenAccesser.RoleId)
                {
                    workFlowButton.Update = false;
                    workFlowButton.DeleteQuery = true;
                }

                if (workflowlevel.LevelNo == screeningValue.AcknowledgeLevel && screeningValue.AcknowledgeLevel != screeningValue.ReviewLevel)
                    workFlowButton.Acknowledge = screeningValue.QueryStatus == QueryStatus.Resolved || screeningValue.QueryStatus == QueryStatus.SelfCorrection;

                if (workflowlevel.LevelNo != -1 && templateBasic.IsNoCRF && !workflowlevel.IsNoCRF)
                {
                    workFlowButton.Generate = false;
                    workFlowButton.SelfCorrection = false;
                    workFlowButton.Acknowledge = false;
                }
            }

            if (templateBasic.Status == ScreeningTemplateStatus.Completed || templateBasic.IsCompleteReview)
                workFlowButton.SelfCorrection = false;

            workFlowButton.Clear = designTemplateDto.IsSubmittedButton;

            return workFlowButton;
        }

        public string GetStatusName(ScreeningTemplateBasic basicDetail, bool myReview, WorkFlowLevelDto workFlowLevel)
        {
            if (basicDetail.IsNA)
                return "Not Applicable";

            if (myReview)
                return "My Review";

            if (basicDetail.Status != ScreeningTemplateStatus.Completed && basicDetail.ReviewLevel != null &&
                basicDetail.ReviewLevel > 0)
            {
                if (workFlowLevel.WorkFlowText != null
                    && workFlowLevel.WorkFlowText.Any(x => x.LevelNo == basicDetail.ReviewLevel))
                    return workFlowLevel.WorkFlowText.FirstOrDefault(x => x.LevelNo == basicDetail.ReviewLevel)
                        ?.RoleName;
                return "Completed";
            }

            return basicDetail.Status.GetDescription();
        }

        public List<LockUnlockListDto> GetLockUnlockList(LockUnlockSearchDto lockUnlockParams)
        {
            var ProjectCode = _context.Project.Find(lockUnlockParams.ParentProjectId).ProjectCode;

            var ProjectDesignId = _context.ProjectDesign.Where(x => x.ProjectId == lockUnlockParams.ParentProjectId).Select(r => r.Id).FirstOrDefault();

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(ProjectDesignId);

            var screeningEntry = _context.ScreeningEntry.Where(r => r.ProjectDesignId == ProjectDesignId);

            if (lockUnlockParams.ProjectId != null) // Change by Tinku for add separate dropdown for parent project (24/06/2022) 
                screeningEntry = screeningEntry.Where(r => r.ProjectId == lockUnlockParams.ProjectId);

            if (lockUnlockParams.SubjectIds != null && lockUnlockParams.SubjectIds.Length > 0)
                screeningEntry = screeningEntry.Where(r => lockUnlockParams.SubjectIds.Contains(r.Id) && r.EntryType != DataEntryType.Screening);

            if (lockUnlockParams.PeriodIds != null && lockUnlockParams.PeriodIds.Length > 0)
                screeningEntry = screeningEntry.Where(r => lockUnlockParams.PeriodIds.Contains(r.ProjectDesignPeriodId));

            var grpresult = screeningEntry.Select(x => new LockUnlockListDto
            {
                Id = x.Id,
                screeningEntryId = x.Id,
                ProjectId = x.ProjectId,
                ProjectDesignId = x.ProjectDesignId,
                ParentProjectId = x.Project.ParentProjectId,
                ProjectCode = ProjectCode,
                IsHardLocked = lockUnlockParams.IsHardLocked,
                IsLocked = lockUnlockParams.IsLocked,
                ProjectName = x.Project.ProjectCode,
                PeriodName = x.ProjectDesignPeriod.DisplayName,
                ScreeningNo = x.ScreeningNo,
                Initial = x.RandomizationId != null ? x.Randomization.Initial : x.Attendance.Volunteer.AliasName,
                SubjectNo = x.RandomizationId != null ? x.Randomization.ScreeningNumber : x.Attendance.Volunteer.VolunteerNo,
                RandomizationNumber = x.RandomizationId != null ? x.Randomization.RandomizationNumber : x.Attendance.Volunteer.VolunteerNo,
                IsElectronicSignature = workflowlevel.IsElectricSignature,
                PeriodCount = All.Where(g => g.ScreeningVisit.ScreeningEntryId == x.Id && (lockUnlockParams.VisitIds == null || lockUnlockParams.VisitIds.Contains(g.ProjectDesignTemplate.ProjectDesignVisitId))
                                && (lockUnlockParams.TemplateIds == null || lockUnlockParams.TemplateIds.Contains(g.ProjectDesignTemplateId))
                                && g.DeletedDate == null && g.IsLocked == lockUnlockParams.IsLocked && g.IsHardLocked == lockUnlockParams.IsHardLocked
                                && (lockUnlockParams.DataEntryStatus != null && lockUnlockParams.DataEntryReviewStatus != null ? lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel)
                                  || lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status) : (lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel))
                                  && (lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status))))
                                    .Select(a => a.ScreeningVisit.ProjectDesignVisit.ProjectDesignPeriodId).Distinct().Count(),
                TemplateCount = All.Where(g => g.ScreeningVisit.ScreeningEntryId == x.Id && (lockUnlockParams.VisitIds == null || lockUnlockParams.VisitIds.Contains(g.ProjectDesignTemplate.ProjectDesignVisitId))
                                && (lockUnlockParams.TemplateIds == null || lockUnlockParams.TemplateIds.Contains(g.ProjectDesignTemplateId))
                                && g.DeletedDate == null && g.IsLocked == lockUnlockParams.IsLocked && g.IsHardLocked == lockUnlockParams.IsHardLocked
                                && (lockUnlockParams.DataEntryStatus != null && lockUnlockParams.DataEntryReviewStatus != null ? lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel)
                                  || lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status) : (lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel))
                                  && (lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status)))).Count(),
                VisitCount = All.Where(g => g.ScreeningVisit.ScreeningEntryId == x.Id && (lockUnlockParams.VisitIds == null || lockUnlockParams.VisitIds.Contains(g.ProjectDesignTemplate.ProjectDesignVisitId))
                                && (lockUnlockParams.TemplateIds == null || lockUnlockParams.TemplateIds.Contains(g.ProjectDesignTemplateId))
                                && g.DeletedDate == null && g.IsLocked == lockUnlockParams.IsLocked && g.IsHardLocked == lockUnlockParams.IsHardLocked
                                && (lockUnlockParams.DataEntryStatus != null && lockUnlockParams.DataEntryReviewStatus != null ? lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel)
                                  || lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status) : (lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel))
                                  && (lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status))))
                                    .Select(a => a.ScreeningVisit.Id).Distinct().Count(),
                lstTemplate = All.Where(g => g.ScreeningVisit.ScreeningEntryId == x.Id && (lockUnlockParams.VisitIds == null || lockUnlockParams.VisitIds.Contains(g.ProjectDesignTemplate.ProjectDesignVisitId))
                                && (lockUnlockParams.TemplateIds == null || lockUnlockParams.TemplateIds.Contains(g.ProjectDesignTemplateId))
                                && g.DeletedDate == null && g.IsLocked == lockUnlockParams.IsLocked && g.IsHardLocked == lockUnlockParams.IsHardLocked
                                && (lockUnlockParams.DataEntryStatus != null && lockUnlockParams.DataEntryReviewStatus != null ? lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel)
                                  || lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status) : (lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel))
                                  && (lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status))))
                    .Select(t => new LockUnlockListDto
                    {
                        TemplateId = t.ProjectDesignTemplateId,
                        ScreeningTemplateId = t.Id,
                        screeningEntryId = t.ScreeningVisit.ScreeningEntryId,
                        ProjectCode = ProjectCode,
                        ProjectName = t.ScreeningVisit.ScreeningEntry.Project.ParentProjectId != null ? t.ScreeningVisit.ScreeningEntry.Project.ProjectCode : "",
                        PeriodName = t.ScreeningVisit.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                        ParentProjectId = t.ScreeningVisit.ScreeningEntry.Project.ParentProjectId,
                        VisitId = t.ScreeningVisit.ProjectDesignVisitId,
                        // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                        VisitName = t.ScreeningVisit.ScreeningVisitName + Convert.ToString(t.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + t.ScreeningVisit.RepeatedVisitNumber),
                        ScreeningTemplateParentId = t.ParentId,
                        // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                        TemplateName = t.RepeatSeqNo == null && t.ParentId == null ? t.ProjectDesignTemplate.DesignOrder + " " + t.ScreeningTemplateName
                                        : t.ProjectDesignTemplate.DesignOrder + "." + t.RepeatSeqNo + " " + t.ScreeningTemplateName,
                        DesignOrder = t.ProjectDesignTemplate.DesignOrder.ToString(),
                        SeqNo = t.ProjectDesignTemplate.DesignOrder,
                        ReviewLevel = t.ReviewLevel,
                        ScreeningTemplateStatus = t.Status
                    }).OrderBy(b => b.VisitId).ThenBy(a => a.SeqNo).ThenBy(a => a.ScreeningTemplateId).ToList()
            }).OrderBy(x => x.ProjectId).ToList();

            grpresult.ForEach(x =>
            {
                x.lstTemplate.ForEach(y =>
                {
                    y.DataEntryStatus = GetStatusName(new ScreeningTemplateBasic { ReviewLevel = y.ReviewLevel, Status = y.ScreeningTemplateStatus }, false, workflowlevel);
                });
            });
            return grpresult.OrderBy(x => x.SubjectNo).Where(x => x.lstTemplate.Count > 0).ToList();
        }


        public ScreeningTemplateValueSaveBasics ValidateVariableValue(ScreeningTemplateValue screeningTemplateValue, List<EditCheckIds> EditCheckIds, CollectionSources? collectionSource)
        {
            var result = new ScreeningTemplateValueSaveBasics();
            result.Children = screeningTemplateValue.Children.Select(r => new ScreeningTemplateValueChildBasic { Id = r.Id }).ToList();
            result.Id = screeningTemplateValue.Id;

            if ((EditCheckIds != null && EditCheckIds.Count() > 0) || collectionSource == CollectionSources.Date
                || collectionSource == CollectionSources.DateTime || collectionSource == CollectionSources.Time)
            {
                var value = screeningTemplateValue.IsNa ? "NA" : screeningTemplateValue.Value;

                if (!screeningTemplateValue.IsNa && screeningTemplateValue.Children != null && screeningTemplateValue.Children.Count > 0)
                    value = string.Join(",", _screeningTemplateValueChildRepository.All.AsNoTracking().Where(x => x.ScreeningTemplateValueId == screeningTemplateValue.Id && x.Value == "true").Select(t => t.ProjectDesignVariableValueId));

                var screeningTemplate = All.AsNoTracking().Where(x => x.Id == screeningTemplateValue.ScreeningTemplateId).
                    Select(r => new { r.Id, r.ScreeningVisitId, r.Status, r.ParentId, VisitParent = r.ScreeningVisit.ParentId, r.ProjectDesignTemplateId, r.ScreeningVisit.ScreeningEntryId, r.ScreeningVisit.ProjectDesignVisitId }).FirstOrDefault();
                if (screeningTemplate != null)
                {


                    var editResult = _editCheckImpactRepository.VariableValidateProcess(screeningTemplate.ScreeningEntryId, screeningTemplateValue.ScreeningTemplateId,
                        value, screeningTemplate.ProjectDesignTemplateId,
                        screeningTemplateValue.ProjectDesignVariableId, EditCheckIds, false, screeningTemplate.ScreeningVisitId, screeningTemplate.ProjectDesignVisitId, screeningTemplateValue.IsNa, screeningTemplate.Status);

                    if (screeningTemplate.ParentId == null && screeningTemplate.VisitParent == null)
                    {
                        var scheduleResult = _scheduleRuleRespository.ValidateByVariable(screeningTemplate.ScreeningEntryId, screeningTemplate.ScreeningVisitId,
                                           screeningTemplateValue.Value, screeningTemplate.ProjectDesignTemplateId,
                                           screeningTemplateValue.ProjectDesignVariableId, true);

                        result.EditCheckResult = _scheduleRuleRespository.VariableResultProcess(editResult, scheduleResult);
                    }
                    else
                        result.EditCheckResult = _scheduleRuleRespository.VariableResultProcess(editResult, null);
                }
            }

            return result;
        }

        public IList<DropDownDto> GetTemplateByLockedDropDown(LockUnlockDDDto lockUnlockDDDto)
        {

            var sites = _context.Project.Where(x => x.ParentProjectId == lockUnlockDDDto.ProjectId).ToList().Select(x => x.Id).ToList(); // Change by Tinku for add separate dropdown for parent project (24/06/2022) 

            var Templates = All.Include(a => a.ProjectDesignTemplate).Include(a => a.ScreeningVisit)
                .ThenInclude(a => a.ScreeningEntry)
                .Where(a => a.DeletedDate == null && lockUnlockDDDto.ChildProjectId > 0 ? a.ScreeningVisit.ScreeningEntry.ProjectId == lockUnlockDDDto.ChildProjectId : sites.Contains(a.ScreeningVisit.ScreeningEntry.ProjectId) // Change by Tinku for add separate dropdown for parent project (24/06/2022) 
                ).ToList();

            if (lockUnlockDDDto.SubjectIds != null)
                Templates = Templates.Where(a => lockUnlockDDDto.SubjectIds.Contains(a.ScreeningVisit.ScreeningEntryId)).ToList();

            if (lockUnlockDDDto.Id != null)
                Templates = Templates.Where(a => lockUnlockDDDto.Id.Contains(a.ScreeningVisit.ProjectDesignVisitId)).ToList();

            Templates = Templates.Where(a => a.IsLocked == lockUnlockDDDto.IsLocked && a.IsHardLocked == lockUnlockDDDto.IsHardLocked).ToList();

            return Templates.GroupBy(w => w.ProjectDesignTemplateId).Select(x => new DropDownDto
            {
                Id = x.FirstOrDefault().ProjectDesignTemplateId,
                // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                Value = x.FirstOrDefault().ScreeningTemplateName,
                ExtraData = x.FirstOrDefault().ProjectDesignTemplate.DesignOrder
            }).OrderBy(x => x.ExtraData).ToList();
        }

        public IList<VisitDeviationReport> GetVisitDeviationReport(VisitDeviationReportSearchDto filters)
        {

            int parentprojectid = filters.ProjectId;
            int? siteId = filters.SiteId;
            var parentIds = new List<int>();
            if (siteId == null)
            {
                parentIds = _context.Project.Where(x => x.ParentProjectId == filters.ProjectId).Select(y => y.Id).ToList();
            }
            else
            {
                parentIds.Add((int)filters.SiteId);
            }

            var studycode = _context.Project.Where(x => x.Id == parentprojectid).Select(s => s.ProjectCode).FirstOrDefault();


            string sqlqry = @";with cts as(
                                select *,ROW_NUMBER() OVER (ORDER BY SiteCode,Initial) Id,'" + studycode + @"' StudyCode,'' RefValueExcel,'' TargetValueExcel,
										case when collectionsource = 3 then
                                        case when TargetValue < DATEADD(day,(NoOfDay - NegativeDeviation) , RefValue) then DATEDIFF(DAY,DATEADD(day,(NoOfDay - NegativeDeviation) , RefValue), TargetValue) else
		                                case when TargetValue > DATEADD(day,(NoOfDay + PositiveDeviation) , RefValue) then DATEDIFF(DAY, DATEADD(day,(NoOfDay + PositiveDeviation) , RefValue), TargetValue) end end 
										else 
										case when collectionsource = 4 then
										case when TargetValue < dateadd(minute,(NegativeDeviation*-1),Dateadd(minute,MM,DATEADD(hour,HH,convert(datetime,RefValue)))) then DATEDIFF(MINUTE,dateadd(minute,(NegativeDeviation*-1),Dateadd(minute,MM,DATEADD(hour,HH,convert(datetime,RefValue)))), TargetValue) else
		                                case when TargetValue > dateadd(minute,PositiveDeviation,Dateadd(minute,MM,DATEADD(hour,HH,convert(datetime,RefValue)))) then DATEDIFF(MINUTE, dateadd(minute,PositiveDeviation,Dateadd(minute,MM,DATEADD(hour,HH,convert(datetime,RefValue)))), TargetValue) end end 
										--case when TargetValue < DATEADD(day,(NoOfDay - NegativeDeviation) , RefValue) then DATEDIFF(DAY,DATEADD(day,(NoOfDay - NegativeDeviation) , RefValue), TargetValue) else
		        --                        case when TargetValue > DATEADD(day,(NoOfDay + PositiveDeviation) , RefValue) then DATEDIFF(DAY, DATEADD(day,(NoOfDay + PositiveDeviation) , RefValue), TargetValue) end end 
										end end Deviation,
										case when CollectionSource = 3 then 'Days' else 'Minutes' end as Unit
                                from
                                (
	                                select Randomization.Initial,Randomization.ScreeningNumber ScreeningNo,isnull(Randomization.RandomizationNumber,'') RandomizationNumber,
									ScreeningEntry.ScreeningDate,ProjectDesignVisit.DisplayName RefVisit,
	                                ProjectDesignTemplate.TemplateName RefTemplate,ProjectDesignVariable.VariableName RefVariable,
                                    convert(datetime,ScreeningTemplateValue.Value) RefValue,
	                                convert(datetime,tvalue.Value) TargetValue,
                                    tVisit.DisplayName TargetVisit,tDesignTemplate.TemplateName TargetTemplate,tvariable.VariableName TargetVariable,
	                                isnull(ProjectScheduleTemplate.NoOfDay,0) NoOfDay,ProjectScheduleTemplate.PositiveDeviation,ProjectScheduleTemplate.NegativeDeviation,Project.ProjectCode SiteCode,
									case when variable.collectionsource = 3 then
                                    CASE WHEN convert(date,tvalue.Value) >= DATEADD(day,(NoOfDay - NegativeDeviation) , convert(date,ScreeningTemplateValue.Value)) 
                                    AND  convert(date,tvalue.Value) <= DATEADD(day,(NoOfDay + PositiveDeviation) , convert(date,ScreeningTemplateValue.Value)) THEN 1 ELSE 0 END 
									else 
									case when variable.collectionsource = 4 then
									CASE WHEN convert(datetime,tvalue.Value) >= dateadd(minute,(NegativeDeviation*-1),Dateadd(minute,MM,DATEADD(hour,HH,convert(datetime,ScreeningTemplateValue.Value)))) 
                                    AND  convert(datetime,tvalue.Value) <= dateadd(minute,PositiveDeviation,Dateadd(minute,MM,DATEADD(hour,HH,convert(datetime,ScreeningTemplateValue.Value)))) THEN 1 ELSE 0 END 
									end 
									end flag,
                                    ScreeningEntry.RandomizationId,ScreeningTemplateValue.ProjectDesignVariableId,ScreeningTemplate.ProjectDesignTemplateId,ScreeningVisit.ProjectDesignVisitId,
                                    ProjectDesignVisit.ProjectDesignPeriodId,Project.Id ProjectId,variable.collectionsource,isnull(HH,0) HH,isnull(MM,0) MM
	                                from
	                                --ScreeningTemplateValue
									(
										select *
										from
										(
											select a.id,a.screeningtemplateid,a.projectdesignvariableid,replace(a.value,',','') value,a.deleteddate
											from
											screeningtemplatevalue a,
											projectdesignvariable b,
											variable c
											where
											a.projectdesignvariableid = b.id and
											b.variableid = c.id and
											c.collectionsource in (3,4) and
											isnull(value,'') <> '' and
											len(value) > 4
										) a
										where
										isdate(a.value) = 1
									) ScreeningTemplateValue
	                                inner join
	                                ScreeningTemplate
	                                on
	                                ScreeningTemplateValue.ScreeningTemplateId = ScreeningTemplate.Id and
	                                ScreeningTemplate.DeletedDate is null and
	                                ScreeningTemplateValue.value is not null --and
	                                --isdate(ScreeningTemplateValue.value) = 1
									inner join
									ScreeningVisit 
									on
									ScreeningTemplate.ScreeningVisitId = ScreeningVisit.Id
	                                inner join
	                                ScreeningEntry
	                                on
	                                ScreeningVisit.ScreeningEntryId = ScreeningEntry.Id
	                                inner join
	                                Randomization
	                                on
	                                ScreeningEntry.RandomizationId = Randomization.Id
	                                inner join
	                                ProjectDesignVariable
	                                on
	                                ScreeningTemplateValue.ProjectDesignVariableId = ProjectDesignVariable.id and
	                                ProjectDesignVariable.deleteddate is null
									inner join
									Variable
									on
									ProjectDesignVariable.variableid = Variable.id and
									variable.Collectionsource in (3,4)
	                                inner join
	                                ProjectDesignTemplate
	                                on
	                                ProjectDesignVariable.projectdesigntemplateid = ProjectDesignTemplate.id and
	                                ProjectDesignTemplate.deleteddate is null
	                                inner join
	                                projectdesignvisit
	                                on
	                                ProjectDesignTemplate.projectdesignvisitid = projectdesignvisit.id and
	                                projectdesignvisit.deleteddate is null
	                                inner join
	                                projectschedule
	                                on
	                                projectschedule.projectdesignvariableid = ProjectDesignVariable.Id and
	                                --projectschedule.projectdesigntemplateid = ProjectDesignTemplate.Id and
	                                projectschedule.deleteddate is null
	                                inner join
	                                ProjectScheduletemplate
	                                on
	                                projectscheduletemplate.projectscheduleid = ProjectSchedule.id and
	                                ProjectSchedule.deleteddate is null and
	                                projectscheduletemplate.operator = 2
	                                inner join
	                                --ScreeningTemplateValue tvalue
									(
										select *
										from
										(
											select a.id,a.screeningtemplateid,a.projectdesignvariableid,replace(a.value,',','') value,a.deleteddate
											from
											screeningtemplatevalue a,
											projectdesignvariable b,
											variable c
											where
											a.projectdesignvariableid = b.id and
											b.variableid = c.id and
											c.collectionsource in (3,4) and
											isnull(value,'') <> '' and
											len(value) > 4
										) a
										where
										isdate(a.value) = 1
									) tvalue
	                                on
	                                tvalue.projectdesignvariableid = ProjectScheduletemplate.projectdesignvariableid and
	                                tvalue.DeletedDate is null and
	                                tvalue.Value is not null --and
	                                --ISDATE(tvalue.Value) = 1
	                                inner join
	                                ScreeningTemplate tTemplate
	                                on
	                                tvalue.ScreeningTemplateId = tTemplate.Id --and
	                                --tTemplate.ProjectDesignTemplateId = ProjectScheduletemplate.ProjectDesignTemplateId
									inner join
									ScreeningVisit tScreeningvisit
									on
									tTemplate.ScreeningVisitId = tScreeningvisit.Id and
									tScreeningvisit.ScreeningEntryId = ScreeningEntry.Id
	                                inner join
	                                ProjectDesignVariable tvariable
	                                on
	                                tvalue.ProjectDesignVariableId = tvariable.Id and
	                                tvariable.DeletedDate is null 
	                                inner join
	                                ProjectDesignTemplate tDesignTemplate
	                                on
	                                tvariable.ProjectDesignTemplateId = tDesignTemplate.Id and
	                                tDesignTemplate.DeletedDate is null 
	                                inner join
	                                ProjectDesignVisit tVisit
	                                on
	                                tDesignTemplate.ProjectDesignVisitId = tVisit.Id and
	                                tVisit.DeletedDate is null
	                                inner join
	                                Project 
	                                on
	                                Randomization.ProjectId = Project.Id
                                ) a
                                )
								select * from cts where flag=0

";
            var finaldata = _context.FromSql<VisitDeviationReport>(sqlqry).ToList();

            finaldata = finaldata.Where(x => parentIds.Contains(x.ProjectId)).ToList();
            if (filters.PeriodIds != null && filters.PeriodIds.Length > 0) finaldata = finaldata.Where(x => filters.PeriodIds.Contains(x.ProjectDesignPeriodId)).ToList();
            if (filters.VisitIds != null && filters.VisitIds.Length > 0) finaldata = finaldata.Where(x => filters.VisitIds.Contains(x.ProjectDesignVisitId)).ToList();
            if (filters.SubjectIds != null && filters.SubjectIds.Length > 0) finaldata = finaldata.Where(x => filters.SubjectIds.Contains(x.RandomizationId)).ToList();
            if (filters.TemplateIds != null && filters.TemplateIds.Length > 0) finaldata = finaldata.Where(x => filters.TemplateIds.Contains(x.ProjectDesignTemplateId)).ToList();
            if (filters.VariableIds != null && filters.VariableIds.Length > 0) finaldata = finaldata.Where(x => filters.VariableIds.Contains(x.ProjectDesignVariableId)).ToList();
            var dateformat = _context.AppSetting.Where(x => x.KeyName == "GeneralSettingsDto.DateFormat").Select(s => s.KeyValue).FirstOrDefault();
            dateformat = dateformat.Replace("/", "\\/");
            var timeformat = _context.AppSetting.Where(x => x.KeyName == "GeneralSettingsDto.TimeFormat").Select(s => s.KeyValue).FirstOrDefault();
            var datetimeformat = dateformat + " " + timeformat;
            for (int i = 0; i < finaldata.Count; i++)
            {
                finaldata[i].RefValueExcel = finaldata[i].RefValue.ToString(datetimeformat);
                finaldata[i].TargetValueExcel = finaldata[i].TargetValue.ToString(datetimeformat);
            }
            return finaldata;
        }

        public bool CheckLockedProject(int ProjectId)
        {
            var ParentProject = _context.Project.Where(x => x.Id == ProjectId).Select(s => s.ParentProjectId).FirstOrDefault();
            var sites = _context.Project.Where(x => x.ParentProjectId == ProjectId).Select(x => x.Id).ToList();

            var ScreeningTemplates = All.Where(y => y.DeletedDate == null && ParentProject != null ? y.ScreeningVisit.ScreeningEntry.ProjectId == ProjectId
            : sites.Contains(y.ScreeningVisit.ScreeningEntry.ProjectId)).ToList();

            var IsLocked = ScreeningTemplates.Count() <= 0 || ScreeningTemplates.Any(y => y.IsLocked == false) ? false : true;
            return IsLocked;
        }

        public IList<ScheduleDueReport> GetScheduleDueReport(ScheduleDueReportSearchDto filters)
        {
            int parentprojectid = filters.ProjectId;
            int? siteId = filters.SiteId;
            var parentIds = new List<int>();
            if (siteId == null)
            {
                parentIds = _context.Project.Where(x => x.ParentProjectId == filters.ProjectId).Select(y => y.Id).ToList();
            }
            else
            {
                parentIds.Add((int)filters.SiteId);
            }
            var studycode = _context.Project.Where(x => x.Id == parentprojectid).Select(s => s.ProjectCode).FirstOrDefault();

            var result = All.Where(x => x.DeletedDate == null && x.ScheduleDate != null && (x.Status == ScreeningTemplateStatus.Pending || x.Status == ScreeningTemplateStatus.InProcess) && x.ScheduleDate <= DateTime.Today);
            if (filters.PeriodIds != null && filters.PeriodIds.ToList().Count > 0) result = result.Where(x => filters.PeriodIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectDesignPeriodId));
            if (filters.VisitIds != null && filters.VisitIds.ToList().Count > 0) result = result.Where(x => filters.VisitIds.Contains(x.ScreeningVisit.ProjectDesignVisitId));
            if (filters.TemplateIds != null && filters.TemplateIds.ToList().Count > 0) result = result.Where(x => filters.TemplateIds.Contains(x.ProjectDesignTemplateId));
            if (filters.SubjectIds != null && filters.SubjectIds.ToList().Count > 0) result = result.Where(x => filters.SubjectIds.Contains(x.ScreeningVisit.ScreeningEntry.RandomizationId));
            if (filters.fromDate != null)
            {
                result = result.Where(x => x.ScheduleDate >= Convert.ToDateTime(filters.fromDate));
            }
            if (filters.toDate != null)
            {
                result = result.Where(x => x.ScheduleDate <= Convert.ToDateTime(filters.toDate));
            }
            if (parentIds != null) result = result.Where(x => parentIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectId));
            var patientstatuslist = Enum.GetValues(typeof(ScreeningPatientStatus))
                        .Cast<ScreeningPatientStatus>().Where(x => x != ScreeningPatientStatus.Completed &&
                                                                    x != ScreeningPatientStatus.Withdrawal &&
                                                                    x != ScreeningPatientStatus.ScreeningFailure &&
                                                                    x != ScreeningPatientStatus.OnHold)
                        .Select(d => (int)d)
                        .ToList();
            result = result.Where(x => patientstatuslist.Contains((int)x.ScreeningVisit.ScreeningEntry.Randomization.PatientStatusId));
            var dateformat = _context.AppSetting.Where(x => x.KeyName == "GeneralSettingsDto.DateFormat").Select(s => s.KeyValue).FirstOrDefault();
            if (dateformat != null)
            {
                dateformat = dateformat.Replace("/", "\\/");
            }

            return result.Select(r => new ScheduleDueReport
            {
                Id = r.Id,
                studyCode = studycode,
                siteCode = r.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                screeningNo = r.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber,
                initial = r.ScreeningVisit.ScreeningEntry.Randomization.Initial,
                randomizationNumber = r.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber,
                // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                visitName = r.ScreeningVisit.ScreeningVisitName,
                // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                templateName = r.ScreeningTemplateName,
                scheduleDate = r.ScheduleDate,
                scheduleDateExcel = Convert.ToDateTime(r.ScheduleDate).ToString(dateformat)
            }).OrderBy(x => x.screeningNo).ThenBy(x => x.visitName).ThenBy(x => x.templateName).ToList();
        }
        //for variable email .prakash chauhan 14-05-2022
        public void SendVariableEmail(ScreeningTemplateValueDto screeningTemplateValueDto, ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto)
        {
            if (screeningTemplateValueDto == null)
            {
                screeningTemplateValueDto = new ScreeningTemplateValueDto();
                if (screeningTemplateValueQueryDto != null)
                {
                    screeningTemplateValueDto.Value = screeningTemplateValueQueryDto.Value;
                    screeningTemplateValueDto.CollectionSource = screeningTemplateValueQueryDto.CollectionSource;
                    screeningTemplateValueDto.Id = screeningTemplateValueQueryDto.ScreeningTemplateValueId;
                    var data = _context.ScreeningTemplateValue.Where(x => x.Id == screeningTemplateValueQueryDto.ScreeningTemplateValueId).FirstOrDefault();
                    if (data != null)
                        screeningTemplateValueDto.ProjectDesignVariableId = data.ProjectDesignVariableId;


                }
            }
            if (screeningTemplateValueDto.CollectionSource == CollectionSources.RadioButton)
            {
                if (screeningTemplateValueDto.ProjectDesignVariableId > 0)
                {
                    var data = _context.SendEmailOnVariableChangeSetting.Where(x => x.ProjectDesignVariableId == screeningTemplateValueDto.ProjectDesignVariableId
                    && x.DeletedDate == null && x.CollectionValue.Contains(screeningTemplateValueDto.Value)).FirstOrDefault();
                    if (data != null)
                    {
                        if (data.Email.Contains(","))
                        {
                            var list = data.Email.Split(',').ToList();
                            if (list != null && list.Count > 0)
                            {
                                list.ForEach(x =>
                                {
                                    _emailSenderRespository.SendVariableValueEmail(screeningTemplateValueDto, x, data.EmailTemplate);
                                });
                            }
                        }
                        else
                        {
                            _emailSenderRespository.SendVariableValueEmail(screeningTemplateValueDto, data.Email, data.EmailTemplate);

                        }
                    }
                }
            }

        }

        public static string PreLabelSetting(ScreeningTemplate t, ProjectDesignTemplate pt, TemplateVariableSequenceNoSetting seq)
        {
            string str = "";
            if (!String.IsNullOrEmpty(pt.PreLabel))
                str = pt.PreLabel;

            if (!seq.IsTemplateSeqNo && t.RepeatSeqNo != null)
            {
                if (!String.IsNullOrEmpty(seq.RepeatPrefix))
                    str += ((!String.IsNullOrEmpty(pt.PreLabel)) ? seq.SeparateSign : "") + seq.RepeatPrefix;

                if (seq.RepeatSeqNo != null)
                {
                    if (seq.RepeatSubSeqNo == null)
                        str += ((!String.IsNullOrEmpty(seq.RepeatPrefix) || (!String.IsNullOrEmpty(pt.PreLabel))) ? seq.SeparateSign : "") + (seq.RepeatSeqNo + t.RepeatSeqNo.Value - 1).ToString();
                    else
                        str += ((!String.IsNullOrEmpty(seq.RepeatPrefix) || (!String.IsNullOrEmpty(pt.PreLabel))) ? seq.SeparateSign : "") + seq.RepeatSeqNo + seq.SeparateSign + (seq.RepeatSubSeqNo + t.RepeatSeqNo.Value - 1).ToString();
                }
            }

            return str;
        }

        // Screening Grid View
        public IList<DesignScreeningTemplateDto> GetScreeningGridView(DesignScreeningTemplateDto designTemplateDto, int ScreeningTemplateId)
        {
            var results = new List<DesignScreeningTemplateDto>();
            var screeningTemplates = All.Where(x => (x.Id == ScreeningTemplateId || x.ParentId == ScreeningTemplateId) && x.DeletedDate == null).ToList();

            var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            GeneralSettings.TimeFormat = GeneralSettings.TimeFormat.Replace("a", "tt");
            var screeningEntryId = All.Where(x => (x.Id == ScreeningTemplateId || x.ParentId == ScreeningTemplateId) && x.DeletedDate == null).Select(x => x.ScreeningVisit.ScreeningEntryId).FirstOrDefault();

            foreach (var item in screeningTemplates)
            {

                var repatTemplate = designTemplateDto.DeepCopy();
                repatTemplate.DesignOrder = item.RepeatSeqNo.ToString();
                repatTemplate.TemplateName = item.ScreeningTemplateName;

                var values = GetScreeningValues(item.Id);
                values.ForEach(t =>
                {
                    var variable = repatTemplate.Variables.FirstOrDefault(v => v.ProjectDesignVariableId == t.ProjectDesignVariableId);
                    if (variable != null)
                    {
                        variable.Id = t.Id;
                        variable.ScreeningValue = t.Value;
                        variable.ScreeningTemplateValueId = t.Id;

                        if (variable.Values != null && t.Value != "" && t.Value != null && (variable.CollectionSource == CollectionSources.RadioButton || variable.CollectionSource == CollectionSources.ComboBox
                       || variable.CollectionSource == CollectionSources.NumericScale))
                        {
                            if (variable.CollectionSource == CollectionSources.NumericScale)
                            {
                                if (t.Value == "0")
                                    variable.ScreeningValue = "";
                                else
                                    variable.ScreeningValue = variable.Values.Where(x => x.Id == Convert.ToInt32(t.Value)).Select(s => s.ValueName.ToString()).FirstOrDefault();
                            }
                            else
                                variable.ScreeningValue = variable.Values.Where(x => x.Id == Convert.ToInt32(t.Value)).Select(s => s.ValueName.ToString()).FirstOrDefault();
                        }
                        if (variable.Values != null && (variable.CollectionSource == CollectionSources.CheckBox || variable.CollectionSource == CollectionSources.MultiCheckBox))
                        {
                            if (variable.CollectionSource == CollectionSources.CheckBox)
                            {
                                if (t.Value == "true")
                                    variable.ScreeningValue = _context.ProjectDesignVariableValue.Where(x => x.ProjectDesignVariable.Id == t.ProjectDesignVariableId && x.DeletedDate == null).Select(s => s.ValueName).FirstOrDefault();
                                else
                                    variable.ScreeningValue = String.Empty;
                            }
                            else
                            {
                                var childValue = t.Children.Where(v => v.Value == "true").Select(x => x.ProjectDesignVariableValueId).ToList();
                                variable.ScreeningValue = string.Join(",", _context.ProjectDesignVariableValue.Where(x => childValue.Contains(x.Id)).Select(s => s.ValueName));
                            }
                        }

                        if (variable.Values != null && t.Value != "" && t.Value != null && variable.CollectionSource == CollectionSources.DateTime)
                        {
                            DateTime dDate;
                            var dt = !string.IsNullOrEmpty(t.Value) ? DateTime.TryParse(t.Value, out dDate) ? DateTime.Parse(t.Value).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : t.Value : "";
                            variable.ScreeningValue = dt;
                        }
                        else if (variable.Values != null && t.Value != "" && t.Value != null && variable.CollectionSource == CollectionSources.Date)
                        {
                            DateTime dDate;
                            string dt = !string.IsNullOrEmpty(t.Value) ? DateTime.TryParse(t.Value, out dDate) ? DateTime.Parse(t.Value).ToString(GeneralSettings.DateFormat, CultureInfo.InvariantCulture) : t.Value : "";
                            variable.ScreeningValue = dt;
                        }
                        else if (variable.Values != null && t.Value != "" && t.Value != null && variable.CollectionSource == CollectionSources.Time)
                        {
                            var dt = !string.IsNullOrEmpty(t.Value) ? DateTime.Parse(t.Value).ToString(GeneralSettings.TimeFormat, CultureInfo.InvariantCulture) : "";
                            variable.ScreeningValue = dt;
                        }

                        if (variable.CollectionSource == CollectionSources.Relation && variable.RelationProjectDesignVariableId > 0)
                        {
                            variable.Values = _screeningTemplateValueRepository.GetScreeningRelation(variable.RelationProjectDesignVariableId ?? 0, screeningEntryId);
                            if (variable.Values.Any() && variable.ScreeningValue != null)
                                variable.ScreeningValue = variable.Values.Where(x => x.Id == Convert.ToInt32(variable.ScreeningValue)).Select(s => s.ValueName).FirstOrDefault();
                        }

                    }
                });
                results.Add(repatTemplate);
            }
            return results;
        }

        public void DeleteRepeatVisitTemplate(int Id)
        {
            var templates = All.Where(t => t.ScreeningVisitId == Id).ToList();
            if (templates.Count != 0)
            {
                templates.ForEach(x =>
                {
                    _screeningTemplateValueRepository.DeleteRepeatTemplateValue(x.Id);
                    var record = Find(x.Id);
                    Delete(record);
                });
            }
        }

        public DesignScreeningTemplateDto GetTemplateForBarcode(DesignScreeningTemplateDto designTemplateDto, int screeningTemplateId, bool IsDosing, bool firstTime)
        {
            var screeningTemplateBasic = GetScreeningTemplateBasic(screeningTemplateId);

            var statusId = (int)screeningTemplateBasic.Status;

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(screeningTemplateBasic.ProjectDesignId);

            designTemplateDto.ScreeningTemplateId = screeningTemplateId;
            designTemplateDto.IsSubmittedButton = statusId < 3 && workflowlevel.IsStartTemplate;
            designTemplateDto.IsUnSubmittedButton = screeningTemplateBasic.Status == ScreeningTemplateStatus.Submitted && workflowlevel.IsStartTemplate;

            if (screeningTemplateBasic.Status == ScreeningTemplateStatus.Reviewed)
                designTemplateDto.IsUnReviewedButton = workflowlevel.LevelNo == screeningTemplateBasic.LastReviewLevel;

            if (workflowlevel.LevelNo >= 0 && designTemplateDto.IsRepeated)
                designTemplateDto.IsRepeated = workflowlevel.IsStartTemplate;

            designTemplateDto.MyReview = workflowlevel.LevelNo == screeningTemplateBasic.ReviewLevel;
            designTemplateDto.ScreeningTemplateId = screeningTemplateBasic.Id;
            designTemplateDto.ParentId = screeningTemplateBasic.ParentId;
            designTemplateDto.IsLocked = screeningTemplateBasic.IsLocked;
            designTemplateDto.Status = screeningTemplateBasic.Status;
            designTemplateDto.StatusName = GetStatusName(screeningTemplateBasic, workflowlevel.LevelNo == screeningTemplateBasic.ReviewLevel, workflowlevel);

            if (screeningTemplateBasic.Status == ScreeningTemplateStatus.Pending)
                _screeningTemplateValueRepository.UpdateDefaultValue(designTemplateDto.Variables, screeningTemplateId);

            if (screeningTemplateBasic.Status == ScreeningTemplateStatus.Pending || screeningTemplateBasic.Status == ScreeningTemplateStatus.InProcess)
                _screeningTemplateValueRepository.UpdateTemplateConfigurationUploadRandomizationValue(designTemplateDto, screeningTemplateId);

            if (!firstTime)
                _screeningTemplateValueRepository.UpdateDefaultValueForDosing(designTemplateDto.Variables, screeningTemplateId, IsDosing);

            SetScreeningValue(designTemplateDto, screeningTemplateBasic, workflowlevel);

            if (designTemplateDto.Status == ScreeningTemplateStatus.Pending && designTemplateDto.IsSchedule &&
                (screeningTemplateBasic.PatientStatus == ScreeningPatientStatus.ScreeningFailure ||
               screeningTemplateBasic.VisitStatus == ScreeningVisitStatus.Withdrawal ||
               screeningTemplateBasic.VisitStatus == ScreeningVisitStatus.Missed ||
               screeningTemplateBasic.VisitStatus == ScreeningVisitStatus.OnHold))
                designTemplateDto.IsSubmittedButton = false;

            designTemplateDto.Variables.Where(x => x.IsEncrypt == true).ToList().ForEach(c =>
            {
                c.ScreeningValueOld = null;
                c.ScreeningValue = null;
                c.HasQueries = false;
                c.WorkFlowButton = null;
                c.EditCheckValidation = null;
            });

            return designTemplateDto;
        }


        public void SendEmailOnVaribleConfiguration(int id)
        {
            var screeningTemplate = All.
                Include(s => s.ScreeningTemplateValues).
                ThenInclude(s => s.ProjectDesignVariable).
                Include(s => s.ScreeningVisit).
                ThenInclude(s => s.ScreeningEntry).
                ThenInclude(s => s.Randomization).
                ThenInclude(s => s.User).Where(s => s.Id == id).FirstOrDefault();
            var result = _emailConfigurationEditCheckRepository.ValidatWithScreeningTemplate(screeningTemplate);
            if (result != null && result.IsValid)
            {
                var finaldata = _emailConfigurationEditCheckRepository.SendEmailonEmailvariableConfiguration(screeningTemplate);
                _emailConfigurationEditCheckRepository.SendEmailonEmailvariableConfigurationSMS(finaldata);

            }
        }

        public List<NAReportDto> NAReport(NAReportSearchDto filters)
        {
            return All.Include(x => x.ScreeningVisit)
                  .ThenInclude(x => x.ScreeningEntry)
                  .ThenInclude(x => x.Randomization)
                  .Where(x => x.ScreeningVisit.ScreeningEntry.ProjectId == filters.SiteId
               && (filters.SubjectIds == null || filters.SubjectIds.Contains(x.ScreeningVisit.ScreeningEntry.Id))
               && (filters.VisitIds == null || filters.VisitIds.Contains(x.ProjectDesignTemplate.ProjectDesignVisitId))
               && (filters.TemplateIds == null || filters.TemplateIds.Contains(x.ProjectDesignTemplateId))
               && (x.ScreeningVisit.Status > ScreeningVisitStatus.NotStarted && x.ScreeningVisit.Status <= ScreeningVisitStatus.InProgress)
               && x.Status == ScreeningTemplateStatus.Pending 
               && !x.IsNA)
                  .Select(x => new NAReportDto
                  {
                      ScreeningTemplateId = x.Id,
                      Visit = x.ScreeningVisit.ScreeningVisitName,
                      VisitStatus = x.ScreeningVisit.Status.GetDescription(),
                      FormName = x.ScreeningTemplateName,
                      FormStatus = x.Status.GetDescription(),
                      ScreeningNo = x.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber,
                      RandomizationNumber = x.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber,
                      Initial = x.ScreeningVisit.ScreeningEntry.Randomization.Initial
                  }).ToList();

        }

        public IList<DropDownDto> GetVisitDropDownForApplicableByProjectId(int ProjectId)
        {
            var visitsByTemplate = All.Where(x => x.DeletedDate == null
            && x.ScreeningVisit.ScreeningEntry.Project.ParentProjectId == ProjectId
            && x.IsNA)
                .Select(x => x.ProjectDesignTemplate.ProjectDesignVisitId).Distinct().ToList();

            var template = _context.ScreeningVisit.Where(x => x.DeletedDate == null 
            && x.IsNA
            && x.ScreeningEntry.Project.ParentProjectId == ProjectId
            ).Select(x => x.ProjectDesignVisitId).Distinct().ToList();

            visitsByTemplate.AddRange(template);

            var visits = _context.ProjectDesignVisit.Where(x => visitsByTemplate.Contains(x.Id))
                .OrderBy(t => t.DesignOrder)
                .Select(t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.DisplayName,
                    Code = t.StudyVersion != null || t.InActiveVersion != null ?
                    "( V : " + t.StudyVersion + (t.StudyVersion != null && t.InActiveVersion != null ? " - " : "" + t.InActiveVersion) + ")" : "",
                    ExtraData = t.IsNonCRF,
                    InActive = t.InActiveVersion != null
                }).ToList();

            return visits;

            return visits;
        }

        public IList<DropDownDto> GetTemplateDropDownForApplicable(int projectDesignVisitId)
        {
            var template = All.Where(x => x.IsNA && x.ProjectDesignTemplate.ProjectDesignVisitId == projectDesignVisitId && x.DeletedDate == null)
                .OrderBy(t => t.ProjectDesignTemplate.DesignOrder)
                .Select(t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.ScreeningTemplateName
                }).ToList();

            return template;
        }

        public IList<DropDownDto> GetSubjectDropDownForApplicable(int ProjectId)
        {
            var visitsByTemplate = All.Where(x => x.DeletedDate == null
            && x.ScreeningVisit.ScreeningEntry.Project.ParentProjectId == ProjectId
            && x.IsNA)
                .Select(x => x.ScreeningVisit.ScreeningEntry.Id).Distinct().ToList();

            var template = _context.ScreeningVisit.Where(x => x.DeletedDate == null && x.ScreeningEntry.Project.ParentProjectId == ProjectId && x.IsNA)
                .Select(x => x.ScreeningEntry.Id).Distinct().ToList();

            visitsByTemplate.AddRange(template);

            var subject = _context.ScreeningEntry.Where(x => visitsByTemplate.Contains(x.Id))
                .OrderBy(t => t.Id)
                .Select(t => new DropDownDto
                {
                    Id = t.Id,
                    Value = Convert.ToString(t.Randomization.ScreeningNumber + " - " +
                                           t.Randomization.Initial +
                                           (t.Randomization.RandomizationNumber == null
                                               ? ""
                                               : " - " + t.Randomization.RandomizationNumber))
                }).ToList();

            return subject;
        }

        public List<NAReportDto> AReport(NAReportSearchDto filters)
        {
            return All.Include(x => x.ScreeningVisit)
                  .ThenInclude(x => x.ScreeningEntry)
                  .ThenInclude(x => x.Randomization)
                  .Where(x => x.ScreeningVisit.ScreeningEntry.ProjectId == filters.SiteId
               && (filters.SubjectIds == null || filters.SubjectIds.Contains(x.ScreeningVisit.ScreeningEntry.Id))
               && (filters.VisitIds == null || filters.VisitIds.Contains(x.ProjectDesignTemplate.ProjectDesignVisitId))
               && (filters.TemplateIds == null || filters.TemplateIds.Contains(x.Id))
               && x.IsNA)
                  .Select(x => new NAReportDto
                  {
                      ScreeningTemplateId = x.Id,
                      Visit = x.ScreeningVisit.ScreeningVisitName,
                      VisitStatus = x.ScreeningVisit.Status.GetDescription(),
                      FormName = x.ScreeningTemplateName,
                      FormStatus = x.Status.GetDescription(),
                      ScreeningNo = x.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber,
                      RandomizationNumber = x.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber,
                      Initial = x.ScreeningVisit.ScreeningEntry.Randomization.Initial
                  }).ToList();

        }

    }
}

