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
    public class ProjectWorkplaceArtificatedocumentRepository : GenericRespository<ProjectWorkplaceArtificatedocument, GscContext>, IProjectWorkplaceArtificatedocumentRepository
    {


        public ProjectWorkplaceArtificatedocumentRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser)
           : base(uow, jwtTokenAccesser)
        {
        }
    }
}
