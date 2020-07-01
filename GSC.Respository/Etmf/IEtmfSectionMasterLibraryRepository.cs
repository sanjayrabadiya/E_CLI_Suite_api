using GSC.Common.GenericRespository;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IEtmfSectionMasterLibraryRepository : IGenericRepository<EtmfSectionMasterLibrary>
    {
        string Duplicate(EtmfSectionMasterLibrary objSave);


    }
}
