using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.SupplyManagement;

namespace GSC.Respository.CTMS
{
    public interface ICtmsMonitoringReportVariableValueChildRepository : IGenericRepository<CtmsMonitoringReportVariableValueChild>
    {
        void Save(CtmsMonitoringReportVariableValue ctmsMonitoringReportVariableValue);
    }
}