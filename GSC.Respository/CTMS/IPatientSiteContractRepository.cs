using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.Master
{
    public interface IPatientSiteContractRepository : IGenericRepository<PatientSiteContract>
    {
        IList<PatientSiteContractGridDto> GetPatientSiteContractList(bool isDeleted, int siteContractId);
    }
}
