using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Project.GeneralConfig
{
    public class UploadLimitRepository : GenericRespository<UploadLimit>, IUploadlimitRepository
    {
        public UploadLimitRepository(IGSCContext context) : base(context)
        {
        }
    }
}
