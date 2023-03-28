using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementUploadFileRepository : IGenericRepository<SupplyManagementUploadFile>
    {
        FileStreamResult DownloadFormat(SupplyManagementUploadFileDto supplyManagementUploadFile, SupplyManagementKitNumberSettings setting);
        List<SupplyManagementUploadFileGridDto> GetSupplyManagementUploadFileList(bool isDeleted, int ProjectId);
        string InsertExcelDataIntoDatabaseTable(SupplyManagementUploadFile supplyManagementUploadFile, SupplyManagementKitNumberSettings setting);

        bool CheckUploadApproalPending(int ProjectId, int SiteId, int CountryId);

        void SendRandomizationUploadSheetEmail(SupplyManagementUploadFile obj);
    }
}