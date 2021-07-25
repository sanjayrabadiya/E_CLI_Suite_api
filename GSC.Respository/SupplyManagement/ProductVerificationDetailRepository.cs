using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class ProductVerificationDetailRepository : GenericRespository<ProductVerificationDetail>, IProductVerificationDetailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public ProductVerificationDetailRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<ProductVerificationDetailDto> GetProductVerificationDetailList(int ProductReceiptId)
        {
            return All.Where(x => (x.DeletedDate == null) && x.ProductReceiptId == ProductReceiptId).
                   ProjectTo<ProductVerificationDetailDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        
    }
}
