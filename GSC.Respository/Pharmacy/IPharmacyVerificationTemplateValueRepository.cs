using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Pharmacy;

namespace GSC.Respository.Pharmacy
{
    public interface
        IPharmacyVerificationTemplateValueRepository : IGenericRepository<PharmacyVerificationTemplateValue>
    {
        List<PharmacyVerificationTemplateValueDto> GetPharmacyVerificationTemplateTree(int pharmacyVerificationEntryId);
    }
}