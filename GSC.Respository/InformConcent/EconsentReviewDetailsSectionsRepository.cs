using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public class EconsentReviewDetailsSectionsRepository : GenericRespository<EconsentReviewDetailsSections>, IEconsentReviewDetailsSectionsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public EconsentReviewDetailsSectionsRepository(IGSCContext context,
                                                IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
    }
}
