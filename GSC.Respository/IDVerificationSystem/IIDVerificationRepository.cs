using GSC.Common.GenericRespository;
using GSC.Data.Dto.IDVerificationSystem;
using GSC.Data.Dto.LabReportManagement;
using GSC.Data.Entities.IDVerificationSystem;
using GSC.Data.Entities.LabReportManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.IDVerificationSystem
{
    public interface IIDVerificationRepository : IGenericRepository<IDVerification>
    {
        List<IDVerificationDto> GetIDVerificationList(bool isDeleted);
        int SaveIDVerificationDocument(IDVerificationDto reportDto);
    }
}
