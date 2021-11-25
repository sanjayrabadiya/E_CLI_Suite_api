using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IEtmfSectionMasterLibraryRepository : IGenericRepository<EtmfSectionMasterLibrary>
    {
        string Duplicate(EtmfSectionMasterLibrary objSave);
        List<DropDownDto> GetSectionMasterLibraryDropDown(int EtmfZoneMasterLibraryId);


    }
}
