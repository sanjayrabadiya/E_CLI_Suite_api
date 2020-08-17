using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IPatientStatusRepository : IGenericRepository<PatientStatus>
    {
        string Duplicate(PatientStatus objSave);
        List<DropDownDto> GetPatientStatusDropDown();
        List<PatientStatusDto> GetPatientStatusList(bool isDeleted);
    }
}