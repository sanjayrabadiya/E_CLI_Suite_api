﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementReceiptRepository : IGenericRepository<SupplyManagementReceipt>
    {
        List<SupplyManagementReceiptGridDto> GetSupplyShipmentReceiptList(bool isDeleted);
    }
}