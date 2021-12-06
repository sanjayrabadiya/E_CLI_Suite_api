using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface ISyncConfigurationMasterRepository : IGenericRepository<SyncConfigurationMaster>
    {
        List<SyncConfigurationMasterGridDto> GetSyncConfigurationMastersList(bool isDeleted);
        string Duplicate(SyncConfigurationMaster objSave);
        List<SyncConfigurationAuditDto> GetAudit();
        string ValidateMasterConfiguration(SyncConfigurationParameterDto details);
        string GetsyncConfigurationPath(SyncConfigurationParameterDto details);
    }
}
