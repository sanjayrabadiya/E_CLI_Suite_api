using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Configuration
{
    public class EmailTemplateRepository : GenericRespository<EmailTemplate>, IEmailTemplateRepository
    {
        public EmailTemplateRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }
    }
}