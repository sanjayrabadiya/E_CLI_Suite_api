using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System.Collections.Generic;

namespace GSC.Respository.Master
{
    public interface IVendorManagementRepository : IGenericRepository<VendorManagement>
    {
        string Duplicate(VendorManagement objSave);
        List<DropDownDto> GetVendorDropDown();
        List<VendorManagementGridDto> GetVendorManagementList(bool isDeleted);
    }
}