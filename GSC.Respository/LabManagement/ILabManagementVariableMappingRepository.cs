using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using System.Collections.Generic;

namespace GSC.Respository.LabManagement
{
    public interface ILabManagementVariableMappingRepository : IGenericRepository<LabManagementVariableMapping>
    {
        void DeleteMapping(LabManagementVariableMappingDto mappingDto);
        List<LabManagementVariableMappingGridDto> GetLabManagementVariableMappingList(int LabMangementConfigurationId);
    }
}