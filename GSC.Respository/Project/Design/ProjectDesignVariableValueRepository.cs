using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignVariableValueRepository : GenericRespository<ProjectDesignVariableValue>,
        IProjectDesignVariableValueRepository
    {
        public ProjectDesignVariableValueRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) :
            base(context)
        {
        }

        public IList<DropDownDto> GetProjectDesignVariableValueDropDown(int projectDesignVariableId)
        {
            return All.Where(x => x.DeletedDate == null &&
                                  x.ProjectDesignVariableId == projectDesignVariableId).OrderBy(o => o.SeqNo)
                .Select(c => new DropDownDto
                {
                    Id = c.Id, Value = c.ValueName, Code = c.ValueCode,
                    ExtraData = c.SeqNo
                }).OrderBy(o => o.ExtraData).ToList();
        }
    }
}