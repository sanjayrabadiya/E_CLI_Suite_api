using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class InvestigatorContactDetailRepository : GenericRespository<InvestigatorContactDetail, GscContext>, IInvestigatorContactDetailRepository
    {
        public InvestigatorContactDetailRepository(IUnitOfWork<GscContext> uow,
    IJwtTokenAccesser jwtTokenAccesser)
    : base(uow, jwtTokenAccesser)
        {
        }


        public IList<InvestigatorContactDetailDto> GetContactList(int InvestigatorContactId)
        {
            return FindByInclude(t => t.InvestigatorContactId == InvestigatorContactId && t.DeletedDate == null).Select(c =>
                new InvestigatorContactDetailDto
                {
                    Id = c.Id,
                    InvestigatorContactId = c.InvestigatorContactId,
                    ContactTypeId = c.ContactTypeId,
                    ContactEmail = c.ContactEmail,
                    ContactName = c.ContactName,
                    ContactNo = c.ContactNo,
                    CompanyId = c.CompanyId
                }).OrderByDescending(t => t.Id).ToList();
        }

        public string DuplicateContact(InvestigatorContactDetail objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ContactNo == objSave.ContactNo && x.DeletedDate == null))
                return "Duplicate Contact No : " + objSave.ContactNo;

            return "";
        }
    }
}
