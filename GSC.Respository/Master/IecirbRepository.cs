using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Master
{
    public class IecirbRepository : GenericRespository<Iecirb>, IIecirbRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public IecirbRepository(IGSCContext context,
        IJwtTokenAccesser jwtTokenAccesser)
        : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public string Duplicate(Iecirb objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ManageSiteId == objSave.ManageSiteId && x.RegistrationNumber == objSave.RegistrationNumber && x.DeletedDate == null))
                return "Duplicate registration number : " + objSave.RegistrationNumber;

            return "";
        }

        public List<DropDownDto> GetIecirbDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.IECIRBName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}
