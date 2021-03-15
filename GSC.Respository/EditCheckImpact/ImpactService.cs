using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.EditCheck;
using GSC.Respository.Project.Schedule;
using GSC.Respository.Screening;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.EditCheckImpact
{
    public class ImpactService : GenericRespository<ScreeningTemplate>, IImpactService
    {
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IMapper _mapper;
        private readonly IEditCheckDetailRepository _editCheckDetailRepository;
        private readonly IProjectDesignVariableValueRepository _projectDesignVariableValueRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IProjectScheduleRepository _projectScheduleRepository;
        private readonly IScreeningTemplateValueChildRepository _screeningTemplateValueChildRepository;
        private readonly IProjectScheduleTemplateRepository _projectScheduleTemplateRepository;

        public ImpactService(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IProjectDesignVariableValueRepository projectDesignVariableValueRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectScheduleTemplateRepository projectScheduleTemplateRepository,
            IScreeningTemplateValueChildRepository screeningTemplateValueChildRepository,
            IProjectScheduleRepository projectScheduleRepository,
            IEditCheckDetailRepository editCheckDetailRepository) : base(context)
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
                ((x.ProjectDesignTemplateId == screeningTemplateBasic.ProjectDesignTemplateId || x.FetchingProjectDesignTemplateId == screeningTemplateBasic.ProjectDesignTemplateId) ||
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
                                  ProjectDesignVisitId = variable.ProjectDesignTemplate.ProjectDesignVisitId,
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
                                  FetchingProjectDesignTemplateId = checkDetail.FetchingProjectDesignTemplateId,
                                  FetchingProjectDesignVariableId = checkDetail.FetchingProjectDesignVariableId,
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
                    var designVariable = _projectDesignVariableRepository.All.AsNoTracking().Where(t => t.DeletedDate == null && t.Annotation == r.VariableAnnotation && t.ProjectDesignTemplateId == projectDesignTemplateId).FirstOrDefault();
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


        public ScreeningTemplate GetScreeningTemplate(int projectDesignTemplateId, int screeningEntryId, int? screeningVisitId)
        {
            var result = All.AsNoTracking().Where(c =>
                    c.ScreeningVisit.ScreeningEntryId == screeningEntryId
                    && c.ProjectDesignTemplateId == projectDesignTemplateId)
                   .Where(t => t.ParentId == null);

            if (screeningVisitId > 0)
                result = result.Where(x => x.ScreeningVisitId == screeningVisitId);
            else
                result = result.Where(x => x.ScreeningVisit.ParentId == null);

            return result.FirstOrDefault();
        }

        public ScheduleTemplateDto GetScreeningTemplateId(int projectDesignTemplateId, int screeningEntryId)
        {
            return All.AsNoTracking().Where(c =>
                   c.ScreeningVisit.ScreeningEntryId == screeningEntryId
                   && c.ProjectDesignTemplateId == projectDesignTemplateId
                   && c.ParentId == null).Select(r => new ScheduleTemplateDto
                   {
                       ScreeningTemplateId = r.Id,
                       Status = r.Status,
                       ProjectDesignTemplateId = r.ProjectDesignTemplateId,
                       ScreeningVisitId = r.ScreeningVisitId
                   }).FirstOrDefault();
        }

        public string GetVariableValue(EditCheckValidateDto editCheckValidateDto, out bool isNa)
        {

            var screeningValue = _screeningTemplateValueRepository.All.AsNoTracking().Where(t =>
                         t.ProjectDesignVariableId == editCheckValidateDto.ProjectDesignVariableId
                         && t.ScreeningTemplate.Id == editCheckValidateDto.ScreeningTemplateId).FirstOrDefault();
            isNa = false;
            if (screeningValue == null) return "";

            var variableValue = screeningValue?.Value;
            isNa = screeningValue.IsNa;
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
                         && t.ScreeningTemplateId == screeningTemplateId).Select(c => c.Value).FirstOrDefault();

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

        public int CollectionValue(string id)
        {
            int projectDesignVariableValueId;

            int.TryParse(id, out projectDesignVariableValueId);

            var result = _projectDesignVariableValueRepository.All.
                Where(x => x.Id == projectDesignVariableValueId).Select(t => t.ValueName).FirstOrDefault();

            int value;

            int.TryParse(result, out value);

            return value;
        }

        public string GetProjectDesignVariableId(int projectDesignVariableId, string collectionSource)
        {

            var variableCollectionSource = _projectDesignVariableRepository.All.Where(x => x.Id == projectDesignVariableId && x.DeletedDate == null).Select(t => t.CollectionSource).FirstOrDefault();

            if (IsNotDropDown(variableCollectionSource))
                return collectionSource;

            int projectDesignVariableValueId;

            int.TryParse(collectionSource, out projectDesignVariableValueId);

            var value = _projectDesignVariableValueRepository.All.Where(x => x.Id == projectDesignVariableValueId).Select(t => t.ValueName).FirstOrDefault();

            projectDesignVariableValueId = _projectDesignVariableValueRepository.All.Where(x => x.ProjectDesignVariableId == projectDesignVariableId && x.ValueName == value && x.DeletedDate == null).Select(t => t.Id).FirstOrDefault();

            return projectDesignVariableValueId.ToString();

        }

        public string ScreeningValueAnnotation(string value, EditCheckRuleBy checkBy, CollectionSources? collectionSource)
        {
            if (string.IsNullOrEmpty(value)) return "";

            if (IsNotDropDown(collectionSource))
                return value;

            if (checkBy == EditCheckRuleBy.ByVariableAnnotation)
                return CollectionValueAnnotation(value, collectionSource);

            var variableValueId = Convert.ToInt32(value);
            return _projectDesignVariableValueRepository.All.Where(x => x.Id == variableValueId).Select(t => t.ValueName).FirstOrDefault();
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
                collectionValue.Split(",").ToList().ForEach(x => { result.Add(Convert.ToInt32(x)); });
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


        public bool CheckReferenceVariable(int projectDesignVariableId)
        {
            return _projectScheduleRepository.All.AsNoTracking().Any(r => r.ProjectDesignVariableId == projectDesignVariableId && r.DeletedDate == null);
        }

        public List<ScheduleCheckValidateDto> GetTargetScheduleByVariableId(int ProjectDesignVariableId)
        {
            var result = _projectScheduleTemplateRepository.All.AsNoTracking().
                Where(x => x.DeletedDate == null && 
                (x.ProjectDesignVariableId == ProjectDesignVariableId || x.ProjectSchedule.ProjectDesignVariableId == ProjectDesignVariableId)).Select(t => new ScheduleCheckValidateDto
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

            //if (result.Count == 0)
            //{
            //    result = _projectScheduleTemplateRepository.All.AsNoTracking().
            //    Where(x => x.DeletedDate == null && x.ProjectSchedule.DeletedDate == null
            //    && x.ProjectSchedule.ProjectDesignVariableId == ProjectDesignVariableId).Select(t => new ScheduleCheckValidateDto
            //    {
            //        ProjectScheduleId = t.ProjectScheduleId,
            //        ProjectScheduleTemplateId = t.Id,
            //        ProjectDesignVariableId = t.ProjectDesignVariableId,
            //        ProjectDesignTemplateId = t.ProjectDesignTemplateId,
            //        Message = t.Message,
            //        CollectionSource = t.ProjectDesignVariable.CollectionSource,
            //        HH = t.HH,
            //        MM = t.MM,
            //        IsTarget = true,
            //        PositiveDeviation = t.PositiveDeviation,
            //        NegativeDeviation = t.NegativeDeviation,
            //        NoOfDay = t.NoOfDay,
            //        Operator = t.Operator
            //    }).ToList();

            //}

            return result;
        }




    }
}
