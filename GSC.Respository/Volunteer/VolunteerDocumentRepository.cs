using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;

namespace GSC.Respository.Volunteer
{
    public class VolunteerDocumentRepository : GenericRespository<VolunteerDocument>,
        IVolunteerDocumentRepository
    {
        public VolunteerDocumentRepository(IGSCContext context)
            : base(context)
        {
        }

        public List<VolunteerDocument> GetDocuments(int volunteerId)
        {
            var documents =
                FindByInclude(t => t.VolunteerId == volunteerId && t.DeletedDate == null, t => t.DocumentType).ToList();

            return documents;
        }
    }
}