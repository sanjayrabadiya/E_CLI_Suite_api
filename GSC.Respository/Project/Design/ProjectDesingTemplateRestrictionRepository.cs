using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesingTemplateRestrictionRepository : GenericRespository<Data.Entities.Project.Design.ProjectDesingTemplateRestriction>, IProjectDesingTemplateRestrictionRepository
    {
        private readonly IGSCContext _context;
        private readonly IRoleRepository _roleRepository;

        public ProjectDesingTemplateRestrictionRepository(IGSCContext context, IRoleRepository roleRepository, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _context = context;
            _roleRepository = roleRepository;
        }

        public List<Data.Dto.Project.Design.ProjectDesingTemplateRestrictionDto> GetProjectDesingTemplateRestrictionDetails(int ProjectDesignTemplateId)
        {
           // Get security role
            var SecurityRole = _context.SecurityRole.Where(t => t.DeletedDate == null && t.Id != 2)
                .Select(t => new ProjectDesingTemplateRestrictionDto
                {
                    SecurityRoleId = t.Id,
                    RoleName = t.RoleName,
                    hasChild = false,
                }).ToList();

            // Get data from permission table if exists and map with user role by template id
            SecurityRole.ForEach(t =>
            {
                var p = All.Where(s => s.ProjectDesignTemplateId == ProjectDesignTemplateId && s.SecurityRoleId == t.SecurityRoleId && s.DeletedDate == null).FirstOrDefault();
                if (p == null) return;
                t.ProjectDesignTemplateId = p.ProjectDesignTemplateId;
                t.IsAdd = p.IsAdd;
                t.IsHide = p.IsHide;
               // t.IsEdit = p.IsEdit;
            });

            return SecurityRole.ToList();
        }

        public void Save(List<Data.Entities.Project.Design.ProjectDesingTemplateRestriction> projectDesingTemplateRestriction)
        {
            // Get only that data which is has add or edit permission
            projectDesingTemplateRestriction = projectDesingTemplateRestriction.Where(t => t.IsAdd || t.IsHide).ToList();
            _context.ProjectDesingTemplateRestriction.AddRange(projectDesingTemplateRestriction);
            _context.Save();
        }

        public void updatePermission(List<Data.Entities.Project.Design.ProjectDesingTemplateRestriction> projectDesingTemplateRestriction)
        {
            foreach (var item in projectDesingTemplateRestriction)
            {
                // Get data from table if already exists 
                var permission = All.Where(x => x.DeletedDate == null && x.SecurityRoleId == item.SecurityRoleId && x.ProjectDesignTemplateId == item.ProjectDesignTemplateId).FirstOrDefault();
                if (permission == null)
                {
                    // if not exists in table than add data
                    if (item.IsAdd || item.IsHide)
                        Add(item);
                }
                else
                {
                    // if exists in table and not any changes than not perform any action on that row
                    if (permission.IsAdd == item.IsAdd && permission.IsHide == item.IsHide) { }
                    else
                    {
                        // if exists in table and than delete first and if any changes than add new row for that record.
                        Delete(permission);
                        if (item.IsAdd || item.IsHide)
                            Add(item);
                    }
                }
            }
            _context.Save();
        }

    }
}
