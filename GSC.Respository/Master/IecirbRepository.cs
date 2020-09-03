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
    public class IecirbRepository : GenericRespository<Iecirb, GscContext>, IIecirbRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public IecirbRepository(IUnitOfWork<GscContext> uow,
IJwtTokenAccesser jwtTokenAccesser)
: base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public IList<IecirbDto> GetIecirbList(int manageSiteId, bool isDeleted)
        {
            return FindByInclude(t => (isDeleted ? t.DeletedDate != null : t.DeletedDate == null) && t.ManageSiteId == manageSiteId).Select(c =>
                new IecirbDto
                {
                    Id = c.Id,
                    ManageSiteId = c.ManageSiteId,
                    IECIRBName = c.IECIRBName,
                    RegistrationNumber = c.RegistrationNumber,
                    IECIRBContactName = c.IECIRBContactName,
                    IECIRBContactEmail = c.IECIRBContactEmail,
                    IECIRBContactNumber = c.IECIRBContactNumber,
                    CompanyId = c.CompanyId,
                    IsDeleted = c.DeletedDate != null
                }).OrderByDescending(t => t.Id).ToList();
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
