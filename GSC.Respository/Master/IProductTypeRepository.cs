using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IProductTypeRepository : IGenericRepository<ProductType>
    {
        List<DropDownDto> GetProductTypeDropDown();
        string Duplicate(ProductType objSave);
        List<ProductTypeGridDto> GetProductTypeList(bool isDeleted);
    }
}