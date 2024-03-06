using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ICentralDepotRepository : IGenericRepository<CentralDepot>
    {
        List<DropDownDto> GetStorageAreaByDepoDropDown();
        List<CentralDepotGridDto> GetCentralDepotList(bool isDeleted);
        List<DropDownDto> GetStorageAreaByProjectDropDownByProjectId(int ProjectId);
        string Duplicate(CentralDepot objSave);
        bool IsCentralExists(int ProjectId);
        string ExistsInReceipt(int Id);
        string StudyUseInReceipt(CentralDepot objSave);
        List<DropDownDto> GetStorageAreaByIdDropDown(int Id);

        List<DropDownDto> GetStorageAreaByProjectDropDown(int? ProjectId, int? CountryId);
    }
}