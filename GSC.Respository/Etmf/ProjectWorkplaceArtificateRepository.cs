using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
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
                    FolderId = y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId,
                    FolderName = ((WorkPlaceFolder)y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription()
                    + " - " + y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemName,
                    ItemId = y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemId,
                    ParentArtificateId = y.ParentArtificateId
                }).ToList();

            var Trial = result.Where(x => x.FolderId == 3).ToList();
            var Country = result.Where(x => x.FolderId == 1).OrderBy(x => x.ItemId).ToList();
            var site = result.Where(x => x.FolderId == 2).OrderBy(x => x.ItemId).ToList();
            List<WorkplaceFolderDto> obj = new List<WorkplaceFolderDto>();
            obj.AddRange(Trial);
            obj.AddRange(Country);
            obj.AddRange(site);

            return obj;
        }

        public WorkplaceChartDto GetDocChart(WorkplaceChartFilterDto filters)
        {
            WorkplaceChartDto result = new WorkplaceChartDto();
            var Artificate = new List<ProjectWorkplaceArtificate>();

            var WorkPlaceDetails = _context.ProjectWorkplaceDetail.Where(x => x.ProjectWorkplace.ProjectId == filters.ProjectId).ToList();
            var rightsWorkplace = new List<int>();
            foreach (var item in WorkPlaceDetails)
            {
                var rights = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId == item.Id && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();
                if (rights != null && rights.IsView)
                {
                    rightsWorkplace.Add(item.Id);
                }
            }

            if (filters.SubSectionId == null || filters.SubSectionId == 0)
            {
                Artificate = All.Include(y => y.ProjectWorkplaceArtificatedocument).ThenInclude(y => y.ProjectArtificateDocumentReview)
                    .Include(y => y.ProjectWorkplaceArtificatedocument).ThenInclude(y => y.ProjectArtificateDocumentApprover)
                    .Include(y => y.EtmfArtificateMasterLbrary).Include(y => y.ProjectWorkplaceSection)
                    .ThenInclude(y => y.ProjectWorkPlaceZone)
                    .ThenInclude(y => y.ProjectWorkplaceDetail)
                    .ThenInclude(y => y.ProjectWorkplace)
                    .Where(y => y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == filters.ProjectId
                    && rightsWorkplace.Contains(y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetailId)
                    && y.DeletedDate == null).ToList();
            }

            if (filters.WorkPlaceFolderId > 0) Artificate = Artificate.Where(y => y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId == filters.WorkPlaceFolderId).ToList();
            if (filters.ZoneId > 0) Artificate = Artificate.Where(y => y.ProjectWorkplaceSection.ProjectWorkPlaceZone.Id == filters.ZoneId).ToList();
            if (filters.SectionId > 0) Artificate = Artificate.Where(y => y.ProjectWorkplaceSection.Id == filters.SectionId).ToList();
            if (filters.ArtificateId > 0) Artificate = Artificate.Where(y => y.Id == filters.ArtificateId).ToList();

            var SubSectionArtificate = _context.ProjectWorkplaceSubSectionArtifact.Include(t => t.ProjectWorkplaceSubSecArtificatedocument)
                .ThenInclude(x => x.ProjectSubSecArtificateDocumentReview)
                .Include(y => y.ProjectWorkplaceSubSecArtificatedocument).ThenInclude(y => y.ProjectSubSecArtificateDocumentApprover)
                .Include(x => x.ProjectWorkplaceSubSection)
                .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace)
                .Where(y => y.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == filters.ProjectId
                && rightsWorkplace.Contains(y.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetailId)
                && y.DeletedDate == null && y.ProjectWorkplaceSubSection.DeletedDate == null).ToList();

            if (filters.WorkPlaceFolderId > 0) SubSectionArtificate = SubSectionArtificate.Where(y => y.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId == filters.WorkPlaceFolderId).ToList();
            if (filters.ZoneId > 0) SubSectionArtificate = SubSectionArtificate.Where(y => y.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.Id == filters.ZoneId).ToList();
            if (filters.SectionId > 0) SubSectionArtificate = SubSectionArtificate.Where(y => y.ProjectWorkplaceSubSection.ProjectWorkplaceSection.Id == filters.SectionId).ToList();
            if (filters.SubSectionId > 0) SubSectionArtificate = SubSectionArtificate.Where(y => y.ProjectWorkplaceSubSection.Id == filters.SubSectionId).ToList();
            if (filters.SubSectionArtificateId > 0) SubSectionArtificate = SubSectionArtificate.Where(y => y.Id == filters.SubSectionArtificateId).ToList();

            result.All = Artificate.Count() + SubSectionArtificate.Count();
            result.Missing = Artificate.Where(y => y.ProjectWorkplaceArtificatedocument.Count == 0).Count()
                + SubSectionArtificate.Where(y => y.ProjectWorkplaceSubSecArtificatedocument.Count == 0).Count();

            result.AllPendingApprove = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(y => y.ProjectArtificateDocumentApprover.Count() != 0)).Count() +
                 SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(y => y.ProjectSubSecArtificateDocumentApprover.Count != 0)).Count();

            result.PendingApprove = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(y => y.ProjectArtificateDocumentApprover.Count() != 0 && y.ProjectArtificateDocumentApprover.Any(c => c.IsApproved == null))).Count() +
                SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(y => y.ProjectSubSecArtificateDocumentApprover.Count() != 0 && y.ProjectSubSecArtificateDocumentApprover.Any(c => c.IsApproved == null))).Count();

            result.Final = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(y => y.Status == ArtifactDocStatusType.Final)).Count() +
                SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(y => y.Status == ArtifactDocStatusType.Final)).Count();

            //result.InComplete = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(y => y.ProjectArtificateDocumentReview.Where(z => z.UserId != y.CreatedBy).Count() == 0)).Count()
            //    + SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(y => y.ProjectSubSecArtificateDocumentReview.Where(z => z.UserId != y.CreatedBy).Count() == 0)).Count();

            result.InComplete = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Where(z => z.ProjectArtificateDocumentReview.Any(y => y.UserId != z.CreatedBy)).Count() != 0).Count() +
                SubSectionArtificate.Where(x=>x.ProjectWorkplaceSubSecArtificatedocument.Where(z => z.ProjectSubSecArtificateDocumentReview.Any(y => y.UserId != z.CreatedBy)).Count() != 0).Count();

            result.PendingReview = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(y => y.ProjectArtificateDocumentReview.Count != 0 && y.ProjectArtificateDocumentReview.GroupBy(z => z.UserId).LastOrDefault().Where(v => v.IsSendBack == false && v.ModifiedDate == null && v.UserId != y.CreatedBy).Count() != 0)).Count() +
                SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(y => y.ProjectSubSecArtificateDocumentReview.Count != 0 && y.ProjectSubSecArtificateDocumentReview.GroupBy(z => z.UserId).LastOrDefault().Where(v => v.IsSendBack == false && v.ModifiedDate == null && v.UserId != y.CreatedBy).Count() != 0)).Count();

            result.AllPendingReview = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(x => x.ProjectArtificateDocumentReview.Where(y => y.UserId != x.CreatedBy).Count() != 0)).Count() +
                SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(x => x.ProjectSubSecArtificateDocumentReview.Where(y => y.UserId != x.CreatedBy).Count() != 0)).Count();

            result.NotRequired = Artificate.Where(x => x.IsNotRequired == true).Count()
                          + SubSectionArtificate.Where(x => x.IsNotRequired == true).Count();

            result.CoreArtificate = Artificate.Where(x => x.EtmfArtificateMasterLbrary.InclutionType == 2 && x.ProjectWorkplaceArtificatedocument.Count() == 0).Count();
            result.RecommendedArtificate = Artificate.Where(x => x.EtmfArtificateMasterLbrary.InclutionType == 1 && x.ProjectWorkplaceArtificatedocument.Count() == 0).Count();

            return result;
        }
    }
}
