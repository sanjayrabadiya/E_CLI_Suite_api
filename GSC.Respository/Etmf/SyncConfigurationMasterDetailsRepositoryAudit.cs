using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
   public class SyncConfigurationMasterDetailsRepositoryAudit : GenericRespository<SyncConfigurationMasterDetailsAudit>, ISyncConfigurationMasterDetailsRepositoryAudit
    {
        public SyncConfigurationMasterDetailsRepositoryAudit(IGSCContext context)
           : base(context)
        {
        }
    }
}
