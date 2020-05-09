using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.EditCheck;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Schedule;
using GSC.Respository.Screening;
using Microsoft.EntityFrameworkCore;
namespace GSC.Respository.Project.EditCheck
{
    public class RuleRepository : GenericRespository<EditCheckDetail, GscContext>
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        private int _projectDesignTemplateId;
        private List<int> _projectDesignVariableId = new List<int>();
        private readonly List<ScreeningTemplateValue> _queryValueList;
        private readonly ISchedulerRuleRespository _schedulerRuleRespository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private int _screeningEntryId;
        private int _screeningTemplateId;
        private readonly List<int> _screeningTemplateIdLists = new List<int>();
        private readonly IScreeningTemplateValueEditCheckRepository _screeningTemplateValueEditCheckRepository;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IMapper _mapper;
        public RuleRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper,
            IScreeningTemplateValueEditCheckRepository screeningTemplateValueEditCheckRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            ISchedulerRuleRespository schedulerRuleRespository,
            IProjectDesignVariableRepository projectDesignVariableRepository) : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateValueEditCheckRepository = screeningTemplateValueEditCheckRepository;
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _queryValueList = new List<ScreeningTemplateValue>();
            _schedulerRuleRespository = schedulerRuleRespository;
            _mapper = mapper;
            _projectDesignVariableRepository = projectDesignVariableRepository;
        }

        public void DefaultInsertVariableRule(ScreeningTemplate screeningTemplate, int projectDesignId, int domainId)
        {
            var data = All.AsNoTracking().Where(x =>
                  x.ProjectDesignTemplateId == screeningTemplate.ProjectDesignTemplateId &&
                  (x.EditCheck.ProjectDesignId == projectDesignId || x.DomainId == domainId))
                .ProjectTo<EditCheckValidateDto>(_mapper.ConfigurationProvider)
                .ToList();

            var annotationList2 = (from variable in _projectDesignVariableRepository.All.AsNoTracking().Where(t =>
                   t.ProjectDesignTemplate.Id == screeningTemplate.ProjectDesignTemplateId)
                                   join checkDetail in All on new { ann = variable.Annotation, designId = projectDesignId } equals new { ann = checkDetail.VariableAnnotation, designId = checkDetail.EditCheck.ProjectDesignId }
                                   select new EditCheckValidateDto
                                   {
                                       ProjectDesignVariableId = variable.Id,
                                       CollectionSource = variable.CollectionSource,
                                       CheckBy = checkDetail.CheckBy,
                                       DataType = variable.DataType,
                                       EditCheckDetailId = checkDetail.Id,
                                       EditCheckId = checkDetail.EditCheckId,
                                       Operator = checkDetail.Operator,
                                       ProjectDesignTemplateId = variable.ProjectDesignTemplateId,
                                       CollectionValue = checkDetail.CollectionValue,
                                       IsReferenceValue = checkDetail.IsReferenceValue,
                                       LogicalOperator = checkDetail.LogicalOperator,
                                       Message = checkDetail.Message,
                                       AutoNumber = checkDetail.EditCheck.AutoNumber,
                                       IsSameTemplate = checkDetail.IsSameTemplate,
                                       IsTarget = checkDetail.IsTarget,
                                       DeletedDate = checkDetail.EditCheck.DeletedDate ?? checkDetail.DeletedDate
                                   }).ToList().Distinct().ToList();

            if (screeningTemplate.IsEditChecked) return;

            _projectDesignTemplateId = screeningTemplate.ProjectDesignTemplateId;
            _screeningTemplateId = screeningTemplate.Id;
            _screeningEntryId = screeningTemplate.ScreeningEntryId;

            var variableList = All.AsNoTracking().Where(x =>
                x.ProjectDesignTemplateId == screeningTemplate.ProjectDesignTemplateId
                && x.DeletedDate == null && x.EditCheck.DeletedDate == null).ToList();

            var annotationList = (from variable in Context.ProjectDesignVariable.AsNoTracking().Where(t =>
                    t.ProjectDesignTemplate.Id == screeningTemplate.ProjectDesignTemplateId)
                                  join checkDetail in Context.EditCheckDetail.Where(t =>
                                          t.DeletedDate == null && t.EditCheck.ProjectDesignId == projectDesignId) on
                                      variable.Annotation equals checkDetail.VariableAnnotation
                                  select new
                                  {
                                      variable.Id,
                                      checkDetail.IsTarget,
                                      EditCheckDetailId = checkDetail.Id,
                                      checkDetail.Operator,
                                      variable.ProjectDesignTemplateId
                                  }).ToList().Distinct().ToList();

            variableList.AddRange(All.AsNoTracking().Where(r => r.IsTarget
                                                                && variableList.Any(a => a.EditCheckId == r.EditCheckId)
                                                                && r.DeletedDate == null).ToList());

            variableList.Distinct().ForEach(x =>
            {
                if (x.ProjectDesignVariableId.HasValue && !annotationList.Any(c => c.Id == x.ProjectDesignVariableId))
                    _screeningTemplateValueEditCheckRepository.InsertUpdate(new ScreeningTemplateValueEditCheckDto
                    {
                        ProjectDesignVariableId = (int)x.ProjectDesignVariableId,
                        ScreeningTemplateId =
                            x.ProjectDesignTemplateId != null && (int)x.ProjectDesignTemplateId == screeningTemplate.ProjectDesignTemplateId
                                ? screeningTemplate.Id
                                : 0,
                        ProjectDesignTemplateId = (int)x.ProjectDesignTemplateId,
                        ScreeningEntryId = screeningTemplate.ScreeningEntryId
                    }, false);
            });

            //annotationList.ForEach(x =>
            //{
            //    if (!variableList.Any(c => c.ProjectDesignVariableId == x.Id))
            //        _screeningTemplateValueEditCheckRepository.InsertUpdate(new ScreeningTemplateValueEditCheckDto
            //        {
            //            ProjectDesignVariableId = x.Id,
            //            ScreeningTemplateId = x.ProjectDesignTemplateId == screeningTemplate.ProjectDesignTemplateId
            //                ? screeningTemplate.Id
            //                : 0,
            //            ProjectDesignTemplateId = x.ProjectDesignTemplateId,
            //            EditCheckDetailId = x.EditCheckDetailId,
            //            IsVerify = x.IsTarget
            //                ? CurrentTemplateValidate(x.EditCheckDetailId, screeningTemplate.ScreeningEntryId,
            //                    screeningTemplate.Id, x.ProjectDesignTemplateId)
            //                : false,
            //            ScreeningEntryId = screeningTemplate.ScreeningEntryId
            //        }, false);
            //});


            _schedulerRuleRespository.ValidateRuleByTemplate(screeningTemplate.Id,
                screeningTemplate.ProjectDesignTemplateId, screeningTemplate.ScreeningEntryId, false,
                ref _projectDesignVariableId);

            screeningTemplate.IsEditChecked = true;
            Context.ScreeningTemplate.Update(screeningTemplate);
            Context.SaveChanges(_jwtTokenAccesser);
        }

        public List<EditCheckDetail> GetTemplateAndDomain(int projectDesignId)
        {
            return All.AsNoTracking().Where(x => x.EditCheck.ProjectDesignId == projectDesignId
                                                 && x.DeletedDate == null && x.EditCheck.DeletedDate == null
                                                 && x.IsTarget &&
                                                 (x.CheckBy == EditCheckRuleBy.ByTemplate ||
                                                  x.CheckBy == EditCheckRuleBy.ByTemplateAnnotation)).ToList();
        }

        public void EnableDisableTemplate(int projectDesignTemplateId, int screeningEntryId)
        {
            if (screeningEntryId == 0) return;

            var editChecks = All.AsNoTracking().Where(x =>
                (x.ProjectDesignTemplateId == projectDesignTemplateId ||
                 Context.ProjectDesignTemplate.Any(r => r.DomainId == x.DomainId && r.Id == projectDesignTemplateId))
                && x.EditCheck.DeletedDate == null
                && x.DeletedDate == null
            ).Select(r => r.EditCheckId).Distinct().ToList();

            if (editChecks.Count() == 0) return;

            var result = All.AsNoTracking().Where(x =>
                    editChecks.Any(r => r == x.EditCheckId) &&
                    !x.IsTarget && x.DeletedDate == null).Include(r => r.ProjectDesignVariable)
                .OrderBy(v => v.EditCheckId)
                .ThenByDescending(v => v.LogicalOperator).ToList();

            var validate = new List<ValidateResult>();

            result.Where(r => !r.IsTarget).ForEach(x =>
            {
                var variableValue = GetVariableValueForParent(x, screeningEntryId);
                var validateRule = ValidateRule(x, variableValue,
                    x.ProjectDesignVariable == null
                        ? CollectionSources.TextBox
                        : x.ProjectDesignVariable.CollectionSource);
                validate.Add(new ValidateResult
                {
                    EditChekId =
                        x.EditCheckId,
                    Value = validateRule.ToString(),
                    Result = Convert.ToString(validateRule),
                    LogicalOperator = string.IsNullOrEmpty(x.LogicalOperator) ? "" : x.LogicalOperator
                });
            });

            var resultTarget = All.AsNoTracking().Where(r => r.IsTarget
                                                             && r.DeletedDate == null
                                                             && editChecks.Any(a => a == r.EditCheckId))
                .Include(c => c.EditCheck).ToList();

            resultTarget.Where(r => validate.Any(a => a.EditChekId == r.EditCheckId)).ForEach(t =>
            {
                var screeningTemplate = Context.ScreeningTemplate
                    .Where(c => c.EditCheckDetailId == t.Id && c.ScreeningEntryId == screeningEntryId).ToList();
                screeningTemplate.ForEach(d =>
                {
                    if (validate.Any(x => x.EditChekId == t.EditCheckId))
                    {
                        var isUpdate = false;

                        if (validate.Any(x => x.EditChekId == t.EditCheckId
                                              && x.LogicalOperator.ToUpper() == "OR" && x.Result.ToUpper() == "TRUE"))
                            isUpdate = true;
                        else if (validate.Any(x => x.EditChekId == t.EditCheckId && x.Result.ToUpper() == "FALSE"))
                            isUpdate = false;
                        else if (validate.Any(x => x.EditChekId == t.EditCheckId))
                            isUpdate = true;

                        d.IsDisable = !isUpdate;

                        if (!isUpdate) UpdateEnableTemplate(d, t);
                        Context.ScreeningTemplate.Update(d);
                    }
                });
            });
        }

        public void ValidateRuleByTemplate(int screeningTemplateId, int projectDesignTemplateId, int screeningEntryId,
            int projectDesignId, bool isParent)
        {
            _projectDesignTemplateId = screeningTemplateId;
            _screeningTemplateId = projectDesignTemplateId;
            _screeningEntryId = screeningEntryId;

            var editChecks = All.AsNoTracking().Where(x =>
                    x.ProjectDesignTemplateId == projectDesignTemplateId &&
                    x.DeletedDate == null && x.EditCheck.DeletedDate == null).Select(r => r.EditCheckId).Distinct()
                .ToList();

            var result = All.AsNoTracking().Where(x =>
                    editChecks.Any(r => r == x.EditCheckId) &&
                    !x.IsTarget && x.DeletedDate == null && x.EditCheck.DeletedDate == null)
                .Include(r => r.ProjectDesignVariable).OrderBy(v => v.EditCheckId)
                .ThenByDescending(v => v.LogicalOperator).ToList();

            var validate = new List<ValidateResult>();

            result.Where(r => !r.IsTarget).ForEach(x =>
            {
                var variableValue = GetVariableValue(x, screeningEntryId, screeningTemplateId, projectDesignTemplateId);
                var validateRule = ValidateRule(x, variableValue,
                    x.ProjectDesignVariable == null
                        ? CollectionSources.TextBox
                        : x.ProjectDesignVariable.CollectionSource);
                var isReference = false;
                if (x.Operator == Operator.Different ||
                    x.Operator == Operator.Percentage)
                {
                    isReference = true;
                    variableValue = validateRule.ToString();
                }


                validate.Add(new ValidateResult
                {
                    EditChekId = x.EditCheckId,
                    Result = Convert.ToString(validateRule == null ? "" : validateRule),
                    LogicalOperator = string.IsNullOrEmpty(x.LogicalOperator) ? "" : x.LogicalOperator,
                    Value = variableValue,
                    IsReference = isReference
                });
            });

            var resultTarget = Context.ScreeningTemplateValueEditCheck.AsNoTracking().Where(r =>
                    r.EditCheckDetail.IsTarget &&
                    r.ScreeningEntryId == screeningEntryId).Include(v => v.ProjectDesignVariable)
                .Include(v => v.EditCheckDetail).ThenInclude(v => v.EditCheck).ToList();

            resultTarget = resultTarget.Where(r =>
                r.EditCheckDetail.DeletedDate == null &&
                r.EditCheckDetail.EditCheck.DeletedDate == null &&
                editChecks.Any(a => a == r.EditCheckDetail.EditCheckId)).ToList();

            resultTarget = resultTarget.Where(x => x.EditCheckDetail.IsTarget &&
                                                   editChecks.Any(c => c == x.EditCheckDetail.EditCheckId)).ToList();

            resultTarget.ForEach(t =>
            {
                if (validate.Any(x => x.EditChekId == t.EditCheckDetail.EditCheckId))
                {
                    var isUpdate = false;

                    if (validate.Any(x => x.EditChekId == t.EditCheckDetail.EditCheckId
                                          && x.LogicalOperator.ToUpper() == "OR" && x.Result.ToUpper() == "TRUE"))
                        isUpdate = true;
                    else if (validate.Any(x =>
                        x.EditChekId == t.EditCheckDetail.EditCheckId && x.Result.ToUpper() == "FALSE"))
                        isUpdate = false;
                    else if (validate.Any(x => x.EditChekId == t.EditCheckDetail.EditCheckId))
                        isUpdate = true;

                    var isClosed = false;
                    if (isUpdate)
                    {
                        var refValue = validate.FirstOrDefault(x => x.EditChekId == t.EditCheckDetail.EditCheckId);
                        isClosed = CheckedClose(t, t.ScreeningTemplateId == screeningTemplateId, refValue.Value, true,
                            refValue.IsReference);
                    }
                    else if (t.EditCheckDetail.Operator == Operator.Enable)
                    {
                        SystemQuery(t, false);
                    }

                    //_screeningTemplateValueEditCheckRepository.UpdateById(new ScreeningTemplateValueEditCheck
                    //{
                    //    Id = t.Id,
                    //    ProjectDesignVariableId = t.ProjectDesignVariableId,
                    //    IsVerify = isUpdate,
                    //    ScreeningTemplateId = t.ScreeningTemplateId,
                    //    EditCheckDetailId = t.EditCheckDetailId,
                    //    IsClosed = isClosed,
                    //    ScreeningEntryId = screeningEntryId
                    //});
                }
            });


            _schedulerRuleRespository.ValidateRuleByTemplate(screeningTemplateId
                , projectDesignTemplateId, screeningEntryId, true, ref _projectDesignVariableId);
        }

        public List<int> GetprojectDesignVariableIds()
        {
            return _projectDesignVariableId;
        }

        public void ValidateRuleByEditCheckDetailId(VariableEditCheckDto variableEditCheckDto)
        {
            _projectDesignTemplateId = variableEditCheckDto.ProjectDesignTemplateId;
            _screeningTemplateId = variableEditCheckDto.ScreeningTemplateId;
            _screeningEntryId = variableEditCheckDto.ScreeningEntryId;

            if (variableEditCheckDto.CollectionSource == CollectionSources.Date ||
                variableEditCheckDto.CollectionSource == CollectionSources.DateTime)
                _schedulerRuleRespository.SchedulerRuleByVariable(variableEditCheckDto, ref _projectDesignVariableId);

            var editCheck = Context.ScreeningTemplateValueEditCheck.AsNoTracking().Where(x =>
                    x.ProjectDesignVariableId == variableEditCheckDto.ProjectDesignVariableId
                    && x.EditCheckDetail.DeletedDate == null
                    && x.ScreeningTemplateId == variableEditCheckDto.ScreeningTemplateId)
                .Select(r => new { r.EditCheckDetail.EditCheckId, r.ScreeningEntryId }).ToList().Distinct().ToList();

            if (variableEditCheckDto.IsFromQuery)
                EnableDisableTemplate(variableEditCheckDto.ProjectDesignTemplateId,
                    variableEditCheckDto.ScreeningEntryId);

            if (editCheck == null || editCheck.Count() == 0) return;

            var result = All.AsNoTracking().Where(x => editCheck.Any(c => c.EditCheckId == x.EditCheckId) &&
                                                       x.DeletedDate == null && x.EditCheck.DeletedDate == null)
                .Include(r => r.ProjectDesignVariable).OrderBy(v => v.EditCheckId)
                .OrderByDescending(v => v.LogicalOperator).ToList();

            var validate = new List<ValidateResult>();
            result = result.Where(r => !r.IsTarget).ToList();
            if (result == null || result.Count() == 0) return;
            result.ForEach(x =>
            {
                var variableValue = GetVariableValue(x, editCheck.FirstOrDefault().ScreeningEntryId,
                    variableEditCheckDto.ScreeningTemplateId, variableEditCheckDto.ProjectDesignTemplateId);
                var validateRule = ValidateRule(x, variableValue,
                    x.ProjectDesignVariable == null
                        ? CollectionSources.TextBox
                        : x.ProjectDesignVariable.CollectionSource);
                var isReference = false;
                if (x.Operator == Operator.Different ||
                    x.Operator == Operator.Percentage )
                {
                    isReference = true;
                    variableValue = validateRule.ToString();
                }


                validate.Add(new ValidateResult
                {
                    EditChekId = x.EditCheckId,
                    Value = variableValue,
                    IsReference = isReference,
                    Result = Convert.ToString(validateRule == null ? "" : validateRule),
                    LogicalOperator = string.IsNullOrEmpty(x.LogicalOperator) ? "" : x.LogicalOperator
                });
            });

            var resultTarget =
                Context.ScreeningTemplateValueEditCheck.AsNoTracking().Where(r =>
                        r.EditCheckDetail.DeletedDate == null &&
                        r.EditCheckDetail.EditCheck.DeletedDate == null &&
                        r.EditCheckDetail.IsTarget &&
                        variableEditCheckDto.IsFromQuery
                            ? r.ScreeningEntryId == editCheck.FirstOrDefault().ScreeningEntryId
                            : r.ScreeningTemplateId == variableEditCheckDto.ScreeningTemplateId
                    ).Include(v => v.ProjectDesignVariable).Include(v => v.EditCheckDetail)
                    .ThenInclude(v => v.EditCheck)
                    .AsNoTracking().ToList();

            resultTarget = resultTarget.Where(x =>
                    x.EditCheckDetail.IsTarget && editCheck.Any(c => c.EditCheckId == x.EditCheckDetail.EditCheckId))
                .ToList();

            if (_screeningTemplateIdLists != null && _screeningTemplateIdLists.Count() > 0)
                resultTarget = resultTarget.Where(b => !_screeningTemplateIdLists.Any(v => v == b.ScreeningTemplateId))
                    .ToList();

            resultTarget.ForEach(t =>
            {
                var isUpdate = false;

                if (validate.Any(x =>
                    x.EditChekId == t.EditCheckDetail.EditCheckId && x.LogicalOperator.ToUpper() == "OR" &&
                    x.Result.ToUpper() == "TRUE"))
                    isUpdate = true;
                else if (validate.Any(x =>
                    x.EditChekId == t.EditCheckDetail.EditCheckId && x.Result.ToUpper() == "FALSE"))
                    isUpdate = false;
                else if (validate.Any(x => x.EditChekId == t.EditCheckDetail.EditCheckId))
                    isUpdate = true;

                var isClosed = false;

                if (isUpdate)
                {
                    var refValue = validate.FirstOrDefault(x => x.EditChekId == t.EditCheckDetail.EditCheckId);
                    isClosed = CheckedClose(t, t.ScreeningTemplateId == variableEditCheckDto.ScreeningTemplateId,
                        refValue.Value, variableEditCheckDto.IsFromQuery, refValue.IsReference);
                }
                else if (variableEditCheckDto.IsFromQuery && t.EditCheckDetail.Operator == Operator.Enable)
                {
                    SystemQuery(t, false);
                }

                _screeningTemplateValueEditCheckRepository.UpdateById(new ScreeningTemplateValueEditCheck
                {
                    Id = t.Id,
                    ProjectDesignVariableId = t.ProjectDesignVariableId,
                    ScreeningTemplateId = t.ScreeningTemplateId,
                    EditCheckDetailId = t.EditCheckDetailId,
                    ScreeningEntryId = editCheck.FirstOrDefault().ScreeningEntryId
                });
            });
        }

        private void UpdateEnableTemplate(ScreeningTemplate screeningTemplate, EditCheckDetail editCheckDetail)
        {
            _screeningTemplateIdLists.Add(screeningTemplate.Id);
            if (screeningTemplate.ReviewLevel == null || screeningTemplate.ReviewLevel == 0) return;
            screeningTemplate.ReviewLevel = 1;
            screeningTemplate.Status = ScreeningStatus.Submitted;

            var screeningTemplateReview = Context.ScreeningTemplateReview.AsNoTracking()
                .Where(x => x.ScreeningTemplateId ==
                            screeningTemplate.Id && x.Status == ScreeningStatus.Reviewed).ToList();
            screeningTemplateReview.ForEach(c =>
            {
                c.IsRepeat = true;
                Context.ScreeningTemplateReview.Update(c);
            });

            var screeningTemplateValue = Context.ScreeningTemplateValue.AsNoTracking()
                .Where(t => t.ScreeningTemplateId == screeningTemplate.Id).ToList();

            screeningTemplateValue.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x.Value) || x.IsNa)
                {
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
                                $"{"Edit Check by "} {editCheckDetail.EditCheck.AutoNumber} {editCheckDetail.Message}",
                            ScreeningTemplateValueId = x.Id
                        },
                        new ScreeningTemplateValueQuery
                        {
                            ScreeningTemplateValue = x,
                            OldValue = x.IsNa ? "NA" : x.Value,
                            QueryStatus = QueryStatus.Open,
                            IsSystem = true,
                            Note =
                                $"{"Edit Check by "} {editCheckDetail.EditCheck.AutoNumber} {editCheckDetail.Message}",
                            ScreeningTemplateValueId = x.Id
                        }, x);
                }
            });
        }

        private void InsertHardSoft(int screeningTemplateId, int projectDesignVariableId, string value, string note,
            Operator _operator)
        {
            var screeningTemplateValue = Context.ScreeningTemplateValue.Where(x =>
                x.ProjectDesignVariableId == projectDesignVariableId &&
                x.ScreeningTemplateId == screeningTemplateId).AsNoTracking().FirstOrDefault();

            if (_operator == Operator.SoftFetch && screeningTemplateValue != null &&
                !string.IsNullOrEmpty(screeningTemplateValue.Value)) return;

            var projectDesignVariable = Context.ProjectDesignVariable.Find(projectDesignVariableId);

            if (!string.IsNullOrEmpty(value) &&
                (projectDesignVariable.CollectionSource == CollectionSources.RadioButton ||
                 projectDesignVariable.CollectionSource == CollectionSources.CheckBox ||
                 projectDesignVariable.CollectionSource == CollectionSources.MultiCheckBox ||
                 projectDesignVariable.CollectionSource == CollectionSources.ComboBox))
            {
                var valueName = Context.ProjectDesignVariableValue.Find(Convert.ToInt32(value))?.ValueName;
                if (!string.IsNullOrEmpty(valueName))
                {
                    var projectDesignVariableValue = Context.ProjectDesignVariableValue.FirstOrDefault(x =>
                        x.ProjectDesignVariableId == projectDesignVariableId
                        && x.ValueName == valueName);
                    if (projectDesignVariableValue != null) value = projectDesignVariableValue.Id.ToString();
                }
            }

            if (_queryValueList.Any(c => c.ProjectDesignVariableId == projectDesignVariableId &&
                                         c.ScreeningTemplateId == screeningTemplateId))
                return;
            var aduits = new List<ScreeningTemplateValueAudit>
            {
                new ScreeningTemplateValueAudit
                {
                    Value = value,
                    OldValue = null,
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
                Context.ScreeningTemplateValue.Add(screeningTemplateValue);
            }
            else
            {
                screeningTemplateValue.Audits = aduits;
                screeningTemplateValue.Value = value;
                screeningTemplateValue.ObjectState = ObjectState.Modified;
                Context.ScreeningTemplateValue.Update(screeningTemplateValue);
            }

            _queryValueList.Add(screeningTemplateValue);
        }

        private bool CheckedClose(ScreeningTemplateValueEditCheck editCheck, bool isCurrent, string value,
            bool isGenerateQuery, bool isReference)
        {
            if (editCheck.EditCheckDetail.Operator == Operator.HardFetch ||
                editCheck.EditCheckDetail.Operator == Operator.SoftFetch)
                InsertHardSoft(editCheck.ScreeningTemplateId, editCheck.ProjectDesignVariableId, value,
                    $"{"Edit Check by "} {editCheck.EditCheckDetail.EditCheck.AutoNumber} {editCheck.EditCheckDetail.Message}",
                    (Operator)editCheck.EditCheckDetail.Operator);

            var isClosed = false;

            if (!CheckSkipOperator(editCheck.EditCheckDetail.Operator))
            {
                var variableValue = GetVariableValueByScreeningTemplateId(editCheck);

                if (isReference)
                {
                    editCheck.EditCheckDetail.CollectionValue = value;
                    editCheck.EditCheckDetail.ObjectState = ObjectState.Unchanged;
                }

                var validateRule = ValidateRule(editCheck.EditCheckDetail,
                    variableValue,
                    editCheck.ProjectDesignVariable == null
                        ? CollectionSources.TextBox
                        : editCheck.ProjectDesignVariable.CollectionSource);
                if (validateRule.ToString().ToUpper() == "TRUE")
                    isClosed = true;
                else if (isGenerateQuery) SystemQuery(editCheck, true);
            }

            return isClosed;
        }

        private void SystemQuery(ScreeningTemplateValueEditCheck editCheck, bool isVerify)
        {
            if (CheckSkipOperator((Operator)editCheck.EditCheckDetail.Operator))
                return;

            var screeningTemplateValue = Context.ScreeningTemplateValue.AsNoTracking().FirstOrDefault
            (t => t.ScreeningTemplateId == editCheck.ScreeningTemplateId
                  && t.ProjectDesignVariableId == editCheck.ProjectDesignVariableId);

            if (_projectDesignVariableId != null &&
                _projectDesignVariableId.Any(c => c == editCheck.ProjectDesignVariableId))
            {
                if (screeningTemplateValue != null)
                {
                    var query = new ScreeningTemplateValueQuery
                    {
                        QueryStatus = QueryStatus.Open,
                        IsSystem = true,
                        QueryLevel = 0,
                        UserRoleId = _jwtTokenAccesser.RoleId,
                        Note =
                            $"{"Edit Check by "} {editCheck.EditCheckDetail.EditCheck.AutoNumber} {editCheck.EditCheckDetail.Message}",
                        ScreeningTemplateValueId = screeningTemplateValue.Id
                    };
                    Context.ScreeningTemplateValueQuery.Add(query);
                }

                return;
            }

            var screeningTemplate = Context.ScreeningTemplate.Find(editCheck.ScreeningTemplateId);
            if (screeningTemplate == null || (int)screeningTemplate.Status < 3)
                return;

            if (screeningTemplateValue == null)
                if (Context.ProjectDesignVariableValue.Any(x => x.DeletedDate == null &&
                                                                x.Id == editCheck.ProjectDesignVariableId))
                {
                    screeningTemplateValue = new ScreeningTemplateValue();

                    screeningTemplateValue.ProjectDesignVariableId = editCheck.ProjectDesignVariableId;
                    screeningTemplateValue.ScreeningTemplateId = editCheck.ScreeningTemplateId;
                }

            if (screeningTemplateValue != null)
            {
                if (screeningTemplateValue.IsSystem && screeningTemplateValue.QueryStatus == QueryStatus.Open)
                    return;

                if (editCheck.EditCheckDetail.Operator == Operator.Enable)
                {
                    if (isVerify && !string.IsNullOrEmpty(screeningTemplateValue.Value))
                        return;
                    if (!isVerify && string.IsNullOrEmpty(screeningTemplateValue.Value))
                        return;
                    screeningTemplateValue.Value = "";
                }

                _projectDesignVariableId.Add(editCheck.ProjectDesignVariableId);
                screeningTemplateValue.IsSystem = true;
                _screeningTemplateValueQueryRepository.GenerateQuery(
                    new ScreeningTemplateValueQueryDto
                    {
                        QueryStatus = QueryStatus.Open,
                        IsSystem = true,
                        Note =
                            $"{"Edit Check by "} {editCheck.EditCheckDetail.EditCheck.AutoNumber} {editCheck.EditCheckDetail.Message}",
                        ScreeningTemplateValueId = screeningTemplateValue.Id
                    },
                    new ScreeningTemplateValueQuery
                    {
                        ScreeningTemplateValue = screeningTemplateValue,
                        QueryStatus = QueryStatus.Open,
                        IsSystem = true,
                        Note =
                            $"{"Edit Check by "} {editCheck.EditCheckDetail.EditCheck.AutoNumber} {editCheck.EditCheckDetail.Message}",
                        ScreeningTemplateValueId = screeningTemplateValue.Id
                    }, screeningTemplateValue);
            }
        }

        private object ValidateRule(EditCheckDetail editCheckDetail, string variableValue,
            CollectionSources collectionSources)
        {
            var ruleValue = editCheckDetail.CollectionValue;
            switch (editCheckDetail.Operator)
            {
                case Operator.NotNull:
                    return !string.IsNullOrEmpty(variableValue);
                case Operator.Null:
                    return string.IsNullOrEmpty(variableValue);
                case Operator.Equal:
                    return ruleValue == variableValue;
                case Operator.NotEqual:
                    return ruleValue != variableValue;
                case Operator.Greater:
                    if (string.IsNullOrEmpty(variableValue) || string.IsNullOrEmpty(ruleValue)) return true;
                    //if (collectionSources == CollectionSources.TextBox)
                    //    return Convert.ToDecimal(variableValue) > Convert.ToDecimal(ruleValue);
                    //else
                    //    return Convert.ToDateTime(variableValue) > Convert.ToDateTime(ruleValue);
                    return Convert.ToDecimal(variableValue) > Convert.ToDecimal(ruleValue);
                case Operator.GreaterEqual:
                    if (string.IsNullOrEmpty(variableValue) || string.IsNullOrEmpty(ruleValue)) return true;
                    //if (collectionSources == CollectionSources.TextBox)
                    //    return Convert.ToDecimal(variableValue) >= Convert.ToDecimal(ruleValue);
                    //else
                    //    return Convert.ToDateTime(variableValue) >= Convert.ToDateTime(ruleValue);
                    return Convert.ToDecimal(variableValue) >= Convert.ToDecimal(ruleValue);
                case Operator.Lessthen:
                    if (string.IsNullOrEmpty(variableValue) || string.IsNullOrEmpty(ruleValue)) return true;
                    //if (collectionSources == CollectionSources.TextBox)
                    //    return Convert.ToDecimal(variableValue) < Convert.ToDecimal(ruleValue);
                    //else
                    //    return Convert.ToDateTime(variableValue) < Convert.ToDateTime(ruleValue);
                    return Convert.ToDecimal(variableValue) < Convert.ToDecimal(ruleValue);
                case Operator.LessthenEqual:
                    if (string.IsNullOrEmpty(variableValue) || string.IsNullOrEmpty(ruleValue)) return true;
                    if (collectionSources == CollectionSources.TextBox)
                        return Convert.ToDecimal(variableValue) <= Convert.ToDecimal(ruleValue);
                    else
                        return Convert.ToDateTime(variableValue) <= Convert.ToDateTime(ruleValue);
                case Operator.Plus:
                    if (string.IsNullOrEmpty(variableValue) || string.IsNullOrEmpty(ruleValue)) return true;
                    //if (collectionSources == CollectionSources.TextBox)
                    //    return Convert.ToDecimal(variableValue) + Convert.ToDecimal(ruleValue);
                    //else
                    //    return Convert.ToDateTime(variableValue).AddDays(Convert.ToInt32(ruleValue));
                    return Convert.ToDecimal(variableValue) + Convert.ToDecimal(ruleValue);
                case Operator.Different:
                    return GetDifferentValue(editCheckDetail);
                case Operator.Percentage:
                    return GetPercentageValue(editCheckDetail);
                //case Operator.Bmi:
                //    return GetBmi(editCheckDetail);
                case Operator.SoftFetch:
                    break;
                case Operator.HardFetch:
                    break;
                case Operator.Required:
                    return !string.IsNullOrEmpty(variableValue);
                case Operator.Optional:
                    break;
                case Operator.Enable:
                    break;
                case Operator.Warning:
                    break;
            }

            return editCheckDetail.Operator;
        }

        private bool CurrentTemplateValidate(int editCheckDetailId, int screeningEntryId, int screeningTemplateId,
            int projectDesignTemplateId)
        {
            var resultTarget = Find(editCheckDetailId);

            if (resultTarget == null) return false;

            var editCheck = All.AsNoTracking().AsNoTracking().Where(x => x.EditCheckId == resultTarget.EditCheckId)
                .ToList();

            if (editCheck == null || editCheck.Count() == 0) return false;

            var result = All.AsNoTracking().Where(x => editCheck.Any(c => c.EditCheckId == x.EditCheckId) &&
                                                       x.DeletedDate == null && x.EditCheck.DeletedDate == null)
                .Include(b => b.ProjectDesignVariable).OrderBy(v => v.EditCheckId)
                .OrderByDescending(v => v.LogicalOperator).ToList();

            var validate = new List<ValidateResult>();

            result.Where(r => !r.IsTarget).ForEach(x =>
            {
                var variableValue = GetVariableValue(x, screeningEntryId, screeningTemplateId, projectDesignTemplateId);
                var validateRule = ValidateRule(x, variableValue,
                    x.ProjectDesignVariable == null
                        ? CollectionSources.TextBox
                        : x.ProjectDesignVariable.CollectionSource);
                validate.Add(new ValidateResult
                {
                    EditChekId = x.EditCheckId,
                    Result = Convert.ToString(validateRule == null ? "" : validateRule),
                    LogicalOperator = string.IsNullOrEmpty(x.LogicalOperator) ? "" : x.LogicalOperator
                });
            });

            var isUpdate = false;
            if (resultTarget != null)
            {
                if (validate.Any(x => x.LogicalOperator.ToUpper() == "OR" && x.Result.ToUpper() == "TRUE"))
                    isUpdate = true;
                else if (validate.Any(x => x.Result.ToUpper() == "FALSE"))
                    isUpdate = false;
                else if (validate.Any())
                    isUpdate = true;
            }

            return isUpdate;
        }

        private string GetPercentageValue(EditCheckDetail editCheckDetail)
        {
            double variableTotal = 0;
            double collectionTotal = 0;
            try
            {
                var result = All.Where(x => x.EditCheckId == editCheckDetail.EditCheckId
                                            && x.IsTarget == editCheckDetail.IsTarget && x.DeletedDate == null)
                    .ToList();

                if (result == null || result.Count() == 0) return "";

                result.ForEach(x =>
                {
                    var variableValue = GetVariableValue(x, _screeningEntryId, _screeningTemplateId,
                        _projectDesignTemplateId);
                    double doubleValue = 0;
                    double.TryParse(variableValue, out doubleValue);
                    variableTotal += doubleValue;
                    doubleValue = 0;
                    double.TryParse(x.CollectionValue, out doubleValue);
                    collectionTotal += doubleValue;
                });

                if (collectionTotal > 0) collectionTotal = collectionTotal / 100;
            }
            catch (Exception ex)
            {
            }

            return Math.Abs(Convert.ToInt32(variableTotal + variableTotal * collectionTotal)).ToString();
        }

        private string GetDifferentValue(EditCheckDetail editCheckDetail)
        {
            var objectValue = "";
            var retunValue = "";
            try
            {
                var result = All.Where(x =>
                    x.EditCheckId == editCheckDetail.EditCheckId && x.IsTarget == editCheckDetail.IsTarget &&
                    x.DeletedDate == null).ToList();

                if (result == null || result.Count() == 0) return "";

                result.ForEach(x =>
                {
                    var variableValue = GetVariableValue(x, _screeningEntryId, _screeningTemplateId,
                        _projectDesignTemplateId);

                    if (!string.IsNullOrEmpty(objectValue))
                        if (!string.IsNullOrEmpty(objectValue) && !string.IsNullOrEmpty(variableValue))
                        {
                            if (x.CollectionValue.ToUpper().Contains("Y"))
                            {
                                var startDate = Convert.ToDateTime(objectValue);
                                var endDate = Convert.ToDateTime(variableValue);

                                var age = startDate.Year - endDate.Year;
                                if (endDate.Date > startDate.AddYears(-age)) age--;

                                retunValue = Convert.ToString(Math.Abs(age));
                            }
                            else if (x.CollectionValue.ToUpper().Contains("M"))
                            {
                                var ts = Convert.ToDateTime(objectValue) - Convert.ToDateTime(variableValue);
                                retunValue = Convert.ToString(Math.Abs(Convert.ToInt32(ts.TotalDays / 30)));
                            }
                            else if (x.CollectionValue.ToUpper().Contains("D"))
                            {
                                var ts = Convert.ToDateTime(objectValue) - Convert.ToDateTime(variableValue);
                                retunValue = Convert.ToString(Math.Abs(Convert.ToInt32(ts.TotalDays)));
                            }
                        }

                    objectValue = variableValue;
                });
            }
            catch (Exception ex)
            {
            }

            return retunValue;
        }

        private string GetBmi(EditCheckDetail editCheckDetail)
        {
            double heigthValue = 0;
            double weightValue = 0;
            var retunValue = "";
            try
            {
                var result = All.Where(x =>
                    x.EditCheckId == editCheckDetail.EditCheckId && x.IsTarget == editCheckDetail.IsTarget &&
                    x.DeletedDate == null).ToList();

                if (result == null || result.Count() == 0) return "";

                result.ForEach(x =>
                {
                    if (x.CollectionValue.ToUpper().Contains("H"))
                        heigthValue = Convert.ToDouble(GetVariableValue(x, _screeningEntryId, _screeningTemplateId,
                            _projectDesignTemplateId));

                    if (x.CollectionValue.ToUpper().Contains("W"))
                        weightValue = Convert.ToDouble(GetVariableValue(x, _screeningEntryId, _screeningTemplateId,
                            _projectDesignTemplateId));

                    var bmiValue = weightValue / (heigthValue * heigthValue) * 10000;

                    retunValue = Math.Round(bmiValue, 2, MidpointRounding.AwayFromZero).ToString();
                });
            }
            catch (Exception ex)
            {
            }

            return retunValue;
        }

        private bool CheckSkipOperator(Operator? _operator)
        {
            if (_operator == Operator.SoftFetch || _operator == Operator.HardFetch ||
                _operator == Operator.Optional || _operator == Operator.Warning)
                return true;
            return false;
        }

        private string GetVariableValueForParent(EditCheckDetail editCheckDetail, int screeningEntryId)
        {
            var screeningValue = Context.ScreeningTemplateValue.AsNoTracking().Where(t =>
                t.ProjectDesignVariableId == editCheckDetail.ProjectDesignVariableId
                && t.ScreeningTemplate.ScreeningEntryId == screeningEntryId
                && t.ScreeningTemplate.ParentId == null
                && t.ScreeningTemplate.ProjectDesignTemplateId == editCheckDetail.ProjectDesignTemplateId).Select(v =>
                new ScreeningTemplateValue
                {
                    Value = v.Value,
                    IsNa = v.IsNa,
                    ProjectDesignVariable = v.ProjectDesignVariable
                }).FirstOrDefault();


            if (screeningValue == null) return "";
            var variableValue = screeningValue?.Value;

            if (screeningValue != null && string.IsNullOrEmpty(variableValue) && screeningValue.IsNa)
                variableValue = "NA";

            if (screeningValue != null &&
                screeningValue.ProjectDesignVariable.CollectionSource == CollectionSources.MultiCheckBox)
                variableValue = GetMultiCheckBox(screeningValue.Id, editCheckDetail.CollectionValue);

            return variableValue;
        }

        private string GetVariableValue(EditCheckDetail editCheckDetail, int screeningEntryId, int screeningTemplateId,
            int projectDesignTemplateId)
        {
            var screeningValue = new ScreeningTemplateValue();
            if (editCheckDetail.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
            {
                screeningValue = Context.ScreeningTemplateValue.AsNoTracking().Where(t =>
                    t.ProjectDesignVariable.Annotation == editCheckDetail.VariableAnnotation
                    && t.ScreeningTemplate.Id == screeningTemplateId).Select(v => new ScreeningTemplateValue
                    {
                        Value = v.Value,
                        IsNa = v.IsNa,
                        ProjectDesignVariable = v.ProjectDesignVariable
                    }).FirstOrDefault();
            }

            else
            {
                if (projectDesignTemplateId == editCheckDetail.ProjectDesignTemplateId)
                    screeningValue = Context.ScreeningTemplateValue.AsNoTracking().Where(t =>
                        t.ProjectDesignVariableId == editCheckDetail.ProjectDesignVariableId
                        && t.ScreeningTemplate.Id == screeningTemplateId).Select(v => new ScreeningTemplateValue
                        {
                            Value = v.Value,
                            IsNa = v.IsNa,
                            ProjectDesignVariable = v.ProjectDesignVariable
                        }).FirstOrDefault();
                else
                    screeningValue = Context.ScreeningTemplateValue.AsNoTracking().Where(t =>
                            t.ProjectDesignVariableId == editCheckDetail.ProjectDesignVariableId
                            && t.ScreeningTemplate.ScreeningEntryId == screeningEntryId
                            && t.ScreeningTemplate.ParentId == null
                            && t.ScreeningTemplate.ProjectDesignTemplateId == editCheckDetail.ProjectDesignTemplateId)
                        .Select(v => new ScreeningTemplateValue
                        {
                            Value = v.Value,
                            IsNa = v.IsNa,
                            ProjectDesignVariable = v.ProjectDesignVariable
                        }).FirstOrDefault();
            }


            if (screeningValue == null) return "";

            var variableValue = screeningValue?.Value;

            if (screeningValue != null && string.IsNullOrEmpty(variableValue) && screeningValue.IsNa)
                variableValue = "NA";

            if (editCheckDetail.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
            {
                if (string.IsNullOrEmpty(variableValue)) return "";

                if (screeningValue.ProjectDesignVariable.CollectionSource == CollectionSources.ComboBox ||
                    screeningValue.ProjectDesignVariable.CollectionSource == CollectionSources.CheckBox ||
                    screeningValue.ProjectDesignVariable.CollectionSource == CollectionSources.RadioButton)
                {
                    var collectionValueId = Convert.ToInt32(editCheckDetail.CollectionValue);
                    var variableValueId = Convert.ToInt32(variableValue);
                    editCheckDetail.CollectionValue =
                        Context.ProjectDesignVariableValue.Find(collectionValueId)?.ValueName;
                    editCheckDetail.ObjectState = ObjectState.Unchanged;
                    variableValue = Context.ProjectDesignVariableValue.Find(variableValueId)?.ValueName;
                }
            }

            if (screeningValue != null &&
                screeningValue.ProjectDesignVariable.CollectionSource == CollectionSources.MultiCheckBox)
                variableValue = GetMultiCheckBox(screeningValue.Id, editCheckDetail.CollectionValue);

            return variableValue;
        }

        private string GetMultiCheckBox(int id, string collectionValue)
        {
            if (string.IsNullOrEmpty(collectionValue)) return "";
            var variableValue = "";
            var multiRecord = Context.ScreeningTemplateValueChild.Any(t => t.ScreeningTemplateValueId == id &&
                                                                           t.ProjectDesignVariableValueId ==
                                                                           Convert.ToInt16(collectionValue)
                                                                           && t.Value == "true");

            if (multiRecord)
                variableValue = collectionValue;

            return variableValue;
        }

        private string GetVariableValueByScreeningTemplateId(
            ScreeningTemplateValueEditCheck screeningTemplateValueEditCheck)
        {
            var screeningValue = Context.ScreeningTemplateValue.AsNoTracking().FirstOrDefault(t =>
                t.ProjectDesignVariableId == screeningTemplateValueEditCheck.ProjectDesignVariableId
                && t.ScreeningTemplate.Id == screeningTemplateValueEditCheck.ScreeningTemplateId);

            var variableValue = screeningValue?.Value;

            if (screeningValue != null && string.IsNullOrEmpty(variableValue) && screeningValue.IsNa)
                variableValue = "NA";

            if (screeningValue != null && screeningTemplateValueEditCheck.ProjectDesignVariable.CollectionSource ==
                CollectionSources.MultiCheckBox)
                variableValue = GetMultiCheckBox(screeningValue.Id,
                    screeningTemplateValueEditCheck.EditCheckDetail.CollectionValue);

            return variableValue;
        }
    }
}