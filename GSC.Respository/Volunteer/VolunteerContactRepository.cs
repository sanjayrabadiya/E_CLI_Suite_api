using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;

namespace GSC.Respository.Volunteer
{
    public class VolunteerContactRepository : GenericRespository<VolunteerContact>,
        IVolunteerContactRepository
    {
        public VolunteerContactRepository(IGSCContext context)
            : base(context)
        {
        }

        public List<VolunteerContactDto> GetContactTypeList(int volunteerId)
        {
            return FindByInclude(t => t.VolunteerId == volunteerId && t.DeletedDate == null, t => t.ContactType).Select(
                c => new VolunteerContactDto
                {
                    Id = c.Id,
                    VolunteerId = c.VolunteerId,
                    ContactName = c.ContactName,
                    ContactNo = c.ContactNo,
                    ContactNoTwo = c.ContactNoTwo,
                    IsDefault = c.IsDefault,
                    IsEmergency = c.IsEmergency,
                    ContactTypeName = c.ContactType?.TypeName,
                    ContactTypeId = c.ContactTypeId
                }).OrderByDescending(t => t.Id).ToList();
        }
    }
}