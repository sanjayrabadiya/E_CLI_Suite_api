using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.UserMgt
{
    public class UserSettingRepository : GenericRespository<UserSetting>, IUserSettingRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public UserSettingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public UserSettingDto GetProjectDefaultData()
        {
            var userSetting = All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null).Select(t => new UserSettingDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                StudyId = t.Project.ParentProjectId,
                CountryId = t.Project.ManageSite.CompanyId,
                UserId = t.UserId,
            }).FirstOrDefault();
            if (userSetting != null)
                if (_context.ProjectRight.Any(x => x.ProjectId == userSetting.ProjectId && x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate==null))
                    return userSetting;
                else
                    return null;
                
            return userSetting;

        }
    }
}
