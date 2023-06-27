using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Respository.Project.Design
{
    public interface IVisitEmailConfigurationRepository : IGenericRepository<VisitEmailConfiguration>
    {
        List<VisitEmailConfigurationGridDto> GetVisitEmailConfigurationList(bool isDeleted, int projectDesignVisitId);
        string Duplicate(VisitEmailConfiguration objSave);
    }
}