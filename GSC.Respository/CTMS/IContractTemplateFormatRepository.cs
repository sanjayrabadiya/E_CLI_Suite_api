using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IContractTemplateFormatRepository : IGenericRepository<ContractTemplateFormat>
    {
        List<ContractTemplateFormatGridDto> GetContractTemplateFormateList(bool isDeleted);
        string Duplicate(ContractTemplateFormat objSave);
        List<DropDownDto> GetContractFormatTypeDropDown();
    }
}