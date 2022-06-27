using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Volunteer
{
    public interface IVolunteerQueryRepository : IGenericRepository<VolunteerQuery>
    {
        VolunteerQuery GetLatest(int VolunteerId, string FiledName);
        IList<VolunteerQueryDto> GetData(int volunteerid);
        IList<VolunteerQueryDto> VolunteerQuerySearch(VolunteerQuerySearchDto search);
        List<VolunteerQuery> GetDetailsByVolunteerId(int VolunteerId);
        List<DropDownDto> GetUsers();
        List<DropDownDto> GetRoles();
    }
}
