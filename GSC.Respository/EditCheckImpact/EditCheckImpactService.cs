using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
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
    public class EditCheckImpactService : IEditCheckImpactService
    {
        private readonly ISchedulerRuleRespository _schedulerRuleRespository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IScreeningTemplateValueEditCheckRepository _screeningTemplateValueEditCheckRepository;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IMapper _mapper;
        private readonly IEditCheckDetailRepository _editCheckDetailRepository;
        private readonly IProjectDesignVariableValueRepository _projectDesignVariableValueRepository;
        //private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        public EditCheckImpactService(IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IScreeningTemplateValueEditCheckRepository screeningTemplateValueEditCheckRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            ISchedulerRuleRespository schedulerRuleRespository,
            //IScreeningTemplateRepository screeningTemplateRepository,
            IProjectDesignVariableValueRepository projectDesignVariableValueRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IEditCheckDetailRepository editCheckDetailRepository)
        {
            _screeningTemplateValueEditCheckRepository = screeningTemplateValueEditCheckRepository;
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _schedulerRuleRespository = schedulerRuleRespository;
            _mapper = mapper;
            _uow = uow;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _editCheckDetailRepository = editCheckDetailRepository;
            //_screeningTemplateRepository = screeningTemplateRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _projectDesignVariableValueRepository = projectDesignVariableValueRepository;
        }
        public List<EditCheckValidateDto> GetEditCheck(ScreeningTemplate screeningTemplate, int projectDesignId, int domainId)
        {
            var result = _editCheckDetailRepository.All.AsNoTracking().Where(c =>
                  _editCheckDetailRepository.All.AsNoTracking().Any(x =>
                  x.ProjectDesignTemplateId == screeningTemplate.ProjectDesignTemplateId &&
                  (x.EditCheck.ProjectDesignId == projectDesignId || x.DomainId == domainId)
                  && x.EditCheckId == c.EditCheckId)
                  && c.CheckBy != EditCheckRuleBy.ByVariableAnnotation)
                .ProjectTo<EditCheckValidateDto>(_mapper.ConfigurationProvider)
                .ToList();

            var annotation = (from variable in _projectDesignVariableRepository.All.AsNoTracking().Where(t =>
                   t.ProjectDesignTemplate.Id == screeningTemplate.ProjectDesignTemplateId)
                              join checkDetail in _editCheckDetailRepository.All on new { ann = variable.Annotation, designId = projectDesignId } equals new { ann = checkDetail.VariableAnnotation, designId = checkDetail.EditCheck.ProjectDesignId }
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
                                  DeletedDate = checkDetail.EditCheck.DeletedDate ?? checkDetail.DeletedDate
                              }).ToList().Distinct().ToList();

            if (annotation != null)
                result.AddRange(annotation);

            return result;

        }

        public ScreeningTemplate GetScreeningTemplate(int projectDesignTemplateId, int screeningEntryId)
        {
            return _uow.Context.ScreeningTemplate.FirstOrDefault(c =>
                   c.ScreeningEntryId == screeningEntryId
                   && c.ProjectDesignTemplateId == projectDesignTemplateId
                   && c.ParentId == null);
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
        private string GetMultiCheckBox(int id)
        {
            if (id == 0) return "";

            return string.Join(", ", _uow.Context.ScreeningTemplateValueChild.
                 Where(t => t.ScreeningTemplateValueId == id
                 && t.Value == "true").Select(a => a.ProjectDesignVariableValueId).ToList());
        }

        public string CollectionValueAnnotation(string collectionValue)
        {
            if (string.IsNullOrEmpty(collectionValue)) return "";

            return string.Join(", ", _projectDesignVariableValueRepository.All
                    .Where(t => ProjectDesignVariableIds(collectionValue).Contains(t.Id)).
                    Select(a => a.ValueName).ToList());
        }

        public string ScreeningValueAnnotation(string value, EditCheckRuleBy checkBy)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (checkBy == EditCheckRuleBy.ByVariableAnnotation)
                return CollectionValueAnnotation(value);

            var variableValueId = Convert.ToInt32(value);
            return _projectDesignVariableValueRepository.Find(variableValueId)?.ValueName;
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

    }
}
