using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.SupplyManagement;

namespace GSC.Respository.CTMS
{
    public interface IManageMonitoringReportVariableChildRepository : IGenericRepository<ManageMonitoringReportVariableChild>
    {
        void Save(ManageMonitoringReportVariable manageMonitoringReportVariable);
    }
}