using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Master
{
    public class SiteTeamRepository : GenericRespository<SiteTeam>, ISiteTeamRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IProjectRightRepository _projectRightRepository;
        public SiteTeamRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper,
            IProjectRightRepository projectRightRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _projectRightRepository = projectRightRepository;
        }
        public string Duplicate(SiteTeamDto objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.RoleId == objSave.RoleId && x.ProjectId == objSave.ProjectId && x.UserId == objSave.UserId && x.DeletedDate == null))
            {
                return "Duplicate Record User";
            }
            return "";
        }

        public List<DropDownDto> GetRoleDropdownForSiteTeam(int projectid)
        {
            var data = _projectRightRepository.FindByInclude(x => x.ProjectId == projectid && x.IsReviewDone == true, x => x.role).
                Select(c => new DropDownDto { Id = c.RoleId, Value = c.role.RoleName, IsDeleted = false }).OrderBy(o => o.Value)
               .Distinct().ToList();
            return data;
        }

        public List<SiteTeamGridDto> GetSiteTeamList(int projectid, bool isDeleted)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == projectid).
                   ProjectTo<SiteTeamGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<UserDto> GetUserDropdownForSiteTeam(int projectid, int roleId)
        {
            var data = _projectRightRepository.FindByInclude(x => x.ProjectId == projectid && x.RoleId == roleId && x.IsReviewDone == true, x => x.User).
                Select(c => new UserDto { Id = c.UserId, UserName = c.User.UserName, Email = c.User.Email, Phone = c.User.Phone, IsDeleted = false }).OrderBy(o => o.UserName)
               .Distinct().ToList();
            return data;
        }
    }
}
