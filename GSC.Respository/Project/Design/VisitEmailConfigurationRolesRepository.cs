using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.Workflow;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Linq;

namespace GSC.Respository.Project.Design
{
    public class VisitEmailConfigurationRolesRepository : GenericRespository<VisitEmailConfigurationRoles>, IVisitEmailConfigurationRolesRepository
    {
        public VisitEmailConfigurationRolesRepository(IGSCContext context) : base(context)
        {
        }

        public void updateVisitEmailRole(VisitEmailConfigurationDto visitEmailDto)
        {
            var visitEmail = All.Where(r => r.DeletedDate == null && r.VisitEmailConfigurationId == visitEmailDto.Id).ToList();

            //add new
            var firstNotSecond = visitEmailDto.RoleId.Except(visitEmail.Select(x => x.SecurityRoleId)).ToList();
            // delete
            var thirdNotFirst = visitEmail.AsEnumerable().Select(x => x.SecurityRoleId).Except(visitEmailDto.RoleId).ToList();


            foreach (var item in firstNotSecond)
            {
                var result = new VisitEmailConfigurationRoles();
                result.VisitEmailConfigurationId = visitEmailDto.Id;
                result.SecurityRoleId = item;
                Add(result);
            }

            foreach (var item in thirdNotFirst)
            {
                var d = visitEmail.First(x => x.SecurityRoleId == item);
                Delete(d);
            }
        }
    }
}
