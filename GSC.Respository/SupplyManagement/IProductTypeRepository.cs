using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;

namespace GSC.Respository.SupplyManagement
{
    public interface IProductTypeRepository : IGenericRepository<ProductType>
    {
        List<DropDownDto> GetProductTypeDropDown();
        string Duplicate(ProductType objSave);
        List<ProductTypeGridDto> GetProductTypeList(bool isDeleted);
    }
}