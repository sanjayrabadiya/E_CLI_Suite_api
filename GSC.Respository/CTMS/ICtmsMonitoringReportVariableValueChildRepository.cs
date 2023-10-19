using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.CTMS
{
    public interface ICtmsMonitoringReportVariableValueChildRepository : IGenericRepository<CtmsMonitoringReportVariableValueChild>
    {
        void Save(CtmsMonitoringReportVariableValue ctmsMonitoringReportVariableValue);
    }
}