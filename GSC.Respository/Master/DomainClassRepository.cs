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
    public class DomainClassRepository : GenericRespository<DomainClass, GscContext>, IDomainClassRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public DomainClassRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }


        public string ValidateDomainClass(DomainClass objSave)
        {
            if (All.Any(
                x => x.Id != objSave.Id && x.DomainClassCode == objSave.DomainClassCode && x.DeletedDate == null))
                return "Duplicate Domain Class Code : " + objSave.DomainClassCode;

            if (All.Any(x =>
                x.Id != objSave.Id && x.DomainClassName == objSave.DomainClassName &&
                x.DomainClassCode == objSave.DomainClassCode && x.DeletedDate == null))
                return "Duplicate Domain Class Name : " + objSave.DomainClassName;

            return "";
        }

        public List<DropDownDto> GetDomainClassDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.DomainClassName, Code = c.DomainClassCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }
    }
}