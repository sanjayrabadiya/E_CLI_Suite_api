using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Pharmacy;

namespace GSC.Respository.Pharmacy
{
    public interface IPharmacyEntryRepository : IGenericRepository<PharmacyEntry>
    {
        PharmacyEntryDto GetDetails(int id);
        void SavePharmacy(PharmacyEntry pharmacyEntry);
        IList<DropDownDto> AutoCompleteSearch(string searchText);
        List<PharmacyEntryDto> GetpharmacyList();
        PharmacyTemplateValueListDto GetpharmacyTemplateValueList(int? projectId, int domainId, int? productTypeId);

        List<PharmacyTemplateValueDto> GetpharmacyTemplateListByEntry(int entryId);
        VariableTemplate GetTemplate(int id);
    }
}