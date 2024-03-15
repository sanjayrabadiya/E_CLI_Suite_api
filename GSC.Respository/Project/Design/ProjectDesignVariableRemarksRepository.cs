using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignVariableRemarksRepository : GenericRespository<ProjectDesignVariableRemarks>, IProjectDesignVariableRemarksRepository
    {
        public ProjectDesignVariableRemarksRepository(IGSCContext context) :
            base(context)
        {
        }

        public IList<DropDownDto> GetProjectDesignVariableRemarksDropDown(int projectDesignVariableId)
        {
            return All.Where(x => x.DeletedDate == null &&
                                  x.ProjectDesignVariableId == projectDesignVariableId)
                .Select(c => new DropDownDto
                {
                    Id = c.Id,
                    Value = c.Remarks
                }).OrderBy(o => o.ExtraData).ToList();
        }
    }
}
