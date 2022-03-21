using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementUploadFileDetailRepository : IGenericRepository<SupplyManagementUploadFileDetail>
    {
        List<SupplyManagementUploadFileDetailDto> GetSupplyManagementUploadFileDetailList(int SupplyManagementUploadFileId);
    }
}