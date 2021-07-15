using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface IProductReceiptRepository : IGenericRepository<ProductReceipt>
    {
        string Duplicate(ProductReceipt objSave);
        List<DropDownDto> GetProductReceipteDropDown(int ProjectId);
        List<ProductReceiptGridDto> GetProductReceiptList(int ProjectId, bool isDeleted);
    }
}