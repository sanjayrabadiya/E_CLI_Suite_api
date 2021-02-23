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
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        public ReleaseSettingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }


        public ReleaseSettingDto GetVersionNum()
        {
            var releasesetting = _context.ReleaseSetting
                     .Select(c => new ReleaseSettingDto
                     {
                         Id = c.Id,
                         VersionNumber = c.VersionNumber
                     }).OrderByDescending(o => o.Id).ToList();

            return releasesetting.FirstOrDefault();
        }
    }
}
