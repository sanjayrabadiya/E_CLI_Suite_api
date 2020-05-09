using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Pharmacy;

namespace GSC.Respository.Pharmacy
{
    public interface IPharmacyVerificationEntryRepository : IGenericRepository<PharmacyVerificationEntry>
    {
        PharmacyVerificationEntryDto GetDetails(int id);
        void SavePharmacyVerificaction(PharmacyVerificationEntry pharmacyVerificationEntry);
        IList<DropDownDto> AutoCompleteSearch(string searchText);
        List<PharmacyVerificationEntryDto> GetpharmacyVerificationList();
        PharmacyVerificationTemplateValueListDto GetpharmacyVerificationTemplateValueList(int projectId, int domainId);

        List<PharmacyVerificationTemplateValueDto> GetpharmacyVerificationTemplateListByEntry(int entryId);
        VariableTemplate GetTemplate(int id);
    }
}