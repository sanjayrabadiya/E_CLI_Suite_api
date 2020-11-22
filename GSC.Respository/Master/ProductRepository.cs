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
    public class ProductRepository : GenericRespository<Product, GscContext>, IProductRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public ProductRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetProductDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.ProductName, Code = c.ProductCode})
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Product objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ProductCode == objSave.ProductCode && x.DeletedDate == null))
                return "Duplicate Product code : " + objSave.ProductCode;

            if (All.Any(x => x.Id != objSave.Id && x.ProductName == objSave.ProductName && x.DeletedDate == null))
                return "Duplicate Product name : " + objSave.ProductName;

            return "";
        }
    }
}