using GSC.Common.GenericRespository;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Configuration
{
    public class SMSSettingRepository : GenericRespository<SMSSetting>, ISMSSettingRepository
    {
        
        public SMSSettingRepository(IGSCContext context) : base(context)
        {
            
        }
    }
}
