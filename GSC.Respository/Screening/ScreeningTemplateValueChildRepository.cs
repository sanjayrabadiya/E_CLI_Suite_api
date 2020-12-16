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
    public class ScreeningTemplateValueChildRepository : GenericRespository<ScreeningTemplateValueChild>, IScreeningTemplateValueChildRepository
    {
        public ScreeningTemplateValueChildRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }


        public void Save(ScreeningTemplateValue screeningTemplateValue)
        {
            if (screeningTemplateValue.Children != null)
            {
                screeningTemplateValue.Children.ForEach(x =>
                {
                    if (x.Id == 0)
                        Add(x);
                    else
                        Update(x);
                });
            }
        }
    }
}
