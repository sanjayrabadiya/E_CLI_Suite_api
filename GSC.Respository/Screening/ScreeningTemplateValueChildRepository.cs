using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueChildRepository: GenericRespository<ScreeningTemplateValueChild>, IScreeningTemplateValueChildRepository
    {
        public ScreeningTemplateValueChildRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }
    }
}
