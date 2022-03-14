using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
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
        private readonly IGSCContext _context;

        public ProductVerificationDetailRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public ProductVerificationDetailDto GetProductVerificationDetailList(int ProductReceiptId)
        {
            var result = All.Where(x => (x.DeletedDate == null) && x.ProductReceiptId == ProductReceiptId).
                   ProjectTo<ProductVerificationDetailDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).FirstOrDefault();
            if (result != null)
            {
                var VerificationData = _context.VerificationApprovalTemplateHistory
                    .Include(x => x.VerificationApprovalTemplate).Where(x => x.VerificationApprovalTemplate.ProductVerificationDetailId == result.Id)
                    .OrderBy(x => x.Id)
                    .LastOrDefault();
                if (VerificationData != null)
                {
                    result.IsApprove = VerificationData.VerificationApprovalTemplate.IsApprove;
                    result.IsSendBack = VerificationData.IsSendBack;
                    result.VerificationApprovalTemplateId = VerificationData.Id;
                    result.IsSendForApprove = false;
                    if (VerificationData.IsSendBack && !result.IsApprove)
                        result.IsSendForApprove = true;
                }
                else
                {
                    result.IsSendForApprove = true;
                }
            }
            return result;
        }



    }
}
