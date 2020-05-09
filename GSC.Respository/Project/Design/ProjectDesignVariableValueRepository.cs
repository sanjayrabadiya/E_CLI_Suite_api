using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignVariableValueRepository : GenericRespository<ProjectDesignVariableValue, GscContext>,
        IProjectDesignVariableValueRepository
    {
        public ProjectDesignVariableValueRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser) :
            base(uow, jwtTokenAccesser)
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