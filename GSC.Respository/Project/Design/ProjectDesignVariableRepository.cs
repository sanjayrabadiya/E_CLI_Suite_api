using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Project.Design
{

    public class ProjectDesignVariableRepository : GenericRespository<ProjectDesignVariable>,
        IProjectDesignVariableRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public ProjectDesignVariableRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _mapper = mapper;
            _context = context;
        }

        public IList<DropDownDto> GetVariabeDropDown(int projectDesignTemplateId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == projectDesignTemplateId)
                .OrderBy(o => o.DesignOrder)
                .Select(c => new DropDownDto
                {
                    Id = c.Id,
                    Value = c.VariableName,
                    Code = c.CoreVariableType.ToString(),
                    ExtraData = c.DesignOrder
                }).OrderBy(o => o.ExtraData).ToList();
        }

        public IList<DropDownVaribleDto> GetVariabeAnnotationDropDown(int projectDesignTemplateId, bool isFormula)
        {
            var result = All.Where(x => x.DeletedDate == null
                                  && x.ProjectDesignTemplateId == projectDesignTemplateId);

            if (isFormula)
                result = result.Where(x => (x.CollectionSource == CollectionSources.TextBox && x.DataType != DataType.Character) || (x.CollectionSource == CollectionSources.NumericScale));

            return result.OrderBy(o => o.DesignOrder).Select(c => new DropDownVaribleDto
            {
                Id = c.Id,
                Value = c.VariableName +
                                         Convert.ToString(string.IsNullOrEmpty(c.Annotation) ? "" : " [" + c.Annotation + "]"),
                Code = c.Annotation,
                DataType = c.DataType,
                CollectionSources = c.CollectionSource,
                ExtraData = _mapper.Map<List<ProjectDesignVariableValueDropDown>>(c.Values.Where(x => x.DeletedDate == null).ToList())
            }).ToList();

        }

        public IList<DropDownVaribleAnnotationDto> GetVariabeAnnotationByDomainDropDown(int domainId, int projectId)
        {
            var result = All.Where(x => x.DeletedDate == null
                                      && x.DomainId == domainId
                                      && x.ProjectDesignTemplate.DeletedDate == null &&
                                    x.ProjectDesignTemplate.ProjectDesignVisit.DeletedDate == null &&
                                    x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DeletedDate == null &&
                                    x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
                                      ).OrderBy(o => o.DesignOrder)
                    .Select(c => new DropDownVaribleAnnotationDto
                    {
                        Id = c.Id,
                        Value = c.Annotation,
                        Code = c.Annotation,
                        DataType = c.DataType,
                        CollectionSources = c.CollectionSource,
                        ExtraData = c.Values.Where(x => x.DeletedDate == null).ToList(),
                    }).Where(x => !String.IsNullOrEmpty(x.Value)).ToList();
            var grpresult = result.GroupBy(x => new { x.Value, x.Code, x.DataType, x.CollectionSources }).Select(s => new DropDownVaribleAnnotationDto
            {
                Id = s.FirstOrDefault().Id,
                Value = s.Key.Value,
                Code = s.Key.Code,
                DataType = s.Key.DataType,
                CollectionSources = s.Key.CollectionSources,
                ListOfVariable = result.Where(x => x.Value == s.Key.Value).ToList(),
            }).ToList();

            return grpresult;
        }

        //Not Use in front please check and remove if not use comment  by vipul
        public IList<DropDownVaribleDto> GetTargetVariabeAnnotationDropDown(int projectDesignTemplateId)
        {
            var query =
                from c in _context.ProjectDesignVariable
                where c.ProjectDesignTemplateId == projectDesignTemplateId &&
                      !(from o in _context.ProjectScheduleTemplate
                        where o.DeletedDate == null
                        select o.ProjectDesignVariableId)
                          .Contains(c.Id)
                select c;


            return query.Where(x => x.DeletedDate == null
                                    && x.ProjectDesignTemplateId == projectDesignTemplateId).OrderBy(o => o.DesignOrder)
                .Select(c => new DropDownVaribleDto
                {
                    Id = c.Id,
                    Value = c.VariableName +
                            Convert.ToString(string.IsNullOrEmpty(c.Annotation) ? "" : " [" + c.Annotation + "]"),
                    Code = c.Annotation,
                    DataType = c.DataType,
                    CollectionSources = c.CollectionSource,
                    ExtraData = _mapper.Map<List<ProjectDesignVariableValueDropDown>>(c.Values.Where(x => x.DeletedDate == null).ToList())
                }).ToList();
        }


        public IList<DropDownDto> GetVariabeAnnotationDropDownForProjectDesign(int projectDesignTemplateId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == projectDesignTemplateId)
                .OrderBy(o => o.DesignOrder)
                .Select(c => new DropDownDto
                {
                    Id = c.Id,
                    Value = c.VariableName,
                    Code = c.CoreVariableType.ToString(),
                    ExtraData = c.DesignOrder
                }).OrderBy(o => o.ExtraData).ToList();
        }


        public string Duplicate(ProjectDesignVariable objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.VariableCode == objSave.VariableCode &&
                x.ProjectDesignTemplateId == objSave.ProjectDesignTemplateId && x.DeletedDate == null))
                return "Duplicate Variable code : " + objSave.VariableCode;

            if (All.Any(x => x.Id != objSave.Id && x.VariableName == objSave.VariableName &&
                             x.ProjectDesignTemplateId == objSave.ProjectDesignTemplateId &&
                             x.DomainId == objSave.DomainId && x.AnnotationTypeId == objSave.AnnotationTypeId &&
                             x.DeletedDate == null))
                return "Duplicate Record : " + objSave.VariableName;

            if (All.Any(x =>
                x.Id != objSave.Id && x.DomainId == objSave.DomainId &&
                x.ProjectDesignTemplateId == objSave.ProjectDesignTemplateId && x.Annotation == objSave.Annotation &&
                !string.IsNullOrEmpty(x.Annotation) && x.DeletedDate == null))
                return "Duplicate Variable Annotation: " + objSave.Annotation;

            if (All.Any(x => x.Id != objSave.Id && x.DomainId == objSave.DomainId &&
                             x.ProjectDesignTemplateId == objSave.ProjectDesignTemplateId &&
                             x.VariableAlias == objSave.VariableAlias && !string.IsNullOrEmpty(x.VariableAlias) &&
                             x.DeletedDate == null)) return "Duplicate Variable Alias: " + objSave.VariableAlias;

            //if (All.Any(x => x.Id != objSave.Id && x.VariableAlias == objSave.VariableAlias && x.DeletedDate == null))
            //{
            //    return "Duplicate Variable alias : " + objSave.VariableAlias;
            //}

            return "";
        }

        public IList<DropDownVaribleDto> GetAnnotationDropDown(int projectDesignId, bool isFormula)
        {
            var result = All.Where(x => x.DeletedDate == null &&
                                        x.ProjectDesignTemplate.DeletedDate == null &&
                                        x.ProjectDesignTemplate.ProjectDesignVisit.DeletedDate == null &&
                                        x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DeletedDate ==
                                        null &&
                                        x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod
                                            .ProjectDesignId == projectDesignId);

            if (isFormula)
                result = result.Where(x => (x.CollectionSource == CollectionSources.TextBox && x.DataType != DataType.Character) || (x.CollectionSource == CollectionSources.NumericScale));

            var variableResult = result.Select(c => new DropDownVaribleDto
            {
                Value = c.Annotation,
                Code = c.Annotation,
                DataType = c.DataType,
                VisitName = c.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName + "." + c.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                CollectionSources = c.CollectionSource,
                ExtraData = _mapper.Map<List<ProjectDesignVariableValueDropDown>>(c.Values.Where(x => x.DeletedDate == null).ToList())
            }).ToList();

            return variableResult.GroupBy(x => new { x.Value, x.Code, x.DataType, x.CollectionSources }).Select
            (c => new DropDownVaribleDto
            {
                Value = c.Key.Value,
                Code = c.Key.Code,
                DataType = c.Key.DataType,
                CollectionSources = c.Key.CollectionSources,
                VisitName = string.Join(", ", variableResult.Where(v => v.Value == c.Key.Value).Select(r => r.VisitName).ToList()),
                ExtraData = variableResult.FirstOrDefault(v => v.Value == c.Key.Value)?.ExtraData
            }).ToList();
        }

        //Added method By Vipul 25062020
        public IList<DropDownVaribleDto> GetTargetVariabeAnnotationForScheduleDropDown(int projectDesignTemplateId)
        {
            var query =
                from c in _context.ProjectDesignVariable
                where c.ProjectDesignTemplateId == projectDesignTemplateId &&
                      !(from o in _context.ProjectScheduleTemplate
                        where o.DeletedDate == null
                        select o.ProjectDesignVariableId)
                          .Contains(c.Id) && (c.CollectionSource == CollectionSources.Date || c.CollectionSource == CollectionSources.Time || c.CollectionSource == CollectionSources.DateTime)
                select c;


            return query.Where(x => x.DeletedDate == null
                                    && x.ProjectDesignTemplateId == projectDesignTemplateId).OrderBy(o => o.DesignOrder)
                .Select(c => new DropDownVaribleDto
                {
                    Id = c.Id,
                    Value = c.VariableName +
                            Convert.ToString(string.IsNullOrEmpty(c.Annotation) ? "" : " [" + c.Annotation + "]"),
                    Code = c.Annotation,
                    DataType = c.DataType,
                    CollectionSources = c.CollectionSource,
                    ExtraData = _mapper.Map<List<ProjectDesignVariableValueDropDown>>(c.Values.Where(x => x.DeletedDate == null).ToList())
                }).ToList();
        }

        //Added method By Vipul 22092020 for visit status in project design get only date and datetime variable
        public IList<DropDownVaribleDto> GetVariabeAnnotationDropDownForVisitStatus(int projectDesignTemplateId)
        {
            var result = All.Where(x => x.DeletedDate == null
                                  && x.ProjectDesignTemplateId == projectDesignTemplateId &&
                                  (x.CollectionSource == CollectionSources.Date || x.CollectionSource == CollectionSources.DateTime));

            return result.OrderBy(o => o.DesignOrder).Select(c => new DropDownVaribleDto
            {
                Id = c.Id,
                Value = c.VariableName +
                                         Convert.ToString(string.IsNullOrEmpty(c.Annotation) ? "" : " [" + c.Annotation + "]"),
                Code = c.Annotation,
                DataType = c.DataType,
                CollectionSources = c.CollectionSource
            }).ToList();
        }
    }
}