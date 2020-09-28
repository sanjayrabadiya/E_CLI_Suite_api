using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System.Collections.Generic;

namespace GSC.Respository.Master
{
    public interface IRegulatoryTypeRepository : IGenericRepository<RegulatoryType>
    {
        string Duplicate(RegulatoryType objSave);
        List<DropDownDto> GetRegulatoryTypeDropDown();
        List<RegulatoryTypeGridDto> GetRegulatoryTypeList(bool isDeleted);
    }
}