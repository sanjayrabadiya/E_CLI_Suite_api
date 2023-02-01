using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceArtificateRepository : GenericRespository<EtmfProjectWorkPlace>, IProjectWorkplaceArtificateRepository
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
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.EtmfProjectWorkPlaceId == sectionId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.EtmfArtificateMasterLbrary.ArtificateName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public List<WorkplaceFolderDto> GetWorkPlaceFolder(int EtmfArtificateMasterLbraryId, int ProjectWorkplaceArtificateId)
        {
            var ParentArtificateId = All.Where(x => x.Id == ProjectWorkplaceArtificateId).FirstOrDefault().ParentArtificateId;

            var ProjectId = All.Where(x => x.Id == ProjectWorkplaceArtificateId).Include(y => y.ProjectWorkPlace)
                .ThenInclude(y => y.ProjectWorkPlace)
                .ThenInclude(y => y.ProjectWorkPlace).ThenInclude(y => y.ProjectWorkPlace).FirstOrDefault();

            var result = All.Where(x => x.EtmfArtificateMasterLbraryId == EtmfArtificateMasterLbraryId && x.Id != ProjectWorkplaceArtificateId
                 && x.Id != ParentArtificateId).Include(y => y.ProjectWorkPlace)
                .ThenInclude(y => y.ProjectWorkPlace)
                .ThenInclude(y => y.ProjectWorkPlace).ThenInclude(y => y.ProjectWorkPlace)
                .Where(y => y.ParentArtificateId == null &&
                y.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectId
                == ProjectId.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectId)
                .Select(y => new WorkplaceFolderDto
                {
                    ProjectWorkplaceArtificateId = y.Id,
                    EtmfArtificateMasterLbraryId = y.EtmfArtificateMasterLbraryId,
                    FolderId = y.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId,
                    FolderName = ((WorkPlaceFolder)y.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription()
                    + " - " + y.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                    ItemId = y.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemId,
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
            var Artificate = new List<EtmfProjectWorkPlace>();

            var WorkPlaceDetails = _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == filters.ProjectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceDetail).ToList();
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
                    .Include(y => y.EtmfArtificateMasterLbrary).Include(y => y.ProjectWorkPlace)
                    .ThenInclude(y => y.ProjectWorkPlace)
                    .ThenInclude(y => y.ProjectWorkPlace)
                    .ThenInclude(y => y.ProjectWorkPlace)
                    .Where(y => y.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectId == filters.ProjectId
                    && rightsWorkplace.Contains(y.ProjectWorkPlace.ProjectWorkPlace.EtmfProjectWorkPlaceId)
                    && y.DeletedDate == null).ToList();
            }

            if (filters.WorkPlaceFolderId > 0) Artificate = Artificate.Where(y => y.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId == filters.WorkPlaceFolderId).ToList();
            if (filters.ZoneId > 0) Artificate = Artificate.Where(y => y.ProjectWorkPlace.ProjectWorkPlace.Id == filters.ZoneId).ToList();
            if (filters.SectionId > 0) Artificate = Artificate.Where(y => y.ProjectWorkPlace.Id == filters.SectionId).ToList();
            if (filters.ArtificateId > 0) Artificate = Artificate.Where(y => y.Id == filters.ArtificateId).ToList();

            var SubSectionArtificate = _context.EtmfProjectWorkPlace.Include(t => t.ProjectWorkplaceSubSecArtificatedocument)
                .ThenInclude(x => x.ProjectSubSecArtificateDocumentReview)
                .Include(y => y.ProjectWorkplaceSubSecArtificatedocument).ThenInclude(y => y.ProjectSubSecArtificateDocumentApprover)
                .Include(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace)
                .Where(y => y.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectId == filters.ProjectId
                && rightsWorkplace.Contains(y.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfProjectWorkPlaceId)
                && y.DeletedDate == null && y.ProjectWorkPlace.DeletedDate == null).ToList();

            if (filters.WorkPlaceFolderId > 0) SubSectionArtificate = SubSectionArtificate.Where(y => y.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId == filters.WorkPlaceFolderId).ToList();
            if (filters.ZoneId > 0) SubSectionArtificate = SubSectionArtificate.Where(y => y.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.Id == filters.ZoneId).ToList();
            if (filters.SectionId > 0) SubSectionArtificate = SubSectionArtificate.Where(y => y.ProjectWorkPlace.ProjectWorkPlace.Id == filters.SectionId).ToList();
            if (filters.SubSectionId > 0) SubSectionArtificate = SubSectionArtificate.Where(y => y.ProjectWorkPlace.Id == filters.SubSectionId).ToList();
            if (filters.SubSectionArtificateId > 0) SubSectionArtificate = SubSectionArtificate.Where(y => y.Id == filters.SubSectionArtificateId).ToList();

            result.All = Artificate.Count() + SubSectionArtificate.Count();
            result.Missing = Artificate.Where(y => y.ProjectWorkplaceArtificatedocument.Count(q => q.DeletedDate == null) == 0 && y.IsNotRequired == false).Count()
                + SubSectionArtificate.Where(y => y.ProjectWorkplaceSubSecArtificatedocument.Count(q => q.DeletedDate == null) == 0 && y.IsNotRequired == false).Count();

            result.AllPendingApprove = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(y => y.ProjectArtificateDocumentApprover.Count(q => q.DeletedDate == null) != 0 && y.DeletedDate == null)).Count() +
                 SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(y => y.ProjectSubSecArtificateDocumentApprover.Count(q => q.DeletedDate == null) != 0 && y.DeletedDate == null)).Count();

            result.PendingApprove = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(y => y.ProjectArtificateDocumentApprover.Count(q => q.DeletedDate == null) != 0 && y.DeletedDate == null && y.ProjectArtificateDocumentApprover.Any(c => c.IsApproved == null && c.DeletedDate == null))).Count() +
                SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(y => y.ProjectSubSecArtificateDocumentApprover.Count(q => q.DeletedDate == null) != 0 && y.DeletedDate == null && y.ProjectSubSecArtificateDocumentApprover.Any(c => c.IsApproved == null && c.DeletedDate == null))).Count();

            result.PendingFinal = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(y => y.Status != ArtifactDocStatusType.Final && y.DeletedDate == null && y.ProjectArtificateDocumentApprover.Count(q => q.DeletedDate == null) != 0 && y.ProjectArtificateDocumentApprover.Where(c => c.DeletedDate == null).GroupBy(g => g.UserId).All(l => l.Any(x => x.IsApproved == true)))).Count() +
            SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(y => y.Status != ArtifactDocStatusType.Final && y.DeletedDate == null && y.ProjectSubSecArtificateDocumentApprover.Count(q => q.DeletedDate == null) != 0 && y.ProjectSubSecArtificateDocumentApprover.Where(c => c.DeletedDate == null).GroupBy(g => g.UserId).All(l => l.Any(x => x.IsApproved == true)))).Count();

            result.Final = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(y => y.Status == ArtifactDocStatusType.Final && y.DeletedDate == null)).Count() +
                SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(y => y.Status == ArtifactDocStatusType.Final && y.DeletedDate == null)).Count();

            //result.InComplete = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(y => y.ProjectArtificateDocumentReview.Where(z => z.UserId != y.CreatedBy).Count() == 0)).Count()
            //    + SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(y => y.ProjectSubSecArtificateDocumentReview.Where(z => z.UserId != y.CreatedBy).Count() == 0)).Count();

            result.InComplete = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(z => z.DeletedDate == null && z.ProjectArtificateDocumentReview.Where(y => y.DeletedDate == null && y.UserId != z.CreatedBy).Count() == 0)).Count() +
                SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(z => z.DeletedDate == null && z.ProjectSubSecArtificateDocumentReview.Where(y => y.DeletedDate == null && y.UserId != z.CreatedBy).Count() == 0)).Count();

            result.PendingReview = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(y => y.ProjectArtificateDocumentReview.Count(q => q.DeletedDate == null) != 0 && y.DeletedDate == null && y.ProjectArtificateDocumentReview.Where(x => x.DeletedDate == null).GroupBy(z => z.UserId).LastOrDefault()?.Where(v => v.IsReviewed == false && v.ModifiedDate == null && v.UserId != y.CreatedBy).Count() != 0)).Count() +
                SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(y => y.ProjectSubSecArtificateDocumentReview.Count(q => q.DeletedDate == null) != 0 && y.DeletedDate == null && y.ProjectSubSecArtificateDocumentReview.Where(x => x.DeletedDate == null).GroupBy(z => z.UserId).LastOrDefault()?.Where(v => v.IsReviewed == false && v.ModifiedDate == null && v.UserId != y.CreatedBy).Count() != 0)).Count();

            result.AllPendingReview = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(x => x.ProjectArtificateDocumentReview.Where(y => y.UserId != x.CreatedBy && x.DeletedDate == null).Count() != 0) && x.DeletedDate == null).Count() +
                SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(x => x.ProjectSubSecArtificateDocumentReview.Where(y => y.UserId != x.CreatedBy && x.DeletedDate == null).Count() != 0) && x.DeletedDate == null).Count();

            result.NotRequired = Artificate.Where(x => x.IsNotRequired == true).Count()
                          + SubSectionArtificate.Where(x => x.IsNotRequired == true).Count();

            result.CoreArtificate = Artificate.Where(x => x.EtmfArtificateMasterLbrary.InclutionType == 2 && x.ProjectWorkplaceArtificatedocument.Count(q => q.DeletedDate == null) == 0).Count() + SubSectionArtificate.Where(y => y.ProjectWorkplaceSubSecArtificatedocument.Count(q => q.DeletedDate == null) == 0).Count();
            result.RecommendedArtificate = Artificate.Where(x => x.EtmfArtificateMasterLbrary.InclutionType == 1 && x.ProjectWorkplaceArtificatedocument.Count(q => q.DeletedDate == null) == 0).Count();

            result.Expired = Artificate.Where(x => x.ProjectWorkplaceArtificatedocument.Any(y => y.Status == ArtifactDocStatusType.Expired && y.DeletedDate == null)).Count() +
               SubSectionArtificate.Where(x => x.ProjectWorkplaceSubSecArtificatedocument.Any(y => y.Status == ArtifactDocStatusType.Expired && y.DeletedDate == null)).Count();


            return result;
        }



        public int ClosestToNumber(List<int?> collection, int target)
        {
            var closest = int.MaxValue;
            var minDifference = int.MaxValue;
            foreach (var element in collection)
            {
                var difference = Math.Abs((long)element - target);
                if (minDifference > difference)
                {
                    minDifference = (int)difference;
                    closest = element.Value;
                }
            }

            return closest;
        }
    }
}
