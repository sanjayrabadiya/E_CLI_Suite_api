using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GSC.Common.WorkingDayHelper;

namespace GSC.Respository.CTMS
{
    public class CtmsActivityRepository : GenericRespository<CtmsActivity>, ICtmsActivityRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public CtmsActivityRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

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
