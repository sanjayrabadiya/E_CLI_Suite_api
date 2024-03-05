using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IHolidayMasterRepository : IGenericRepository<HolidayMaster>
    {
        List<HolidayMasterGridDto> GetHolidayList(bool isDeleted);
        List<DateTime> GetHolidayList(int projectId);
        List<HolidayMasterListDto> GetProjectWiseHolidayList(int StudyPlanId);
        string DuplicateHoliday(HolidayMaster objSave);
    }
}
