using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Master
{
    public interface IPageConfigurationRepository : IGenericRepository<PageConfiguration>
    {
        List<DropDownDto> GetPageConfigurationDropDown();
        string Duplicate(PageConfiguration objSave);
        List<PageConfigurationGridDto> GetPageConfigurationList(bool isDeleted);
        List<PageConfigurationGridDto> GetPageConfigurationListByScreen(int screenId, bool isDeleted);
        PageConfigurationDto GetById(int id);
        List<PageConfigurationDto> GetPageConfigurationByAppScreen(int screenId);
        List<PageConfigurationCommon> GetPageConfigurationByAppScreen(string screenCode);
    }
}
