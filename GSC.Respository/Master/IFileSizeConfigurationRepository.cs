using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Master
{
    public interface IFileSizeConfigurationRepository : IGenericRepository<FileSizeConfiguration>
    {
        List<DropDownDto> GetFileSizeConfigurationDropDown();
        string Duplicate(FileSizeConfiguration objSave);
        List<FileSizeConfigurationGridDto> GetFileSizeConfigurationList(bool isDeleted);
    }
}
