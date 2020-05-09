using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Pharmacy;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Pharmacy
{
    public class PharmacyVerificationTemplateValueRepository :
        GenericRespository<PharmacyVerificationTemplateValue, GscContext>, IPharmacyVerificationTemplateValueRepository
    {
        public PharmacyVerificationTemplateValueRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser
        )
            : base(uow, jwtTokenAccesser)
        {
        }

        public List<PharmacyVerificationTemplateValueDto> GetPharmacyVerificationTemplateTree(
            int pharmacyVerificationEntryId)
        {
            return All.Where(s => s.PharmacyVerificationEntryId == pharmacyVerificationEntryId && s.DeletedDate == null)
                .Select(s => new PharmacyVerificationTemplateValueDto
                {
                    Id = s.Id,
                    PharmacyVerificationEntryId = s.PharmacyVerificationEntryId,
                    VariableId = s.VariableId,
                    Status = s.Status,
                    StatusName = Enum.GetName(typeof(IsFormType), s.Status),
                    Value = s.Value
                }).OrderBy(o => o.VariableName).ToList();
        }

        //public PharmacyTemplateValue SaveValue(PharmacyTemplateValue pharmacyTemplateValue)
        //{
        //    if (pharmacyTemplateValue == null)
        //        return null;

        //    var result = All.FirstOrDefault(x => x.PharmacyEntryId == pharmacyTemplateValue.PharmacyEntryId
        //                                         && x.VariableId == pharmacyTemplateValue.VariableId
        //                                         && x.Value == pharmacyTemplateValue.Value
        //                                         && x.Status == pharmacyTemplateValue.Status
        //                                         //&& x.ReviewLevel == pharmacyTemplateValue.ReviewLevel
        //                                         //&& x.AcknowledgeLevel == pharmacyTemplateValue.AcknowledgeLevel
        //                                         );
        //    if (result != null)
        //        return result;

        //    return pharmacyTemplateValue;
        //}

        //public VariableDto GetPharmacyVariable(VariableDto designVariableDto, int PharmacyEntryId)
        //{
        //    //designTemplateDto.StatusName = getStatusName(screeningTemplateObject, workflowlevel.LevelNo == screeningTemplateObject.ReviewLevel);
        //    var values = Context.PharmacyTemplateValue.Where(t => t.PharmacyEntryId == PharmacyEntryId).ToList();
        //    values.ForEach(t =>
        //    {
        //        var variable = designVariableDto;
        //        //var variable = designVariableDto..FirstOrDefault(v => v.Id == t.ProjectDesignVariableId);
        //        //var variable = Context.Variable.Where(x => x.Id == pharmacyTemplate.VariableId).FirstOrDefault();
        //        if (variable != null)
        //        {
        //            variable.Id = t.Id;
        //            //variable.VariableName = t.;
        //            //variable.ScreeningTemplateValueId = t.Id;
        //            //variable.QueryStatus = t.QueryStatus;
        //            //variable.HasComments = t.Comments.Any();
        //            //variable.HasQueries = t.Queries.Any();

        //            //variable.WorkFlowButton = SetWorkFlowButton(t, workflowlevel, designTemplateDto, screeningTemplateObject);

        //            //variable.DocPath = t.DocPath != null ? documentUrl + t.DocPath : null;
        //            //if (!string.IsNullOrWhiteSpace(variable.ScreeningValue))
        //            //{
        //            //    variable.IsValid = true;
        //            //}

        //            //if (variable.Values != null)
        //            //{
        //            //    variable.Values.ForEach(val =>
        //            //    {
        //            //        var childValue = t.Children.FirstOrDefault(v => v.ProjectDesignVariableValueId == val.Id);
        //            //        if (childValue != null)
        //            //        {
        //            //            variable.IsValid = true;
        //            //            val.ScreeningValue = childValue.Value;
        //            //            val.ScreeningValueOld = childValue.Value;
        //            //            val.ScreeningTemplateValueChildId = childValue.Id;
        //            //        }
        //            //    });
        //            //}
        //        }
        //    });

        //    //designVariableDto.Variables.ForEach(t =>
        //    //{

        //    //    if (t.ScreeningValue == null)
        //    //    {
        //    //        t.ScreeningValue = t.DefaultValue;
        //    //    }
        //    //});

        //    return designVariableDto;
        //}
    }
}