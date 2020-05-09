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
    public class ContactTypeRepository : GenericRespository<ContactType, GscContext>, IContactTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public ContactTypeRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetContactTypeDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.TypeName, Code = c.ContactCode})
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(ContactType objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ContactCode == objSave.ContactCode && x.DeletedDate == null))
                return "Duplicate Contact code : " + objSave.ContactCode;
            if (All.Any(x => x.Id != objSave.Id && x.TypeName == objSave.TypeName && x.DeletedDate == null))
                return "Duplicate ContactType name : " + objSave.TypeName;
            return "";
        }
    }
}