using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkplaceSubSectionRepository : IGenericRepository<EtmfProjectWorkPlace>
    {
        string Duplicate(EtmfProjectWorkPlace objSave);
        EtmfProjectWorkPlaceDto getSectionDetail(EtmfProjectWorkPlaceDto projectWorkplaceSubSectionDto);
        EtmfProjectWorkPlaceDto updateSectionDetailFolder(EtmfProjectWorkPlaceDto projectWorkplaceSubSectionDto);
        List<DropDownDto> GetDrodDown(int zoneId);
        string DeletSectionDetailFolder(int id);
    }
}
