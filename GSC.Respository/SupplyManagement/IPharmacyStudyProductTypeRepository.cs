using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface IPharmacyStudyProductTypeRepository : IGenericRepository<PharmacyStudyProductType>
    {
        List<PharmacyStudyProductTypeGridDto> GetPharmacyStudyProductTypeList(bool isDeleted);
        List<DropDownDto> GetPharmacyStudyProductTypeDropDown(int ProjectId);
        string Duplicate(PharmacyStudyProductType objSave);
    }
}