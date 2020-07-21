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
    public class ProjectWorkplaceArtificateRepository : GenericRespository<ProjectWorkplaceArtificate, GscContext>, IProjectWorkplaceArtificateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ProjectWorkplaceArtificateRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser)
           : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetProjectWorkPlaceArtificateDropDown(int sectionId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.ProjectWorkplaceSectionId == sectionId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.EtmfArtificateMasterLbrary.ArtificateName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}
