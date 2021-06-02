using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;

namespace GSC.Respository.Volunteer
{
    public interface IVolunteerContactRepository : IGenericRepository<VolunteerContact>
    {
        List<VolunteerContactDto> GetContactTypeList(int volunteerId);
        //List<VolunteerContact> GetContacts(int volunteerId);
    }
}