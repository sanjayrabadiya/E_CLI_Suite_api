using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GSC.Respository.LabManagement
{
    public interface ILabManagementUploadExcelDataRepository : IGenericRepository<LabManagementUploadExcelData>
    {
        List<LabManagementUploadExcelDataDto> GetExcelDataList(int labManagementUploadDataId);
        FileStreamResult GetDataNotUseInDataEntry(LabManagementUploadDataDto search);
    }
}