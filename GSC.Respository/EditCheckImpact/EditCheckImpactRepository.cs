using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.EditCheck;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Medra;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.EditCheckImpact
{
    public class EditCheckImpactRepository : GenericRespository<ScreeningTemplate>, IEditCheckImpactRepository
    {
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        private readonly IImpactService _impactService;
        private readonly IEditCheckRuleRepository _editCheckRuleRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private List<EditCheckTargetValidationList> _editCheckTargetValidationLists;
        private readonly IMeddraCodingRepository _meddraCodingRepository;
        private readonly IScreeningTemplateValueChildRepository _screeningTemplateValueChildRepository;
        private readonly IScreeningTemplateReviewRepository _screeningTemplateReviewRepository;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateValueAuditRepository _screeningTemplateValueAuditRepository;
        private readonly IProjectDesignVariableValueRepository _projectDesignVariableValueRepository;
        private readonly IScreeningTemplateLockUnlockRepository _screeningTemplateLockUnlockRepository;
        private readonly IScreeningTemplateEditCheckValueRepository _screeningTemplateEditCheckValueRepository;
        public EditCheckImpactRepository(IImpactService editCheckImpactService,
            IMapper mapper, IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateLockUnlockRepository screeningTemplateLockUnlockRepository,
            IProjectDesignVariableValueRepository projectDesignVariableValueRepository,
            IScreeningTemplateValueAuditRepository screeningTemplateValueAuditRepository,
            IMeddraCodingRepository meddraCodingRepository,
            IScreeningTemplateValueChildRepository screeningTemplateValueChildRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IScreeningTemplateReviewRepository screeningTemplateReviewRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IEditCheckRuleRepository editCheckRuleRepository,
            IScreeningTemplateEditCheckValueRepository screeningTemplateEditCheckValueRepository) : base(context)
        {
            _impactService = editCheckImpactService;
            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateLockUnlockRepository = screeningTemplateLockUnlockRepository;
            _screeningTemplateValueChildRepository = screeningTemplateValueChildRepository;
            _screeningTemplateReviewRepository = screeningTemplateReviewRepository;
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _editCheckRuleRepository = editCheckRuleRepository;
            _projectDesignVariableValueRepository = projectDesignVariableValueRepository;
            _meddraCodingRepository = meddraCodingRepository;
            _screeningTemplateValueAuditRepository = screeningTemplateValueAuditRepository;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _screeningTemplateEditCheckValueRepository = screeningTemplateEditCheckValueRepository;
        }

        public List<EditCheckValidateDto> CheckValidation(DesignScreeningTemplateDto projectDesignTemplateDto, List<Data.Dto.Screening.ScreeningTemplateValueBasic> values, ScreeningTemplateBasic screeningTemplateBasic, bool isQuery)
        {

            var result = _impactService.GetEditCheck(screeningTemplateBasic);

            if (!isQuery)
            {
                var editCheckIds = result.Where(x => x.ProjectDesignTemplateId == screeningTemplateBasic.ProjectDesignTemplateId && x.IsTarget).Select(t => t.EditCheckId).ToList();
                result.Where(t => !editCheckIds.Contains(t.EditCheckId)).ToList().ForEach(b => b.IsSkip = true);
            }

            result.ForEach(r =>
            {
                r.ScreeningEntryId = screeningTemplateBasic.ScreeningEntryId;

                if ((r.IsSameTemplate || r.IsTarget) && r.ProjectDesignTemplateId == screeningTemplateBasic.ProjectDesignTemplateId)
                {
                    r.ScreeningTemplateId = screeningTemplateBasic.Id;
                    var screeningValue = values.FirstOrDefault(c => c.ProjectDesignVariableId == r.ProjectDesignVariableId);
                    if (screeningValue != null)
                    {
                        r.ScreeningTemplateValue = screeningValue?.Value;
                        r.IsNa = screeningValue.IsNa;
                    }
                    if (screeningValue != null && screeningValue.Children != null && screeningValue.Children.Count > 0)
                        r.ScreeningTemplateValue = string.Join(",", screeningValue.Children.Where(x => x.Value == "true").Select(t => t.ProjectDesignVariableValueId));
                }
                else if (r.ProjectDesignTemplateId != null && !r.IsSkip)
                {
                    var refTemplate = _impactService.GetScreeningTemplate((int)r.ProjectDesignTemplateId, screeningTemplateBasic.ScreeningEntryId,
                       r.ProjectDesignVisitId == screeningTemplateBasic.ProjectDesignVisitId ? screeningTemplateBasic.ScreeningVisitId : (int?)null);
                    if (refTemplate != null)
                    {
                        r.ScreeningTemplateId = refTemplate.Id;
                        var statusId = (int)refTemplate.Status;
                        if ((statusId > 2 &&
                         (r.CheckBy == EditCheckRuleBy.ByTemplate ||
                         r.CheckBy == EditCheckRuleBy.ByTemplateAnnotation)) ||
                         r.CheckBy == EditCheckRuleBy.ByVariable ||
                         r.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
                        {
                            r.ScreeningTemplateValue = _impactService.GetVariableValue(r, out bool isNa);
                            r.IsNa = isNa;
                        }
                    }
                }

                if (r.CheckBy == EditCheckRuleBy.ByVariableAnnotation && !r.IsSkip)
                {
                    r.CollectionValue = _impactService.CollectionValueAnnotation(r.CollectionValue, r.CollectionSource);
                    r.ScreeningTemplateValue = _impactService.ScreeningValueAnnotation(r.ScreeningTemplateValue, r.CheckBy, r.CollectionSource);
                }

                if (r.CollectionSource == CollectionSources.NumericScale && !r.IsSkip && !string.IsNullOrEmpty(r.ScreeningTemplateValue))
                {
                    if (projectDesignTemplateDto == null)
                        r.NumberScale = _impactService.CollectionValue(r.ScreeningTemplateValue);
                    else
                    {
                        var projectVariable = projectDesignTemplateDto.Variables.Where(x => x.ProjectDesignVariableId == r.ProjectDesignVariableId).FirstOrDefault();
                        if (projectVariable != null)
                        {
                            int projectVariableValueId;
                            int.TryParse(r.ScreeningTemplateValue, out projectVariableValueId);
                            var valueName = projectVariable.Values.Where(x => x.Id == projectVariableValueId).Select(t => t.ValueName).FirstOrDefault();
                            int.TryParse(valueName, out projectVariableValueId);
                            r.NumberScale = projectVariableValueId;

                            if (projectVariableValueId == 0)
                                r.NumberScale = _impactService.CollectionValue(r.ScreeningTemplateValue);
                        }
                    }
                }

            });

            return TargetValidateProcess(result);
        }


        public List<EditCheckTargetValidationList> VariableValidateProcess(int screeningEntryId, int screeningTemplateId, string value, int projectDesignTemplateId, int projectDesignVariableId, List<EditCheckIds> editCheckIds, bool isQueryRaise, int screeningVisitId, int? projectDesignVisitId, bool isNa)
        {
            if (editCheckIds == null || editCheckIds.Count == 0)
                return new List<EditCheckTargetValidationList>();

            _context.DetachAllEntities();

            var result = _impactService.GetEditCheckByVaiableId(projectDesignTemplateId, projectDesignVariableId, editCheckIds);

            if (!isQueryRaise)
            {
                var Ids = result.Where(x => (x.CheckBy == EditCheckRuleBy.ByTemplate || x.CheckBy == EditCheckRuleBy.ByTemplateAnnotation || x.ProjectDesignTemplateId == projectDesignTemplateId) && x.IsTarget).Select(t => t.EditCheckId).Distinct().ToList();
                result = result.Where(t => Ids.Contains(t.EditCheckId)).ToList();
            }

            var editTargetValidation = new List<EditCheckTargetValidationList>();
            VariableProcess(result.Where(x => !x.IsOnlyTarget).ToList(), screeningEntryId, screeningTemplateId, value, projectDesignTemplateId, projectDesignVariableId, editTargetValidation, isQueryRaise, screeningVisitId, projectDesignVisitId, isNa, false);
            VariableProcess(result.Where(x => x.IsOnlyTarget).ToList(), screeningEntryId, screeningTemplateId, value, projectDesignTemplateId, projectDesignVariableId, editTargetValidation, isQueryRaise, screeningVisitId, projectDesignVisitId, isNa, false);

            if (isQueryRaise)
            {
                var targetScreeningTemplates = result.Where(x => x.IsTarget).Select(x => x.ScreeningTemplateId).Distinct().ToList();
                var repeatTemplateIds = All.Where(x => x.Status > ScreeningTemplateStatus.InProcess
                && targetScreeningTemplates.Contains((int)x.ParentId)).Select(t => new
                {
                    t.Id,
                    t.ProjectDesignTemplateId,
                    t.ScreeningVisitId
                }).ToList();
                repeatTemplateIds.ForEach(t =>
                {
                    VariableProcess(result.Where(x => !x.IsOnlyTarget).ToList(), screeningEntryId, t.Id, value, projectDesignTemplateId, projectDesignVariableId, new List<EditCheckTargetValidationList>(), isQueryRaise, screeningVisitId, projectDesignVisitId, isNa, true);
                });

                var screeningVisitIds = repeatTemplateIds.Select(t => t.ScreeningVisitId).Distinct().ToList();
                var projectDesignTemplateIds = repeatTemplateIds.Select(t => t.ProjectDesignTemplateId).Distinct().ToList();

                var repeatVisitIds = All.Where(x => x.Status > ScreeningTemplateStatus.InProcess &&
                   screeningVisitIds.Contains((int)x.ScreeningVisit.ParentId) && projectDesignTemplateIds.Contains(x.ProjectDesignTemplateId)).Select(t => new
                   {
                       t.Id,
                       t.ScreeningVisitId
                   }).ToList();
                repeatVisitIds.ForEach(t =>
                {
                    VariableProcess(result.Where(x => !x.IsOnlyTarget).ToList(), screeningEntryId, t.Id, value, projectDesignTemplateId, projectDesignVariableId, new List<EditCheckTargetValidationList>(), isQueryRaise, t.ScreeningVisitId, projectDesignVisitId, isNa, true);
                });
            }

            return editTargetValidation;
        }

        private void VariableProcess(List<EditCheckValidateDto> result, int screeningEntryId, int screeningTemplateId, string value, int projectDesignTemplateId, int projectDesignVariableId, List<EditCheckTargetValidationList> editTargetValidation, bool isQueryRaise, int screeningVisitId, int? projectDesignVisitId, bool isNa, bool isRepated)
        {
            result.ForEach(r =>
            {
                r.ScreeningEntryId = screeningEntryId;
                if ((r.IsSameTemplate || r.IsTarget) && !isRepated && r.ProjectDesignTemplateId == projectDesignTemplateId && r.ProjectDesignVariableId == projectDesignVariableId)
                {
                    r.ScreeningTemplateValue = value;
                    r.IsNa = isNa;
                    r.ScreeningTemplateId = screeningTemplateId;
                }
                else if (r.IsSameTemplate || (isRepated && r.IsTarget))
                {
                    if (!r.ValueApply)
                    {
                        r.ScreeningTemplateId = screeningTemplateId;
                        r.ScreeningTemplateValue = _impactService.GetVariableValue(r, out bool isNa);
                        r.IsNa = isNa;
                        result.Where(t => t.ProjectDesignVariableId == r.ProjectDesignVariableId && t.ProjectDesignTemplateId == r.ProjectDesignTemplateId).
                        ToList().ForEach(t =>
                        {
                            t.ScreeningTemplateId = screeningTemplateId;
                            t.ScreeningTemplateValue = r.ScreeningTemplateValue;
                            t.IsNa = r.IsNa;
                            t.ValueApply = true;
                        });
                    }
                }
                else if (r.ProjectDesignTemplateId != null && !isRepated)
                {
                    var refTemplate = _impactService.GetScreeningTemplate((int)r.ProjectDesignTemplateId, screeningEntryId, projectDesignVisitId == r.ProjectDesignVisitId ? screeningVisitId : (int?)null);
                    if (refTemplate != null)
                    {
                        r.ScreeningTemplateId = refTemplate.Id;
                        var statusId = (int)refTemplate.Status;
                        if ((statusId > 2 &&
                        (r.CheckBy == EditCheckRuleBy.ByTemplate ||
                        r.CheckBy == EditCheckRuleBy.ByTemplateAnnotation)) ||
                        r.CheckBy == EditCheckRuleBy.ByVariable ||
                        r.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
                        {
                            r.ScreeningTemplateValue = _impactService.GetVariableValue(r, out bool isNa);
                            r.IsNa = isNa;
                        }
                    }
                }
                if (r.CheckBy == EditCheckRuleBy.ByVariableAnnotation && !isRepated)
                {
                    r.CollectionValue = _impactService.CollectionValueAnnotation(r.CollectionValue, r.CollectionSource);
                    r.ScreeningTemplateValue = _impactService.ScreeningValueAnnotation(r.ScreeningTemplateValue, r.CheckBy, r.CollectionSource);
                }

                if (r.IsFormula && r.CollectionSource == CollectionSources.NumericScale && !string.IsNullOrEmpty(r.ScreeningTemplateValue) && r.NumberScale == 0)
                    r.NumberScale = _impactService.CollectionValue(r.ScreeningTemplateValue);
            });

            var targetResult = TargetValidateProcess(result).Where(r => r.IsTarget && r.ScreeningTemplateId > 0).ToList();
            targetResult.Where(r => r.Operator == Operator.Enable && (r.CheckBy == EditCheckRuleBy.ByTemplate || r.CheckBy == EditCheckRuleBy.ByTemplateAnnotation)).ToList().ForEach(r =>
              {
                  if (r.ValidateType == EditCheckValidateType.Passed)
                      UpdateTemplateDisable(r.CheckBy == EditCheckRuleBy.ByTemplate ? (int)r.ProjectDesignTemplateId : 0, r.CheckBy == EditCheckRuleBy.ByTemplate ? 0 : (int)r.DomainId, screeningVisitId);
                  else
                      UpdateEnableTemplate(r.CheckBy == EditCheckRuleBy.ByTemplate ? (int)r.ProjectDesignTemplateId : 0, r.CheckBy == EditCheckRuleBy.ByTemplate ? 0 : (int)r.DomainId, screeningVisitId, r, editTargetValidation, isQueryRaise);

              });
            _context.Save();

            var variableResult = UpdateVariale(targetResult.Where(r => r.ScreeningTemplateId > 0 && (r.CheckBy == EditCheckRuleBy.ByVariable || r.CheckBy == EditCheckRuleBy.ByVariableAnnotation)).ToList(), true, isQueryRaise);

            if (variableResult != null)
                editTargetValidation.AddRange(variableResult);

            _context.Save();
        }

        public List<EditCheckTargetValidationList> UpdateVariale(List<EditCheckValidateDto> editCheckValidateDto, bool isVariable, bool isQueryRaise)
        {
            if (_editCheckTargetValidationLists == null)
                _editCheckTargetValidationLists = new List<EditCheckTargetValidationList>();
            var variableIds = editCheckValidateDto.Where(a => a.ScreeningTemplateId > 0).Select(c => c.ProjectDesignVariableId).Distinct().ToList();
            variableIds.ForEach(t =>
            {
                EditCheckTargetValidationList editCheckTarget = _editCheckTargetValidationLists.
                FirstOrDefault(o => o.ProjectDesignVariableId == t) ?? new EditCheckTargetValidationList();

                editCheckTarget.ProjectDesignVariableId = t;
                editCheckValidateDto.Where(c => c.ProjectDesignVariableId == t).
                ToList().ForEach(r =>
                {
                    string note = $"{r.AutoNumber} {r.Message}";

                    if (r.Operator == Operator.Enable)
                    {
                        editCheckTarget.EditCheckDisable = r.ValidateType != EditCheckValidateType.Passed;
                        editCheckTarget.OriginalValidationType = editCheckTarget.EditCheckDisable ? ValidationType.None : ValidationType.Required;
                        if (editCheckTarget.EditCheckDisable)
                        {
                            editCheckTarget.Value = "";
                            if (!string.IsNullOrEmpty(r.ScreeningTemplateValue)) editCheckTarget.IsValueSet = true;
                            editCheckTarget.Note = note;
                        }
                        if (string.IsNullOrEmpty(r.ScreeningTemplateValue) && r.ValidateType == EditCheckValidateType.Passed)
                        {
                            if (isQueryRaise) editCheckTarget.HasQueries = true;
                            r.ValidateType = EditCheckValidateType.Failed;
                        }
                        editCheckTarget.InfoType = r.ValidateType == EditCheckValidateType.Failed ? EditCheckInfoType.Failed : EditCheckInfoType.Info;
                    }
                    else if (r.IsFormula || r.Operator == Operator.HardFetch)
                    {
                        editCheckTarget.Value = r.ScreeningTemplateValue;
                        editCheckTarget.IsValueSet = true;
                        editCheckTarget.Note = note;
                        editCheckTarget.InfoType = r.ValidateType == EditCheckValidateType.Failed ? EditCheckInfoType.Failed : EditCheckInfoType.Info;
                        editCheckTarget.OriginalValidationType = ValidationType.None;
                        editCheckTarget.EditCheckDisable = true;
                    }
                    else if (r.Operator == Operator.NotNull && r.IsOnlyTarget)
                    {
                        editCheckTarget.OriginalValidationType = ValidationType.Required;
                    }
                    else if (r.Operator == Operator.SoftFetch)
                    {
                        editCheckTarget.InfoType = EditCheckInfoType.Info;
                        editCheckTarget.Value = r.ScreeningTemplateValue;
                        editCheckTarget.IsSoftFetch = true;
                        editCheckTarget.Note = note;
                    }
                    else
                    {
                        if (isQueryRaise) editCheckTarget.HasQueries = r.ValidateType == EditCheckValidateType.Failed;
                        editCheckTarget.InfoType = r.ValidateType == EditCheckValidateType.Failed ? EditCheckInfoType.Failed : EditCheckInfoType.Info;
                    }

                    if (r.Operator == Operator.Required && r.ValidateType == EditCheckValidateType.ReferenceVerifed)
                        editCheckTarget.OriginalValidationType = ValidationType.Hard;

                    if ((r.Operator == Operator.Optional || r.Operator == Operator.SoftFetch) && r.ValidateType == EditCheckValidateType.ReferenceVerifed)
                        editCheckTarget.OriginalValidationType = ValidationType.Soft;


                    if (r.IsNa)
                    {
                        editCheckTarget.HasQueries = false;
                        editCheckTarget.InfoType = EditCheckInfoType.Info;
                        r.ValidateType = EditCheckValidateType.NotProcessed;
                    }

                    if (r.Operator == Operator.Warning && (r.ValidateType == EditCheckValidateType.Failed || editCheckTarget.HasQueries))
                    {
                        editCheckTarget.HasQueries = false;
                        editCheckTarget.InfoType = EditCheckInfoType.Warning;
                    }

                    var editCheckMessage = new EditCheckMessage();
                    editCheckMessage.AutoNumber = r.AutoNumber;
                    editCheckMessage.Message = r.Message;
                    editCheckMessage.InfoType = editCheckTarget.InfoType;
                    editCheckMessage.ValidateType = r.ValidateType.GetDescription();

                    if (r.Operator == Operator.Warning && r.ValidateType == EditCheckValidateType.Failed)
                        editCheckMessage.ValidateType = EditCheckValidateType.Warning.GetDescription();

                    editCheckMessage.SampleResult = r.SampleResult;
                    editCheckTarget.EditCheckMsg.Add(editCheckMessage);

                    if (isVariable && (editCheckTarget.IsValueSet || editCheckTarget.IsSoftFetch))
                    {
                        editCheckTarget.ScreeningTemplateValueId = InsertScreeningValue(r.ScreeningTemplateId,
                            r.ProjectDesignVariableId, editCheckTarget.Value, note,
                            editCheckTarget.IsSoftFetch, r.CollectionSource, editCheckTarget.EditCheckDisable);
                    }

                    var isEditCheckRefValue = false;

                    if (isQueryRaise)
                        isEditCheckRefValue = _screeningTemplateEditCheckValueRepository.CheckUpdateEditCheckRefValue(r.ScreeningTemplateId,
                                r.ProjectDesignVariableId, r.EditCheckDetailId, r.ValidateType, r.SampleResult);

                    if (editCheckTarget.HasQueries && isEditCheckRefValue)
                    {
                        editCheckTarget.HasQueries = SystemQuery(r.ScreeningTemplateId, r.ProjectDesignVariableId, r.AutoNumber, r.Message);
                        editCheckMessage.HasQueries = editCheckTarget.HasQueries;
                    }

                });

                if (editCheckTarget.EditCheckMsg.Any(r => r.InfoType == EditCheckInfoType.Warning))
                    editCheckTarget.InfoType = EditCheckInfoType.Warning;

                if (editCheckTarget.EditCheckMsg.Any(r => r.InfoType == EditCheckInfoType.Failed))
                    editCheckTarget.InfoType = EditCheckInfoType.Failed;

                if (editCheckTarget.EditCheckMsg != null)
                    editCheckTarget.HasQueries = editCheckTarget.EditCheckMsg.Any(x => x.HasQueries);

                _editCheckTargetValidationLists.Add(editCheckTarget);
            });

            return _editCheckTargetValidationLists;
        }


        private List<ScreeningTemplate> GetEnableTemplate(int projectDesignTemplateId, int domainId, int screeningVisitId)
        {
            List<ScreeningTemplate> screeningTemplates = null;
            if (projectDesignTemplateId > 0)
                screeningTemplates = All.AsNoTracking().
                    Where(r => r.ProjectDesignTemplateId == projectDesignTemplateId &&
                    r.ScreeningVisitId == screeningVisitId).ToList();
            else
                screeningTemplates = All.AsNoTracking().
                    Where(r => r.ProjectDesignTemplate.DomainId == domainId
                    && r.ScreeningVisitId == screeningVisitId).ToList();

            return screeningTemplates;
        }


        private void UpdateTemplateDisable(int projectDesignTemplateId, int domainId, int screeningVisitId)
        {
            List<ScreeningTemplate> screeningTemplates = GetEnableTemplate(projectDesignTemplateId, domainId, screeningVisitId);
            screeningTemplates.ForEach(r =>
            {
                r.IsDisable = false;
                Update(r);
            });
            _context.Save();
        }

        private void UpdateEnableTemplate(int projectDesignTemplateId, int domainId, int screeningVisitId, EditCheckValidateDto editCheckValidateDto, List<EditCheckTargetValidationList> editCheckTarget, bool isQueryRaise)
        {
            List<ScreeningTemplate> screeningTemplates = GetEnableTemplate(projectDesignTemplateId, domainId, screeningVisitId);

            screeningTemplates.ForEach(r =>
            {
                r.IsDisable = true;

                bool isReviewed = _screeningTemplateReviewRepository.All.AsNoTracking().Any(x => x.ScreeningTemplateId == r.Id && x.Status == ScreeningTemplateStatus.Reviewed);
                bool isFound = false;
                var isTemplateQuery = false;

                if (isReviewed && isQueryRaise)
                {
                    if (r.Status > ScreeningTemplateStatus.Submitted)
                    {
                        var screeningTemplateReview = _screeningTemplateReviewRepository.All.AsNoTracking().Where(x => x.ScreeningTemplateId == r.Id).ToList();

                        screeningTemplateReview.ForEach(c =>
                        {
                            c.IsRepeat = true;
                            _screeningTemplateReviewRepository.Update(c);
                        });

                        r.ReviewLevel = 1;
                        r.Status = ScreeningTemplateStatus.Submitted;
                    }
                    TemplateQuery(r.Id, editCheckValidateDto);
                    isFound = true;
                }
                else if (r.Status == ScreeningTemplateStatus.InProcess || r.Status == ScreeningTemplateStatus.Submitted)
                {
                    r.ReviewLevel = null;
                    r.Progress = 0;
                    r.Status = ScreeningTemplateStatus.Pending;
                    TemplateValueAduit(r.Id, editCheckValidateDto);
                    isFound = true;
                }

                if (isFound)
                {
                    editCheckTarget.Add(new EditCheckTargetValidationList
                    {
                        ScreeningTemplateId = r.Id,
                        Status = r.Status
                    });
                }

                if (isTemplateQuery)
                    UnLockTemplate(r);

                Update(r);
                _context.Save();
            });
        }

        private bool TemplateQuery(int screeningTemplateId, EditCheckValidateDto editCheckValidateDto)
        {
            var isRaiseQuery = false;

            var screeningTemplateValue = _screeningTemplateValueRepository.All.AsNoTracking().
                Where(t => t.ScreeningTemplateId == screeningTemplateId).ToList();

            screeningTemplateValue.ForEach(x =>
            {
                isRaiseQuery = true;
                x.IsSystem = true;
                x.Value = null;
                x.IsNa = false;
                _screeningTemplateValueQueryRepository.GenerateQuery(
                    new ScreeningTemplateValueQueryDto
                    {
                        OldValue = x.IsNa ? "NA" : x.Value,
                        QueryStatus = QueryStatus.Open,
                        IsSystem = true,
                        Note =
                            $"{"Edit Check by "} {editCheckValidateDto.AutoNumber} {editCheckValidateDto.Message}",
                        ScreeningTemplateValueId = x.Id
                    },
                    new ScreeningTemplateValueQuery
                    {
                        ScreeningTemplateValue = x,
                        OldValue = x.IsNa ? "NA" : x.Value,
                        QueryStatus = QueryStatus.Open,
                        IsSystem = true,
                        Note =
                            $"{"Edit Check by "} {editCheckValidateDto.AutoNumber} {editCheckValidateDto.Message}",
                        ScreeningTemplateValueId = x.Id
                    }, x);
            });

            return isRaiseQuery;
        }

        private void TemplateValueAduit(int screeningTemplateId, EditCheckValidateDto editCheckValidateDto)
        {
            var screeningTemplateValue = _screeningTemplateValueRepository.All
                .AsNoTracking().Include(r => r.Children).ThenInclude(r => r.ProjectDesignVariableValue).
                Where(t => t.ScreeningTemplateId == screeningTemplateId).ToList();

            screeningTemplateValue.ForEach(x =>
            {
                string oldValue = "";
                if (x.Children != null && x.Children.Count > 0)
                {
                    x.Children.ForEach(a =>
                    {
                        if (!string.IsNullOrEmpty(a.Value) && a.Value == "true")
                            oldValue += a.ProjectDesignVariableValue?.ValueName;

                        if (!string.IsNullOrEmpty(oldValue))
                            oldValue += ",";

                        _screeningTemplateValueChildRepository.Remove(a);
                    });
                }
                else if (!string.IsNullOrEmpty(x.Value))
                {
                    oldValue = x.Value;
                }
                else if (x.IsNa)
                    oldValue = "NA";

                x.IsSystem = false;
                x.Value = null;
                x.IsNa = false;
                x.QueryStatus = null;

                if (!string.IsNullOrEmpty(oldValue))
                {
                    var screeningTemplateValueAudit = new ScreeningTemplateValueAudit
                    {
                        OldValue = oldValue,
                        Value = "",
                        ScreeningTemplateValueId = x.Id,
                        Note = $"{editCheckValidateDto.AutoNumber} {editCheckValidateDto.Message}"
                    };

                    _screeningTemplateValueAuditRepository.Save(screeningTemplateValueAudit);
                    _meddraCodingRepository.UpdateEditCheck(x.Id);
                }
                _screeningTemplateValueRepository.Update(x);
            });
        }


        private List<EditCheckValidateDto> TargetValidateProcess(List<EditCheckValidateDto> result)
        {
            var targetValues = result.Where(r => r.IsTarget).Select(c => c.EditCheckId).Distinct().ToList();

            targetValues.ForEach(r =>
            {
                var editCheckValidates = result.Where(c => c.EditCheckId == r).
                Select(x => new EditCheckValidate
                {
                    Id = x.EditCheckDetailId,
                    CollectionValue = x.CollectionValue,
                    CollectionValue2 = x.CollectionValue2,
                    OperatorName = x.Operator != null ? x.Operator.GetDescription() : "",
                    LogicalOperator = x.LogicalOperator,
                    EndParens = x.EndParens,
                    StartParens = x.StartParens,
                    CollectionSource = x.CollectionSource,
                    DataType = x.DataType,
                    InputValue = x.IsFormula && x.CollectionSource == CollectionSources.NumericScale ? x.NumberScale.ToString() : x.ScreeningTemplateValue,
                    IsReferenceValue = x.IsReferenceValue,
                    Operator = x.Operator,
                    IsFormula = x.IsFormula,
                    IsTarget = x.IsTarget
                }).ToList();


                var validateResult = _editCheckRuleRepository.ValidateEditCheck(editCheckValidates);

                if (validateResult != null)
                {
                    result.Where(c => c.EditCheckId == r && c.IsTarget).ToList().ForEach(
                    t =>
                    {
                        t.ValidateType = validateResult.IsValid ? EditCheckValidateType.ReferenceVerifed : EditCheckValidateType.NotProcessed;
                        if (validateResult.Target != null)
                        {
                            var singleTarget = validateResult.Target.FirstOrDefault(a => a.Id == t.EditCheckDetailId);
                            if (singleTarget != null)
                            {
                                if (t.Operator != Operator.Enable)
                                    t.ScreeningTemplateValue = singleTarget.Result;

                                t.SampleResult = singleTarget.SampleText + " " + validateResult.SampleText;

                                if (singleTarget.IsValid && validateResult.IsValid)
                                    t.ValidateType = EditCheckValidateType.Passed;
                                else if (!singleTarget.IsValid && !validateResult.IsValid)
                                    t.ValidateType = EditCheckValidateType.NotProcessed;
                                else
                                    t.ValidateType = EditCheckValidateType.Failed;

                            }

                        }
                    });
                }
            });

            return result;
        }

        public int InsertScreeningValue(int screeningTemplateId, int projectDesignVariableId, string value, string note, bool isSoftFetch, CollectionSources? collectionSource, bool isDisable)
        {
            var screeningTemplateValue = _screeningTemplateValueRepository.All.AsNoTracking().Where(x =>
                x.ProjectDesignVariableId == projectDesignVariableId &&
                x.ScreeningTemplateId == screeningTemplateId).FirstOrDefault();

            if (screeningTemplateValue != null && screeningTemplateValue.Value == value)
                return 0;
            if (isSoftFetch && screeningTemplateValue != null && !string.IsNullOrEmpty(screeningTemplateValue.Value))
                return 0;

            if (screeningTemplateValue == null && string.IsNullOrEmpty(value))
                return 0;


            var valueName = value;
            var oldValueName = screeningTemplateValue?.Value;

            if (screeningTemplateValue?.IsNa == true)
                oldValueName = "N/A";


            if (collectionSource == CollectionSources.RadioButton ||
                 collectionSource == CollectionSources.CheckBox ||
                 collectionSource == CollectionSources.MultiCheckBox ||
                 collectionSource == CollectionSources.NumericScale ||
                 collectionSource == CollectionSources.ComboBox)
            {
                decimal id;
                decimal.TryParse(value, out id);
                if (id > 0)
                    valueName = _projectDesignVariableValueRepository.All.Where(x => x.Id == id).Select(t => t.ValueName).FirstOrDefault();

                decimal.TryParse(oldValueName, out id);
                if (id > 0)
                    oldValueName = _projectDesignVariableValueRepository.All.Where(x => x.Id == id).Select(t => t.ValueName).FirstOrDefault();

            }


            if (screeningTemplateValue == null)
            {
                screeningTemplateValue = new ScreeningTemplateValue
                {
                    ScreeningTemplateId = screeningTemplateId,
                    ProjectDesignVariableId = projectDesignVariableId,
                    Value = value
                };
                _screeningTemplateValueRepository.Add(screeningTemplateValue);
            }
            else
            {
                screeningTemplateValue.Value = value;

                if (isDisable && string.IsNullOrEmpty(value))
                    screeningTemplateValue.IsNa = false;

                _screeningTemplateValueRepository.Update(screeningTemplateValue);
            }

            var aduit = new ScreeningTemplateValueAudit
            {
                ScreeningTemplateValue = screeningTemplateValue,
                ScreeningTemplateValueId = screeningTemplateValue.Id,
                Value = valueName,
                OldValue = oldValueName,
                Note = note
            };
            _screeningTemplateValueAuditRepository.Save(aduit);

            _context.Save();
            _context.DetachAllEntities();

            return screeningTemplateValue.Id;
        }

        bool SystemQuery(int screeningTemplateId, int projectDesignVariableId, string autoNumber, string message)
        {
            var screeningTemplateValue = _screeningTemplateValueRepository.All.AsNoTracking().Where
            (t => t.ScreeningTemplateId == screeningTemplateId
                  && t.ProjectDesignVariableId == projectDesignVariableId).FirstOrDefault();

            if (screeningTemplateValue != null)
            {

                if (screeningTemplateValue.IsNa)
                    return false;

                if (screeningTemplateValue.QueryStatus == QueryStatus.Acknowledge ||
                    screeningTemplateValue.QueryStatus == QueryStatus.Open ||
                    screeningTemplateValue.QueryStatus == QueryStatus.Reopened ||
                    screeningTemplateValue.QueryStatus == QueryStatus.Reopened ||
                    screeningTemplateValue.QueryStatus == QueryStatus.Resolved)
                    return false;

                var screeningTemplate = All.AsNoTracking().Where(x => x.Id == screeningTemplateId).FirstOrDefault();


                if ((int)screeningTemplate.Status < 3)
                    return false;

                string note = $"{"Reference by "} {autoNumber} {message}";
                screeningTemplateValue.IsSystem = true;
                _screeningTemplateValueQueryRepository.GenerateQuery(
                    new ScreeningTemplateValueQueryDto
                    {
                        QueryStatus = QueryStatus.Open,
                        IsSystem = true,
                        Note = note,
                        ScreeningTemplateValueId = screeningTemplateValue.Id
                    },
                    new ScreeningTemplateValueQuery
                    {
                        ScreeningTemplateValue = screeningTemplateValue,
                        QueryStatus = QueryStatus.Open,
                        IsSystem = true,
                        Note = note,
                        ScreeningTemplateValueId = screeningTemplateValue.Id
                    }, screeningTemplateValue);


                var isUnLock = UnLockTemplate(screeningTemplate);
                if (isUnLock)
                    Update(screeningTemplate);

                _context.Save();
                _context.DetachAllEntities();

                return true;
            }

            return false;
        }


        public bool UnLockTemplate(ScreeningTemplate screeningTemplate)
        {
            var isUnLock = false;
            if (screeningTemplate != null && screeningTemplate.IsLocked)
            {
                var screeningVisit = All.AsNoTracking().Where(x => x.Id == screeningTemplate.Id).Select(t => new { t.ScreeningVisit.ScreeningEntry.ProjectId, t.ScreeningVisit.ScreeningEntryId }).FirstOrDefault();
                var auditReasonId = _context.AuditReason.Where(x => x.ModuleId == AuditModule.Common && x.IsOther && x.DeletedDate == null).Select(t => t.Id).FirstOrDefault();
                screeningTemplate.IsLocked = false;
                Update(screeningTemplate);
                var lockAudit = new ScreeningTemplateLockUnlockAudit
                {
                    ScreeningTemplateId = screeningTemplate.Id,
                    ScreeningEntryId = screeningVisit.ScreeningEntryId,
                    ProjectId = screeningVisit.ProjectId,
                    AuditReasonId = auditReasonId,
                    AuditReasonComment = "Automatically unlock due to edit check"
                };
                _screeningTemplateLockUnlockRepository.Insert(lockAudit);
                isUnLock = true;
            }

            return isUnLock;
        }

    }


}
