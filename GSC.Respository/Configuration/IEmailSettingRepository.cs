using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Configuration;

namespace GSC.Respository.Configuration
{
    public interface IEmailSettingRepository : IGenericRepository<EmailSetting>
    {
        List<DropDownDto> GetEmailFromDropDown();
    }
}