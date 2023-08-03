using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabReportManagement;
using GSC.Data.Entities.LabReportManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.LabReportManagement
{
    public interface ILabReportRepository : IGenericRepository<LabReport>
    {
        List<LabReportGridDto> GetLabReports(bool isDeleted);
        int SaveLabReportDocument(LabReportDto reportDto);
    }
}
