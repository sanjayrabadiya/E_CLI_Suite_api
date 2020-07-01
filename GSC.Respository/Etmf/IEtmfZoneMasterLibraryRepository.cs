using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IEtmfZoneMasterLibraryRepository : IGenericRepository<EtmfZoneMasterLibrary>
    {
        List<EtmfZoneMasterLibrary> ExcelDataConvertToEntityformat(List<MasterLibraryDto> data);

        string Duplicate(EtmfZoneMasterLibrary objSave);
    }
}
