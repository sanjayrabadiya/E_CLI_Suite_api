using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class CtmsApprovalRolesRepository : GenericRespository<CtmsApprovalRoles>, ICtmsApprovalRolesRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public CtmsApprovalRolesRepository(IGSCContext context, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<CtmsApprovalRolesGridDto> GetCtmsApprovalWorkFlowList(int projectId, bool isDeleted)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == projectId)
                      .ProjectTo<CtmsApprovalRolesGridDto>(_mapper.ConfigurationProvider)
                      .OrderByDescending(x => x.Id)
                      .ToList();
        }

        public List<CtmsApprovalUsersGridDto> GetCtmsApprovalWorkFlowDetailsList(int projectId, bool isDeleted)
        {
            return _context.CtmsApprovalUsers
                           .Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.CtmsApprovalRoles.ProjectId == projectId)
                           .ProjectTo<CtmsApprovalUsersGridDto>(_mapper.ConfigurationProvider)
                           .OrderByDescending(x => x.Id)
                           .ToList();
        }

        public void ChildUserApprovalAdd(CtmsApprovalRolesDto obj, int id)
        {
            var newUsers = obj.CtmsApprovalUsers
                              .Select(s => s.UserId)
                              .Where(item => !_context.CtmsApprovalUsers.Any(x => x.CtmsApprovalRolesId == id && x.DeletedDate == null && x.UserId == item))
                              .Select(item => new CtmsApprovalUsers { UserId = item, CtmsApprovalRolesId = id });

            if (newUsers.Any())
            {
                _context.CtmsApprovalUsers.AddRange(newUsers);
                _context.Save();
            }
        }

        public void DeleteChildWorkflowEmailUser(int id)
        {
            var list = _context.CtmsApprovalUsers.Where(s => s.DeletedDate == null && s.CtmsApprovalRolesId == id).ToList();
            if (list.Any())
            {
                _context.CtmsApprovalUsers.RemoveRange(list);
                _context.Save();
            }
        }

        public string Duplicate(CtmsApprovalRolesDto obj)
        {
            var isDuplicate = obj.CtmsApprovalUsers
                                 .Any(item => _context.CtmsApprovalUsers
                                                    .Include(x => x.CtmsApprovalRoles)
                                                    .Any(c => c.Id != obj.Id && c.UserId == item.UserId && c.CtmsApprovalRoles.SecurityRoleId == obj.SecurityRoleId
                                                         && c.CtmsApprovalRoles.ProjectId == obj.ProjectId && c.DeletedDate == null
                                                         && c.CtmsApprovalRoles.DeletedDate == null && c.CtmsApprovalRoles.TriggerType == obj.TriggerType));

            return isDuplicate ? "already assigned for this role!" : "";
        }

        public List<DropDownDto> GetRoleCtmsRights(int projectId)
        {
            return _context.UserAccess.Include(s => s.UserRole).ThenInclude(d => d.SecurityRole)
                                      .Where(w => w.DeletedBy == null && w.ParentProjectId == projectId && w.ProjectId == projectId)
                                      .Select(x => new DropDownDto { Id = x.UserRole.SecurityRole.Id, Value = x.UserRole.SecurityRole.RoleName })
                                      .Distinct()
                                      .ToList();
        }

        public List<DropDownDto> GetUserCtmsRights(int roleId, int projectId, int siteId)
        {
            return _context.UserAccess.Include(s => s.UserRole).ThenInclude(d => d.User)
                                      .Where(w => w.DeletedBy == null && w.ParentProjectId == projectId && (siteId > 0 ? w.ProjectId == siteId : w.ProjectId == projectId)
                                            && w.UserRole.SecurityRole.Id == roleId && w.UserRole.UserId != _jwtTokenAccesser.UserId)
                                      .Select(x => new DropDownDto { Id = x.UserRole.User.Id, Value = x.UserRole.User.UserName })
                                      .Distinct()
                                      .ToList();
        }

        public bool CheckIsApprover(int projectId, TriggerType triggerType)
        {
            return _context.CtmsApprovalUsers
                           .Any(x => x.DeletedDate == null && x.CtmsApprovalRoles.ProjectId == projectId
                                  && x.CtmsApprovalRoles.TriggerType == triggerType && x.UserId == _jwtTokenAccesser.UserId);
        }

        public bool CheckIsApproverForSiteContract(int projectId, int siteId, TriggerType triggerType)
        {
            return _context.CtmsApprovalUsers
                           .Any(x => x.DeletedDate == null && x.CtmsApprovalRoles.ProjectId == projectId && x.CtmsApprovalRoles.SiteId == siteId
                                  && x.CtmsApprovalRoles.TriggerType == triggerType && x.UserId == _jwtTokenAccesser.UserId);
        }

        public List<DropDownDto> GetSiteList(int projectId)
        {
            return _context.UserAccess.Where(w => w.DeletedDate == null && w.ParentProjectId == projectId && w.ParentProjectId != w.ProjectId && w.Project.DeletedDate == null
                                            && w.UserRole.SecurityRole.Id == _jwtTokenAccesser.RoleId && w.UserRole.UserId == _jwtTokenAccesser.UserId)
                                      .Select(x => new DropDownDto { Id = x.ProjectId, Value = x.Project.ManageSite.SiteName })
                                      .Distinct()
                                      .ToList();
        }
    }
}
