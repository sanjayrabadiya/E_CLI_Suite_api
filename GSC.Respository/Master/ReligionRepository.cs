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
    public class ReligionRepository : GenericRespository<Religion, GscContext>, IReligionRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public ReligionRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetReligionDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.ReligionName,IsDeleted=c.DeletedDate!=null}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Religion objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ReligionName == objSave.ReligionName && x.DeletedDate == null))
                return "Duplicate Religion name : " + objSave.ReligionName;
            return "";
        }
    }
}