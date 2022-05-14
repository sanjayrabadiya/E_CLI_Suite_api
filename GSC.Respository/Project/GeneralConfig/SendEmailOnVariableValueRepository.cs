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
    public class SendEmailOnVariableValueRepository : GenericRespository<SendEmailOnVariableValue>, ISendEmailOnVariableValueRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public SendEmailOnVariableValueRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }
        public void test() { }
    }
}
