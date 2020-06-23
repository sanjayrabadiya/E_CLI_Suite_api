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
    public class DrugRepository : GenericRespository<Drug, GscContext>, IDrugRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public DrugRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetDrugDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.DrugName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Drug objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.DrugName == objSave.DrugName && x.DeletedDate == null))
                return "Duplicate Drug name : " + objSave.DrugName;
            return "";
        }
    }
}