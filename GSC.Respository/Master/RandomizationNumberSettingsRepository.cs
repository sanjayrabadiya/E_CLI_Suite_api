using GSC.Common.GenericRespository;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Master
{
    public class RandomizationNumberSettingsRepository : GenericRespository<RandomizationNumberSettings>, IRandomizationNumberSettingsRepository
    {
        private readonly IGSCContext _context;
        public RandomizationNumberSettingsRepository(IGSCContext context) : base(context)
        {
            _context = context;
        }
    }
}
