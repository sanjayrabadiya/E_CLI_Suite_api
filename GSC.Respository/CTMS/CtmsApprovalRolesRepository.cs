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

        public CtmsApprovalRolesRepository(IGSCContext context,
            IMapper mapper, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
        public List<CtmsApprovalRolesGridDto> GetCtmsApprovalWorkFlowList(int projectId, bool isDeleted)
        {
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectId == projectId).
                    ProjectTo<CtmsApprovalRolesGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return data;
        }

        public List<CtmsApprovalUsersGridDto> GetCtmsApprovalWorkFlowDetailsList(int projectId, bool isDeleted)
        {
            var data = _context.CtmsApprovalUsers.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.CtmsApprovalRoles.ProjectId == projectId).
                    ProjectTo<CtmsApprovalUsersGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return data;
        }
        public void ChildUserApprovalAdd(CtmsApprovalRolesDto obj, int id)
        {
            if (obj.CtmsApprovalUsers.Count > 0)
            {
                foreach (var item in obj.CtmsApprovalUsers.Select(s => s.UserId))
                {
                    var data = _context.CtmsApprovalUsers.Any(x => x.CtmsApprovalRolesId == id
                    && x.DeletedDate == null && x.UserId == item);
                    if (!data)
                    {
                        CtmsApprovalUsers detail = new CtmsApprovalUsers();
                        detail.UserId = item;
                        detail.CtmsApprovalRolesId = id;
                        _context.CtmsApprovalUsers.Add(detail);
                        _context.Save();
                    }
                }
            }
        }

        public void DeleteChildWorkflowEmailUser(CtmsApprovalRolesDto obj, int id)
        {
            var list = _context.CtmsApprovalUsers.Where(s => s.DeletedDate == null && s.CtmsApprovalRolesId == id).ToList();
            if (list.Count > 0)
            {
                _context.CtmsApprovalUsers.RemoveRange(list);
                _context.Save();
            }
        }

        public string Duplicate(CtmsApprovalRolesDto obj)
        {
            if (obj.Id > 0)
            {
                foreach (var item in obj.CtmsApprovalUsers)
                {
                    var checkExist = _context.CtmsApprovalUsers.Include(x => x.CtmsApprovalRoles)
                        .Any(c => c.Id != obj.Id && c.UserId == item.UserId && c.CtmsApprovalRoles.SecurityRoleId == obj.SecurityRoleId
                        && c.CtmsApprovalRoles.ProjectId == obj.ProjectId && c.DeletedDate == null && c.CtmsApprovalRoles.DeletedDate == null && c.CtmsApprovalRoles.TriggerType == obj.TriggerType);

                    if (checkExist)
                    {
                        return "already assigned for this role!";
                    }
                }
            }
            else
            {
                foreach (var item in obj.CtmsApprovalUsers)
                {
                    var checkExist = _context.CtmsApprovalUsers.Include(x => x.CtmsApprovalRoles)
                        .Any(c => c.UserId == item.UserId && c.CtmsApprovalRoles.SecurityRoleId == obj.SecurityRoleId
                        && c.CtmsApprovalRoles.ProjectId == obj.ProjectId && c.DeletedDate == null && c.CtmsApprovalRoles.DeletedDate == null && c.CtmsApprovalRoles.TriggerType == obj.TriggerType);

                    if (checkExist)
                    {
                        return "already assigned for this role!";
                    }
                }
            }
            return "";
        }

        public List<DropDownDto> GetRoleCtmsRights(int projectId)
        {
            var projectrights = _context.UserAccess.Include(s => s.UserRole).ThenInclude(d => d.SecurityRole).Where(w => w.DeletedBy == null && w.ParentProjectId == projectId && w.ProjectId == projectId).
                        Select(x => new DropDownDto { Id = x.UserRole.SecurityRole.Id, Value = x.UserRole.SecurityRole.RoleName }).Distinct().ToList();

            return projectrights;
        }
        public List<DropDownDto> GetUserCtmsRights(int roleId, int projectId)
        {

            var projectrights = _context.UserAccess.Include(s => s.UserRole).ThenInclude(d => d.User).
                   Where(w => w.DeletedBy == null && w.ParentProjectId == projectId && w.ProjectId == projectId && w.UserRole.SecurityRole.Id == roleId && w.UserRole.UserId != _jwtTokenAccesser.UserId).
                   Select(x => new DropDownDto { Id = x.UserRole.User.Id, Value = x.UserRole.User.UserName }).Distinct().ToList();

            return projectrights;

        }

        public bool CheckIsApprover(int projectId,TriggerType triggerType)
        {
            var isPresent = _context.CtmsApprovalUsers.Any(x => x.DeletedDate == null && x.CtmsApprovalRoles.ProjectId == projectId
            && x.CtmsApprovalRoles.TriggerType == triggerType && x.UserId == _jwtTokenAccesser.UserId);
            return isPresent;
        }
    }
}
