using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Configuration
{
    public class EmailTemplateRepository : GenericRespository<EmailTemplate, GscContext>, IEmailTemplateRepository
    {
        public EmailTemplateRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}