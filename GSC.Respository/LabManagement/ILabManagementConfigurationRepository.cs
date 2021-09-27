using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using System.Collections.Generic;

namespace GSC.Respository.LabManagement
{
    public interface ILabManagementConfigurationRepository : IGenericRepository<Data.Entities.LabManagement.LabManagementConfiguration>
    {
        string Duplicate(Data.Entities.LabManagement.LabManagementConfiguration objSave);
        List<LabManagementConfigurationGridDto> GetConfigurationList(bool isDeleted);
        object[] GetMappingData(int LabManagementConfigurationId);
    }
}