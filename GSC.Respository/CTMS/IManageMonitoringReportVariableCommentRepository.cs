using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.CTMS
{
    public interface IManageMonitoringReportVariableCommentRepository : IGenericRepository<ManageMonitoringReportVariableComment>
    {
        IList<ManageMonitoringReportVariableCommentDto> GetComments(int manageMonitoringReportVariableId);
        void UpdateQuery(ManageMonitoringReportVariableCommentDto manageMonitoringReportVariableCommentDto, ManageMonitoringReportVariableComment manageMonitoringReportVariableComment, ManageMonitoringReportVariable manageMonitoringReportVariable);
    }
}