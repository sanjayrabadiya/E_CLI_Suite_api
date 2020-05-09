using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Client;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Client
{
    public class ClientHistoryRepository : GenericRespository<ClientHistory, GscContext>, IClientHistoryRepository
    {
        public ClientHistoryRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}