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
    }
}