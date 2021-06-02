using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Volunteer
{
    public class VolunteerContactRepository : GenericRespository<VolunteerContact>,
        IVolunteerContactRepository
    {
        private readonly IGSCContext _context;
        public VolunteerContactRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _context = context;
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
                    IsDefault = c.IsDefault,
                    IsEmergency = c.IsEmergency,
                    ContactTypeName = c.ContactType.TypeName,
                    ContactTypeId = c.ContactTypeId
                }).OrderByDescending(t => t.Id).ToList();
        }

        //public List<VolunteerContact> GetContacts(int volunteerId)
        //{
        //    var addresses = FindByInclude(t => t.VolunteerId == volunteerId && t.DeletedDate == null, t => t.Location, t => t.ContactType).OrderByDescending(t => t.Id).ToList();

        //    foreach (var address in addresses)
        //    {
        //        var Cid = address.ContactTypeId;

        //        if (address.Location == null)
        //            continue;

        //        var id = address.Location.CountryId;
        //        address.Location.CountryName = id > 0 ? _context.Country.Find(id).CountryName : "";

        //        id = address.Location.StateId;
        //        address.Location.StateName = id > 0 ? _context.State.Find(id).StateName : "";

        //        id = address.Location.CityId;
        //        address.Location.CityName = id > 0 ? _context.City.Find(id).CityName : "";
        //        id = address.Location.CityAreaId;
        //        address.Location.CityAreaName = id > 0 ? _context.CityArea.Find(id).AreaName : "";
        //    }

        //    return addresses;
        //}

    }
}