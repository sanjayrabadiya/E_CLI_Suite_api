using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IDesignationRepository : IGenericRepository<Designation>
    {
        List<DesignationGridDto> GetDesignationList(bool isDeleted);
        string Duplicate(Designation objSave);
    }
}