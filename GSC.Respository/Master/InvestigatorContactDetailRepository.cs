using System;
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
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public InvestigatorContactDetailRepository(IUnitOfWork<GscContext> uow,IJwtTokenAccesser jwtTokenAccesser)
    : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }


        public IList<InvestigatorContactDetailDto> GetContactList(int InvestigatorContactId)
        {
            return FindByInclude(t => t.InvestigatorContactId == InvestigatorContactId && t.DeletedDate == null).Select(c =>
                new InvestigatorContactDetailDto
                {
                    Id = c.Id,
                    InvestigatorContactId = c.InvestigatorContactId,
                    ContactTypeId = c.ContactTypeId,
                    SecurityRoleId = c.SecurityRoleId,
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

        public IList<InvestigatorContactDetailDto> GetContactList(int projectId, bool isDeleted)
        {
            return FindByInclude(t => t.InvestigatorContactId == projectId && isDeleted ? t.DeletedDate != null : t.DeletedDate == null).Select(c =>
     new InvestigatorContactDetailDto
     {
         Id = c.Id,
         InvestigatorContactId = c.InvestigatorContactId,
         ContactTypeId = c.ContactTypeId,
         SecurityRoleId = c.SecurityRoleId,
         ContactEmail = c.ContactEmail,
         ContactName = c.ContactName,
         ContactNo = c.ContactNo,
         CompanyId = c.CompanyId,
         IsDeleted = c.DeletedDate != null
     }).OrderByDescending(t => t.Id).ToList();
        }

        public List<DropDownDto> GetInvestigatorContactDetailDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.ContactName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}
