using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class CtmsActivityRepository : GenericRespository<CtmsActivity>, ICtmsActivityRepository
    {
        public CtmsActivityRepository(IGSCContext context) : base(context)
        {}
        public List<DropDownDto> GetCtmsActivityList()
        {
            var result = All.Where(x => x.DeletedDate == null).Select(x=> new DropDownDto
            {
                Id = x.Id,
                Value = x.ActivityName,
                Code = x.ActivityCode
            }).ToList();

            return result;
        }
    }
}
