using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using GSC.Shared.Generic;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Barcode
{
    public interface IBarcodeAuditRepository : IGenericRepository<BarcodeAudit>
    {
        void Save(string tableName, AuditAction action, int recordId);
        IList<BarcodeAuditDto> GetBarcodeAuditDetails(int ProjectId);
    }
}
