using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IPhaseManagementRepository : IGenericRepository<PhaseManagement>
    {
        List<DropDownDto> GetPhaseManagementDropDown();
        string Duplicate(PhaseManagement objSave);
        List<PhaseManagementGridDto> GetPhaseManagementList(bool isDeleted);

    }
}