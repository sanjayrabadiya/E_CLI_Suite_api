using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Master
{
    public class ProductTypeRepository : GenericRespository<ProductType, GscContext>, IProductTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public ProductTypeRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetProductTypeDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.ProductTypeName, Code = c.ProductTypeCode})
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(ProductType objSave)
        {
            if (All.Any(
                x => x.Id != objSave.Id && x.ProductTypeCode == objSave.ProductTypeCode && x.DeletedDate == null))
                return "Duplicate ProductType code : " + objSave.ProductTypeCode;

            if (All.Any(
                x => x.Id != objSave.Id && x.ProductTypeName == objSave.ProductTypeName && x.DeletedDate == null))
                return "Duplicate ProductType name : " + objSave.ProductTypeName;


            return "";
        }

        public List<ProductTypeGridDto> GetProductTypeList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<ProductTypeGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}