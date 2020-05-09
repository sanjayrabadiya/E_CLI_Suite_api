using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Pharmacy;

namespace GSC.Respository.Pharmacy
{
    public interface
        IPharmacyVerificationTemplateValueRepository : IGenericRepository<PharmacyVerificationTemplateValue>
    {
        List<PharmacyVerificationTemplateValueDto> GetPharmacyVerificationTemplateTree(int pharmacyEntryId);

        //PharmacyTemplateValue SaveValue(PharmacyTemplateValue pharmacyTemplateValue);

        //VariableDto GetPharmacyVariable(VariableDto designVariableDto, int PharmacyEntryId);
    }
}