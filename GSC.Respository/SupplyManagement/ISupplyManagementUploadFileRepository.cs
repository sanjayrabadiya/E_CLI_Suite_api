using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementUploadFileRepository : IGenericRepository<SupplyManagementUploadFile>
    {
        FileStreamResult DownloadFormat(SupplyManagementUploadFileDto supplyManagementUploadFile);
        List<SupplyManagementUploadFileGridDto> GetSupplyManagementUploadFileList(bool isDeleted, int ProjectId);
        string InsertExcelDataIntoDatabaseTable(SupplyManagementUploadFile supplyManagementUploadFile);

        bool CheckUploadApproalPending(int ProjectId, int SiteId, int CountryId);

        void SendRandomizationUploadSheetEmail(SupplyManagementUploadFileDto obj);
    }
}