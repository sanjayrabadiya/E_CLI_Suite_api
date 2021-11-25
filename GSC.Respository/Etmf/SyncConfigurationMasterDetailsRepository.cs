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
   public class SyncConfigurationMasterDetailsRepository : GenericRespository<SyncConfigurationMasterDetails>, ISyncConfigurationMasterDetailsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;     
        public SyncConfigurationMasterDetailsRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
           : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;            
        }
    }
}
