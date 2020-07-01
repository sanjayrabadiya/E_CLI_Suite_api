using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class EtmfArtificateMasterLbraryRepository : GenericRespository<EtmfArtificateMasterLbrary, GscContext>, IEtmfArtificateMasterLbraryRepository
    {
        public EtmfArtificateMasterLbraryRepository(IUnitOfWork<GscContext> uow,
         IJwtTokenAccesser jwtTokenAccesser)
         : base(uow, jwtTokenAccesser)
        {
        }


    }
}