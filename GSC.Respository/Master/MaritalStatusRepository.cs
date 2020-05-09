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
    public class MaritalStatusRepository : GenericRespository<MaritalStatus, GscContext>, IMaritalStatusRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public MaritalStatusRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetMaritalStatusDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.MaritalStatusName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(MaritalStatus objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.MaritalStatusName == objSave.MaritalStatusName && x.DeletedDate == null))
                return "Duplicate MaritalStatus name : " + objSave.MaritalStatusName;
            return "";
        }
    }
}