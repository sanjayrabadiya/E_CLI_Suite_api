using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        string Duplicate(Product objSave);
        List<DropDownDto> GetProductDropDown();
    }
}