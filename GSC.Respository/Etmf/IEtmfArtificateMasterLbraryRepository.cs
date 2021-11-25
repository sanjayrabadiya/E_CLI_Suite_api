using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IEtmfArtificateMasterLbraryRepository : IGenericRepository<EtmfArtificateMasterLbrary>
    {
        List<MasterLibraryJoinDto> GetArtifcateWithAllList();
        List<MasterLibraryJoinDto> GetArtifcateWithAllListByVersion(int ParentProjectId);
        List<DropDownDto> GetArtificateDropDown(int EtmfSectionMasterLibraryId);
    }
}
