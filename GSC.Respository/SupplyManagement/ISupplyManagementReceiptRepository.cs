﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementReceiptRepository : IGenericRepository<SupplyManagementReceipt>
    {
        List<SupplyManagementReceiptGridDto> GetSupplyShipmentReceiptList(int parentProjectId, int SiteId, bool isDeleted);

        List<SupplyManagementReceiptHistoryGridDto> GetSupplyShipmentReceiptHistory(int id);

        List<KitAllocatedList> GetKitAllocatedList(int id, string Type);

        void UpdateKitStatus(SupplyManagementReceiptDto supplyManagementshipmentDto, SupplyManagementShipment supplyManagementShipment);

        string CheckValidationKitReciept(SupplyManagementReceiptDto supplyManagementshipmentDto);

        string CheckExpiryOnReceipt(int projectId, int id);
    }
}