using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Project.Design
{
    public class TemplateVariableSequenceNoSettingRepository : GenericRespository<TemplateVariableSequenceNoSetting>, ITemplateVariableSequenceNoSettingRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public TemplateVariableSequenceNoSettingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }



    }
}
