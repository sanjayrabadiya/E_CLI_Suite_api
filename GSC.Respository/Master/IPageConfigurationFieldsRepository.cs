using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Master
{
    public interface IPageConfigurationFieldsRepository : IGenericRepository<PageConfigurationFields>
    {
        string Duplicate(PageConfigurationFields objSave);
        List<PageConfigurationFieldsGridDto> GetPageConfigurationList(bool isDeleted);
        List<DropDownDto> GetPageConfigurationFieldsDropDown(int screenId);
    }
}
