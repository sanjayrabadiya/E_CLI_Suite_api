using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceArtificateRepository : GenericRespository<ProjectWorkplaceArtificate>, IProjectWorkplaceArtificateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public ProjectWorkplaceArtificateRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser)
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public List<DropDownDto> GetProjectWorkPlaceArtificateDropDown(int sectionId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.ProjectWorkplaceSectionId == sectionId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.EtmfArtificateMasterLbrary.ArtificateName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public List<WorkplaceFolderDto> GetWorkPlaceFolder(int EtmfArtificateMasterLbraryId, int ProjectWorkplaceArtificateId)
        {
            var ParentArtificateId = All.Where(x => x.Id == ProjectWorkplaceArtificateId).FirstOrDefault().ParentArtificateId;

            var ProjectId = All.Where(x => x.Id == ProjectWorkplaceArtificateId).Include(y => y.ProjectWorkplaceSection)
                .ThenInclude(y => y.ProjectWorkPlaceZone)
                .ThenInclude(y => y.ProjectWorkplaceDetail).ThenInclude(y => y.ProjectWorkplace).FirstOrDefault();

            var result = All.Where(x => x.EtmfArtificateMasterLbraryId == EtmfArtificateMasterLbraryId && x.Id != ProjectWorkplaceArtificateId
                 && x.Id != ParentArtificateId).Include(y => y.ProjectWorkplaceSection)
                .ThenInclude(y => y.ProjectWorkPlaceZone)
                .ThenInclude(y => y.ProjectWorkplaceDetail).ThenInclude(y => y.ProjectWorkplace)
                .Where(y => y.ParentArtificateId == null &&
                y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId
                == ProjectId.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId)
                .Select(y => new WorkplaceFolderDto
                {
                    ProjectWorkplaceArtificateId = y.Id,
                    EtmfArtificateMasterLbraryId = y.EtmfArtificateMasterLbraryId,
                    FolderName = ((WorkPlaceFolder)y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription()
                    + " - " + y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemName,
                    ParentArtificateId = y.ParentArtificateId
                }).ToList();

            return result;
        }

        public WorkplaceChartDto GetRedChart(int id)
        {
            WorkplaceChartDto result = new WorkplaceChartDto();
            var ArtificateDoc = All.Include(y => y.ProjectWorkplaceArtificatedocument).Include(y => y.ProjectWorkplaceSection)
                .ThenInclude(y => y.ProjectWorkPlaceZone)
                .ThenInclude(y => y.ProjectWorkplaceDetail)
                .ThenInclude(y => y.ProjectWorkplace)
                .Where(y => y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == id
                ).ToList();

            var SubSectionArtificateDoc = _context.ProjectWorkplaceSubSectionArtifact.Include(t => t.ProjectWorkplaceSubSecArtificatedocument)
                .Include(x => x.ProjectWorkplaceSubSection)
                .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace)
                .Where(y => y.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == id
                ).ToList();

            result.All = ArtificateDoc.Count() + SubSectionArtificateDoc.Count();
            result.Missing = ArtificateDoc.Where(y => y.ProjectWorkplaceArtificatedocument.Count == 0).Count()
                + SubSectionArtificateDoc.Where(y => y.ProjectWorkplaceSubSecArtificatedocument.Count == 0).Count();

            return result;
        }
    }
}
