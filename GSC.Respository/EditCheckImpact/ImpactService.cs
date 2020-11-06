using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Schedule;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.EditCheck;
using GSC.Respository.Project.Schedule;
using GSC.Respository.Screening;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.EditCheckImpact
{
    public class ImpactService : GenericRespository<ScreeningTemplate, GscContext>, IImpactService
    {
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IMapper _mapper;
        private readonly IEditCheckDetailRepository _editCheckDetailRepository;
        private readonly IProjectDesignVariableValueRepository _projectDesignVariableValueRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IProjectScheduleRepository _projectScheduleRepository;
        private readonly IScreeningTemplateValueChildRepository _screeningTemplateValueChildRepository;
        private readonly IProjectScheduleTemplateRepository _projectScheduleTemplateRepository;
        public ImpactService(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IProjectDesignVariableValueRepository projectDesignVariableValueRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectScheduleTemplateRepository projectScheduleTemplateRepository,
            IScreeningTemplateValueChildRepository screeningTemplateValueChildRepository,
            IProjectScheduleRepository projectScheduleRepository,
            IEditCheckDetailRepository editCheckDetailRepository) : base(uow, jwtTokenAccesser)
        {
            _mapper = mapper;
            _editCheckDetailRepository = editCheckDetailRepository;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _projectScheduleRepository = projectScheduleRepository;
            _projectScheduleTemplateRepository = projectScheduleTemplateRepository;
            _screeningTemplateValueChildRepository = screeningTemplateValueChildRepository;
            _projectDesignVariableValueRepository = projectDesignVariableValueRepository;
        }
        public List<EditCheckValidateDto> GetEditCheck(ScreeningTemplateBasic screeningTemplateBasic)
        {
            var result = _editCheckDetailRepository.All.AsNoTracking().Where(c =>
                c.DeletedDate == null &&
                 c.CheckBy != EditCheckRuleBy.ByVariableAnnotation &&
                _editCheckDetailRepository.All.AsNoTracking().Any(x =>
                ((x.ProjectDesignTemplateId == screeningTemplateBasic.ProjectDesignTemplateId) ||
                (x.EditCheck.ProjectDesignId == screeningTemplateBasic.ProjectDesignId
                && x.DomainId != null && x.DomainId == screeningTemplateBasic.DomainId))
                && x.EditCheckId == c.EditCheckId && x.EditCheck.DeletedDate == null))
                .ProjectTo<EditCheckValidateDto>(_mapper.ConfigurationProvider).ToList();

            result.ForEach(r =>
            {
                if (r.CheckBy == EditCheckRuleBy.ByTemplateAnnotation
                && r.DomainId == screeningTemplateBasic.DomainId)
                    r.ProjectDesignTemplateId = screeningTemplateBasic.ProjectDesignTemplateId;
            });

            var annotation = (from variable in _projectDesignVariableRepository.All.AsNoTracking().
                              Where(t => t.ProjectDesignTemplateId == screeningTemplateBasic.ProjectDesignTemplateId
                              && t.DeletedDate == null && t.Annotation != null)
                              join checkDetail in _editCheckDetailRepository.All.AsNoTracking().
                              Where(r => r.DeletedDate == null && r.EditCheck.DeletedDate == null && r.VariableAnnotation != null) on
                              new
                              {
                                  ann = variable.Annotation,
                                  designId = screeningTemplateBasic.ProjectDesignId
                              }
                              equals new
                              {
                                  ann = checkDetail.VariableAnnotation,
                                  designId = checkDetail.EditCheck.ProjectDesignId
                              }
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
                                  CollectionValue2 = checkDetail.CollectionValue2,
                                  IsReferenceValue = checkDetail.IsReferenceValue,
                                  LogicalOperator = checkDetail.LogicalOperator,
                                  Message = checkDetail.Message,
                                  AutoNumber = checkDetail.EditCheck.AutoNumber,
                                  IsSameTemplate = checkDetail.IsSameTemplate,
                                  IsTarget = checkDetail.IsTarget,
                                  StartParens = checkDetail.StartParens,
                                  EndParens = checkDetail.EndParens,
                              }).Distinct().ToList();

            if (annotation != null)
                result.AddRange(annotation);

            return result.OrderBy(t => t.EditCheckId).ToList();

        }


        public List<EditCheckValidateDto> GetEditCheckByVaiableId(int projectDesignTemplateId, int projectDesignVariableId, List<EditCheckIds> editCheckIds)
        {
            var Ids = editCheckIds.Select(r => r.EditCheckId).Distinct().ToList();

            var result = _editCheckDetailRepository.All.AsNoTracking().Where(c =>
                c.DeletedDate == null && Ids.Contains(c.EditCheckId)
                 && c.EditCheck.DeletedDate == null)
                .ProjectTo<EditCheckValidateDto>(_mapper.ConfigurationProvider).ToList();


            result.ForEach(r =>
            {
                if (r.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
                {
                    r.ProjectDesignTemplateId = projectDesignTemplateId;
                    var designVariable = _projectDesignVariableRepository.All.AsNoTracking().Where(t => t.Annotation == r.VariableAnnotation && t.ProjectDesignTemplateId == projectDesignTemplateId).FirstOrDefault();
                    if (designVariable != null)
                    {
                        r.ProjectDesignVariableId = designVariable.Id;
                        r.CollectionSource = designVariable.CollectionSource;
                        r.DataType = designVariable.DataType;
                    }

                };
            });

            return result.OrderBy(t => t.EditCheckId).ToList();
        }


        public ScreeningTemplate GetScreeningTemplate(int projectDesignTemplateId, int screeningEntryId, int screeningVisitId)
        {
            return All.AsNoTracking().Where(c =>
                   c.ScreeningVisit.ScreeningEntryId == screeningEntryId
                   && c.ProjectDesignTemplateId == projectDesignTemplateId)
                   .ToList().Where(t => t.ParentId == null && t.ScreeningVisitId == screeningVisitId).FirstOrDefault();
        }

        public ScheduleTemplateDto GetScreeningTemplateId(int projectDesignTemplateId, int screeningEntryId)
        {
            return All.AsNoTracking().Where(c =>
                   c.ScreeningVisit.ScreeningEntryId == screeningEntryId
                   && c.ProjectDesignTemplateId == projectDesignTemplateId
                   && c.ParentId == null).Select(r => new ScheduleTemplateDto
                   {
                       ScreeningTemplateId = r.Id,
                       Status = r.Status
                   }).FirstOrDefault();
        }

        public string GetVariableValue(EditCheckValidateDto editCheckValidateDto)
        {

            var screeningValue = _screeningTemplateValueRepository.All.AsNoTracking().Where(t =>
                         t.ProjectDesignVariableId == editCheckValidateDto.ProjectDesignVariableId
                         && t.ScreeningTemplate.Id == editCheckValidateDto.ScreeningTemplateId).FirstOrDefault();

            if (screeningValue == null) return "";

            var variableValue = screeningValue?.Value;

            if (screeningValue != null && string.IsNullOrEmpty(variableValue) && screeningValue.IsNa)
                return "NA";

            if (screeningValue != null &&
                editCheckValidateDto.CollectionSource == CollectionSources.MultiCheckBox)
                variableValue = GetMultiCheckBox(screeningValue.Id);

            return variableValue;
        }

        public string GetVariableValue(int screeningTemplateId, int projectDesignVariableId)
        {
            var screeningValue = _screeningTemplateValueRepository.All.AsNoTracking().Where(t =>
                         t.ProjectDesignVariableId == projectDesignVariableId
                         && t.ScreeningTemplateId == screeningTemplateId
                         && t.ScreeningTemplate.ParentId == null
                         ).Select(c => c.Value).FirstOrDefault();

            return screeningValue;
        }


        private string GetMultiCheckBox(int id)
        {
            if (id == 0) return "";

            return string.Join(", ", _screeningTemplateValueChildRepository.All.AsNoTracking().
                 Where(t => t.ScreeningTemplateValueId == id
                 && t.Value == "true").Select(a => a.ProjectDesignVariableValueId).ToList());
        }

        public string CollectionValueAnnotation(string collectionValue, CollectionSources? collectionSource)
        {
            if (string.IsNullOrEmpty(collectionValue)) return "";

            if (IsNotDropDown(collectionSource))
                return collectionValue;

            var projectDesignVariableIds = ProjectDesignVariableIds(collectionValue);
            return string.Join(", ", _projectDesignVariableValueRepository.All
                    .AsNoTracking().Where(t => projectDesignVariableIds.Contains(t.Id)).
                    Select(a => a.ValueName).ToList());
        }

        public string ScreeningValueAnnotation(string value, EditCheckRuleBy checkBy, CollectionSources? collectionSource)
        {
            if (string.IsNullOrEmpty(value)) return "";

            if (IsNotDropDown(collectionSource))
                return value;

            if (checkBy == EditCheckRuleBy.ByVariableAnnotation)
                return CollectionValueAnnotation(value, collectionSource);

            var variableValueId = Convert.ToInt32(value);
            return _projectDesignVariableValueRepository.All.Where(x => x.Id == variableValueId).FirstOrDefault()?.ValueName;
        }

        bool IsNotDropDown(CollectionSources? collectionSource)
        {
            return collectionSource == null || collectionSource == CollectionSources.Date ||
              collectionSource == CollectionSources.DateTime ||
              collectionSource == CollectionSources.PartialDate ||
              collectionSource == CollectionSources.TextBox ||
              collectionSource == CollectionSources.Time;
        }

        List<int> ProjectDesignVariableIds(string collectionValue)
        {
            List<int> result = new List<int>();
            if (!string.IsNullOrEmpty(collectionValue))
            {
                collectionValue.Split(",").ForEach(x => { result.Add(Convert.ToInt32(x)); });
            }
            return result;
        }


        public List<ScheduleCheckValidateDto> GetTargetSchedule(int projectDesignTemplateId, bool isQuery)
        {
            var result = _projectScheduleTemplateRepository.All.AsNoTracking().Where(x => x.DeletedDate == null);

            if (isQuery)
                result = result.Where(x => (x.ProjectDesignTemplateId == projectDesignTemplateId ||
                 _projectScheduleRepository.All.AsNoTracking().Any(t => t.ProjectDesignTemplateId == projectDesignTemplateId
                 && t.Id == x.ProjectScheduleId && t.DeletedDate == null)));
            else
                result = result.Where(x => x.ProjectDesignTemplateId == projectDesignTemplateId);

            return result.Select(t => new ScheduleCheckValidateDto
            {
                ProjectScheduleId = t.ProjectScheduleId,
                ProjectScheduleTemplateId = t.Id,
                ProjectDesignVariableId = t.ProjectDesignVariableId,
                ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                Message = t.Message,
                CollectionSource = t.ProjectDesignVariable.CollectionSource,
                HH = t.HH,
                MM = t.MM,
                IsTarget = true,
                PositiveDeviation = t.PositiveDeviation,
                NegativeDeviation = t.NegativeDeviation,
                NoOfDay = t.NoOfDay,
                Operator = t.Operator
            }).ToList();
        }

        public List<ScheduleCheckValidateDto> GetReferenceSchedule(List<int> projectScheduleId)
        {
            return _projectScheduleRepository.All.AsNoTracking().Where(r =>
                projectScheduleId.Contains(r.Id)
                && r.DeletedDate == null).Select(t => new ScheduleCheckValidateDto
                {
                    ProjectScheduleId = t.Id,
                    ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                    ProjectDesignVariableId = t.ProjectDesignVariableId,
                    AutoNumber = t.AutoNumber
                }).ToList();
        }

        public List<ScheduleCheckValidateDto> GetTargetScheduleByVariableId(int ProjectDesignVariableId)
        {
            return _projectScheduleTemplateRepository.All.AsNoTracking().
                Where(x => x.DeletedDate == null && (x.ProjectDesignVariableId == ProjectDesignVariableId ||
                 _projectScheduleRepository.All.Any(t => t.ProjectDesignVariableId == ProjectDesignVariableId
                 && t.Id == x.ProjectScheduleId && t.DeletedDate == null))).Select(t => new ScheduleCheckValidateDto
                 {
                     ProjectScheduleId = t.ProjectScheduleId,
                     ProjectScheduleTemplateId = t.Id,
                     ProjectDesignVariableId = t.ProjectDesignVariableId,
                     ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                     Message = t.Message,
                     CollectionSource = t.ProjectDesignVariable.CollectionSource,
                     HH = t.HH,
                     MM = t.MM,
                     IsTarget = true,
                     PositiveDeviation = t.PositiveDeviation,
                     NegativeDeviation = t.NegativeDeviation,
                     NoOfDay = t.NoOfDay,
                     Operator = t.Operator
                 }).ToList();
        }


    }
}
