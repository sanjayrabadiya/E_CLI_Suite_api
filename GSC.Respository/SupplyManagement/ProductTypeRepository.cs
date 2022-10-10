using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.SupplyManagement
{
    public class ProductTypeRepository : GenericRespository<ProductType>, IProductTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public ProductTypeRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetProductTypeDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.ProductTypeCode + " - " + c.ProductTypeName, Code = c.ProductTypeCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(ProductType objSave)
        {
            if (objSave.Id > 0)
            {
                if (All.Any(
                    x => x.Id != objSave.Id && x.ProductTypeCode == objSave.ProductTypeCode && x.DeletedDate == null))
                    return "Duplicate ProductType code : " + objSave.ProductTypeCode;
            }
            else
            {
                if (All.Any(
                   x => x.ProductTypeCode == objSave.ProductTypeCode && x.DeletedDate == null))
                    return "Duplicate ProductType code : " + objSave.ProductTypeCode;
            }

            return "";
        }

        public List<ProductTypeGridDto> GetProductTypeList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<ProductTypeGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}