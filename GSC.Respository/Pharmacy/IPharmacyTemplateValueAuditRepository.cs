using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Pharmacy;

namespace GSC.Respository.Pharmacy
{
    public interface IPharmacyTemplateValueAuditRepository : IGenericRepository<PharmacyTemplateValueAudit>
    {
        IList<PharmacyAuditDto> GetAudits(int pharmacyTemplateValueId);
    }
}