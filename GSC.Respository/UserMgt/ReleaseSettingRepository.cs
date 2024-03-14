using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.UserMgt
{
    public class ReleaseSettingRepository : GenericRespository<ReleaseSetting>, IReleaseSettingRepository
    {      
        public ReleaseSettingRepository(IGSCContext context)
            : base(context)
        {
           
        }
    }
}
