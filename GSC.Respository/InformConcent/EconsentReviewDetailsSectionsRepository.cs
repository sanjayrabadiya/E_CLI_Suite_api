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
        public EconsentReviewDetailsSectionsRepository(IGSCContext context) : base(context)
        {
        }
    }
}
