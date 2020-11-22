using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Client;
using GSC.Data.Entities.Client;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Client
{
    public class ClientContactRepository : GenericRespository<ClientContact>, IClientContactRepository
    {
        public ClientContactRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }


        public IList<ClientContactDto> GetContactList(int clientId, bool isDeleted)
        {
            return FindByInclude(t => (isDeleted ? t.DeletedDate != null : t.DeletedDate == null) && t.ClientId == clientId, t => t.ContactType).Select(c =>
                new ClientContactDto
                {
                    Id = c.Id,
                    ClientId = c.ClientId,
                    ContactName = c.ContactName,
                    ContactNo = c.ContactNo,
                    IsDefault = c.IsDefault,
                    ContactTypeName = c.ContactType.TypeName,
                    ContactTypeId = c.ContactTypeId,
                    IsDeleted = c.DeletedDate != null
                }).OrderByDescending(t => t.Id).ToList();
        }

        public string DuplicateContact(ClientContact objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ContactNo == objSave.ContactNo && x.DeletedDate == null))
                return "Duplicate Contact No : " + objSave.ContactNo;

            return "";
        }
    }
}