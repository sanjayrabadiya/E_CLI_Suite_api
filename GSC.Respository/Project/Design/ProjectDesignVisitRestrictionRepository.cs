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
    public class ProjectDesignVisitRestrictionRepository : GenericRespository<ProjectDesignVisitRestriction>, IProjectDesignVisitRestrictionRepository
    {
        private readonly IGSCContext _context;
        private readonly IRoleRepository _roleRepository;

        public ProjectDesignVisitRestrictionRepository(IGSCContext context, IRoleRepository roleRepository, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _context = context;
            _roleRepository = roleRepository;
        }

        public List<ProjectDesignVisitRestrictionDto> GetProjectDesignVisitRestrictionDetails(int ProjectDesignVisitId)
        {
            // Get security role
            var SecurityRole = _context.SecurityRole.Where(t => t.DeletedDate == null && t.Id != 2)
                .Select(t => new ProjectDesignVisitRestrictionDto
                {
                    SecurityRoleId = t.Id,
                    RoleName = t.RoleName,
                    hasChild = false,
                }).ToList();

            // Get data from permission table if exists and map with user role by Visit id
            SecurityRole.ForEach(t =>
            {
                var p = All.Where(s => s.ProjectDesignVisitId == ProjectDesignVisitId && s.SecurityRoleId == t.SecurityRoleId && s.DeletedDate == null).FirstOrDefault();
                if (p == null) return;
                t.ProjectDesignVisitId = p.ProjectDesignVisitId;
                t.IsAdd = p.IsAdd;
                // t.IsEdit = p.IsEdit;
            });

            return SecurityRole.ToList();
        }

        public void Save(List<ProjectDesignVisitRestriction> projectDesignVisitRestriction)
        {
            // Get only that data which is has add or edit permission
            projectDesignVisitRestriction = projectDesignVisitRestriction.Where(t => t.IsAdd).ToList();
            _context.ProjectDesignVisitRestriction.AddRange(projectDesignVisitRestriction);
            _context.Save();
        }

        public void updatePermission(List<ProjectDesignVisitRestriction> projectDesignVisitRestriction)
        {
            foreach (var item in projectDesignVisitRestriction)
            {
                // Get data from table if already exists 
                var permission = All.Where(x => x.DeletedDate == null && x.SecurityRoleId == item.SecurityRoleId && x.ProjectDesignVisitId == item.ProjectDesignVisitId).FirstOrDefault();
                if (permission == null)
                {
                    // if not exists in table than add data
                    if (item.IsAdd || item.IsAdd)
                        Add(item);
                }
                else
                {
                    // if exists in table and not any changes than not perform any action on that row
                    if (permission.IsAdd == item.IsAdd) { }
                    else
                    {
                        // if exists in table and than delete first and if any changes than add new row for that record.
                        Delete(permission);
                        if (item.IsAdd || item.IsAdd)
                            Add(item);
                    }
                }
            }
            _context.Save();
        }
    }
}
