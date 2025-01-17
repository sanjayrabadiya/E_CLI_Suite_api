﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.ModelBinding;
using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Project.Design
{

    public class ProjectDesignVariableRepository : GenericRespository<ProjectDesignVariable>,
        IProjectDesignVariableRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly ISupplyManagementAllocationRepository _supplyManagementAllocationRepository;
        private readonly ISupplyManagementFactorMappingRepository _supplyManagementFactorMappingRepository;
        public ProjectDesignVariableRepository(IGSCContext context, IMapper mapper, IStudyVersionRepository studyVersionRepository, 
            ISupplyManagementAllocationRepository supplyManagementAllocationRepository,
            ISupplyManagementFactorMappingRepository supplyManagementFactorMappingRepository) : base(context)
        {
            _mapper = mapper;
            _context = context;
            _supplyManagementAllocationRepository = supplyManagementAllocationRepository;
            _supplyManagementFactorMappingRepository = supplyManagementFactorMappingRepository;
        }



        public IList<DropDownVaribleDto> GetVariabeAnnotationDropDown(int projectDesignTemplateId, bool isFormula)
        {
            var result = All.Where(x => x.DeletedDate == null
                                  && x.ProjectDesignTemplateId == projectDesignTemplateId);

            if (isFormula)
                result = result.Where(x => (x.CollectionSource == CollectionSources.TextBox && x.DataType != DataType.Character
                || x.CollectionSource == CollectionSources.Time || x.CollectionSource == CollectionSources.NumericScale
                || x.CollectionSource == CollectionSources.HorizontalScale));

            return result.OrderBy(o => o.DesignOrder).Select(c => new DropDownVaribleDto
            {
                Id = c.Id,
                Value = c.DesignOrder + ". " + c.VariableName +
                                         Convert.ToString(string.IsNullOrEmpty(c.Annotation) ? "" : " [" + c.Annotation + "]"),
                Code = c.Annotation,
                DataType = c.DataType,
                CollectionSources = c.CollectionSource,
                ExtraData = isFormula ? null : _mapper.Map<List<ProjectDesignVariableValueDropDown>>(c.Values.Where(x => x.DeletedDate == null).ToList()),
                InActive = c.InActiveVersion != null
            }).ToList();

        }

        public IList<DropDownVaribleDto> GetVariabeAnnotationDropDownforhardsoftfetch(int projectDesignTemplateId, int variableId)
        {
            var result = All.Where(x => x.DeletedDate == null
                                  && x.ProjectDesignTemplateId == projectDesignTemplateId);

            var data = result.OrderBy(o => o.DesignOrder).Select(c => new DropDownVaribleDto
            {
                Id = c.Id,
                Value = c.DesignOrder + ". " + c.VariableName +
                                        Convert.ToString(string.IsNullOrEmpty(c.Annotation) ? "" : " [" + c.Annotation + "]"),
                Code = c.Annotation,
                DataType = c.DataType,
                CollectionSources = c.CollectionSource,
                ExtraData = _mapper.Map<List<ProjectDesignVariableValueDropDown>>(c.Values.Where(x => x.DeletedDate == null).ToList()),
                InActive = c.InActiveVersion != null
            }).Where(x => x.Id != variableId).ToList();           
            return data;

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
                Id = s.First().Id,
                Value = s.Key.Value,
                Code = s.Key.Code,
                DataType = s.Key.DataType,
                CollectionSources = s.Key.CollectionSources,
                ListOfVariable = result.Where(x => x.Value == s.Key.Value).ToList(),
            }).ToList();

            return grpresult;
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


        public IList<ProjectDesignVariableBasicDto> GetVariabeBasic(int projectDesignTemplateId, CheckVersionDto checkVersion)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == projectDesignTemplateId)
                .Select(c => new ProjectDesignVariableBasicDto
                {
                    Id = c.Id,
                    Value = c.VariableName,
                    InActiveVersion = c.InActiveVersion,
                    StudyVersion = c.StudyVersion,
                    DesignOrder = c.DesignOrder,
                    DisplayVersion = c.StudyVersion > 0 && c.InActiveVersion > 0 ?
                    $"(V : {c.StudyVersion} - {c.InActiveVersion})" : c.StudyVersion > 0 ? $"(N : {c.StudyVersion})" :
                    c.InActiveVersion != null ? $"(D : {c.InActiveVersion})" : "",
                    AllowActive = checkVersion.VersionNumber == c.InActiveVersion && c.InActiveVersion != null
                }).OrderBy(o => o.DesignOrder).ToList();
        }



        public string Duplicate(ProjectDesignVariable objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.VariableCode == objSave.VariableCode &&
                x.ProjectDesignTemplateId == objSave.ProjectDesignTemplateId && x.DeletedDate == null))
                return "Duplicate Variable code : " + objSave.VariableCode;


            if (All.Any(x =>
                x.Id != objSave.Id && x.DomainId == objSave.DomainId &&
                x.ProjectDesignTemplateId == objSave.ProjectDesignTemplateId && x.Annotation == objSave.Annotation &&
                !string.IsNullOrEmpty(x.Annotation) && x.DeletedDate == null))
                return "Duplicate Variable Annotation: " + objSave.Annotation;

            if (All.Any(x => x.Id != objSave.Id && x.DomainId == objSave.DomainId &&
                             x.ProjectDesignTemplateId == objSave.ProjectDesignTemplateId &&
                             x.VariableAlias == objSave.VariableAlias && !string.IsNullOrEmpty(x.VariableAlias) &&
                             x.DeletedDate == null)) return "Duplicate Variable Alias: " + objSave.VariableAlias;

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
                result = result.Where(x => (x.CollectionSource == CollectionSources.TextBox && x.DataType != DataType.Character) || x.CollectionSource == CollectionSources.HorizontalScale || x.CollectionSource == CollectionSources.NumericScale);

            var variableResult = result.Select(c => new DropDownVaribleDto
            {
                Value = c.Annotation,
                Code = c.Annotation,
                DataType = c.DataType,
                VisitName = c.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName + "." + c.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                CollectionSources = c.CollectionSource,
                ExtraData = isFormula ? null : _mapper.Map<List<ProjectDesignVariableValueDropDown>>(c.Values.Where(x => x.DeletedDate == null).ToList())
            }).ToList();

            return variableResult.GroupBy(x => new { x.Value, x.Code, x.DataType, x.CollectionSources }).Select
            (c => new DropDownVaribleDto
            {
                Value = c.Key.Value,
                Code = c.Key.Code,
                DataType = c.Key.DataType,
                CollectionSources = c.Key.CollectionSources,
                VisitName = string.Join(", ", variableResult.Where(v => v.Value == c.Key.Value).Select(r => r.VisitName).ToList()),
                ExtraData = isFormula ? null : variableResult.Find(v => v.Value == c.Key.Value)?.ExtraData
            }).Where(x => x.Value != null).ToList();
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
                          .Contains(c.Id) && (c.CollectionSource == CollectionSources.Date || c.CollectionSource == CollectionSources.DateTime)
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

        //Added method By Vipul for Get Variable By Multiple Template DropDown
        public IList<DropDownDto> GetVariableByMultipleTemplateDropDown(int?[] templateIds)
        {
            // change  on 5/6/2023 version impact
            var result = All.Where(x => x.DeletedDate == null && templateIds.Contains(x.ProjectDesignTemplateId) && x.InActiveVersion == null);
            return result.OrderBy(o => o.DesignOrder).Select(c => new DropDownDto
            {
                Id = c.Id,
                Value = c.VariableName
            }).ToList();
        }

        public ProjectDesignVariableRelationDto GetProjectDesignVariableRelation(int id)
        {
            return All.Where(x => x.Id == id).Select(t => new ProjectDesignVariableRelationDto
            {
                Id = t.Id,
                RelationProjectDesignVariableId = t.Id,
                ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                ProjectDesignVisitId = t.ProjectDesignTemplate.ProjectDesignVisitId
            }).FirstOrDefault();

        }

        public IList<DropDownDto> GetVariabeDropDownForLabManagementMapping(int projectDesignTemplateId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == projectDesignTemplateId && (x.CollectionSource == CollectionSources.TextBox
            || x.CollectionSource == CollectionSources.MultilineTextBox || x.CollectionSource == CollectionSources.Date))

                //&& x.CollectionSource != CollectionSources.MultiCheckBox && x.CollectionSource != CollectionSources.NumericScale
                //&& x.CollectionSource != CollectionSources.RadioButton && x.CollectionSource != CollectionSources.Relation)
                .OrderBy(o => o.DesignOrder)
                .Select(c => new DropDownDto
                {
                    Id = c.Id,
                    Value = c.VariableName,
                    Code = c.CoreVariableType.ToString(),
                    ExtraData = c.DesignOrder
                }).OrderBy(o => o.ExtraData).ToList();
        }

        public IList<DropDownDto> GetVariabeDropDownForRelationMapping(int projectDesignTemplateId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == projectDesignTemplateId && (x.CollectionSource == CollectionSources.TextBox
            || x.CollectionSource == CollectionSources.MultilineTextBox || x.CollectionSource == CollectionSources.Date || x.CollectionSource == CollectionSources.Time
            || x.CollectionSource == CollectionSources.DateTime || x.CollectionSource == CollectionSources.PartialDate || x.CollectionSource == CollectionSources.HorizontalScale))

                //&& x.CollectionSource != CollectionSources.MultiCheckBox && x.CollectionSource != CollectionSources.NumericScale
                //&& x.CollectionSource != CollectionSources.RadioButton && x.CollectionSource != CollectionSources.Relation)
                .OrderBy(o => o.DesignOrder)
                .Select(c => new DropDownDto
                {
                    Id = c.Id,
                    Value = c.VariableName,
                    Code = c.CoreVariableType.ToString(),
                    ExtraData = c.DesignOrder
                }).OrderBy(o => o.ExtraData).ToList();
        }

        public string NonChangeVariableCode(ProjectDesignVariableDto variable)
        {
            string[] codes = { "V003", "001", "V004", "SAE003", "SAE001", "SAE002", "Cd001", "Dev001", "Disc001", "DiscR001" };

            if (variable.Id != 0)
            {
                var varriable = Find(variable.Id);
                bool a = Array.Exists(codes, element => element == varriable.VariableCode);

                if (a)
                {
                    if (Array.Exists(codes, element => element == variable.VariableCode))
                        return "";
                    else
                        return "Can't edit record with this variable code!";
                }
                return "";
            }
            else
            {
                if (Array.Exists(codes, element => element == variable.VariableCode))
                    return "Can't add record with this variable code!";
                else
                    return "";
            }
        }

        public string ValidationIWRS(ProjectDesignVariable variable)
        {
            if (variable.InActiveVersion == null && _supplyManagementAllocationRepository.All.Any(x => x.DeletedDate == null && x.ProjectDesignVariableId == variable.Id))
            {
                return "Can't edit record, Already used in Allocation!";
            }
            if (variable.InActiveVersion == null && _supplyManagementFactorMappingRepository.All.Any(x => x.DeletedDate == null && x.ProjectDesignVariableId == variable.Id))
            {
                return "Can't edit record, Already used in Fector Mapping!";
            }
            return "";
        }
    }
}