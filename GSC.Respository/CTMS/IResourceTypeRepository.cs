using GSC.Common.GenericRespository;
using System.Collections.Generic;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Dto.CTMS;

namespace GSC.Respository.CTMS
{
    public interface IResourceTypeRepository : IGenericRepository<ResourceType>
    {
        //List<DropDownDto> GetResourceTypeDropDown();
        string Duplicate(ResourceType objSave);
        List<ResourceTypeGridDto> GetResourceTypeList(bool isDeleted);
        List<DropDownDto> GetUnitTypeDropDown();
        List<DropDownDto> GetDesignationDropDown();
        List<DropDownDto> GetDesignationDropDown(int resourceTypeID, int resourceSubTypeID);
    }
}