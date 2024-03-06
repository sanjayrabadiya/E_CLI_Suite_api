using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ICtmsMonitoringReportVariableValueRepository : IGenericRepository<CtmsMonitoringReportVariableValue>
    {
        void UpdateChild(List<CtmsMonitoringReportVariableValueChild> children);
        List<CtmsMonitoringReportVariableValueBasic> GetVariableValues(int CtmsMonitoringReportId);
        string GetValueForAudit(CtmsMonitoringReportVariableValueDto cstmsMonitoringReportVariableValueDto, CtmsMonitoringReportVariableValueChildDto? ctmsMonitoringReportVariableValueChildDto);
        void DeleteChild(int ctmsMonitoringReportVariableValueId);
        bool GetQueryStatusByReportId(int ctmsMonitoringReportId);
        void SaveVariableValue(CtmsMonitoringReportVariableValueSaveDto ctmsMonitoringReportVariableValueSaveDto);
        void UploadDocument(CtmsMonitoringReportVariableValueDto ctmsMonitoringReportVariableValueSaveDto);
    }
}