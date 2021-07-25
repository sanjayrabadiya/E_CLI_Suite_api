using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public interface IProductVerificationDetailRepository : IGenericRepository<ProductVerificationDetail>
    {
        List<ProductVerificationDetailDto> GetProductVerificationDetailList(int ProductReceiptId);
    }
}
