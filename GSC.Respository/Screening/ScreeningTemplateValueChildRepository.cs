using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueChildRepository: GenericRespository<ScreeningTemplateValueChild, GscContext>, IScreeningTemplateValueChildRepository
    {
        public ScreeningTemplateValueChildRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}
