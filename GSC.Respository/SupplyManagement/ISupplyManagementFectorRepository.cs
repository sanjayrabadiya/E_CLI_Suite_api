using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementFectorRepository : IGenericRepository<SupplyManagementFector>
    {
        List<SupplyManagementFectorGridDto> GetListByProjectId(int projectId, bool isDeleted);
        SupplyManagementFectorDto GetById(int id);
        void DeleteChild(int Id);
        SupplyManagementFector UpdateFactorFormula(int id);
        FactorCheckResult ValidateSubjecWithFactor(Randomization randomization);
        bool CheckfactorrandomizationStarted(int projectId);
        bool CheckUploadRandomizationsheet(int projectId);
    }
}