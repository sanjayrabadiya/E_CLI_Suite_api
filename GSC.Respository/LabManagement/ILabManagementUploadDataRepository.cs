using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using System.Collections.Generic;

namespace GSC.Respository.LabManagement
{
    public interface ILabManagementUploadDataRepository : IGenericRepository<LabManagementUploadData>
    {
        List<LabManagementUploadDataGridDto> GetUploadDataList(bool isDeleted);
    }
}