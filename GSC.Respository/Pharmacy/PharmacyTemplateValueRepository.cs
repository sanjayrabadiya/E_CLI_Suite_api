using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Pharmacy;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Pharmacy
{
    public class PharmacyTemplateValueRepository : GenericRespository<PharmacyTemplateValue, GscContext>,
        IPharmacyTemplateValueRepository
    {
        public PharmacyTemplateValueRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser
        )
            : base(uow, jwtTokenAccesser)
        {
        }

        //public void UpdateVariableOnSubmit(int projectDesignTemplateId, int pharmacyTemplateId)
        //{
        //    var screeningVariable = FindBy(x => x.DeletedDate == null && x.PharmacyTemplateId == pharmacyTemplateId).ToList();

        //    var templateVariable = _projectDesignVariableRepository.
        //        FindBy(t => t.ProjectDesignTemplateId == projectDesignTemplateId).ToList().
        //        Where(x => !screeningVariable.Any(a => a.ProjectDesignVariableId == x.Id)).ToList();

        //    foreach (var variable in templateVariable)
        //    {
        //        this.Add(new PharmacyTemplateValue
        //        {
        //            PharmacyTemplateId = pharmacyTemplateId,
        //            ProjectDesignVariableId = variable.Id
        //        });
        //    }
        //}

        //public QueryStatusDto GetQueryStatusCount(int pharmacyTemplateId)
        //{
        //    var result = All.Where(x => x.DeletedDate == null
        //    && x.PharmacyTemplateId == pharmacyTemplateId).ToList();
        //    if (result != null)
        //    {
        //        return GetQueryStatusByModel(result, pharmacyTemplateId);
        //    }

        //    return null;

        //}

        //public QueryStatusDto GetQueryStatusByModel(List<PharmacyTemplateValue> pharmacyTemplateValue, int pharmacyTemplateId)
        //{
        //    if (pharmacyTemplateValue == null) return null;

        //    var result = pharmacyTemplateValue.Where(x => x.PharmacyTemplateId == pharmacyTemplateId &&
        //    x.QueryStatus != QueryStatus.Closed && x.QueryStatus != QueryStatus.SelfCorrection && x.QueryStatus != null).ToList();
        //    if (result != null && result.Count>0)
        //    {
        //        QueryStatusDto queryStatusDto = new QueryStatusDto();
        //        queryStatusDto.Items = result.GroupBy(r => r.QueryStatus).
        //        Select(t => new QueryStatusCount { QueryStatus = ((QueryStatus)t.Key).GetDescription(), Total = t.Count() }).ToList();
        //        queryStatusDto.TotalQuery = queryStatusDto.Items.Sum(x => x.Total);
        //        return queryStatusDto;
        //    }

        //    return null;

        //}

        public List<PharmacyTemplateValueDto> GetPharmacyTemplateTree(int pharmacyEntryId)
        {
            return All.Where(s => s.PharmacyEntryId == pharmacyEntryId && s.DeletedDate == null).Select(s =>
                new PharmacyTemplateValueDto
                {
                    Id = s.Id,
                    PharmacyEntryId = s.PharmacyEntryId,
                    VariableId = s.VariableId,
                    Status = s.Status,
                    StatusName = Enum.GetName(typeof(IsFormType), s.Status),
                    // VariableName = s.Variables.VariableName,
                    Value = s.Value
                    //ReviewLevel = s.ReviewLevel,
                    //AcknowledgeLevel = s.AcknowledgeLevel,
                    //MyReview = reviewLevel == s.ReviewLevel,
                    //TemplateQueryStatus = _screeningTemplateValueRepository.GetQueryStatusByModel(templateValues, s.Id),
                    //IsRepeated = parentId == null ? s.ProjectDesignTemplate.IsRepeated : false,
                    //Children = s.ParentId != null ? GetTemplateTree(screeningEntryId, s.ParentId, templateValues, reviewLevel) : null
                }).OrderBy(o => o.VariableName).ToList();
        }

        public PharmacyTemplateValue SaveValue(PharmacyTemplateValue pharmacyTemplateValue)
        {
            if (pharmacyTemplateValue == null)
                return null;

            var result = All.FirstOrDefault(x => x.PharmacyEntryId == pharmacyTemplateValue.PharmacyEntryId
                                                 && x.VariableId == pharmacyTemplateValue.VariableId
                                                 && x.Value == pharmacyTemplateValue.Value
                                                 && x.Status == pharmacyTemplateValue.Status
                //&& x.ReviewLevel == pharmacyTemplateValue.ReviewLevel
                //&& x.AcknowledgeLevel == pharmacyTemplateValue.AcknowledgeLevel
            );
            if (result != null)
                return result;

            return pharmacyTemplateValue;
        }

        public VariableDto GetPharmacyVariable(VariableDto designVariableDto, int pharmacyEntryId)
        {
            //designTemplateDto.StatusName = getStatusName(screeningTemplateObject, workflowlevel.LevelNo == screeningTemplateObject.ReviewLevel);
            var values = Context.PharmacyTemplateValue.Where(t => t.PharmacyEntryId == pharmacyEntryId).ToList();
            values.ForEach(t =>
            {
                var variable = designVariableDto;
                //var variable = designVariableDto..FirstOrDefault(v => v.Id == t.ProjectDesignVariableId);
                //var variable = Context.Variable.Where(x => x.Id == pharmacyTemplate.VariableId).FirstOrDefault();
                if (variable != null)
                    variable.Id = t.Id;
                //variable.VariableName = t.;
                //variable.ScreeningTemplateValueId = t.Id;
                //variable.QueryStatus = t.QueryStatus;
                //variable.HasComments = t.Comments.Any();
                //variable.HasQueries = t.Queries.Any();

                //variable.WorkFlowButton = SetWorkFlowButton(t, workflowlevel, designTemplateDto, screeningTemplateObject);

                //variable.DocPath = t.DocPath != null ? documentUrl + t.DocPath : null;
                //if (!string.IsNullOrWhiteSpace(variable.ScreeningValue))
                //{
                //    variable.IsValid = true;
                //}

                //if (variable.Values != null)
                //{
                //    variable.Values.ForEach(val =>
                //    {
                //        var childValue = t.Children.FirstOrDefault(v => v.ProjectDesignVariableValueId == val.Id);
                //        if (childValue != null)
                //        {
                //            variable.IsValid = true;
                //            val.ScreeningValue = childValue.Value;
                //            val.ScreeningValueOld = childValue.Value;
                //            val.ScreeningTemplateValueChildId = childValue.Id;
                //        }
                //    });
                //}
            });

            //designVariableDto.Variables.ForEach(t =>
            //{

            //    if (t.ScreeningValue == null)
            //    {
            //        t.ScreeningValue = t.DefaultValue;
            //    }
            //});

            return designVariableDto;
        }
    }
}