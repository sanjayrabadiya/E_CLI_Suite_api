using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;

namespace GSC.Respository.LabManagement
{
    public interface ILabManagementVariableMappingRepository : IGenericRepository<LabManagementVariableMapping>
    {
        void DeleteMapping(LabManagementVariableMappingDto mappingDto);
    }
}