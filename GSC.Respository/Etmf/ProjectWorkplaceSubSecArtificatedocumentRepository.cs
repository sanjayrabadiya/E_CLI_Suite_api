using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.UserMgt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;
using System.Text;
using Syncfusion.EJ2.DocumentEditor;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Microsoft.EntityFrameworkCore;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceSubSecArtificatedocumentRepository : GenericRespository<ProjectWorkplaceSubSecArtificatedocument>, IProjectWorkplaceSubSecArtificatedocumentRepository
    {

        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IProjectSubSecArtificateDocumentHistoryRepository _projectSubSecArtificateDocumentHistoryRepository;

        public ProjectWorkplaceSubSecArtificatedocumentRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IUploadSettingRepository uploadSettingRepository,
           IUserRepository userRepository, IMapper mapper,
           IProjectSubSecArtificateDocumentHistoryRepository projectSubSecArtificateDocumentHistoryRepository
           )
           : base(context)
        {
            _uploadSettingRepository = uploadSettingRepository;
            _userRepository = userRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _projectSubSecArtificateDocumentHistoryRepository = projectSubSecArtificateDocumentHistoryRepository;
        }

        public string getArtifactSectionDetail(ProjectWorkplaceSubSecArtificatedocumentDto projectWorkplaceSubSectionDto)
        {
            var data = (from artifact in _context.EtmfProjectWorkPlace.Where(x => x.Id == projectWorkplaceSubSectionDto.ProjectWorkplaceSubSectionArtifactId)
                        join subsection in _context.EtmfProjectWorkPlace on artifact.EtmfProjectWorkPlaceId equals subsection.Id
                        join section in _context.EtmfProjectWorkPlace on subsection.EtmfProjectWorkPlaceId equals section.Id
                        join etmfsection in _context.EtmfMasterLibrary on section.EtmfMasterLibraryId equals etmfsection.Id
                        join workzone in _context.EtmfProjectWorkPlace on section.EtmfProjectWorkPlaceId equals workzone.Id
                        join etmfZone in _context.EtmfMasterLibrary on workzone.EtmfMasterLibraryId equals etmfZone.Id
                        join workdetail in _context.EtmfProjectWorkPlace on workzone.EtmfProjectWorkPlaceId equals workdetail.Id
                        join work in _context.EtmfProjectWorkPlace on workdetail.EtmfProjectWorkPlaceId equals work.Id
                        join project in _context.Project on work.ProjectId equals project.Id

                        join countryleft in _context.Country on workdetail.ItemId equals countryleft.Id into countryl
                        from country in countryl.DefaultIfEmpty()
                        join projectsite in _context.Project on workdetail.ItemId equals projectsite.Id into siteleft
                        from site in siteleft.DefaultIfEmpty()
                        select new EtmfProjectWorkPlaceDto
                        {
                            SectionName = etmfsection.SectionName,
                            ProjectWorkplaceSectionId = section.Id,
                            ProjectWorkplaceZoneId = workzone.Id,
                            ZonName = etmfZone.ZonName,
                            WorkPlaceFolderId = workdetail.WorkPlaceFolderId,
                            ChildName = workdetail.WorkPlaceFolderId == 1 ? country.CountryName :
                                        workdetail.WorkPlaceFolderId == 2 ? site.ProjectCode + " - " + site.ProjectName : null,
                            ProjectName = project.ProjectCode.Replace("/", ""),
                            SubSectionName = subsection.SubSectionName,
                            SubSectionArtifactName = artifact.ArtifactName
                        }).FirstOrDefault();

            string filePath = string.Empty;
            string path = string.Empty;

            if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Country.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), data.SubSectionArtifactName.Trim());
            else if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Site.GetDescription(),
                data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), data.SubSectionArtifactName.Trim());
            else if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Trial.GetDescription(),
                   data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), data.SubSectionArtifactName.Trim());
            //filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path);
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), path);
            bool projectPathExists = Directory.Exists(filePath);
            if (!projectPathExists)
                System.IO.Directory.CreateDirectory(Path.Combine(filePath));

            return path;
        }

        public int deleteSubsectionArtifactfile(int id)
        {
            var data = (from artifactdoc in _context.ProjectWorkplaceSubSecArtificatedocument.Where(x => x.Id == id)
                        join artifact in _context.EtmfProjectWorkPlace on artifactdoc.ProjectWorkplaceSubSectionArtifactId equals artifact.Id
                        join subsection in _context.EtmfProjectWorkPlace on artifact.EtmfProjectWorkPlaceId equals subsection.Id
                        join section in _context.EtmfProjectWorkPlace on subsection.EtmfProjectWorkPlaceId equals section.Id
                        join etmfsection in _context.EtmfMasterLibrary on section.EtmfMasterLibraryId equals etmfsection.Id
                        join workzone in _context.EtmfProjectWorkPlace on section.EtmfProjectWorkPlaceId equals workzone.Id
                        join etmfZone in _context.EtmfMasterLibrary on workzone.EtmfMasterLibraryId equals etmfZone.Id
                        join workdetail in _context.EtmfProjectWorkPlace on workzone.EtmfProjectWorkPlaceId equals workdetail.Id
                        join work in _context.EtmfProjectWorkPlace on workdetail.EtmfProjectWorkPlaceId equals work.Id
                        join project in _context.Project on work.ProjectId equals project.Id

                        join countryleft in _context.Country on workdetail.ItemId equals countryleft.Id into countryl
                        from country in countryl.DefaultIfEmpty()
                        join projectsite in _context.Project on workdetail.ItemId equals projectsite.Id into siteleft
                        from site in siteleft.DefaultIfEmpty()
                        select new ProjectWorkplaceSubSecArtificatedocumentDto
                        {
                            Sectionname = etmfsection.SectionName,

                            Zonename = etmfZone.ZonName,
                            FolderType = workdetail.WorkPlaceFolderId,
                            Sitename = workdetail.WorkPlaceFolderId == 1 ? country.CountryName :
                                        workdetail.WorkPlaceFolderId == 2 ? site.ProjectCode + " - " + site.ProjectName : null,
                            Projectname = project.ProjectCode.Replace("/", ""),
                            SubsectionName = subsection.SubSectionName,
                            Artificatename = artifact.ArtifactName,
                            DocumentName = artifactdoc.DocumentName

                        }).FirstOrDefault();

            string filePath = string.Empty;
            string path = string.Empty;

            if (data.FolderType == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(data.Projectname, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Country.GetDescription(),
                  data.Sitename.Trim(), data.Zonename.Trim(), data.Sectionname.Trim(), data.SubsectionName.Trim(), data.Artificatename.Trim());
            else if (data.FolderType == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(data.Projectname, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Site.GetDescription(),
                data.Sitename.Trim(), data.Zonename.Trim(), data.Sectionname.Trim(), data.SubsectionName.Trim(), data.Artificatename.Trim());
            else if (data.FolderType == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(data.Projectname, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Trial.GetDescription(),
                   data.Zonename.Trim(), data.Sectionname.Trim(), data.SubsectionName.Trim(), data.Artificatename.Trim());
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), path, data.DocumentName);

            System.IO.File.Delete(Path.Combine(filePath));

            return id;
        }

        public List<CommonArtifactDocumentDto> GetExpiredDocumentReports(int projectId)
        {
            List<CommonArtifactDocumentDto> dataList = new List<CommonArtifactDocumentDto>();

            var documentList = All.Include(x => x.ProjectWorkplaceSubSectionArtifact)
                .Include(x => x.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary)
                .Include(x => x.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary)
                .Where(x => x.DeletedDate == null && x.ExpiryDate != null && x.Status == ArtifactDocStatusType.Expired && x.ProjectWorkplaceSubSectionArtifact.ProjectId == projectId && (x.CreatedBy == _jwtTokenAccesser.UserId ||
              _context.ProjectSubSecArtificateDocumentReview.Any(m => m.ProjectWorkplaceSubSecArtificateDocumentId == x.Id && m.UserId == _jwtTokenAccesser.UserId && m.DeletedDate == null)
              || _context.ProjectSubSecArtificateDocumentApprover.Any(m => m.ProjectWorkplaceSubSecArtificateDocumentId == x.Id && m.UserId == _jwtTokenAccesser.UserId && m.DeletedDate == null)))
              .ToList().OrderByDescending(x => x.Id);

            foreach (var item in documentList)
            {
                CommonArtifactDocumentDto obj = new CommonArtifactDocumentDto();
                obj.Id = item.Id;
                obj.ProjectWorkplaceSubSectionArtifactId = item.ProjectWorkplaceSubSectionArtifactId;
                obj.Artificatename = item.ProjectWorkplaceSubSectionArtifact.ArtifactName;
                obj.SectionName = item.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.SectionName;
                obj.DocumentName = item.DocumentName;
                obj.SubSectionName = item.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.SubSectionName;
                obj.ZoneName = item.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName;
                //obj.DocPath = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), _jwtTokenAccesser.CompanyId.ToString(), item.DocPath, item.DocumentName);
                obj.FullDocPath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), item.DocPath);
                obj.CreatedByUser = _userRepository.Find((int)item.CreatedBy).UserName;
                obj.CreatedDate = item.CreatedDate;
                obj.Level = 5.2;
                obj.ExtendedName = item.DocumentName.Contains('_') ? item.DocumentName.Substring(0, item.DocumentName.LastIndexOf('_')) : item.DocumentName;
                obj.Version = item.Version;
                obj.StatusName = item.Status.GetDescription();
                obj.Status = (int)item.Status;
                obj.SendBy = !(item.CreatedBy == _jwtTokenAccesser.UserId);
                obj.SendAndSendBack = !(item.CreatedBy == _jwtTokenAccesser.UserId);
                obj.IsAccepted = item.IsAccepted;
                //obj.EtmfArtificateMasterLbraryId = item.ProjectWorkplaceSubSectionArtifact.EtmfArtificateMasterLbraryId;
                //obj.Reviewer = users.OrderBy(x => x.SequenceNo).ToList();
                //obj.ReviewStatus = Review.Count() == 0 ? "" : Review.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsReviewed == true)) ? "Reviewed" : Review.GroupBy(u => u.UserId).All(z => z.All(x => x.IsReviewed == false) && z.Any(x => x.IsSendBack == true)) ? "Send Back" : "Send";
                //obj.IsReview = Review.Count() == 0 ? false : Review.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsReviewed == true)) ? true : false;
                obj.IsSendBack = _context.ProjectSubSecArtificateDocumentReview.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == item.Id && x.UserId == _jwtTokenAccesser.UserId).OrderByDescending(x => x.Id).Select(z => z.IsSendBack).FirstOrDefault();
                //obj.ApprovedStatus = ApproveList.Count() == 0 ? "" : ApproveList.GroupBy(u => u.UserId).All(z => z.All(x => x.IsApproved == false)) ? "Reject" : ApproveList.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsApproved == true)) ? "Approved" : "Send For Approval";
                //obj.Approver = ApproverName.OrderBy(x => x.SequenceNo).OrderBy(x => x.CreatedDate).ToList();
                obj.IsReplyAllComment = item.IsReplyAllComment;
                //obj.SequenceNo = currentReviewer?.SequenceNo;
                //obj.ApproveSequenceNo = currentApprover?.SequenceNo;
                obj.AddedBy = item.CreatedBy == _jwtTokenAccesser.UserId;
                //obj.IsApproveDoc = ApproveList.Any(x => x.UserId == _jwtTokenAccesser.UserId && x.IsApproved == null) ? true : false;
                obj.ExpiryDate = item.ExpiryDate;
                dataList.Add(obj);
            }

            return dataList;
        }


        public List<CommonArtifactDocumentDto> GetSubSecDocumentList(int Id)
        {
            var artificate = _context.EtmfProjectWorkPlace.Where(x => x.Id == Id).Include(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace).FirstOrDefault();

            var rights = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId == artificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfProjectWorkPlaceId
                        && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null)
                        .OrderByDescending(x => x.Id).FirstOrDefault();

            List<CommonArtifactDocumentDto> dataList = new List<CommonArtifactDocumentDto>();
            var _docService = new DocumentService();
            var reviewdocument = _context.ProjectSubSecArtificateDocumentReview.Where(c => c.DeletedDate == null
            && c.UserId == _jwtTokenAccesser.UserId).Select(x => x.ProjectWorkplaceSubSecArtificateDocumentId).ToList();

            if (reviewdocument == null || reviewdocument.Count == 0) return dataList;
            var documentList = FindByInclude(x => x.ProjectWorkplaceSubSectionArtifactId == Id && x.DeletedDate == null && (x.CreatedBy == _jwtTokenAccesser.UserId ||
                _context.ProjectSubSecArtificateDocumentReview.Any(m => m.ProjectWorkplaceSubSecArtificateDocumentId == x.Id && m.UserId == _jwtTokenAccesser.UserId && m.DeletedDate == null)
                || _context.ProjectSubSecArtificateDocumentApprover.Any(m => m.ProjectWorkplaceSubSecArtificateDocumentId == x.Id && m.UserId == _jwtTokenAccesser.UserId && m.DeletedDate == null)), x => x.ProjectWorkplaceSubSectionArtifact, m => m.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfUserPermission)
                .ToList().OrderByDescending(x => x.Id);

            foreach (var item in documentList)
            {
                var reviewerList = _context.ProjectSubSecArtificateDocumentReview.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == item.Id && x.UserId != item.CreatedBy && x.DeletedDate == null).Select(z => new { UserId = z.UserId, SequenceNo = z.SequenceNo, isSendBack = z.IsSendBack, IsReview = z.IsReviewed, CreatedDate = z.CreatedDate, SendBackDate = z.SendBackDate, DueDate = z.DueDate }).ToList();
                var users = new List<DocumentUsers>();
                reviewerList.ForEach(r =>
                {
                    var role = item.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfUserPermission.FirstOrDefault(q => q.UserId == r.UserId && q.DeletedDate == null && q.RoleId != null);
                    DocumentUsers obj = new DocumentUsers();
                    obj.UserName = _userRepository.Find(r.UserId).UserName;
                    obj.RoleName = role != null ? _context.SecurityRole.Find(role.RoleId.Value).RoleName : "";
                    obj.SequenceNo = r.SequenceNo;
                    obj.UserId = r.UserId;
                    obj.IsSendBack = r.isSendBack;
                    obj.IsReview = r.IsReview;
                    obj.CreatedDate = r.CreatedDate;
                    obj.SendBackDate = r.SendBackDate;
                    obj.DueDate = r.DueDate;
                    obj.IsDueDateExpired = r.DueDate == null ? false : r.DueDate.Value.Date < DateTime.Now.Date && r.IsReview == false;
                    users.Add(obj);
                });
                var Review = _context.ProjectSubSecArtificateDocumentReview.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == item.Id
                && x.UserId != item.CreatedBy && x.DeletedDate == null).ToList();

                var ApproveList = _context.ProjectSubSecArtificateDocumentApprover.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == item.Id && x.DeletedDate == null).OrderByDescending(x => x.Id).ToList()
                    .Select(y => new ProjectSubSecArtificateDocumentApprover
                    {
                        Id = y.Id,
                        UserId = y.UserId,
                        ProjectWorkplaceSubSecArtificateDocumentId = y.ProjectWorkplaceSubSecArtificateDocumentId,
                        IsApproved = y.IsApproved,
                        SequenceNo = y.SequenceNo,
                        CreatedDate = y.CreatedDate,
                        ModifiedDate = y.ModifiedDate,
                        DueDate = y.DueDate
                    }).ToList();

                var ApproverName = new List<DocumentUsers>();
                ApproveList.ForEach(r =>
                {
                    var role = item.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfUserPermission.FirstOrDefault(q => q.UserId == r.UserId && q.DeletedDate == null && q.RoleId != null);
                    DocumentUsers obj = new DocumentUsers();
                    obj.UserName = _userRepository.Find(r.UserId).UserName;
                    obj.RoleName = role != null ? _context.SecurityRole.Find(role.RoleId.Value).RoleName : "";
                    obj.SequenceNo = r.SequenceNo;
                    obj.IsSendBack = r.IsApproved;
                    obj.CreatedDate = r.CreatedDate;
                    obj.SendBackDate = r.ModifiedDate;
                    obj.DueDate = r.DueDate;
                    obj.IsDueDateExpired = r.DueDate == null ? false : r.DueDate.Value.Date < DateTime.Now.Date && (r.IsApproved == null || r.IsApproved == false);
                    ApproverName.Add(obj);
                });

                var currentReviewer = _context.ProjectSubSecArtificateDocumentReview.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == item.Id
              && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null && x.IsSendBack == false && x.IsReviewed == false).FirstOrDefault();


                var currentApprover = _context.ProjectSubSecArtificateDocumentApprover.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == item.Id
               && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null && (x.IsApproved == null || x.IsApproved == false)).FirstOrDefault();

                CommonArtifactDocumentDto obj = new CommonArtifactDocumentDto();
                obj.Id = item.Id;
                obj.ProjectWorkplaceSubSectionArtifactId = item.ProjectWorkplaceSubSectionArtifactId;
                obj.Artificatename = item.ProjectWorkplaceSubSectionArtifact.ArtifactName;
                obj.DocumentName = item.DocumentName;
                obj.ExtendedName = item.DocumentName.Contains('_') ? item.DocumentName.Substring(0, item.DocumentName.LastIndexOf('_')) : item.DocumentName;
                obj.FullDocPath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), item.DocPath);
                if (item.Status == ArtifactDocStatusType.Final)
                {
                    var changeDocumentName = _docService.GetEtmfOldFileName(obj.FullDocPath, obj.ExtendedName);
                    obj.DocPath = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), _jwtTokenAccesser.CompanyId.ToString(), item.DocPath, changeDocumentName);
                    obj.DocumentName = changeDocumentName;
                }
                else
                    obj.DocPath = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), _jwtTokenAccesser.CompanyId.ToString(), item.DocPath, item.DocumentName);
                obj.CreatedByUser = _userRepository.Find((int)item.CreatedBy).UserName;
                obj.CreatedDate = item.CreatedDate;
                obj.Level = 5.2;
                obj.Version = item.Version;
                obj.StatusName = item.Status.GetDescription();
                obj.Status = (int)item.Status;
                obj.SendBy = !(item.CreatedBy == _jwtTokenAccesser.UserId);
                obj.SendAndSendBack = !(item.CreatedBy == _jwtTokenAccesser.UserId);
                obj.IsAccepted = item.IsAccepted;
                //obj.EtmfArtificateMasterLbraryId = item.ProjectWorkplaceSubSectionArtifact.EtmfArtificateMasterLbraryId;
                obj.Reviewer = users.OrderBy(x => x.SequenceNo).ToList();
                obj.ReviewStatus = Review.Count() == 0 ? "" : Review.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsReviewed == true)) ? "Reviewed" : Review.GroupBy(u => u.UserId).All(z => z.All(x => x.IsReviewed == false) && z.Any(x => x.IsSendBack == true)) ? "Send Back" : "Send";
                obj.IsReview = Review.Count() == 0 ? false : Review.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsReviewed == true)) ? true : false;
                obj.IsSendBack = _context.ProjectSubSecArtificateDocumentReview.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == item.Id && x.UserId == _jwtTokenAccesser.UserId).OrderByDescending(x => x.Id).Select(z => z.IsSendBack).FirstOrDefault();
                obj.ApprovedStatus = ApproveList.Count() == 0 ? "" : ApproveList.GroupBy(u => u.UserId).All(z => z.All(x => x.IsApproved == false)) ? "Reject" : ApproveList.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsApproved == true)) ? "Approved" : "Send For Approval";
                obj.Approver = ApproverName.OrderBy(x => x.SequenceNo).OrderBy(x => x.CreatedDate).ToList();
                obj.IsReplyAllComment = item.IsReplyAllComment;
                obj.SequenceNo = currentReviewer?.SequenceNo;
                obj.ApproveSequenceNo = currentApprover?.SequenceNo;
                obj.AddedBy = item.CreatedBy == _jwtTokenAccesser.UserId;
                obj.IsApproveDoc = ApproveList.Any(x => x.UserId == _jwtTokenAccesser.UserId && x.IsApproved == null) ? true : false;
                obj.ExpiryDate = item.ExpiryDate;
                dataList.Add(obj);
            }
            return dataList.OrderByDescending(q => q.CreatedDate).ToList();
        }

        public CommonArtifactDocumentDto GetDocument(int id)
        {
            var document = All.Include(x => x.ProjectWorkplaceSubSectionArtifact).Where(x => x.Id == id && x.DeletedDate == null).FirstOrDefault();


            var currentReviewer = _context.ProjectSubSecArtificateDocumentReview.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == id
              && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null && x.IsSendBack == false && x.IsReviewed == false).FirstOrDefault();


            var currentApprover = _context.ProjectSubSecArtificateDocumentApprover.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == id
           && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null && (x.IsApproved == null || x.IsApproved == false)).FirstOrDefault();


            var reviewerList = _context.ProjectSubSecArtificateDocumentReview.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == document.Id && x.UserId != document.CreatedBy && x.DeletedDate == null).ToList();


            CommonArtifactDocumentDto obj = new CommonArtifactDocumentDto();
            obj.Id = document.Id;
            obj.ProjectWorkplaceSubSectionArtifactId = document.ProjectWorkplaceSubSectionArtifactId;
            obj.DocumentName = document.DocumentName;
            obj.ExtendedName = document.DocumentName.Contains('_') ? document.DocumentName.Substring(0, document.DocumentName.LastIndexOf('_')) : document.DocumentName;
            obj.DocPath = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, document.DocumentName);
            obj.FullDocPath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), document.DocPath);
            obj.CreatedByUser = _userRepository.Find((int)document.CreatedBy).UserName;
            obj.CreatedDate = document.CreatedDate;
            obj.Version = document.Version;
            obj.StatusName = document.Status.GetDescription();
            obj.Status = (int)document.Status;
            obj.Level = 5.2;
            obj.SendBy = !(document.CreatedBy == _jwtTokenAccesser.UserId);
            obj.AddedBy = (document.CreatedBy == _jwtTokenAccesser.UserId);
            obj.IsSendBack = _context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == document.Id && x.UserId == _jwtTokenAccesser.UserId).OrderByDescending(x => x.Id).Select(z => z.IsSendBack).FirstOrDefault();
            obj.IsAccepted = document.IsAccepted;
            obj.ApprovedStatus = document.IsAccepted == null ? "" : document.IsAccepted == true ? "Approved" : "Rejected";
            obj.IsReplyAllComment = document.IsReplyAllComment;
            obj.SequenceNo = currentReviewer?.SequenceNo;
            obj.ApproveSequenceNo = currentApprover?.SequenceNo;
            obj.ReviewStatus = reviewerList.Count() == 0 ? "" : reviewerList.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsReviewed == true)) ? "Send Back" : "Send";
            obj.IsReview = reviewerList.Count() == 0 ? false : reviewerList.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsReviewed == true)) ? true : false;
            return obj;
        }

        public ProjectWorkplaceSubSecArtificatedocument AddDocument(ProjectWorkplaceSubSecArtificatedocumentDto projectWorkplaceSubSecArtificatedocumentDto)
        {
            string path = getArtifactSectionDetail(projectWorkplaceSubSecArtificatedocumentDto);
            // string filePath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path);
            string filePath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), path);
            string FileName = DocumentService.SaveWorkplaceDocument(projectWorkplaceSubSecArtificatedocumentDto.FileModel, filePath, projectWorkplaceSubSecArtificatedocumentDto.FileName);

            projectWorkplaceSubSecArtificatedocumentDto.Id = 0;
            var projectWorkplaceArtificatedocument = _mapper.Map<ProjectWorkplaceSubSecArtificatedocument>(projectWorkplaceSubSecArtificatedocumentDto);
            projectWorkplaceArtificatedocument.DocumentName = FileName;
            projectWorkplaceArtificatedocument.DocPath = path;
            projectWorkplaceArtificatedocument.Status = ArtifactDocStatusType.Draft;
            projectWorkplaceArtificatedocument.Version = "1.0";
            projectWorkplaceArtificatedocument.ParentDocumentId = projectWorkplaceSubSecArtificatedocumentDto.ParentDocumentId;
            return projectWorkplaceArtificatedocument;
        }

        public string ImportData(int Id)
        {
            var document = Find(Id);
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var FullPath = System.IO.Path.Combine(upload.DocumentPath, _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, document.DocumentName);
            string path = FullPath;
            if (!File.Exists(path))
                return null;
            Stream stream = System.IO.File.OpenRead(path);
            string json = ImportWordDocument(stream, path);
            stream.Close();
            return json;
        }

        public string ImportWordDocument(Stream stream, string FullPath)
        {
            string sfdtText = "";
            var Extension = Path.GetExtension(FullPath);
            EJ2WordDocument document = EJ2WordDocument.Load(stream, GetFormatType(Extension.ToLower()));
            sfdtText = Newtonsoft.Json.JsonConvert.SerializeObject(document);
            document.Dispose();
            return sfdtText;
        }
        internal static FormatType GetFormatType(string format)
        {
            if (string.IsNullOrEmpty(format))
                throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
            switch (format.ToLower())
            {
                case ".dotx":
                case ".docx":
                case ".docm":
                case ".dotm":
                    return FormatType.Docx;
                case ".dot":
                case ".doc":
                    return FormatType.Doc;
                case ".rtf":
                    return FormatType.Rtf;
                case ".txt":
                    return FormatType.Txt;
                case ".xml":
                    return FormatType.WordML;
                default:
                    throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
            }
        }
        public void UpdateApproveDocument(int documentId, bool IsAccepted)
        {
            var document = All.Where(x => x.Id == documentId).FirstOrDefault();
            document.IsAccepted = IsAccepted;
            Update(document);
            _context.Save();
        }

        public string SaveDocumentInFolder(ProjectWorkplaceSubSecArtificatedocument projectWorkplaceSubSecArtificatedocument, CustomParameter param)
        {
            string filePath = string.Empty;
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var fileName = projectWorkplaceSubSecArtificatedocument.DocumentName.Contains('_') ? projectWorkplaceSubSecArtificatedocument.DocumentName.Substring(0, projectWorkplaceSubSecArtificatedocument.DocumentName.LastIndexOf('_')) : projectWorkplaceSubSecArtificatedocument.DocumentName;
            var docName = fileName + "_" + DateTime.Now.Ticks + ".docx";
            filePath = Path.Combine(upload.DocumentPath, _jwtTokenAccesser.CompanyId.ToString(), projectWorkplaceSubSecArtificatedocument.DocPath, docName);

            Byte[] byteArray = Convert.FromBase64String(param.documentData);
            Stream stream = new MemoryStream(byteArray);
            FormatType type = GetFormatTypeExport(filePath);

            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            if (type != FormatType.Docx)
            {
                Syncfusion.DocIO.DLS.WordDocument document = new Syncfusion.DocIO.DLS.WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
                document.Save(fileStream, GetDocIOFomatType(type));
                document.Close();
            }
            else
            {
                stream.Position = 0;
                stream.CopyTo(fileStream);
            }
            stream.Dispose();
            fileStream.Dispose();
            return docName;
        }

        internal static Syncfusion.DocIO.FormatType GetDocIOFomatType(FormatType type)
        {
            switch (type)
            {
                case FormatType.Docx:
                    return (Syncfusion.DocIO.FormatType)FormatType.Docx;
                case FormatType.Doc:
                    return (Syncfusion.DocIO.FormatType)FormatType.Doc;
                case FormatType.Rtf:
                    return (Syncfusion.DocIO.FormatType)FormatType.Rtf;
                case FormatType.Txt:
                    return (Syncfusion.DocIO.FormatType)FormatType.Txt;
                case FormatType.WordML:
                    return (Syncfusion.DocIO.FormatType)FormatType.WordML;
                default:
                    throw new NotSupportedException("DocIO does not support this file format.");
            }
        }

        internal static FormatType GetFormatTypeExport(string fileName)
        {
            int index = fileName.LastIndexOf('.');
            string format = index > -1 && index < fileName.Length - 1 ? fileName.Substring(index + 1) : "";

            if (string.IsNullOrEmpty(format))
                throw new NotSupportedException("EJ2 Document editor does not support this file format.");
            switch (format.ToLower())
            {
                case "dotx":
                case "docx":
                case "docm":
                case "dotm":
                    return FormatType.Docx;
                case "dot":
                case "doc":
                    return FormatType.Doc;
                case "rtf":
                    return FormatType.Rtf;
                case "txt":
                    return FormatType.Txt;
                case "xml":
                    return FormatType.WordML;
                default:
                    throw new NotSupportedException("EJ2 Document editor does not support this file format.");
            }
        }

        public ProjectWorkplaceSubSecArtificatedocument WordToPdf(int Id)
        {
            var document = Find(Id);
            var outputname = "";
            if (document?.DocumentName.Split('.').LastOrDefault() == "docx" || document?.DocumentName.Split('.').LastOrDefault() == "doc")
            {
                var parent = document.ParentDocumentId != null ? Find((int)document.ParentDocumentId) : null;

                var filepath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, document.DocumentName);
                FileStream docStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                Syncfusion.DocIO.DLS.WordDocument wordDocument = new Syncfusion.DocIO.DLS.WordDocument(docStream, Syncfusion.DocIO.FormatType.Automatic);
                DocIORenderer render = new DocIORenderer();
                render.Settings.PreserveFormFields = true;
                PdfDocument pdfDocument = render.ConvertToPDF(wordDocument);
                render.Dispose();
                wordDocument.Dispose();
                MemoryStream outputStream = new MemoryStream();
                pdfDocument.Save(outputStream);
                pdfDocument.Close();

                outputname = document.DocumentName.Substring(0, document.DocumentName.LastIndexOf('_')) + "_" + DateTime.Now.Ticks + ".pdf";
                var outputFile = Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, outputname);
                FileStream file = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
                outputStream.WriteTo(file);
            }
            document.DocumentName = string.IsNullOrEmpty(outputname) ? document.DocumentName : outputname;
            document.Status = ArtifactDocStatusType.Final;
            //document.Version = document.ParentDocumentId != null ? (double.Parse(parent.Version) + 1).ToString("0.0") : (double.Parse(document.Version) + 1).ToString("0.0");
            //document.Version = "1.0";
            return document;
        }

        public string GetDocumentHistory(int Id)
        {
            var history = _projectSubSecArtificateDocumentHistoryRepository.Find(Id);
            var document = Find(history.ProjectWorkplaceSubSecArtificateDocumentId);
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var FullPath = Path.Combine(upload.DocumentPath, _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, history.DocumentName);
            string path = FullPath;
            if (!File.Exists(path))
                return null;
            Stream stream = File.OpenRead(path);
            string json = ImportWordDocument(stream, path);
            stream.Close();
            return json;
        }

        public CommonArtifactDocumentDto GetDocumentForPdfHistory(int Id)
        {
            CommonArtifactDocumentDto obj = new CommonArtifactDocumentDto();
            var history = _projectSubSecArtificateDocumentHistoryRepository.Find(Id);
            var document = Find(history.ProjectWorkplaceSubSecArtificateDocumentId);
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var FullPath = Path.Combine(upload.DocumentUrl, _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, history.DocumentName);
            obj.FullDocPath = FullPath;
            return obj;
        }

        public void UpdateDocumentExpiryStatus()
        {
            var allDocuments = All.Where(q => q.DeletedDate == null && q.ExpiryDate != null && (q.ExpiryDate.HasValue ? q.ExpiryDate.Value.Date <= DateTime.Now.Date : false));
            foreach (var document in allDocuments)
            {
                document.Status = ArtifactDocStatusType.Expired;
                Update(document);
            }
            _context.Save();
        }

        public void UpdateSubDocumentComment(int documentId, bool? isComment)
        {
            var doc = All.FirstOrDefault(x => x.Id == documentId);
            doc.IsReplyAllComment = isComment;
            Update(doc);
            _context.Save();
        }

        public List<ProjectSubSecArtificateDocumentExpiryHistoryDto> GetSubSectionDocumentHistory(int documentId)
        {
            var docHistory = (from history in _context.ProjectSubSecArtificateDocumentHistory.Where(q => q.ProjectWorkplaceSubSecArtificateDocumentId == documentId && q.ExpiryDate.HasValue)
                              join auditReasonTemp in _context.AuditTrail.Where(x => x.TableName == "ProjectSubSecArtificateDocumentHistory" && x.ColumnName == "Document Expiry Date")
                                      on history.Id equals auditReasonTemp.RecordId into auditReasonDto
                              from auditReason in auditReasonDto.DefaultIfEmpty()
                              select new ProjectSubSecArtificateDocumentExpiryHistoryDto()
                              {
                                  DocumentName = history.DocumentName,
                                  CreatedDate = history.CreatedDate,
                                  CreatedByName = history.CreatedByUser.UserName,
                                  Reason = auditReason.Reason,
                                  ReasonOth = auditReason.ReasonOth,
                                  Id = history.Id,
                                  ExpiryDate = history.ExpiryDate
                              }).ToList();
            return docHistory;
        }
    }
}
