using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkPlaceZoneRepository : IGenericRepository<EtmfProjectWorkPlace>
    {
        List<DropDownDto> GetProjectWorkPlaceZoneDropDown(int CountryId);
        List<DropDownDto> GetProjectByZone(int ParentProjectId);

    }
}
