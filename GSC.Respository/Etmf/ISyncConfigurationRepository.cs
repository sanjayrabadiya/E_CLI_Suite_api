using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface ISyncConfigurationRepository : IGenericRepository<SyncConfiguration>
    {
        List<SyncConfigurationGridDto> GetsyncConfigurationList(bool isDeleted, int ProjectId);
        List<DropDownEnum> GetProjectWorkPlaceDetails(int ProjectId, short WorkPlaceFolderId);
        List<ProjectDropDown> GetProjectDropDownEtmf();
    }
}
