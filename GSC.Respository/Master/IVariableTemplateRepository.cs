using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IVariableTemplateRepository : IGenericRepository<VariableTemplate>
    {
        List<DropDownDto> GetVariableTemplateDropDown();
        List<DropDownDto> GetVariableTemplateByDomainId(int domainId);
        VariableTemplate GetTemplate(int id);
        string Duplicate(VariableTemplate objSave);
        List<Variable> GetVariableNotAddedinTemplate(int variableTemplateId);
        List<DropDownDto> GetVariableTemplateByCRFByDomainId(bool isNonCRF, int domainId);
        void AddRequieredTemplate(Variable variable);
        List<DropDownDto> GetVariableTemplateNonCRFDropDown();
        DesignVerificationApprovalTemplateDto GetVerificationApprovalTemplate(int id);
        List<DropDownDto> GetVariableTemplateByModuleId(int moduleId);
    }
}