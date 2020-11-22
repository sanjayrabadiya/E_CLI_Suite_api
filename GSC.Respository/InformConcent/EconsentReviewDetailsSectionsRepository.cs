using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public class EconsentReviewDetailsSectionsRepository : GenericRespository<EconsentReviewDetailsSections, GscContext>, IEconsentReviewDetailsSectionsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork<GscContext> _uow;
        public EconsentReviewDetailsSectionsRepository(IUnitOfWork<GscContext> uow,
                                                IJwtTokenAccesser jwtTokenAccesser) : base(uow, jwtTokenAccesser)
        {
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
    }
}
