using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public class EconsentSetupPatientStatusRepository : GenericRespository<EconsentSetupPatientStatus, GscContext>, IEconsentSetupPatientStatusRepository
    {
        //private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public EconsentSetupPatientStatusRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser) : base(uow, jwtTokenAccesser)
        {
            //_jwtTokenAccesser = jwtTokenAccesser;
        }
    }
}
