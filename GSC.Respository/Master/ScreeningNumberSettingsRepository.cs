using GSC.Common.GenericRespository;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Master
{
    public class ScreeningNumberSettingsRepository : GenericRespository<ScreeningNumberSettings>, IScreeningNumberSettingsRepository
    {
        private readonly IGSCContext _context;
        public ScreeningNumberSettingsRepository(IGSCContext context) : base(context)
        {
            _context = context;
        }
    }
}
