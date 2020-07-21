using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceSectionRepository: GenericRespository<ProjectWorkplaceSection, GscContext>, IProjectWorkplaceSectionRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ProjectWorkplaceSectionRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser)
           : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetProjectWorkPlaceSectionDropDown(int zoneId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.ProjectWorkPlaceZoneId == zoneId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.EtmfSectionMasterLibrary.SectionName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}
