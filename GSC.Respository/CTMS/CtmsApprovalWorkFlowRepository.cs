using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class CtmsApprovalWorkFlowRepository : GenericRespository<CtmsApprovalWorkFlow>, ICtmsApprovalWorkFlowRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
 
        public CtmsApprovalWorkFlowRepository(IGSCContext context,
            IMapper mapper) : base(context)
        {
            _mapper = mapper;
            _context = context;
        }
        public List<CtmsApprovalWorkFlowGridDto> GetCtmsApprovalWorkFlowList(int projectId, bool isDeleted)
        {
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectId == projectId).
                    ProjectTo<CtmsApprovalWorkFlowGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(t =>
            {
                if (t.RoleId > 0)
                {
                    var role = _context.SecurityRole.FirstOrDefault(x => x.Id == t.RoleId);
                    if (role != null)
                        t.RoleName = role.RoleName;
                }
                var Users = _context.CtmsApprovalWorkFlowDetail.Where(x => x.DeletedDate == null && x.CtmsApprovalWorkFlowId == t.Id).Select(a => a.Users.UserName).ToList();

                if (Users.Count > 0)
                {
                    t.Users = string.Join(",", Users.Distinct());
                }
            });

            return data;
        }
        public void ChildUserApprovalAdd(CtmsApprovalWorkFlowDto obj, int id)
        {
            if (obj.CtmsApprovalWorkFlowDetails.Count > 0)
            {
                foreach (var item in obj.CtmsApprovalWorkFlowDetails.Select(s => s.UserId))
                {
                    var data = _context.CtmsApprovalWorkFlowDetail.Where(x => x.CtmsApprovalWorkFlowId == id && x.DeletedDate == null && x.UserId == item).FirstOrDefault();
                    if (data == null)
                    {
                        CtmsApprovalWorkFlowDetail detail = new CtmsApprovalWorkFlowDetail();
                        detail.UserId = item;
                        detail.CtmsApprovalWorkFlowId = id;
                        _context.CtmsApprovalWorkFlowDetail.Add(detail);
                        _context.Save();
                    }
                    else
                    {
                        var data1 = _context.CtmsApprovalWorkFlowDetail.Where(x => x.CtmsApprovalWorkFlowId == id && x.DeletedDate != null && x.UserId == item).FirstOrDefault();
                        if (data1 != null)
                        {
                            data1.DeletedBy = null;
                            data1.DeletedDate = null;
                            _context.CtmsApprovalWorkFlowDetail.Add(data1);
                            _context.Save();
                        }
                    }

                }
            }
        }

        public void DelectChildWorkflowEmailUser(CtmsApprovalWorkFlowDto obj, int id)
        {
            var list = _context.CtmsApprovalWorkFlowDetail.Where(s => s.DeletedDate == null && s.CtmsApprovalWorkFlowId == id).ToList();
            if (list.Count > 0)
            {
                _context.CtmsApprovalWorkFlowDetail.RemoveRange(list);
                _context.Save();
            }
        }

        public string Duplicate(CtmsApprovalWorkFlowDto obj)
        {
            if (obj.Id > 0)
            {
                var data = _context.CtmsApprovalWorkFlow.Where(x => x.Id != obj.Id && x.ProjectId == obj.ProjectId && x.DeletedDate == null && x.SecurityRoleId == obj.RoleId).FirstOrDefault();
                if (data != null)
                {
                    return "already assigned for this role!";
                }
            }
            else
            {
                var data = _context.CtmsApprovalWorkFlow.Where(x => x.ProjectId == obj.ProjectId && x.DeletedDate == null && x.SecurityRoleId == obj.RoleId && x.TriggerType == obj.TriggerType).FirstOrDefault();
                if (data != null)
                {
                    return "already assigned for this role!";
                }
            }
            return "";
        }

        public List<DropDownDto> GetRoleCtmsRights(int projectId)
        {
            var projectrights = _context.UserAccess.Include(s=> s.UserRole).ThenInclude(d=>d.SecurityRole).Where(w=>w.DeletedBy== null && w.ParentProjectId == projectId && w.ProjectId== projectId).
                        Select(x=> new DropDownDto { Id=x.UserRole.SecurityRole.Id, Value = x.UserRole.SecurityRole.RoleName }).Distinct().ToList();

            return projectrights;
        }
        public List<DropDownDto> GetUserCtmsRights(int roleId, int projectId)
        {

            var projectrights = _context.UserAccess.Include(s => s.UserRole).ThenInclude(d => d.User).
                   Where(w => w.DeletedBy == null && w.ParentProjectId == projectId && w.ProjectId == projectId && w.UserRole.SecurityRole.Id == roleId).
                   Select(x => new DropDownDto { Id = x.UserRole.User.Id, Value = x.UserRole.User.UserName }).Distinct().ToList();

            return projectrights;

        }
    }
}
