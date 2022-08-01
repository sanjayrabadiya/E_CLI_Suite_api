using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementShipmentRepository : IGenericRepository<SupplyManagementShipment>
    {
        List<SupplyManagementShipmentGridDto> GetSupplyShipmentList(bool isDeleted);

        string GenerateShipmentNo();

        string ApprovalValidation(SupplyManagementShipmentDto supplyManagementshipmentDto);

        string GetShipmentNo();

        void Assignkits(SupplyManagementRequest shipmentdata, SupplyManagementShipmentDto supplyManagementshipmentDto);
    }
}