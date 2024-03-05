using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.CTMS
{
    public interface ICtmsMonitoringReportVariableValueQueryRepository : IGenericRepository<CtmsMonitoringReportVariableValueQuery>
    {
        IList<CtmsMonitoringReportVariableValueQueryDto> GetQueries(int ctmsMonitoringReportVariableValueId);
        void UpdateQuery(CtmsMonitoringReportVariableValueQueryDto ctmsMonitoringReportVariableValueQueryDto, CtmsMonitoringReportVariableValueQuery CtmsMonitoringReportVariableValueQuery, CtmsMonitoringReportVariableValue ctmsMonitoringReportVariableValue);
        void GenerateQuery(CtmsMonitoringReportVariableValueQueryDto ctmsMonitoringReportVariableValueQueryDto,
           CtmsMonitoringReportVariableValueQuery ctmsMonitoringReportVariableValueQuery, CtmsMonitoringReportVariableValue ctmsMonitoringReportVariableValue);
        void SaveCloseQuery(CtmsMonitoringReportVariableValueQuery ctmsMonitoringReportVariableValueQuery);
    }
}