using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.EditCheck;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Medra;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.Base;

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
            IEditCheckRuleRepository editCheckRuleRepository) : base(context)
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
        }

        public List<EditCheckValidateDto> CheckValidation(List<Data.Dto.Screening.ScreeningTemplateValueBasic> values, ScreeningTemplateBasic screeningTemplateBasic, bool isQuery)
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

                if (r.IsSameTemplate && r.ProjectDesignTemplateId == screeningTemplateBasic.ProjectDesignTemplateId)
                {
                    r.ScreeningTemplateId = screeningTemplateBasic.Id;
                    var screeningValue = values.FirstOrDefault(c => c.ProjectDesignVariableId == r.ProjectDesignVariableId);
                    r.ScreeningTemplateValue = screeningValue?.Value;

                    if (screeningValue != null && screeningValue.Children != null && screeningValue.Children.Count > 0)
                        r.ScreeningTemplateValue = string.Join(",", screeningValue.Children.Where(x => x.Value == "true").Select(t => t.ProjectDesignVariableValueId));
                }
                else if (r.ProjectDesignTemplateId != null && !r.IsSkip)
                {
                    var refTemplate = _impactService.GetScreeningTemplate((int)r.ProjectDesignTemplateId, screeningTemplateBasic.ScreeningEntryId, screeningTemplateBasic.ScreeningVisitId);
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
                            r.ScreeningTemplateValue = _impactService.GetVariableValue(r);
                        }
                    }
                }

                if (r.CheckBy == EditCheckRuleBy.ByVariableAnnotation && !r.IsSkip)
                {
                    r.CollectionValue = _impactService.CollectionValueAnnotation(r.CollectionValue, r.CollectionSource);
                    r.ScreeningTemplateValue = _impactService.ScreeningValueAnnotation(r.ScreeningTemplateValue, r.CheckBy, r.CollectionSource);
                }
            });

            return TargetValidateProcess(result);
        }


        public List<EditCheckTargetValidationList> VariableValidateProcess(int screeningEntryId, int screeningTemplateId, string value, int projectDesignTemplateId, int projectDesignVariableId, List<EditCheckIds> editCheckIds, bool isQueryRaise, int screeningVisitId)
        {
            if (editCheckIds == null || editCheckIds.Count == 0)
                return new List<EditCheckTargetValidationList>();

            _context.DetachAllEntities();

            var result = _impactService.GetEditCheckByVaiableId(projectDesignTemplateId, projectDesignVariableId, editCheckIds);

            if (!isQueryRaise)
            {
                var Ids = result.Where(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.IsTarget).Select(t => t.EditCheckId).ToList();
                result = result.Where(t => Ids.Contains(t.EditCheckId)).ToList();
            }

            var editTargetValidation = new List<EditCheckTargetValidationList>();
            VariableProcess(result.Where(x => !x.IsOnlyTarget).ToList(), screeningEntryId, screeningTemplateId, value, projectDesignTemplateId, projectDesignVariableId, editTargetValidation, isQueryRaise, screeningVisitId);
            VariableProcess(result.Where(x => x.IsOnlyTarget).ToList(), screeningEntryId, screeningTemplateId, value, projectDesignTemplateId, projectDesignVariableId, editTargetValidation, isQueryRaise, screeningVisitId);

            return editTargetValidation;
        }

        private void VariableProcess(List<EditCheckValidateDto> result, int screeningEntryId, int screeningTemplateId, string value, int projectDesignTemplateId, int projectDesignVariableId, List<EditCheckTargetValidationList> editTargetValidation, bool isQueryRaise, int screeningVisitId)
        {
            result.ForEach(r =>
            {
                r.ScreeningEntryId = screeningEntryId;
                if (r.IsSameTemplate && r.ProjectDesignTemplateId == projectDesignTemplateId && r.ProjectDesignVariableId == projectDesignVariableId)
                {
                    r.ScreeningTemplateValue = value;
                    r.ScreeningTemplateId = screeningTemplateId;
                }
                else if (r.IsSameTemplate)
                {
                    r.ScreeningTemplateId = screeningTemplateId;
                    r.ScreeningTemplateValue = _impactService.GetVariableValue(r);
                }
                else if (r.ProjectDesignTemplateId != null)
                {
                    var refTemplate = _impactService.GetScreeningTemplate((int)r.ProjectDesignTemplateId, screeningEntryId, screeningVisitId);
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
                            r.ScreeningTemplateValue = _impactService.GetVariableValue(r);
                        }
                    }
                }
                if (r.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
                {
                    r.CollectionValue = _impactService.CollectionValueAnnotation(r.CollectionValue, r.CollectionSource);
                    r.ScreeningTemplateValue = _impactService.ScreeningValueAnnotation(r.ScreeningTemplateValue, r.CheckBy, r.CollectionSource);
                }
            });

            var targetResult = TargetValidateProcess(result).Where(r => r.IsTarget).ToList();
            targetResult.Where(r => r.ValidateType != EditCheckValidateType.RuleValidated && (r.CheckBy == EditCheckRuleBy.ByTemplate || r.CheckBy == EditCheckRuleBy.ByTemplateAnnotation)).ToList().ForEach(r =>
            {
                if (r.CheckBy == EditCheckRuleBy.ByTemplate)
                    UpdateEnableTemplate((int)r.ProjectDesignTemplateId, 0, screeningEntryId, r, editTargetValidation);
                else
                    UpdateEnableTemplate(0, (int)r.DomainId, screeningEntryId, r, editTargetValidation);
            });
            _context.Save();

            var variableResult = UpdateVariale(targetResult.Where(r => r.CheckBy == EditCheckRuleBy.ByVariable || r.CheckBy == EditCheckRuleBy.ByVariableAnnotation).ToList(), true, isQueryRaise);

            if (variableResult != null)
                editTargetValidation.AddRange(variableResult);

            _context.Save();
        }

        public List<EditCheckTargetValidationList> UpdateVariale(List<EditCheckValidateDto> editCheckValidateDto, bool isVariable, bool isQueryRaise)
        {
            if (_editCheckTargetValidationLists == null)
                _editCheckTargetValidationLists = new List<EditCheckTargetValidationList>();
            var variableIds = editCheckValidateDto.Select(c => c.ProjectDesignVariableId).Distinct().ToList();
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
                        editCheckTarget.EditCheckDisable = r.ValidateType != EditCheckValidateType.RuleValidated;
                        editCheckTarget.OriginalValidationType = editCheckTarget.EditCheckDisable ? ValidationType.None : ValidationType.Required;
                        if (editCheckTarget.EditCheckDisable)
                        {
                            editCheckTarget.Value = "";
                            editCheckTarget.IsValueSet = true;
                            editCheckTarget.Note = note;
                        }
                        if (string.IsNullOrEmpty(r.ScreeningTemplateValue) && r.ValidateType == EditCheckValidateType.RuleValidated)
                        {
                            if (isQueryRaise) editCheckTarget.HasQueries = true;
                            r.ValidateType = EditCheckValidateType.Failed;
                        }
                        editCheckTarget.isInfo = r.ValidateType != EditCheckValidateType.Failed;
                    }
                    else if (r.IsFormula || r.Operator == Operator.HardFetch)
                    {
                        editCheckTarget.Value = r.ScreeningTemplateValue;
                        editCheckTarget.IsValueSet = true;
                        editCheckTarget.Note = note;
                        editCheckTarget.isInfo = true;
                        editCheckTarget.OriginalValidationType = ValidationType.None;
                        editCheckTarget.EditCheckDisable = true;
                    }
                    else if (r.Operator == Operator.NotNull && r.IsOnlyTarget)
                    {
                        editCheckTarget.OriginalValidationType = ValidationType.Required;
                    }
                    else if (r.Operator == Operator.SoftFetch)
                    {
                        editCheckTarget.isInfo = true;
                        editCheckTarget.Value = r.ScreeningTemplateValue;
                        editCheckTarget.IsSoftFetch = true;
                        editCheckTarget.Note = note;
                    }
                    else
                    {
                        if (isQueryRaise) editCheckTarget.HasQueries = r.ValidateType == EditCheckValidateType.Failed;
                        editCheckTarget.isInfo = r.ValidateType != EditCheckValidateType.Failed;
                    }

                    if (r.Operator == Operator.Required && r.ValidateType == EditCheckValidateType.ReferenceVerifed)
                        editCheckTarget.OriginalValidationType = ValidationType.Hard;

                    if ((r.Operator == Operator.Optional || r.Operator == Operator.SoftFetch) && r.ValidateType == EditCheckValidateType.ReferenceVerifed)
                        editCheckTarget.OriginalValidationType = ValidationType.Soft;

                    var editCheckMessage = new EditCheckMessage();
                    editCheckMessage.AutoNumber = r.AutoNumber;
                    editCheckMessage.Message = r.Message;
                    editCheckMessage.isInfo = editCheckTarget.isInfo;
                    editCheckMessage.ValidateType = r.ValidateType.GetDescription();
                    editCheckMessage.SampleResult = r.SampleResult;
                    editCheckTarget.EditCheckMsg.Add(editCheckMessage);

                    if (isVariable && (editCheckTarget.IsValueSet || editCheckTarget.IsSoftFetch))
                    {
                        editCheckTarget.ScreeningTemplateValueId = InsertScreeningValue(r.ScreeningTemplateId,
                            r.ProjectDesignVariableId, editCheckTarget.Value, note,
                            editCheckTarget.IsSoftFetch, r.CollectionSource, editCheckTarget.EditCheckDisable);
                    }

                    if (editCheckTarget.HasQueries)
                    {
                        editCheckTarget.HasQueries = SystemQuery(r.ScreeningTemplateId,
                            r.ProjectDesignVariableId, r.AutoNumber, r.Message, r.SampleResult);
                        editCheckMessage.HasQueries = editCheckTarget.HasQueries;
                    }

                });
                editCheckTarget.isInfo = !editCheckTarget.EditCheckMsg.Any(r => !r.isInfo);

                if (editCheckTarget.EditCheckMsg != null)
                    editCheckTarget.HasQueries = editCheckTarget.EditCheckMsg.Any(x => x.HasQueries);

                _editCheckTargetValidationLists.Add(editCheckTarget);
            });

            return _editCheckTargetValidationLists;
        }

        private void UpdateEnableTemplate(int projectDesignTemplateId, int domainId, int screeningEntryId, EditCheckValidateDto editCheckValidateDto, List<EditCheckTargetValidationList> editCheckTarget)
        {
            List<ScreeningTemplate> screeningTemplates = null;
            if (projectDesignTemplateId > 0)
                screeningTemplates = All.AsNoTracking().
                    Where(r => r.ProjectDesignTemplateId == projectDesignTemplateId &&
                    r.ScreeningVisit.ScreeningEntryId == screeningEntryId
                    && r.Status > ScreeningTemplateStatus.Pending
                    && !r.IsDisable).ToList();
            else
                screeningTemplates = All.AsNoTracking().
                    Where(r => r.ProjectDesignTemplate.DomainId == domainId
                    && r.ScreeningVisit.ScreeningEntryId == screeningEntryId
                    && r.Status > ScreeningTemplateStatus.Pending
                    && !r.IsDisable
                    ).ToList();

            screeningTemplates.ForEach(r =>
            {
                var isTemplateQuery = false;

                var screeningTemplateReview = _screeningTemplateReviewRepository.All.AsNoTracking()
               .Where(x => x.ScreeningTemplateId == r.Id).ToList();
                screeningTemplateReview.ForEach(c =>
                {
                    c.IsRepeat = true;
                    _screeningTemplateReviewRepository.Update(c);
                });

                if (r.Status == ScreeningTemplateStatus.Submitted || r.Status == ScreeningTemplateStatus.InProcess)
                    TemplateValueAduit(r.Id, editCheckValidateDto);
                else
                    isTemplateQuery = TemplateQuery(r.Id, editCheckValidateDto);

                if (r.Status == ScreeningTemplateStatus.InProcess || r.Status == ScreeningTemplateStatus.Submitted)
                {
                    r.ReviewLevel = null;
                    r.Progress = 0;
                    r.Status = ScreeningTemplateStatus.Pending;
                }
                else
                {
                    r.ReviewLevel = 1;
                    r.Status = ScreeningTemplateStatus.Submitted;
                }
                editCheckTarget.Add(new EditCheckTargetValidationList
                {
                    ScreeningTemplateId = r.Id,
                    Status = r.Status
                });
                r.IsDisable = true;

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
                        ReasonId = null,
                        UserId = _jwtTokenAccesser.UserId,
                        UserRoleId = _jwtTokenAccesser.RoleId,
                        ScreeningTemplateValueId = x.Id,
                        Note = $"{editCheckValidateDto.AutoNumber} {editCheckValidateDto.Message}"
                    };

                    _screeningTemplateValueAuditRepository.Add(screeningTemplateValueAudit);
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
                    InputValue = x.ScreeningTemplateValue,
                    IsReferenceValue = x.IsReferenceValue,
                    Operator = x.Operator,
                    IsFormula = x.IsFormula,
                    IsTarget = x.IsTarget
                }).ToList();

                var targetResult = _editCheckRuleRepository.ValidateEditCheck(editCheckValidates);

                if (targetResult != null)
                {
                    result.Where(c => c.EditCheckId == r && c.IsTarget).ToList().ForEach(
                    t =>
                    {

                        t.ValidateType = targetResult.IsValid ? EditCheckValidateType.ReferenceVerifed : EditCheckValidateType.NotProcessed;
                        if (targetResult.IsValid && targetResult.Target != null)
                        {
                            var singleTarget = targetResult.Target.FirstOrDefault(a => a.Id == t.EditCheckDetailId);
                            if (singleTarget != null)
                            {
                                if (t.Operator != Operator.Enable)
                                    t.ScreeningTemplateValue = singleTarget.Result;

                                t.SampleResult = singleTarget.ResultMessage ?? singleTarget.SampleText;
                                t.ValidateType = singleTarget.IsValid ? EditCheckValidateType.RuleValidated : EditCheckValidateType.Failed; t.ValidateType = singleTarget.IsValid ? EditCheckValidateType.RuleValidated : EditCheckValidateType.Failed;
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

            var aduits = new List<ScreeningTemplateValueAudit>
            {
                new ScreeningTemplateValueAudit
                {
                    Value = valueName,
                    OldValue = oldValueName,
                    Note = note
                }
            };
            if (screeningTemplateValue == null)
            {
                screeningTemplateValue = new ScreeningTemplateValue
                {
                    ScreeningTemplateId = screeningTemplateId,
                    ProjectDesignVariableId = projectDesignVariableId,
                    Value = value,
                    Audits = aduits
                };
                _screeningTemplateValueRepository.Add(screeningTemplateValue);
            }
            else
            {
                screeningTemplateValue.Audits = aduits;
                screeningTemplateValue.Value = value;

                if (isDisable && string.IsNullOrEmpty(value))
                    screeningTemplateValue.IsNa = false;

                _screeningTemplateValueRepository.Update(screeningTemplateValue);
            }
            _context.Save();
            _context.DetachAllEntities();

            return screeningTemplateValue.Id;
        }

        bool SystemQuery(int screeningTemplateId, int projectDesignVariableId, string autoNumber, string message, string sampleResult)
        {
            var screeningTemplateValue = _screeningTemplateValueRepository.All.AsNoTracking().Where
            (t => t.ScreeningTemplateId == screeningTemplateId
                  && t.ProjectDesignVariableId == projectDesignVariableId).FirstOrDefault();

            if (screeningTemplateValue != null)
            {
                if (screeningTemplateValue.IsSystem)
                    return false;

                var screeningTemplate = All.AsNoTracking().Where(x => x.Id == screeningTemplateId).FirstOrDefault();
                if ((int)screeningTemplate.Status < 3)
                    return false;

                var screeningTemplateValueQuery = _screeningTemplateValueQueryRepository.All.Where(x => x.ScreeningTemplateValueId == screeningTemplateValue.Id).
                    Select(t => new { t.IsSystem, t.EditCheckRefValue }).FirstOrDefault();

                if (screeningTemplateValueQuery != null && screeningTemplateValueQuery.IsSystem)
                {
                    if (screeningTemplateValueQuery.EditCheckRefValue == sampleResult) return false;
                }

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
                        EditCheckRefValue = sampleResult,
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
