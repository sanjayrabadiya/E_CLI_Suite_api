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
    public class ProductFormRepository : GenericRespository<MProductForm, GscContext>, IProductFomRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public ProductFormRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetProductFormDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.FormName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(MProductForm objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.FormName == objSave.FormName && x.DeletedDate == null))
                return "Duplicate Form name : " + objSave.FormName;
            return "";
        }
    }
}