﻿using GSC.Common.GenericRespository;
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
                    FolderName = ((WorkPlaceFolder)y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription()
                    + " - " + y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemName,
                    ParentArtificateId = y.ParentArtificateId
                }).ToList();

            return result;
        }

        public WorkplaceChartDto GetRedChart(int id)
        {
            WorkplaceChartDto result = new WorkplaceChartDto();
            var Artificate = All.Include(y => y.ProjectWorkplaceArtificatedocument).Include(y => y.ProjectWorkplaceSection)
                .ThenInclude(y => y.ProjectWorkPlaceZone)
                .ThenInclude(y => y.ProjectWorkplaceDetail)
                .ThenInclude(y => y.ProjectWorkplace)
                .Where(y => y.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == id
                ).ToList();

            var ArtificateDocument = _context.ProjectWorkplaceArtificatedocument.Include(x => x.ProjectArtificateDocumentApprover)
                .Include(x => x.ProjectArtificateDocumentReview).Include(x => x.ProjectArtificateDocumentComment)
                .Include(x => x.ProjectArtificateDocumentHistory).Include(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.ProjectWorkplaceSection)
                .ThenInclude(y => y.ProjectWorkPlaceZone)
                .ThenInclude(y => y.ProjectWorkplaceDetail)
                .ThenInclude(y => y.ProjectWorkplace)
                .Where(y => y.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == id
                ).ToList();

            var SubSecArtificateDocument = _context.ProjectWorkplaceSubSecArtificatedocument.Include(x => x.ProjectSubSecArtificateDocumentApprover)
                .Include(x => x.ProjectSubSecArtificateDocumentReview).Include(x => x.ProjectSubSecArtificateDocumentComment)
                .Include(x => x.ProjectSubSecArtificateDocumentHistory).Include(x => x.ProjectWorkplaceSubSectionArtifact)
                .ThenInclude(x => x.ProjectWorkplaceSubSection)
                .ThenInclude(x => x.ProjectWorkplaceSection)
                .ThenInclude(y => y.ProjectWorkPlaceZone)
                .ThenInclude(y => y.ProjectWorkplaceDetail)
                .ThenInclude(y => y.ProjectWorkplace)
                .Where(y => y.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == id
                ).ToList();

            var SubSectionArtificate = _context.ProjectWorkplaceSubSectionArtifact.Include(t => t.ProjectWorkplaceSubSecArtificatedocument)
                .Include(x => x.ProjectWorkplaceSubSection)
                .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace)
                .Where(y => y.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == id
                ).ToList();

            result.All = Artificate.Count() + SubSectionArtificate.Count();
            result.Missing = Artificate.Where(y => y.ProjectWorkplaceArtificatedocument.Count == 0).Count()
                + SubSectionArtificate.Where(y => y.ProjectWorkplaceSubSecArtificatedocument.Count == 0).Count();

            result.AllPendingApprove = ArtificateDocument.Where(x => x.ProjectArtificateDocumentApprover.Count() != 0).Count()
                + SubSecArtificateDocument.Where(x => x.ProjectSubSecArtificateDocumentApprover.Count != 0).Count();
            result.PendingApprove = ArtificateDocument.Where(x => x.ProjectArtificateDocumentApprover.Count() != 0 && x.IsAccepted == null).Count()
                + SubSecArtificateDocument.Where(x => x.ProjectSubSecArtificateDocumentApprover.Count() != 0 && x.IsAccepted == null).Count();

            result.AllDocument = ArtificateDocument.Count() + SubSecArtificateDocument.Count();
            result.Final = ArtificateDocument.Where(x => x.Status == ArtifactDocStatusType.Final).Count() + SubSecArtificateDocument.Where(x => x.Status == ArtifactDocStatusType.Final).Count();

            result.InComplete = ArtificateDocument.Where(x => x.ProjectArtificateDocumentReview.Count() == 0).Count() +
                    SubSecArtificateDocument.Where(x => x.ProjectSubSecArtificateDocumentReview.Count() == 0).Count();

            result.PendingReview = ArtificateDocument.Where(x => x.ProjectArtificateDocumentReview.Count != 0 && x.ProjectArtificateDocumentReview.GroupBy(x => x.UserId).LastOrDefault().Where(x => x.IsSendBack == false && x.ModifiedDate == null).Count() != 0).Count()
                + SubSecArtificateDocument.Where(x => x.ProjectSubSecArtificateDocumentReview.Count != 0 && x.ProjectSubSecArtificateDocumentReview.GroupBy(x => x.UserId).LastOrDefault().Where(x => x.IsSendBack == false && x.ModifiedDate == null).Count() != 0).Count(); 
            result.AllPendingReview = ArtificateDocument.Where(x => x.ProjectArtificateDocumentReview.Count != 0).Count()
                + SubSecArtificateDocument.Where(x => x.ProjectSubSecArtificateDocumentReview.Count != 0).Count();
            
            return result;
        }
    }
}
