﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public interface IProductVerificationRepository : IGenericRepository<ProductVerification>
    {
        ProductVerificationDto GetProductVerificationList(int productReceiptId);
        List<ProductVerificationGridDto> GetProductVerification(int ProjectId, bool isDeleted);
        List<ProductVerificationGridDto> GetProductVerificationSummary(int productReceiptId);
    }
}
