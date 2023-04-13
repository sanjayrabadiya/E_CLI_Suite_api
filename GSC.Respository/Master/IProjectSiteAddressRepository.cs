using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Master
{
    public interface IProjectSiteAddressRepository : IGenericRepository<ProjectSiteAddress>
    {
        List<ProjectSiteAddressGridDto> GetProjectSiteAddressList(bool isDelete);
        string Duplicate(ProjectSiteAddress projectSite);
        List<ProjectSiteAddressGridDto> GetProjectSiteAddressByProject(bool isDeleted, int projectId, int manageSiteId);
    }
}
