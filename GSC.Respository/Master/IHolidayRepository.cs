using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IHolidayRepository : IGenericRepository<Holiday>
    {
        IList<HolidayGridDto> GetHolidayList(int Id, bool isDeleted);

        string DuplicateHoliday(Holiday objSave);

        List<DropDownDto> GetHolidayDropDown();
    }
}
