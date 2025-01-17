﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementRequestRepository : IGenericRepository<SupplyManagementRequest>
    {
        List<DropDownDto> GetSiteDropdownforShipmentRequest(int ProjectId, int ParenrProjectId);
        ProductUnitType GetPharmacyStudyProductUnitType(int id);

        List<SupplyManagementRequestGridDto> GetShipmentRequestList(int parentProjectId, int SiteId, bool isDeleted);

        bool CheckAvailableRemainingQty(int reqQty, int ProjectId, int PharmacyStudyProductTypeId);
        List<KitListApprove> GetAvailableKit(int SupplyManagementRequestId);

        int GetAvailableRemainingQty(int SupplyManagementRequestId, SupplyManagementKitNumberSettings supplyManagementKitNumberSettings);
        void SendrequestEmail(int id);

        void SendrequestApprovalEmail(int id);

        string CheckValidationShipmentRequest(SupplyManagementRequestDto supplyManagementRequestDto);


        Task ShipmentRequestEmailSchedule();

    }
}