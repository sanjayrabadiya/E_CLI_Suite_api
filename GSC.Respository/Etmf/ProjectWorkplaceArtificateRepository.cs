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
            var ParentArtificateId = All.First(x => x.Id == ProjectWorkplaceArtificateId).ParentArtificateId;

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

            var WorkPlaceDetails = _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == filters.ProjectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceDetail).Select(x => x.Id).ToList();
            var rightsWorkplace = new List<int>();
            foreach (var item in WorkPlaceDetails)
            {
                var rights = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId == item && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();
                if (rights != null && rights.IsView)
                {
                    rightsWorkplace.Add(item);
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

            result.All = Artificate.Count + SubSectionArtificate.Count;

            result.Missing = Artificate.Count(y => y.ProjectWorkplaceArtificatedocument.TrueForAll(q => q.DeletedDate != null || y.IsNotRequired)) + SubSectionArtificate.Count(y => y.ProjectWorkplaceSubSecArtificatedocument.TrueForAll(q => q.DeletedDate != null || y.IsNotRequired));

            result.AllPendingApprove = Artificate.Count(x => x.ProjectWorkplaceArtificatedocument.Exists(y => y.ProjectArtificateDocumentApprover.Exists(q => q.DeletedDate == null)
            && y.DeletedDate == null)) + SubSectionArtificate.Count(x => x.ProjectWorkplaceSubSecArtificatedocument.Exists(y => y.ProjectSubSecArtificateDocumentApprover.Exists(q => q.DeletedDate == null) && y.DeletedDate == null));

            result.PendingApprove = Artificate.Count(x => x.ProjectWorkplaceArtificatedocument.Exists(y => y.ProjectArtificateDocumentApprover.Exists(q => q.DeletedDate == null && (q.IsApproved == null || q.IsApproved == false)) && y.DeletedDate == null)) + SubSectionArtificate.Count(x => x.ProjectWorkplaceSubSecArtificatedocument.Exists(y => y.ProjectSubSecArtificateDocumentApprover.Exists(q => q.DeletedDate == null && (q.IsApproved == null || q.IsApproved == false)) && y.DeletedDate == null));

            result.PendingFinal = Artificate.Count(x => x.ProjectWorkplaceArtificatedocument.Exists(y => y.Status != ArtifactDocStatusType.Final && y.DeletedDate == null && y.ProjectArtificateDocumentApprover.Exists(q => q.DeletedDate == null && (q.IsApproved == null || q.IsApproved == false)))) + SubSectionArtificate.Count(x => x.ProjectWorkplaceSubSecArtificatedocument.Exists(y => y.Status != ArtifactDocStatusType.Final && y.DeletedDate == null && y.ProjectSubSecArtificateDocumentApprover.Exists(q => q.DeletedDate == null && (q.IsApproved == null || q.IsApproved == false))));

            result.Final = Artificate.Count(x => x.ProjectWorkplaceArtificatedocument.Exists(y => y.Status == ArtifactDocStatusType.Final && y.DeletedDate == null)) + SubSectionArtificate.Count(x => x.ProjectWorkplaceSubSecArtificatedocument.Exists(y => y.Status == ArtifactDocStatusType.Final && y.DeletedDate == null));

            result.InComplete = Artificate.Count(x => x.ProjectWorkplaceArtificatedocument.Exists(z => z.DeletedDate == null && z.ProjectArtificateDocumentReview.TrueForAll(y => y.DeletedDate != null || y.UserId == z.CreatedBy))) + SubSectionArtificate.Count(x => x.ProjectWorkplaceSubSecArtificatedocument.Exists(z => z.DeletedDate == null && z.ProjectSubSecArtificateDocumentReview.TrueForAll(y => y.DeletedDate != null || y.UserId == z.CreatedBy)));

            result.PendingReview = Artificate.Count(x =>
     x.ProjectWorkplaceArtificatedocument.Exists(y =>
         y.ProjectArtificateDocumentReview.Exists(q =>
             q.DeletedDate == null &&
             (!q.IsReviewed || q.ModifiedDate == null) &&
             q.UserId != y.CreatedBy
         ) &&
         y.DeletedDate == null
     )
 ) + SubSectionArtificate.Count(x =>
     x.ProjectWorkplaceSubSecArtificatedocument.Exists(y =>
         y.ProjectSubSecArtificateDocumentReview.Exists(q =>
             q.DeletedDate == null &&
             (!q.IsReviewed || q.ModifiedDate == null) &&
             q.UserId != y.CreatedBy
         ) &&
         y.DeletedDate == null
     )
 );

            result.AllPendingReview = Artificate.Count(x =>
                x.ProjectWorkplaceArtificatedocument.Exists(y =>
                    y.ProjectArtificateDocumentReview.Exists(q =>
                        q.DeletedDate == null &&
                        q.UserId != y.CreatedBy
                    ) &&
                    y.DeletedDate == null
                )
            ) + SubSectionArtificate.Count(x =>
                x.ProjectWorkplaceSubSecArtificatedocument.Exists(y =>
                    y.ProjectSubSecArtificateDocumentReview.Exists(q =>
                        q.DeletedDate == null &&
                        q.UserId != y.CreatedBy
                    ) &&
                    y.DeletedDate == null
                )
            );

            result.NotRequired = Artificate.Count(x => x.IsNotRequired) + SubSectionArtificate.Count(x => x.IsNotRequired);


            result.CoreArtificate = Artificate.Count(x =>
      x.EtmfArtificateMasterLbrary.InclutionType == 2 &&
      x.ProjectWorkplaceArtificatedocument.TrueForAll(q => q.DeletedDate != null)
  ) + SubSectionArtificate.Count(y =>
      y.ProjectWorkplaceSubSecArtificatedocument.TrueForAll(q => q.DeletedDate != null)
  );

            result.RecommendedArtificate = Artificate.Count(x =>
                x.EtmfArtificateMasterLbrary.InclutionType == 1 &&
                x.ProjectWorkplaceArtificatedocument.TrueForAll(q => q.DeletedDate != null)
            );

            result.Expired = Artificate.Count(x =>
                x.ProjectWorkplaceArtificatedocument.Exists(y =>
                    y.Status == ArtifactDocStatusType.Expired &&
                    y.DeletedDate == null
                )
            ) + SubSectionArtificate.Count(x =>
                x.ProjectWorkplaceSubSecArtificatedocument.Exists(y =>
                    y.Status == ArtifactDocStatusType.Expired &&
                    y.DeletedDate == null
                )
            );


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
