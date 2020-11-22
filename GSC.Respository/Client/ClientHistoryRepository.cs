using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Client;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Client
{
    public class ClientHistoryRepository : GenericRespository<ClientHistory>, IClientHistoryRepository
    {
        public ClientHistoryRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }
    }
}