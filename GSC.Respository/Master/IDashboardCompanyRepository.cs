using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Configuration;

namespace GSC.Respository.Configuration
{
    public interface IDashboardCompanyRepository : IGenericRepository<Company>
    {
        List<DashboardCompanyGridDto> GetDashboardCompanyList();
        dynamic GetDashboardProjectsStatus();
    }
}