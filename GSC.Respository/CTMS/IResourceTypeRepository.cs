using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IResourceTypeRepository : IGenericRepository<ResourceType>
    {
        List<DropDownDto> GetResourceTypeDropDown();
        string Duplicate(ResourceType objSave);
        List<ResourceTypeGridDto> GetResourceTypeList(bool isDeleted);
    }
}