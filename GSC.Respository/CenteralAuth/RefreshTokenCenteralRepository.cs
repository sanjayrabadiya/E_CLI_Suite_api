using GSC.Centeral.Context;
using GSC.Centeral.GenericRespository;
using GSC.Centeral.UnitOfWork;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.CenteralAuth
{
    public class RefreshTokenCenteralRepository : GenericCenteralRespository<RefreshToken, CenteralContext>, IRefreshTokenCenteralRepository
    {
        public RefreshTokenCenteralRepository(
            IUnitOfWorkCenteral<CenteralContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}
