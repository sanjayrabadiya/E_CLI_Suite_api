using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Volunteer
{
    public class VolunteerDocumentRepository : GenericRespository<VolunteerDocument, GscContext>,
        IVolunteerDocumentRepository
    {
        public VolunteerDocumentRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }

        public List<VolunteerDocument> GetDocuments(int volunteerId)
        {
            var documents =
                FindByInclude(t => t.VolunteerId == volunteerId && t.DeletedDate == null, t => t.DocumentType).ToList();

            //foreach (var document in documents)
            //{
            //    var id = document.DocumentTypeId;
            //    document.DocumentTypeName = id > 0 ? Context.Countries.Find(id).CountryName : "";

            //    id = document.Location.StateId;
            //    document.Location.StateName = id > 0 ? Context.State.Find(id).StateName : "";

            //    id = document.Location.CityId;
            //    document.Location.CityName = id > 0 ? Context.City.Find(id).CityName : "";
            //}

            return documents;
        }
    }
}