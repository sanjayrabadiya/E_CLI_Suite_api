﻿using GSC.Common.GenericRespository;
using GSC.Data.Entities.AdverseEvent;
using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.AdverseEvent
{
    public class AdverseEventSettingsDetailRepository : GenericRespository<AdverseEventSettingsDetails>, IAdverseEventSettingsDetailRepository
    {
        public AdverseEventSettingsDetailRepository(IGSCContext context) : base(context)
        {

        }
    }
}
