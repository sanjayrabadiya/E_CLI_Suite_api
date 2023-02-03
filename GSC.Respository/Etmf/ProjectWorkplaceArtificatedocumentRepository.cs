using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;
using System.Text;
using Syncfusion.EJ2.DocumentEditor;
using GSC.Respository.Audit;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using System.Data;
using Syncfusion.Pdf.Grid;
using Syncfusion.Pdf.Parsing;
using GSC.Data.Dto.Configuration;
using GSC.Shared.Generic;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceArtificatedocumentRepository : GenericRespository<ProjectWorkplaceArtificatedocument>, IProjectWorkplaceArtificatedocumentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IEtmfMasterLbraryRepository _etmfMasterLibraryRepository;
        private readonly IAuditTrailRepository _auditTrailRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;
        private readonly IAppSettingRepository _appSettingRepository;
        public ProjectWorkplaceArtificatedocumentRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IUploadSettingRepository uploadSettingRepository,
           IUserRepository userRepository,
           IMapper mapper,
           IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
           IProjectRepository projectRepository,
           IEtmfMasterLbraryRepository etmfMasterLibraryRepository,
           IAuditTrailRepository auditTrailRepository,
           IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
           IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
           IAppSettingRepository appSettingRepository
           )
           : base(context)
        {
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _userRepository = userRepository;
            _etmfArtificateMasterLbraryRepository = etmfArtificateMasterLbraryRepository;
            _projectRepository = projectRepository;
            _etmfMasterLibraryRepository = etmfMasterLibraryRepository;
            _auditTrailRepository = auditTrailRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _appSettingRepository = appSettingRepository;
        }

        public int deleteFile(int id)
        {
            string filename = string.Empty;
            var data = (from artifactdoc in _context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == id)
                        join artifact in _context.EtmfProjectWorkPlace on artifactdoc.ProjectWorkplaceArtificateId equals artifact.Id
                        join etmfartifact in _context.EtmfArtificateMasterLbrary on artifact.EtmfArtificateMasterLbraryId equals etmfartifact.Id
                        join section in _context.EtmfProjectWorkPlace on artifact.EtmfProjectWorkPlaceId equals section.Id
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
                            Artificatename = etmfartifact.ArtificateName,
                            DocumentName = artifactdoc.DocumentName,
                        }).FirstOrDefault();

            string filePath = string.Empty;
            string path = string.Empty;

            if (data.FolderType == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(data.Projectname, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Country.GetDescription(),
                  data.Sitename.Trim(), data.Zonename.Trim(), data.Sectionname.Trim(), data.Artificatename);
            else if (data.FolderType == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(data.Projectname, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Site.GetDescription(),
                data.Sitename.Trim(), data.Zonename.Trim(), data.Sectionname.Trim(), data.Artificatename);
            else if (data.FolderType == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(data.Projectname, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Trial.GetDescription(),
                   data.Zonename.Trim(), data.Sectionname.Trim(), data.Artificatename);
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), path, data.DocumentName);
            System.IO.File.Delete(Path.Combine(filePath));

            return id;
        }

        public void UpdateApproveDocument(int documentId, bool IsAccepted)
        {
            var document = All.Where(x => x.Id == documentId).FirstOrDefault();
            document.IsAccepted = IsAccepted;
            Update(document);
            _context.Save();
        }

        public List<CommonArtifactDocumentDto> GetExpiredDocumentReports(int projectId)
        {
            List<CommonArtifactDocumentDto> dataList = new List<CommonArtifactDocumentDto>();

            var documentList = All.Include(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.EtmfArtificateMasterLbrary)
           .ThenInclude(x => x.EtmfSectionMasterLibrary).ThenInclude(x => x.EtmfZoneMasterLibrary)
           .Where(x => x.DeletedDate == null && x.ProjectWorkplaceArtificate.ProjectId == projectId && x.ExpiryDate != null
           && (x.CreatedBy == _jwtTokenAccesser.UserId ||
           _context.ProjectArtificateDocumentReview.Any(m => m.ProjectWorkplaceArtificatedDocumentId == x.Id && m.UserId == _jwtTokenAccesser.UserId && m.DeletedDate == null)
           || _context.ProjectArtificateDocumentApprover.Any(m => m.ProjectWorkplaceArtificatedDocumentId == x.Id && m.UserId == _jwtTokenAccesser.UserId && m.DeletedDate == null))).ToList();
            foreach (var item in documentList)
            {
                CommonArtifactDocumentDto obj = new CommonArtifactDocumentDto();
                obj.Id = item.Id;
                obj.ProjectWorkplaceSubSectionArtifactId = item.ProjectWorkplaceArtificateId;
                obj.ProjectWorkplaceArtificateId = item.ProjectWorkplaceArtificateId;
                obj.Artificatename = _etmfArtificateMasterLbraryRepository.Find(item.ProjectWorkplaceArtificate.EtmfArtificateMasterLbraryId).ArtificateName;
                obj.SectionName = _etmfMasterLibraryRepository.Find(item.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.EtmfSectionMasterLibraryId).SectionName;
                obj.ZoneName = _etmfMasterLibraryRepository.Find(item.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.EtmfSectionMasterLibrary.EtmfMasterLibraryId).ZonName;
                obj.DocumentName = item.DocumentName;
                obj.ExtendedName = item.DocumentName.Contains('_') ? item.DocumentName.Substring(0, item.DocumentName.LastIndexOf('_')) : item.DocumentName;
                obj.DocPath = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), _jwtTokenAccesser.CompanyId.ToString(), item.DocPath, item.DocumentName);
                obj.FullDocPath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), item.DocPath);
                obj.CreatedByUser = _userRepository.Find((int)item.CreatedBy).UserName;
                //obj.Reviewer = users.OrderBy(x => x.SequenceNo).OrderBy(x => x.CreatedDate).ToList();
                obj.CreatedDate = item.CreatedDate;
                obj.Version = item.Version;
                obj.IsMoved = item.IsMoved;
                obj.StatusName = item.Status.GetDescription();
                obj.Status = (int)item.Status;
                obj.Level = 6;
                obj.SendBy = !(item.CreatedBy == _jwtTokenAccesser.UserId);
                obj.SendAndSendBack = !(item.CreatedBy == _jwtTokenAccesser.UserId);
                //obj.ReviewStatus = Review.Count() == 0 ? "" : Review.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsReviewed == true)) ? "Reviewed" : Review.GroupBy(u => u.UserId).All(z => z.All(x => x.IsReviewed == false) && z.Any(x => x.IsSendBack == true)) ? "Send Back" : "Send";
                //obj.IsReview = Review.Count() == 0 ? false : Review.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsReviewed == true)) ? true : false;
                obj.IsSendBack = _context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id && x.UserId == _jwtTokenAccesser.UserId).OrderByDescending(x => x.Id).Select(z => z.IsSendBack).FirstOrDefault();
                obj.IsAccepted = item.IsAccepted;
                //obj.ApprovedStatus = ApproveList.Count() == 0 ? "" : ApproveList.GroupBy(u => u.UserId).All(z => z.All(x => x.IsApproved == false)) ? "Reject" : ApproveList.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsApproved == true)) ? "Approved" : "Send For Approval";
                //obj.Approver = ApproverName.OrderBy(x => x.SequenceNo).ToList();
                obj.EtmfArtificateMasterLbraryId = item.ProjectWorkplaceArtificate.EtmfArtificateMasterLbraryId;
                //obj.IsApproveDoc = ApproveList.Any(x => x.UserId == _jwtTokenAccesser.UserId && x.IsApproved == null) ? true : false;
                obj.AddedBy = item.CreatedBy == _jwtTokenAccesser.UserId;
                obj.IsReplyAllComment = item.IsReplyAllComment;
                //obj.SequenceNo = currentReviewer?.SequenceNo;
                //obj.ApproveSequenceNo = currentApprover?.SequenceNo;
                obj.ExpiryDate = item.ExpiryDate;
                dataList.Add(obj);
            }

            return dataList;
        }

        public List<CommonArtifactDocumentDto> GetDocumentList(int id)
        {
            List<CommonArtifactDocumentDto> dataList = new List<CommonArtifactDocumentDto>();

            var artificate = _context.EtmfProjectWorkPlace.Where(x => x.Id == id).Include(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace).FirstOrDefault();

            var rights = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId == artificate.ProjectWorkPlace.ProjectWorkPlace.EtmfProjectWorkPlaceId
                         && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null)
                         .OrderByDescending(x => x.Id).FirstOrDefault();

            var documentList = All.Include(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.EtmfArtificateMasterLbrary)
                .ThenInclude(x => x.EtmfSectionMasterLibrary).ThenInclude(x => x.EtmfZoneMasterLibrary)
                .Where(x => x.ProjectWorkplaceArtificateId == id && x.DeletedDate == null
                && (x.CreatedBy == _jwtTokenAccesser.UserId ||
                _context.ProjectArtificateDocumentReview.Any(m => m.ProjectWorkplaceArtificatedDocumentId == x.Id && m.UserId == _jwtTokenAccesser.UserId && m.DeletedDate == null)
                || _context.ProjectArtificateDocumentApprover.Any(m => m.ProjectWorkplaceArtificatedDocumentId == x.Id && m.UserId == _jwtTokenAccesser.UserId && m.DeletedDate == null))).ToList();

            foreach (var item in documentList)
            {
                var reviewerList = _context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id && x.UserId != item.CreatedBy && x.DeletedDate == null).Select(z => new { UserId = z.UserId, SequenceNo = z.SequenceNo, IsSendBack = z.IsSendBack, IsReview = z.IsReviewed, CreatedDate = z.CreatedDate, SendBackDate = z.SendBackDate }).ToList();
                var users = new List<DocumentUsers>();
                reviewerList.ForEach(r =>
                {
                    DocumentUsers obj = new DocumentUsers();
                    obj.UserName = _userRepository.Find(r.UserId).UserName;
                    obj.UserId = r.UserId;
                    obj.SequenceNo = r.SequenceNo;
                    obj.IsSendBack = r.IsSendBack;
                    obj.IsReview = r.IsReview;
                    obj.CreatedDate = r.CreatedDate;
                    obj.SendBackDate = r.SendBackDate;
                    users.Add(obj);
                });

                var Review = _context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id
                && x.UserId != item.CreatedBy && x.DeletedDate == null).ToList();

                var ApproveList = _context.ProjectArtificateDocumentApprover.Where(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id && x.DeletedDate == null).OrderByDescending(x => x.Id).ToList()
                   .Select(y => new ProjectArtificateDocumentApprover
                   {
                       Id = y.Id,
                       UserId = y.UserId,
                       ProjectWorkplaceArtificatedDocumentId = y.ProjectWorkplaceArtificatedDocumentId,
                       IsApproved = y.IsApproved,
                       SequenceNo = y.SequenceNo,
                       CreatedDate = y.CreatedDate,
                       ModifiedDate = y.ModifiedDate
                   }).ToList();

                var ApproverName = new List<DocumentUsers>();
                ApproveList.ForEach(r =>
                {
                    DocumentUsers obj = new DocumentUsers();
                    obj.UserName = _userRepository.Find(r.UserId).UserName;
                    obj.SequenceNo = r.SequenceNo;
                    obj.IsSendBack = r.IsApproved;
                    obj.CreatedDate = r.CreatedDate;
                    obj.SendBackDate = r.ModifiedDate;
                    ApproverName.Add(obj);
                });

                var currentReviewer = _context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id
                && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null && x.IsSendBack == false && x.IsReviewed == false).FirstOrDefault();

                var currentApprover = _context.ProjectArtificateDocumentApprover.Where(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id
               && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null && (x.IsApproved == null || x.IsApproved == false)).FirstOrDefault();

                CommonArtifactDocumentDto obj = new CommonArtifactDocumentDto();
                obj.Id = item.Id;
                obj.ProjectWorkplaceSubSectionArtifactId = item.ProjectWorkplaceArtificateId;
                obj.ProjectWorkplaceArtificateId = item.ProjectWorkplaceArtificateId;
                obj.Artificatename = _etmfArtificateMasterLbraryRepository.Find(item.ProjectWorkplaceArtificate.EtmfArtificateMasterLbraryId).ArtificateName;
                obj.SectionName = _etmfMasterLibraryRepository.Find(item.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.EtmfSectionMasterLibraryId).SectionName;
                obj.ZoneName = _etmfMasterLibraryRepository.Find(item.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.EtmfSectionMasterLibrary.EtmfMasterLibraryId).ZonName;
                obj.DocumentName = item.DocumentName;
                obj.ExtendedName = item.DocumentName.Contains('_') ? item.DocumentName.Substring(0, item.DocumentName.LastIndexOf('_')) : item.DocumentName;
                obj.DocPath = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), _jwtTokenAccesser.CompanyId.ToString(), item.DocPath, item.DocumentName);
                obj.FullDocPath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), item.DocPath);
                obj.CreatedByUser = _userRepository.Find((int)item.CreatedBy).UserName;
                obj.Reviewer = users.OrderBy(x => x.SequenceNo).OrderBy(x => x.CreatedDate).ToList();
                obj.CreatedDate = item.CreatedDate;
                obj.Version = item.Version;
                obj.IsMoved = item.IsMoved;
                obj.StatusName = item.Status.GetDescription();
                obj.Status = (int)item.Status;
                obj.Level = 6;
                obj.SendBy = !(item.CreatedBy == _jwtTokenAccesser.UserId);
                obj.SendAndSendBack = !(item.CreatedBy == _jwtTokenAccesser.UserId);
                obj.ReviewStatus = Review.Count() == 0 ? "" : Review.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsReviewed == true)) ? "Reviewed" : Review.GroupBy(u => u.UserId).All(z => z.All(x => x.IsReviewed == false) && z.Any(x => x.IsSendBack == true)) ? "Send Back" : "Send";
                obj.IsReview = Review.Count() == 0 ? false : Review.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsReviewed == true)) ? true : false;
                obj.IsSendBack = _context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id && x.UserId == _jwtTokenAccesser.UserId).OrderByDescending(x => x.Id).Select(z => z.IsSendBack).FirstOrDefault();
                obj.IsAccepted = item.IsAccepted;
                obj.ApprovedStatus = ApproveList.Count() == 0 ? "" : ApproveList.GroupBy(u => u.UserId).All(z => z.All(x => x.IsApproved == false)) ? "Reject" : ApproveList.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsApproved == true)) ? "Approved" : "Send For Approval";
                obj.Approver = ApproverName.OrderBy(x => x.SequenceNo).ToList();
                obj.EtmfArtificateMasterLbraryId = item.ProjectWorkplaceArtificate.EtmfArtificateMasterLbraryId;
                obj.IsApproveDoc = ApproveList.Any(x => x.UserId == _jwtTokenAccesser.UserId && x.IsApproved == null) ? true : false;
                obj.AddedBy = item.CreatedBy == _jwtTokenAccesser.UserId;
                obj.IsReplyAllComment = item.IsReplyAllComment;
                obj.SequenceNo = currentReviewer?.SequenceNo;
                obj.ApproveSequenceNo = currentApprover?.SequenceNo;
                obj.ExpiryDate = item.ExpiryDate;
                dataList.Add(obj);
            }
            return dataList.OrderByDescending(q => q.CreatedDate).ToList();
        }

        public CommonArtifactDocumentDto GetDocument(int id)
        {
            var document = All.Include(x => x.ProjectWorkplaceArtificate).Where(x => x.Id == id && x.DeletedDate == null).FirstOrDefault();


            var currentReviewer = _context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == id
              && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null && x.IsSendBack == false && x.IsReviewed == false).FirstOrDefault();

            var currentApprover = _context.ProjectArtificateDocumentApprover.Where(x => x.ProjectWorkplaceArtificatedDocumentId == id
           && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null && (x.IsApproved == null || x.IsApproved == false)).FirstOrDefault();

            var reviewerList = _context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == document.Id && x.UserId != document.CreatedBy && x.DeletedDate == null).ToList();


            CommonArtifactDocumentDto obj = new CommonArtifactDocumentDto();
            obj.Id = document.Id;
            obj.ProjectWorkplaceSubSectionArtifactId = document.ProjectWorkplaceArtificateId;
            obj.ProjectWorkplaceArtificateId = document.ProjectWorkplaceArtificateId;
            obj.DocumentName = document.DocumentName;
            obj.ExtendedName = document.DocumentName.Contains('_') ? document.DocumentName.Substring(0, document.DocumentName.LastIndexOf('_')) : document.DocumentName;
            obj.DocPath = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, document.DocumentName);
            obj.FullDocPath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), document.DocPath);
            obj.CreatedByUser = _userRepository.Find((int)document.CreatedBy).UserName;
            obj.CreatedDate = document.CreatedDate;
            obj.Version = document.Version;
            obj.StatusName = document.Status.GetDescription();
            obj.Status = (int)document.Status;
            obj.Level = 6;
            obj.IsMoved = document.IsMoved;
            obj.SendBy = !(document.CreatedBy == _jwtTokenAccesser.UserId);
            obj.AddedBy = (document.CreatedBy == _jwtTokenAccesser.UserId);
            obj.IsSendBack = _context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == document.Id && x.UserId == _jwtTokenAccesser.UserId).OrderByDescending(x => x.Id).Select(z => z.IsSendBack).FirstOrDefault();
            obj.IsAccepted = document.IsAccepted;
            obj.ApprovedStatus = document.IsAccepted == null ? "" : document.IsAccepted == true ? "Approved" : "Rejected";
            obj.EtmfArtificateMasterLbraryId = document.ProjectWorkplaceArtificate.EtmfArtificateMasterLbraryId;
            obj.IsReplyAllComment = document.IsReplyAllComment;
            obj.SequenceNo = currentReviewer?.SequenceNo;
            obj.ApproveSequenceNo = currentApprover?.SequenceNo;
            obj.ReviewStatus = reviewerList.Count() == 0 ? "" : reviewerList.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsReviewed == true)) ? "Send Back" : "Send";
            obj.IsReview = reviewerList.Count() == 0 ? false : reviewerList.GroupBy(u => u.UserId).All(z => z.Any(x => x.IsReviewed == true)) ? true : false;
            return obj;
        }

        public string Duplicate(ProjectWorkplaceArtificatedocument objSave, ProjectWorkplaceArtificatedocumentDto objSaveDto)
        {
            if (All.Where(x => GetDocumentOriginalName(x.DocumentName, objSaveDto.FileName) == true && x.Id != objSave.Id && x.ProjectWorkplaceArtificateId == objSave.ProjectWorkplaceArtificateId
             && x.DeletedDate == null).ToList().Count > 0)
                return "Duplicate Document name : " + objSaveDto.FileName;
            return "";
        }

        public bool GetDocumentOriginalName(string name, string name2)
        {
            if (name.Substring(0, name.LastIndexOf('_')) == name2)
                return true;
            return false;
        }

        public ProjectWorkplaceArtificatedocument AddDocument(ProjectWorkplaceArtificatedocumentDto projectWorkplaceArtificatedocumentDto)
        {
            var Project = _projectRepository.Find(projectWorkplaceArtificatedocumentDto.ProjectId);
            var Projectname = Project.ProjectCode.Replace("/", "");

            string filePath = string.Empty;
            string path = string.Empty;

            //if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Country)

            //    path = System.IO.Path.Combine(Projectname, WorkPlaceFolder.Country.GetDescription(),
            //      projectWorkplaceArtificatedocumentDto.Countryname.Trim(), projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());
            //else if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Site)
            //    path = System.IO.Path.Combine(Projectname, WorkPlaceFolder.Site.GetDescription(),
            //     projectWorkplaceArtificatedocumentDto.Sitename.Trim(), projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());
            //else if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Trial)
            //    path = System.IO.Path.Combine(Projectname, WorkPlaceFolder.Trial.GetDescription(),
            //       projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());

            //filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path);
            //string FileName = DocumentService.SaveWorkplaceDocument(projectWorkplaceArtificatedocumentDto.FileModel, filePath, projectWorkplaceArtificatedocumentDto.FileName);

            if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(Projectname, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Country.GetDescription(),
                  projectWorkplaceArtificatedocumentDto.Countryname.Trim(), projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());
            else if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(Projectname, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Site.GetDescription(),
                 projectWorkplaceArtificatedocumentDto.Sitename.Trim(), projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());
            else if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(Projectname, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Trial.GetDescription(),
                   projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());

            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), path);
            string FileName = DocumentService.SaveWorkplaceDocument(projectWorkplaceArtificatedocumentDto.FileModel, filePath, projectWorkplaceArtificatedocumentDto.FileName);

            projectWorkplaceArtificatedocumentDto.Id = 0;
            var projectWorkplaceArtificatedocument = _mapper.Map<ProjectWorkplaceArtificatedocument>(projectWorkplaceArtificatedocumentDto);
            projectWorkplaceArtificatedocument.DocumentName = FileName;
            projectWorkplaceArtificatedocument.DocPath = path;
            projectWorkplaceArtificatedocument.Status = ArtifactDocStatusType.Draft;
            projectWorkplaceArtificatedocument.Version = "1.0";
            projectWorkplaceArtificatedocument.ParentDocumentId = projectWorkplaceArtificatedocumentDto.ParentDocumentId;
            return projectWorkplaceArtificatedocument;
        }

        public ProjectWorkplaceArtificatedocument AddMovedDocument(WorkplaceFolderDto data)
        {
            var document = All.Where(x => x.Id == data.DocumentId)
               .Include(d => d.ProjectArtificateDocumentReview)
               .Include(d => d.ProjectArtificateDocumentApprover)
               .Include(d => d.ProjectArtificateDocumentComment)
               .Include(d => d.ProjectArtificateDocumentHistory)
               .AsNoTracking().FirstOrDefault();

            return document;
        }

        public List<DropDownDto> GetEtmfZoneDropdown(int projectId)
        {
            var version = _context.EtmfProjectWorkPlace.Include(x => x.EtmfMasterLibrary)
                .Where(x => x.ProjectId == projectId)
                .Select(x => x.EtmfMasterLibrary.Version).FirstOrDefault();

            return _context.EtmfMasterLibrary
                .Where(x => x.Version == version)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.ZonName })
                .ToList();
        }

        public List<DropDownDto> GetEtmfCountrySiteDropdown(int projectId, int folderId)
        {
            //int workplaceid = _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == projectId && x.DeletedDate == null).FirstOrDefault()?.Id;

            var data = _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == projectId && x.WorkPlaceFolderId == folderId && x.DeletedDate == null && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceDetail)
                        .Select(x => new DropDownDto
                        {
                            Id = x.Id,
                            Value = x.ItemName
                        }).ToList();
            return data;
        }

        public List<DropDownDto> GetEtmfSectionDropdown(int zoneId)
        {
            return _etmfMasterLibraryRepository.FindBy(x => x.EtmfMasterLibraryId == zoneId)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.SectionName })//.OrderBy(o => o.Value)
               .ToList();
        }

        public List<DropDownDto> GetEtmfArtificateDropdown(int sectionId)
        {
            return _etmfArtificateMasterLbraryRepository.FindBy(x => x.EtmfSectionMasterLibraryId == sectionId)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.ArtificateName })//.OrderBy(o => o.Value)
               .ToList();
        }


        public List<DropDownDto> GetEtmfSubSectionDropdown(int sectionId)
        {
            var subSections = _context.EtmfProjectWorkPlace.Where(x => x.EtmfMasterLibraryId == sectionId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSection).Select(s => s.Id).ToList();
            return _context.EtmfProjectWorkPlace.Where(x => subSections.Contains(x.EtmfProjectWorkPlaceId) && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSection)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.SubSectionName })//.OrderBy(o => o.Value)
               .ToList();
        }

        public List<DropDownDto> GetEtmfSubSectionArtificateDropdown(int subSectionId)
        {
            return _context.EtmfProjectWorkPlace.Where(x => x.EtmfProjectWorkPlaceId == subSectionId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSectionArtifact)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.ArtifactName })//.OrderBy(o => o.Value)
               .ToList();
        }

        public IList<EtmfAuditLogReportDto> GetEtmfAuditLogReport(EtmfAuditLogReportSearchDto filters)
        {
            var ProjectCode = _context.Project.Where(x => x.Id == filters.projectId).FirstOrDefault().ProjectCode;
            var workplace = _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == filters.projectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlace).ToList().FirstOrDefault();
            var workplacedetail = new List<int>();
            if (workplace == null)
            {
                return null;
            }
            if (filters.folderId != null)
            {
                if (filters.countrySiteId != null)
                {
                    workplacedetail = _context.EtmfProjectWorkPlace.Where(x => x.EtmfProjectWorkPlaceId == workplace.Id
                    && x.WorkPlaceFolderId == filters.folderId && x.Id == filters.countrySiteId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceDetail).Select(y => y.Id).ToList();
                }
                else
                {
                    workplacedetail = _context.EtmfProjectWorkPlace.Where(x => x.EtmfProjectWorkPlaceId == workplace.Id && x.WorkPlaceFolderId == filters.folderId
                    && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceDetail).Select(y => y.Id).ToList();
                }
            }
            else
            {
                workplacedetail = _context.EtmfProjectWorkPlace.Where(x => x.EtmfProjectWorkPlaceId == workplace.Id
                && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceDetail).Select(y => y.Id).ToList();
            }
            var workplacezone = new List<int>();
            if (filters.zoneId != null)
            {
                workplacezone = _context.EtmfProjectWorkPlace.Where(x => x.EtmfMasterLibraryId == filters.zoneId
                && workplacedetail.Contains(x.EtmfProjectWorkPlaceId) && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceZone).Select(y => y.Id).ToList();
            }
            else
            {
                workplacezone = _context.EtmfProjectWorkPlace.Where(x => workplacedetail.Contains(x.EtmfProjectWorkPlaceId)
                && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceZone).Select(y => y.Id).ToList();
            }
            var workplacesection = new List<int>();
            if (filters.sectionId != null)
            {
                workplacesection = _context.EtmfProjectWorkPlace.Where(x => x.EtmfMasterLibraryId == filters.sectionId && workplacezone.Contains(x.EtmfProjectWorkPlaceId)
                && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSection).Select(y => y.Id).ToList();
            }
            else
            {
                workplacesection = _context.EtmfProjectWorkPlace.Where(x => workplacezone.Contains(x.EtmfProjectWorkPlaceId)
                && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSection).Select(y => y.Id).ToList();
            }
            var workplaceartificate = new List<int>();
            if (filters.artificateId != null)
            {
                workplaceartificate = _context.EtmfProjectWorkPlace.Where(x => x.EtmfArtificateMasterLbraryId == filters.artificateId && workplacesection.Contains(x.EtmfProjectWorkPlaceId)
                && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceArtificate).Select(y => y.Id).ToList();
            }
            else
            {
                workplaceartificate = _context.EtmfProjectWorkPlace.Where(x => workplacesection.Contains(x.EtmfProjectWorkPlaceId)
                && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceArtificate).Select(y => y.Id).ToList();
            }
            var workplaceartificatedocument = new List<int>();
            workplaceartificatedocument = FindByInclude(x => workplaceartificate.Contains(x.ProjectWorkplaceArtificateId)).Select(y => y.Id).ToList();

            #region section
            var projectWorkplaceArtificatedocuments = All.Include(x => x.ProjectWorkplaceArtificate).
                ThenInclude(x => x.EtmfArtificateMasterLbrary).
                Include(x => x.ProjectWorkplaceArtificate).
                ThenInclude(x => x.ProjectWorkPlace).
                ThenInclude(x => x.ProjectWorkPlace).
                ThenInclude(x => x.ProjectWorkPlace).
                Include(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary).
                Include(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                .Where(x => workplaceartificate.Contains(x.ProjectWorkplaceArtificateId)
                && x.ProjectWorkplaceArtificate.ProjectId == filters.projectId
                && x.ProjectWorkplaceArtificate.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceArtificate).OrderByDescending(x => x.Id).ToList();

            var projectWorkplaceArtificatedocumentreviews = _context.ProjectArtificateDocumentReview.Where(x => workplaceartificatedocument.Contains(x.ProjectWorkplaceArtificatedDocumentId)).ToList();
            var projectWorkplaceArtificatedocumentapprover = _context.ProjectArtificateDocumentApprover.Where(x => workplaceartificatedocument.Contains(x.ProjectWorkplaceArtificatedDocumentId)).ToList();
            var auditrialdata = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectWorkplaceArtificatedocument" && x.Reason != null).ToList();

            var Documents = projectWorkplaceArtificatedocuments.Select(r => new EtmfAuditLogReportDto
            {
                Id = r.Id,
                projectCode = ProjectCode,
                folderName = ((WorkPlaceFolder)r.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription(),
                countrysiteName = r.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                zoneName = r.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                sectionName = r.ProjectWorkplaceArtificate.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                artificateName = r.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName,
                documentName = r.DocumentName,
                version = r.Version,
                status = r.Status.GetDescription(),
                ParentArtificateId = r.ProjectWorkplaceArtificate.ParentArtificateId,
                CreatedBy = r.CreatedBy,
                CreatedDate = r.CreatedDate,
                DeletedDate = r.DeletedDate,
                DeletedBy = r.DeletedBy,
                ModifiedDate = r.ModifiedDate
            }).ToList();

            var cretaedData = Documents.Select(r =>
            {
                r.action = r.ParentArtificateId != null ? "Move" : "Created";
                r.userName = _userRepository.Find((int)r.CreatedBy).UserName;
                r.actionDate = r.CreatedDate;
                r.auditComment = auditrialdata.Where(x => x.Action == "Added" && x.ColumnName == "Document Name" && x.RecordId == r.Id).ToList().FirstOrDefault()?.ReasonOth;
                r.auditReason = auditrialdata.Where(x => x.Action == "Added" && x.ColumnName == "Document Name" && x.RecordId == r.Id).ToList().FirstOrDefault()?.Reason;
                return r;
            }).ToList();

            var sendData = (from doc in Documents
                            join review in projectWorkplaceArtificatedocumentreviews on doc.Id equals review.ProjectWorkplaceArtificatedDocumentId
                            where doc.CreatedBy != review.UserId
                            select new EtmfAuditLogReportDto
                            {
                                projectCode = doc.projectCode,
                                folderName = doc.folderName,
                                countrysiteName = doc.countrysiteName,
                                zoneName = doc.zoneName,
                                sectionName = doc.sectionName,
                                artificateName = doc.artificateName,
                                documentName = doc.documentName,
                                version = doc.version,
                                status = doc.status,
                                action = "Send for Review",
                                userName = _userRepository.Find(review.UserId).UserName,
                                actionDate = review.CreatedDate
                            }).ToList();

            var sendBackData = (from doc in Documents
                                join review in projectWorkplaceArtificatedocumentreviews on doc.Id equals review.ProjectWorkplaceArtificatedDocumentId
                                where review.IsSendBack == true
                                select new EtmfAuditLogReportDto
                                {
                                    projectCode = doc.projectCode,
                                    folderName = doc.folderName,
                                    countrysiteName = doc.countrysiteName,
                                    zoneName = doc.zoneName,
                                    sectionName = doc.sectionName,
                                    artificateName = doc.artificateName,
                                    documentName = doc.documentName,
                                    version = doc.version,
                                    status = doc.status,
                                    action = "Send Back",
                                    userName = _userRepository.Find(review.UserId).UserName,
                                    actionDate = review.SendBackDate,
                                    auditComment = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectArtificateDocumentReview" && x.Action == "Modified" && x.ColumnName == "Is SendBack" && x.RecordId == review.Id).FirstOrDefault()?.ReasonOth,
                                    auditReason = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectArtificateDocumentReview" && x.Action == "Modified" && x.ColumnName == "Is SendBack" && x.RecordId == review.Id).FirstOrDefault()?.Reason,
                                }).ToList();

            var DeletesSendReview = (from doc in Documents
                                     join review in projectWorkplaceArtificatedocumentreviews on doc.Id equals review.ProjectWorkplaceArtificatedDocumentId
                                     where review.DeletedDate != null
                                     select new EtmfAuditLogReportDto
                                     {
                                         projectCode = doc.projectCode,
                                         folderName = doc.folderName,
                                         countrysiteName = doc.countrysiteName,
                                         zoneName = doc.zoneName,
                                         sectionName = doc.sectionName,
                                         artificateName = doc.artificateName,
                                         documentName = doc.documentName,
                                         version = doc.version,
                                         status = doc.status,
                                         action = "Deleted Review",
                                         userName = _userRepository.Find(review.UserId).UserName,
                                         actionDate = review.DeletedDate,
                                         auditComment = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectArtificateDocumentReview" && x.Action == "Deleted" && x.RecordId == review.Id).FirstOrDefault()?.ReasonOth,
                                         auditReason = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectArtificateDocumentReview" && x.Action == "Deleted" && x.RecordId == review.Id).FirstOrDefault()?.Reason,
                                     }).ToList();

            var sendforApproveData = (from doc in Documents
                                      join approve in projectWorkplaceArtificatedocumentapprover on doc.Id equals approve.ProjectWorkplaceArtificatedDocumentId
                                      select new EtmfAuditLogReportDto
                                      {
                                          projectCode = doc.projectCode,
                                          folderName = doc.folderName,
                                          countrysiteName = doc.countrysiteName,
                                          zoneName = doc.zoneName,
                                          sectionName = doc.sectionName,
                                          artificateName = doc.artificateName,
                                          documentName = doc.documentName,
                                          version = doc.version,
                                          status = doc.status,
                                          action = "Send for Approve",
                                          userName = _userRepository.Find(approve.UserId).UserName,
                                          actionDate = approve.CreatedDate
                                      }).ToList();

            var ApprovedData = (from doc in Documents
                                join approve in projectWorkplaceArtificatedocumentapprover on doc.Id equals approve.ProjectWorkplaceArtificatedDocumentId
                                where approve.IsApproved != null
                                select new EtmfAuditLogReportDto
                                {
                                    projectCode = doc.projectCode,
                                    folderName = doc.folderName,
                                    countrysiteName = doc.countrysiteName,
                                    zoneName = doc.zoneName,
                                    sectionName = doc.sectionName,
                                    artificateName = doc.artificateName,
                                    documentName = doc.documentName,
                                    version = doc.version,
                                    status = doc.status,
                                    action = approve.IsApproved == true ? "Approved" : "Rejected",
                                    userName = _userRepository.Find((int)approve.UserId).UserName,
                                    actionDate = approve.ModifiedDate,
                                    auditComment = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectArtificateDocumentApprover" && x.RecordId == approve.Id && x.Action == "Modified" && x.ColumnName == "Is Approved").FirstOrDefault()?.ReasonOth,
                                    auditReason = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectArtificateDocumentApprover" && x.RecordId == approve.Id && x.Action == "Modified" && x.ColumnName == "Is Approved").FirstOrDefault()?.Reason,
                                }).ToList();

            var DeletedApprovedData = (from doc in Documents
                                       join approve in projectWorkplaceArtificatedocumentapprover on doc.Id equals approve.ProjectWorkplaceArtificatedDocumentId
                                       where approve.DeletedDate != null
                                       select new EtmfAuditLogReportDto
                                       {
                                           projectCode = doc.projectCode,
                                           folderName = doc.folderName,
                                           countrysiteName = doc.countrysiteName,
                                           zoneName = doc.zoneName,
                                           sectionName = doc.sectionName,
                                           artificateName = doc.artificateName,
                                           documentName = doc.documentName,
                                           version = doc.version,
                                           status = doc.status,
                                           action = "Deleted Approve",
                                           userName = _userRepository.Find(approve.UserId).UserName,
                                           actionDate = approve.DeletedDate,
                                           auditComment = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectArtificateDocumentApprover" && x.RecordId == approve.Id && x.Action == "Deleted").FirstOrDefault()?.ReasonOth,
                                           auditReason = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectArtificateDocumentApprover" && x.RecordId == approve.Id && x.Action == "Deleted").FirstOrDefault()?.Reason,
                                       }).ToList();


            var deletedData = Documents.Where(x => x.DeletedDate != null).Select(r =>
            {
                r.action = "Delete";
                r.userName = _userRepository.Find((int)r.DeletedBy).UserName;
                r.actionDate = r.DeletedDate;
                r.auditComment = auditrialdata.Where(x => x.Action == "Deleted" && x.RecordId == r.Id).ToList().FirstOrDefault()?.ReasonOth;
                r.auditReason = auditrialdata.Where(x => x.Action == "Deleted" && x.RecordId == r.Id).ToList().FirstOrDefault()?.Reason;
                return r;
            }).ToList();

            var supersededata = Documents.Where(x => x.status == ArtifactDocStatusType.Supersede.GetDescription()).Select(r =>
            {
                r.action = "Supersede";
                r.userName = _userRepository.Find((int)r.CreatedBy).UserName;
                r.actionDate = auditrialdata.Where(x => x.Action == "Modified" && x.ColumnName == "Status" && x.NewValue == "Supersede" && x.RecordId == r.Id).ToList().FirstOrDefault()?.CreatedDate;
                r.auditComment = auditrialdata.Where(x => x.Action == "Modified" && x.ColumnName == "Status" && x.NewValue == "Supersede" && x.RecordId == r.Id).ToList().FirstOrDefault()?.ReasonOth;
                r.auditReason = auditrialdata.Where(x => x.Action == "Modified" && x.ColumnName == "Status" && x.NewValue == "Supersede" && x.RecordId == r.Id).ToList().FirstOrDefault()?.Reason;
                return r;
            }).ToList();

            var finaldata = Documents.Where(x => x.status == ArtifactDocStatusType.Final.GetDescription()).Select(r =>
            {
                r.action = "Final";
                r.userName = _userRepository.Find((int)r.CreatedBy).UserName;
                r.actionDate = r.ModifiedDate;
                //actionDate = auditrialdata.Where(x => x.Action == "Modified" && x.ColumnName == "Status" && x.NewValue == "Final" && x.RecordId == r.Id).ToList().FirstOrDefault()?.CreatedDate,
                //auditComment = auditrialdata.Where(x => x.Action == "Modified" && x.ColumnName == "Status" && x.NewValue == "Final" && x.RecordId == r.Id).ToList().FirstOrDefault()?.ReasonOth,
                //auditReason = auditrialdata.Where(x => x.Action == "Modified" && x.ColumnName == "Status" && x.NewValue == "Final" && x.RecordId == r.Id).ToList().FirstOrDefault()?.Reason?.ReasonName
                return r;
            }).ToList();

            var Artificate = _context.EtmfProjectWorkPlace.Include(x => x.EtmfArtificateMasterLbrary).
                Include(x => x.ProjectWorkPlace).
                ThenInclude(x => x.ProjectWorkPlace).
                ThenInclude(x => x.ProjectWorkPlace)
                .Include(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                .Include(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                .Where(x => workplaceartificate.Contains(x.Id) && x.ProjectId == filters.projectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceArtificate)
                .Select(r => new EtmfAuditLogReportDto
                {
                    Id = r.Id,
                    projectCode = ProjectCode,
                    folderName = ((WorkPlaceFolder)r.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription(),
                    countrysiteName = r.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                    zoneName = r.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                    sectionName = r.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                    artificateName = r.EtmfArtificateMasterLbrary.ArtificateName,
                    ParentArtificateId = r.ParentArtificateId,
                    CreatedBy = r.CreatedBy,
                    CreatedDate = r.CreatedDate,
                    DeletedDate = r.DeletedDate,
                    DeletedBy = r.DeletedBy,
                    ModifiedDate = r.ModifiedDate
                }).ToList();

            var requiredArtificate = (from artificate in Artificate
                                      join audit in _context.AuditTrail.Where(x => x.TableName == "EtmfProjectWorkPlace" && x.ColumnName == "Is NotRequired" && x.Action == "Modified")
                                      on artificate.Id equals audit.RecordId
                                      select new EtmfAuditLogReportDto
                                      {
                                          projectCode = artificate.projectCode,
                                          folderName = artificate.folderName,
                                          countrysiteName = artificate.countrysiteName,
                                          zoneName = artificate.zoneName,
                                          sectionName = artificate.sectionName,
                                          artificateName = artificate.artificateName,
                                          action = audit.OldValue == "Yes" ? "Remove Not Required" : "Not Required",
                                          userName = _userRepository.Find((int)audit.CreatedBy).UserName,
                                          actionDate = audit.CreatedDate,
                                          auditComment = audit.ReasonOth,
                                          auditReason = audit.Reason,
                                      }).ToList();

            #endregion

            #region SubSection
            var projectWorkplaceSubsectionDocuments = _context.ProjectWorkplaceSubSecArtificatedocument.Include(x => x.ProjectWorkplaceSubSectionArtifact)
                .ThenInclude(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace)
                .Include(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                .Include(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                .Where(x => workplacesection.Contains(x.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.EtmfProjectWorkPlaceId) && x.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectId == filters.projectId && x.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSection)
                .OrderByDescending(x => x.Id).ToList();

            var SubSecDocuments = projectWorkplaceSubsectionDocuments.Select(r => new EtmfAuditLogReportDto
            {
                Id = r.Id,
                projectCode = ProjectCode,
                folderName = ((WorkPlaceFolder)r.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription(),
                countrysiteName = r.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                zoneName = r.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                sectionName = r.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                subSectionName = r.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.SubSectionName,
                artificateName = r.ProjectWorkplaceSubSectionArtifact.ArtifactName,
                documentName = r.DocumentName,
                version = r.Version,
                status = r.Status.GetDescription(),
                CreatedBy = r.CreatedBy,
                CreatedDate = r.CreatedDate,
                DeletedDate = r.DeletedDate,
                DeletedBy = r.DeletedBy,
                ModifiedDate = r.ModifiedDate
            }).ToList();

            var projectWorkplaceSubSecdocumentreviews = _context.ProjectSubSecArtificateDocumentReview.Where(x => SubSecDocuments.Select(x => x.Id).Contains(x.ProjectWorkplaceSubSecArtificateDocumentId)).ToList();
            var projectWorkplaceSubSecdocumentapprover = _context.ProjectSubSecArtificateDocumentApprover.Where(x => SubSecDocuments.Select(x => x.Id).Contains(x.ProjectWorkplaceSubSecArtificateDocumentId)).ToList();
            var SubSecAuditTrialData = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectWorkplaceSubSecArtificatedocument" && x.Reason != null).ToList();

            var SubSecCretaedData = SubSecDocuments.Select(r =>
            {
                r.action = "Created";
                r.userName = _userRepository.Find((int)r.CreatedBy).UserName;
                r.actionDate = r.CreatedDate;
                r.auditComment = SubSecAuditTrialData.Where(x => x.Action == "Added" && x.ColumnName == "Document Name" && x.RecordId == r.Id).ToList().FirstOrDefault()?.ReasonOth;
                r.auditReason = SubSecAuditTrialData.Where(x => x.Action == "Added" && x.ColumnName == "Document Name" && x.RecordId == r.Id).ToList().FirstOrDefault()?.Reason;
                return r;
            }).ToList();

            var SubSecSendData = (from doc in SubSecDocuments
                                  join review in projectWorkplaceSubSecdocumentreviews on doc.Id equals review.ProjectWorkplaceSubSecArtificateDocumentId
                                  where doc.CreatedBy != review.UserId
                                  select new EtmfAuditLogReportDto
                                  {
                                      projectCode = doc.projectCode,
                                      folderName = doc.folderName,
                                      countrysiteName = doc.countrysiteName,
                                      zoneName = doc.zoneName,
                                      sectionName = doc.sectionName,
                                      subSectionName = doc.subSectionName,
                                      artificateName = doc.artificateName,
                                      documentName = doc.documentName,
                                      version = doc.version,
                                      status = doc.status,
                                      action = "Send for Review",
                                      userName = _userRepository.Find(review.UserId).UserName,
                                      actionDate = review.CreatedDate
                                  }).ToList();

            var SubSecSendBackData = (from doc in SubSecDocuments
                                      join review in projectWorkplaceSubSecdocumentreviews on doc.Id equals review.ProjectWorkplaceSubSecArtificateDocumentId
                                      where review.IsSendBack == true
                                      select new EtmfAuditLogReportDto
                                      {
                                          projectCode = doc.projectCode,
                                          folderName = doc.folderName,
                                          countrysiteName = doc.countrysiteName,
                                          zoneName = doc.zoneName,
                                          sectionName = doc.sectionName,
                                          subSectionName = doc.subSectionName,
                                          artificateName = doc.artificateName,
                                          documentName = doc.documentName,
                                          version = doc.version,
                                          status = doc.status,
                                          action = "Send Back",
                                          userName = _userRepository.Find(review.UserId).UserName,
                                          actionDate = review.SendBackDate,
                                          auditComment = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectSubSecArtificateDocumentReview" && x.Action == "Modified" && x.ColumnName == "Is SendBack" && x.RecordId == review.Id).FirstOrDefault()?.ReasonOth,
                                          auditReason = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectSubSecArtificateDocumentReview" && x.Action == "Modified" && x.ColumnName == "Is SendBack" && x.RecordId == review.Id).FirstOrDefault()?.Reason,
                                      }).ToList();

            var DeletedSubSecreviewer = (from doc in SubSecDocuments
                                         join review in projectWorkplaceSubSecdocumentreviews on doc.Id equals review.ProjectWorkplaceSubSecArtificateDocumentId
                                         where review.DeletedDate != null
                                         select new EtmfAuditLogReportDto
                                         {
                                             projectCode = doc.projectCode,
                                             folderName = doc.folderName,
                                             countrysiteName = doc.countrysiteName,
                                             zoneName = doc.zoneName,
                                             sectionName = doc.sectionName,
                                             subSectionName = doc.subSectionName,
                                             artificateName = doc.artificateName,
                                             documentName = doc.documentName,
                                             version = doc.version,
                                             status = doc.status,
                                             action = "Deleted Review",
                                             userName = _userRepository.Find(review.UserId).UserName,
                                             actionDate = review.DeletedDate,
                                             auditComment = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectSubSecArtificateDocumentReview" && x.Action == "Deleted" && x.RecordId == review.Id).FirstOrDefault()?.ReasonOth,
                                             auditReason = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectSubSecArtificateDocumentReview" && x.Action == "Deleted" && x.RecordId == review.Id).FirstOrDefault()?.Reason,
                                         }).ToList();

            var SubSecSendforApproveData = (from doc in SubSecDocuments
                                            join approve in projectWorkplaceSubSecdocumentapprover on doc.Id equals approve.ProjectWorkplaceSubSecArtificateDocumentId
                                            select new EtmfAuditLogReportDto
                                            {
                                                projectCode = doc.projectCode,
                                                folderName = doc.folderName,
                                                countrysiteName = doc.countrysiteName,
                                                zoneName = doc.zoneName,
                                                sectionName = doc.sectionName,
                                                subSectionName = doc.subSectionName,
                                                artificateName = doc.artificateName,
                                                documentName = doc.documentName,
                                                version = doc.version,
                                                status = doc.status,
                                                action = "Send for Approve",
                                                userName = _userRepository.Find(approve.UserId).UserName,
                                                actionDate = approve.CreatedDate
                                            }).ToList();

            var SubSecApprovedData = (from doc in SubSecDocuments
                                      join approve in projectWorkplaceSubSecdocumentapprover on doc.Id equals approve.ProjectWorkplaceSubSecArtificateDocumentId
                                      where approve.IsApproved != null
                                      select new EtmfAuditLogReportDto
                                      {
                                          projectCode = doc.projectCode,
                                          folderName = doc.folderName,
                                          countrysiteName = doc.countrysiteName,
                                          zoneName = doc.zoneName,
                                          sectionName = doc.sectionName,
                                          subSectionName = doc.subSectionName,
                                          artificateName = doc.artificateName,
                                          documentName = doc.documentName,
                                          version = doc.version,
                                          status = doc.status,
                                          action = approve.IsApproved == true ? "Approved" : "Rejected",
                                          userName = _userRepository.Find((int)approve.UserId).UserName,
                                          actionDate = approve.ModifiedDate,
                                          auditComment = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectSubSecArtificateDocumentApprover" && x.RecordId == approve.Id && x.Action == "Modified" && x.ColumnName == "Is Approved").FirstOrDefault()?.ReasonOth,
                                          auditReason = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectSubSecArtificateDocumentApprover" && x.RecordId == approve.Id && x.Action == "Modified" && x.ColumnName == "Is Approved").FirstOrDefault()?.Reason,
                                      }).ToList();

            var DeletedSubSecApprovedData = (from doc in SubSecDocuments
                                             join approve in projectWorkplaceSubSecdocumentapprover on doc.Id equals approve.ProjectWorkplaceSubSecArtificateDocumentId
                                             where approve.DeletedDate != null
                                             select new EtmfAuditLogReportDto
                                             {
                                                 projectCode = doc.projectCode,
                                                 folderName = doc.folderName,
                                                 countrysiteName = doc.countrysiteName,
                                                 zoneName = doc.zoneName,
                                                 sectionName = doc.sectionName,
                                                 subSectionName = doc.subSectionName,
                                                 artificateName = doc.artificateName,
                                                 documentName = doc.documentName,
                                                 version = doc.version,
                                                 status = doc.status,
                                                 action = "Deleted Approve",
                                                 userName = _userRepository.Find((int)approve.UserId).UserName,
                                                 actionDate = approve.DeletedDate,
                                                 auditComment = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectSubSecArtificateDocumentApprover" && x.RecordId == approve.Id && x.Action == "Deleted").FirstOrDefault()?.ReasonOth,
                                                 auditReason = _auditTrailRepository.FindByInclude(x => x.TableName == "ProjectSubSecArtificateDocumentApprover" && x.RecordId == approve.Id && x.Action == "Deleted").FirstOrDefault()?.Reason,
                                             }).ToList();

            var SubSecDeletedData = SubSecDocuments.Where(x => x.DeletedDate != null).Select(r =>
            {
                r.action = "Delete";
                r.userName = _userRepository.Find((int)r.DeletedBy).UserName;
                r.actionDate = r.DeletedDate;
                r.auditComment = SubSecAuditTrialData.Where(x => x.Action == "Deleted" && x.RecordId == r.Id).ToList().FirstOrDefault()?.ReasonOth;
                r.auditReason = SubSecAuditTrialData.Where(x => x.Action == "Deleted" && x.RecordId == r.Id).ToList().FirstOrDefault()?.Reason;
                return r;
            }).ToList();

            var SubSecSupersedeData = SubSecDocuments.Where(x => x.status == ArtifactDocStatusType.Supersede.GetDescription()).Select(r =>
            {
                r.action = "Supersede";
                r.userName = _userRepository.Find((int)r.CreatedBy).UserName;
                r.actionDate = SubSecAuditTrialData.Where(x => x.Action == "Modified" && x.ColumnName == "Status" && x.NewValue == "Supersede" && x.RecordId == r.Id).ToList().FirstOrDefault()?.CreatedDate;
                r.auditComment = SubSecAuditTrialData.Where(x => x.Action == "Modified" && x.ColumnName == "Status" && x.NewValue == "Supersede" && x.RecordId == r.Id).ToList().FirstOrDefault()?.ReasonOth;
                r.auditReason = SubSecAuditTrialData.Where(x => x.Action == "Modified" && x.ColumnName == "Status" && x.NewValue == "Supersede" && x.RecordId == r.Id).ToList().FirstOrDefault()?.Reason;
                return r;
            }).ToList();

            var SubSecFinalData = SubSecDocuments.Where(x => x.status == ArtifactDocStatusType.Final.GetDescription()).Select(r =>
            {
                r.action = "Final";
                r.userName = _userRepository.Find((int)r.CreatedBy).UserName;
                r.actionDate = r.ModifiedDate;
                //actionDate = auditrialdata.Where(x => x.Action == "Modified" && x.ColumnName == "Status" && x.NewValue == "Final" && x.RecordId == r.Id).ToList().FirstOrDefault()?.CreatedDate,
                //auditComment = auditrialdata.Where(x => x.Action == "Modified" && x.ColumnName == "Status" && x.NewValue == "Final" && x.RecordId == r.Id).ToList().FirstOrDefault()?.ReasonOth,
                //auditReason = auditrialdata.Where(x => x.Action == "Modified" && x.ColumnName == "Status" && x.NewValue == "Final" && x.RecordId == r.Id).ToList().FirstOrDefault()?.Reason?.ReasonName
                return r;
            }).ToList();

            var subsection = _context.EtmfProjectWorkPlace
                .Include(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace)
                .Include(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                .Include(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                .Where(x => workplacesection.Contains(x.EtmfProjectWorkPlaceId) && x.ProjectId == filters.projectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSection)
                .OrderByDescending(x => x.Id).ToList();

            var subsectionAdded = subsection.Where(x => x.DeletedDate == null).Select(r => new EtmfAuditLogReportDto
            {
                Id = r.Id,
                projectCode = ProjectCode,
                folderName = ((WorkPlaceFolder)r.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription(),
                countrysiteName = r.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                zoneName = r.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                sectionName = r.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                subSectionName = r.SubSectionName,
                CreatedBy = r.CreatedBy,
                CreatedDate = r.CreatedDate,
                DeletedDate = r.DeletedDate,
                DeletedBy = r.DeletedBy,
                ModifiedDate = r.ModifiedDate,
                action = "SubSection Added",
                actionDate = r.CreatedDate,
                userName = _userRepository.Find((int)r.CreatedBy).UserName,
                auditReason = _auditTrailRepository.FindByInclude(x => x.RecordId == r.Id && x.TableName == "EtmfProjectWorkPlace" && x.Action == "Added" && x.ColumnName == "SubSection Name").FirstOrDefault()?.Reason,
                auditComment = _auditTrailRepository.FindByInclude(x => x.RecordId == r.Id && x.TableName == "EtmfProjectWorkPlace" && x.Action == "Added" && x.ColumnName == "SubSection Name").FirstOrDefault()?.ReasonOth,
            }).ToList();

            var subsectionModified = subsection.Where(x => x.ModifiedDate != null).Select(r => new EtmfAuditLogReportDto
            {
                Id = r.Id,
                projectCode = ProjectCode,
                folderName = ((WorkPlaceFolder)r.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription(),
                countrysiteName = r.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                zoneName = r.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                sectionName = r.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                subSectionName = r.SubSectionName,
                CreatedBy = r.CreatedBy,
                CreatedDate = r.CreatedDate,
                DeletedDate = r.DeletedDate,
                DeletedBy = r.DeletedBy,
                ModifiedDate = r.ModifiedDate,
                action = "SubSection Modified",
                actionDate = r.ModifiedDate,
                userName = _userRepository.Find((int)r.ModifiedBy).UserName,
                auditReason = _auditTrailRepository.FindByInclude(x => x.RecordId == r.Id && x.TableName == "EtmfProjectWorkPlace" && x.Action == "Modified" && x.ColumnName == "SubSection Name").FirstOrDefault()?.Reason,
                auditComment = _auditTrailRepository.FindByInclude(x => x.RecordId == r.Id && x.TableName == "EtmfProjectWorkPlace" && x.Action == "Modified" && x.ColumnName == "SubSection Name").FirstOrDefault()?.ReasonOth,
            }).ToList();

            var subsectionDeleted = subsection.Where(x => x.DeletedDate != null).Select(r => new EtmfAuditLogReportDto
            {
                Id = r.Id,
                projectCode = ProjectCode,
                folderName = ((WorkPlaceFolder)r.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription(),
                countrysiteName = r.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                zoneName = r.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                sectionName = r.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                subSectionName = r.SubSectionName,
                CreatedBy = r.CreatedBy,
                CreatedDate = r.CreatedDate,
                DeletedDate = r.DeletedDate,
                DeletedBy = r.DeletedBy,
                ModifiedDate = r.ModifiedDate,
                action = "SubSection Deleted",
                actionDate = r.DeletedDate,
                userName = _userRepository.Find((int)r.DeletedBy).UserName,
                auditReason = _auditTrailRepository.FindByInclude(x => x.RecordId == r.Id && x.TableName == "EtmfProjectWorkPlace" && x.Action == "Deleted").FirstOrDefault().Reason,
                auditComment = _auditTrailRepository.FindByInclude(x => x.RecordId == r.Id && x.TableName == "EtmfProjectWorkPlace" && x.Action == "Deleted").FirstOrDefault().ReasonOth,
            }).ToList();

            var subsectionArtificate = _context.EtmfProjectWorkPlace
                .Include(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace)
                .Include(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                .Include(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                .Where(x => workplacesection.Contains(x.ProjectWorkPlace.EtmfProjectWorkPlaceId) && x.ProjectWorkPlace.ProjectId == filters.projectId && x.ProjectWorkPlace.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSectionArtifact)
                .Select(r => new EtmfAuditLogReportDto
                {
                    Id = r.Id,
                    projectCode = ProjectCode,
                    folderName = ((WorkPlaceFolder)r.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription(),
                    countrysiteName = r.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                    zoneName = r.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                    sectionName = r.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                    subSectionName = r.ProjectWorkPlace.SubSectionName,
                    artificateName = r.ArtifactName,
                    CreatedBy = r.CreatedBy,
                    CreatedDate = r.CreatedDate,
                    DeletedDate = r.DeletedDate,
                    DeletedBy = r.DeletedBy,
                    ModifiedDate = r.ModifiedDate
                }).OrderByDescending(x => x.Id).ToList();

            var subsectionRequiredArtificate = (from subsecArtificate in subsectionArtificate
                                                join audit in _context.AuditTrail.Where(x => x.TableName == "EtmfProjectWorkPlace" && x.ColumnName == "Is NotRequired" && x.Action == "Modified")
                                                on subsecArtificate.Id equals audit.RecordId
                                                select new EtmfAuditLogReportDto
                                                {
                                                    projectCode = subsecArtificate.projectCode,
                                                    folderName = subsecArtificate.folderName,
                                                    countrysiteName = subsecArtificate.countrysiteName,
                                                    zoneName = subsecArtificate.zoneName,
                                                    sectionName = subsecArtificate.sectionName,
                                                    subSectionName = subsecArtificate.subSectionName,
                                                    artificateName = subsecArtificate.artificateName,
                                                    action = audit.OldValue == "Yes" ? "Remove Not Required" : "Not Required",
                                                    userName = _userRepository.Find((int)audit.CreatedBy).UserName,
                                                    actionDate = audit.CreatedDate,
                                                    auditComment = audit.ReasonOth,
                                                    auditReason = audit.Reason,
                                                }).ToList();

            var subsectionArtificateAdded = subsectionArtificate.Where(x => x.DeletedDate == null).Select(r => new EtmfAuditLogReportDto
            {
                projectCode = r.projectCode,
                folderName = r.folderName,
                countrysiteName = r.countrysiteName,
                zoneName = r.zoneName,
                sectionName = r.sectionName,
                subSectionName = r.subSectionName,
                artificateName = r.artificateName,
                action = "Sub Section Artificate Added",
                userName = _userRepository.Find((int)r.CreatedBy).UserName,
                actionDate = r.CreatedDate,
            }).ToList();

            #endregion

            var result = new List<EtmfAuditLogReportDto>();
            result.AddRange(cretaedData);
            result.AddRange(sendData);
            result.AddRange(sendBackData);
            result.AddRange(sendforApproveData);
            result.AddRange(ApprovedData);
            result.AddRange(deletedData);
            result.AddRange(supersededata);
            result.AddRange(finaldata);
            result.AddRange(DeletesSendReview);
            result.AddRange(DeletedApprovedData);
            result.AddRange(requiredArtificate);

            result.AddRange(SubSecCretaedData);
            result.AddRange(SubSecSendData);
            result.AddRange(SubSecSendBackData);
            result.AddRange(SubSecSendforApproveData);
            result.AddRange(SubSecApprovedData);
            result.AddRange(SubSecDeletedData);
            result.AddRange(SubSecSupersedeData);
            result.AddRange(SubSecFinalData);
            result.AddRange(DeletedSubSecreviewer);
            result.AddRange(DeletedSubSecApprovedData);
            result.AddRange(subsectionAdded);
            result.AddRange(subsectionDeleted);
            result.AddRange(subsectionModified);
            result.AddRange(subsectionRequiredArtificate);
            result.AddRange(subsectionArtificateAdded);
            return result.Distinct().OrderByDescending(x => x.actionDate).ToList();
        }
        public string ImportWordDocument(Stream stream, string FullPath)
        {
            string sfdtText = "";
            var Extension = System.IO.Path.GetExtension(FullPath);
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

        public string SaveDocumentInFolder(ProjectWorkplaceArtificatedocument projectWorkplaceArtificatedocument, CustomParameter param)
        {
            string filePath = string.Empty;
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var fileName = projectWorkplaceArtificatedocument.DocumentName.Contains('_') ? projectWorkplaceArtificatedocument.DocumentName.Substring(0, projectWorkplaceArtificatedocument.DocumentName.LastIndexOf('_')) : projectWorkplaceArtificatedocument.DocumentName;
            var docName = fileName + "_" + DateTime.Now.Ticks + ".docx";
            filePath = System.IO.Path.Combine(upload.DocumentPath, _jwtTokenAccesser.CompanyId.ToString(), projectWorkplaceArtificatedocument.DocPath, docName);

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

        public string ImportData(int Id)
        {
            var document = Find(Id);
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var FullPath = Path.Combine(upload.DocumentPath, _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, document.DocumentName);
            string path = FullPath;
            if (!System.IO.File.Exists(path))
                return null;
            Stream stream = System.IO.File.OpenRead(path);
            string json = ImportWordDocument(stream, path);
            stream.Close();
            return json;
        }

        public ProjectWorkplaceArtificatedocument WordToPdf(int Id)
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
                //add signature
                pdfDocument = CreateSignature(pdfDocument, Id);

                render.Dispose();
                wordDocument.Dispose();
                MemoryStream outputStream = new MemoryStream();
                pdfDocument.Save(outputStream);
                pdfDocument.Close();

                outputname = document.DocumentName.Substring(0, document.DocumentName.LastIndexOf('_')) + "_" + DateTime.Now.Ticks + ".pdf";
                var outputFile = Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, outputname);
                FileStream file = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
                outputStream.WriteTo(file);

                docStream.Close();
                docStream.Dispose();
                file.Close();
                file.Dispose();
                outputStream.Close();
                outputStream.Dispose();
            }
            else if (document?.DocumentName.Split('.').LastOrDefault() == "pdf")
            {
                var filepath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, document.DocumentName);
                FileStream docStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
                PdfDocument pdfDocument = new PdfDocument();
                pdfDocument.ImportPageRange(loadedDocument, 0, loadedDocument.Pages.Count - 1);
                pdfDocument = CreateSignature(pdfDocument, Id);

                MemoryStream outputStream = new MemoryStream();
                pdfDocument.Save(outputStream);
                pdfDocument.Close();

                outputname = document.DocumentName.Substring(0, document.DocumentName.LastIndexOf('_')) + "_" + DateTime.Now.Ticks + ".pdf";
                var outputFile = Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, outputname);
                FileStream file = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
                outputStream.WriteTo(file);

                docStream.Close();
                docStream.Dispose();
                file.Close();
                file.Dispose();
                outputStream.Close();
                outputStream.Dispose();
            }
            document.DocumentName = string.IsNullOrEmpty(outputname) ? document.DocumentName : outputname;
            document.Status = ArtifactDocStatusType.Final;
            //document.Version = document.ParentDocumentId != null ? (double.Parse(parent.Version) + 1).ToString("0.0") : (double.Parse(document.Version) + 1).ToString("0.0");
            //document.Version = "1.0";
            return document;
        }

        private PdfDocument CreateSignature(PdfDocument pdfDocument, int Id)
        {
            PdfSection section = pdfDocument.Sections.Add();
            PdfPage page = section.Pages.Add();
            section.PageSettings.Margins.All = Convert.ToInt32(0.5 * 100);


            RectangleF bounds = new RectangleF(page.GetClientSize().Width - 75, 0, 65f, 65f);
            PdfLayoutResult layoutresult = null;
            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            layoutFormat.Layout = PdfLayoutType.Paginate;
            layoutFormat.Break = PdfLayoutBreakType.FitElement;

            PdfStringFormat tocformat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Top);
            PdfTextElement indexheader = new PdfTextElement();
            layoutresult = new PdfLayoutResult(page, bounds);

            tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
            indexheader = new PdfTextElement("Review Document History", new PdfStandardFont(PdfFontFamily.TimesRoman, 14, PdfFontStyle.Bold), PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            layoutresult = indexheader.Draw(layoutresult.Page, new Syncfusion.Drawing.RectangleF(0, layoutresult.Bounds.Bottom + 10, layoutresult.Page.GetClientSize().Width, layoutresult.Page.GetClientSize().Height), layoutFormat);

            PdfGridRow header;
            PdfGridCellStyle headerStyle = new PdfGridCellStyle();
            headerStyle.Borders.All = new PdfPen(Color.Black);
            headerStyle.BackgroundBrush = new PdfSolidBrush(Color.Gray);
            headerStyle.TextBrush = PdfBrushes.Black;
            headerStyle.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 12f, PdfFontStyle.Bold);

            PdfGrid pdfGrid = new PdfGrid();
            pdfGrid.DataSource = ReviewDocumentHistory(Id);
            header = pdfGrid.Headers[0];
            header.ApplyStyle(headerStyle);
            layoutresult = pdfGrid.Draw(layoutresult.Page, new Syncfusion.Drawing.RectangleF(0, layoutresult.Bounds.Bottom + 10, layoutresult.Page.GetClientSize().Width, layoutresult.Page.GetClientSize().Height));


            tocformat = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
            indexheader = new PdfTextElement("Document Approval History", new PdfStandardFont(PdfFontFamily.TimesRoman, 14, PdfFontStyle.Bold), PdfBrushes.Black);
            indexheader.StringFormat = tocformat;
            layoutresult = indexheader.Draw(layoutresult.Page, new Syncfusion.Drawing.RectangleF(0, layoutresult.Bounds.Bottom + 10, layoutresult.Page.GetClientSize().Width, layoutresult.Page.GetClientSize().Height), layoutFormat);

            pdfGrid = new PdfGrid();
            pdfGrid.DataSource = DocumentApprovalhistory(Id);
            header = pdfGrid.Headers[0];
            header.ApplyStyle(headerStyle);
            layoutresult = pdfGrid.Draw(layoutresult.Page, new Syncfusion.Drawing.RectangleF(0, layoutresult.Bounds.Bottom + 10, layoutresult.Page.GetClientSize().Width, layoutresult.Page.GetClientSize().Height));

            return pdfDocument;
        }


        private DataTable ReviewDocumentHistory(int Id)
        {

            var History = _projectWorkplaceArtificateDocumentReviewRepository.GetArtificateDocumentHistory(Id);
            var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);

            var dataTable = new DataTable();
            dataTable.Columns.Add("Key");
            dataTable.Columns.Add("Document Name");
            dataTable.Columns.Add("Sent By");
            dataTable.Columns.Add("Sent For Review Date ");
            dataTable.Columns.Add("Comment");
            dataTable.Columns.Add("Sent To");
            dataTable.Columns.Add("Review Status");
            dataTable.Columns.Add("Review Date");
            dataTable.Columns.Add("Reason");
            dataTable.Columns.Add("ReasonDetails");
            foreach (var item in History)
            {
                var CreatedDate = item.CreatedDate != null ? Convert.ToDateTime(item.CreatedDate).UtcDateTime().ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : "";
                var SendBackDate = item.SendBackDate != null ? Convert.ToDateTime(item.SendBackDate).UtcDateTime().ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : "";
                dataTable.Rows.Add(new object[] { item.Id, item.DocumentName, item.CreatedByUser, CreatedDate, item.Message, item.UserName, item.IsSendBack, SendBackDate, item.Reason, item.ReasonOth });
            }
            return dataTable;
        }

        private DataTable DocumentApprovalhistory(int Id)
        {
            var History = _projectArtificateDocumentApproverRepository.GetArtificateDocumentApproverHistory(Id);
            var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            var dataTable = new DataTable();
            dataTable.Columns.Add("Key");
            dataTable.Columns.Add("Document Name");
            dataTable.Columns.Add("Sent By");
            dataTable.Columns.Add("sent Date");
            dataTable.Columns.Add("Comment");
            dataTable.Columns.Add("Approved");
            dataTable.Columns.Add("Approved By");
            dataTable.Columns.Add("Approved Date");
            dataTable.Columns.Add("Reason");
            dataTable.Columns.Add("ReasonDetails");
            foreach (var item in History)
            {
                var CreatedDate = item.CreatedDate != null ? Convert.ToDateTime(item.CreatedDate).UtcDateTime().ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : "";
                var ModifiedDate = item.ModifiedDate != null ? Convert.ToDateTime(item.ModifiedDate).UtcDateTime().ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : "";
                dataTable.Rows.Add(new object[] { item.Id, item.DocumentName, item.CreatedByUser, CreatedDate, item.Comment, item.IsApproved, item.UserName, ModifiedDate, item.Reason, item.ReasonOth });
            }
            return dataTable;
        }


        public IList<EtmfStudyReportDto> GetEtmfStudyReport(StudyReportSearchDto filters)
        {
            var ProjectCode = _context.Project.Where(x => x.Id == filters.projectId).FirstOrDefault().ProjectCode;
            //var workplace = _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == filters.projectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceDetail).ToList().FirstOrDefault();

            var workplacedetail = filters.folderId != null ? filters.countrySiteId != null ? _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == filters.projectId && x.WorkPlaceFolderId == filters.folderId
            && x.Id == filters.countrySiteId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceDetail).Select(y => y.Id).ToList() :
                _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == filters.projectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceDetail && x.WorkPlaceFolderId == filters.folderId).Select(y => y.Id).ToList() :
                _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == filters.projectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceDetail).Select(y => y.Id).ToList();


            var rightsWorkplace = _context.EtmfUserPermission.Where(x => workplacedetail.Contains(x.ProjectWorkplaceDetailId) && x.UserId == _jwtTokenAccesser.UserId
            && x.DeletedDate == null && x.IsView == true && x.IsDelete == true)
            .Select(y => y.ProjectWorkplaceDetailId).ToList();

            var workplacezone = filters.zoneId != null ? _context.EtmfProjectWorkPlace.Where(x => x.EtmfMasterLibraryId == filters.zoneId && x.ProjectId == filters.projectId && workplacedetail.Contains(x.EtmfProjectWorkPlaceId) && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceZone).Select(y => y.Id).ToList()
            : _context.EtmfProjectWorkPlace.Where(x => rightsWorkplace.Contains(x.EtmfProjectWorkPlaceId) && x.ProjectId == filters.projectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceZone).Select(y => y.Id).ToList();

            var workplacesection = filters.sectionId != null ? _context.EtmfProjectWorkPlace.Where(x => x.EtmfMasterLibraryId == filters.sectionId && x.ProjectId == filters.projectId && workplacezone.Contains(x.EtmfProjectWorkPlaceId) && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSection).Select(y => y.Id).ToList()
            : _context.EtmfProjectWorkPlace.Where(x => workplacezone.Contains(x.EtmfProjectWorkPlaceId) && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSection && x.ProjectId == filters.projectId).Select(y => y.Id).ToList();

            var workplaceartificate = filters.artificateId != null ? _context.EtmfProjectWorkPlace.Where(x => x.EtmfArtificateMasterLbraryId == filters.artificateId && x.ProjectId == filters.projectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceArtificate && workplacesection.Contains(x.EtmfProjectWorkPlaceId)).Select(y => y.Id).ToList()
            : _context.EtmfProjectWorkPlace.Where(x => workplacesection.Contains(x.EtmfProjectWorkPlaceId) && x.ProjectId == filters.projectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceArtificate).Select(y => y.Id).ToList();

            var workplaceartificatedocument = FindByInclude(x => workplaceartificate.Contains(x.ProjectWorkplaceArtificateId)).Select(y => y.Id).ToList();

            var subsection = filters.subSectionId != null ? _context.EtmfProjectWorkPlace.Where(x => x.Id == filters.subSectionId && x.ProjectId == filters.projectId && workplacesection.Contains(x.EtmfProjectWorkPlaceId) && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSection).Select(y => y.Id).ToList()
                : _context.EtmfProjectWorkPlace.Where(x => workplacesection.Contains(x.EtmfProjectWorkPlaceId) && x.ProjectId == filters.projectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSection && x.DeletedDate == null).Select(y => y.Id).ToList();

            var subsectionArtificate = filters.subSectionArtificateId != null ? _context.EtmfProjectWorkPlace.Where(x => x.Id == filters.subSectionArtificateId && x.ProjectId == filters.projectId && subsection.Contains(x.EtmfProjectWorkPlaceId) && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSectionArtifact).Select(y => y.Id).ToList()
                : _context.EtmfProjectWorkPlace.Where(x => subsection.Contains(x.EtmfProjectWorkPlaceId) && x.DeletedDate == null && x.ProjectId == filters.projectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSectionArtifact).Select(y => y.Id).ToList();

            var subsecDocument = _context.ProjectWorkplaceSubSecArtificatedocument.Where(x => subsectionArtificate.Contains(x.ProjectWorkplaceSubSectionArtifactId)).Select(y => y.Id).ToList();
            var result = new List<EtmfStudyReportDto>();

            if (filters.statusId == null || (filters.statusId != null && filters.statusId == 1))
            {
                var reviewer = _context.ProjectArtificateDocumentReview.Include(x => x.ProjectWorkplaceArtificatedDocument)
                    .ThenInclude(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.ProjectWorkPlace)
                    .ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace)
                    .Include(x => x.ProjectWorkplaceArtificatedDocument).ThenInclude(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.EtmfArtificateMasterLbrary)
                    .Include(x => x.ProjectWorkplaceArtificatedDocument).ThenInclude(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                    .Include(x => x.ProjectWorkplaceArtificatedDocument).ThenInclude(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                    .Where(x => workplaceartificatedocument.Contains(x.ProjectWorkplaceArtificatedDocumentId) && x.DeletedDate == null
                    && x.UserId != x.ProjectWorkplaceArtificatedDocument.CreatedBy && x.IsSendBack == false && x.IsReviewed == false
                    && (filters.userId == null || filters.userId == x.UserId)
                    ).ToList();

                var subsecReviewer = _context.ProjectSubSecArtificateDocumentReview.Include(x => x.ProjectWorkplaceSubSecArtificateDocument)
                    .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace)
                    .ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace)
                    .Include(x => x.ProjectWorkplaceSubSecArtificateDocument).ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                    .Include(x => x.ProjectWorkplaceSubSecArtificateDocument).ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                    .Where(x => subsecDocument.Contains(x.ProjectWorkplaceSubSecArtificateDocumentId) && x.DeletedDate == null
                    && x.UserId != x.ProjectWorkplaceSubSecArtificateDocument.CreatedBy && x.IsSendBack == false && x.IsReviewed == false
                    && (filters.userId == null || filters.userId == x.UserId)).ToList();

                var reviewerData = reviewer.Select(r => new EtmfStudyReportDto
                {
                    Id = r.Id,
                    projectCode = ProjectCode,
                    artificateDocumentId = r.ProjectWorkplaceArtificatedDocument.Id,
                    projectId = r.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectId,
                    folderName = ((WorkPlaceFolder)r.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription(),
                    countrysiteName = r.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                    zoneName = r.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                    sectionName = r.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                    artificateName = r.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName,
                    documentName = r.ProjectWorkplaceArtificatedDocument.DocumentName,
                    version = r.ProjectWorkplaceArtificatedDocument.Version,
                    status = (WorkplaceStatus.SentForReview).GetDescription(),
                    statusId = WorkplaceStatus.SentForReview,
                    CreatedBy = r.CreatedBy,
                    CreatedDate = r.CreatedDate,
                    userName = _userRepository.Find(r.UserId).UserName,
                    userId = r.UserId,
                    Level = 6,
                }).ToList();

                var subsecReviewData = subsecReviewer.Select(r => new EtmfStudyReportDto
                {
                    Id = r.Id,
                    projectCode = ProjectCode,
                    artificateDocumentId = r.ProjectWorkplaceSubSecArtificateDocument.Id,
                    projectId = r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectId,
                    folderName = ((WorkPlaceFolder)r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription(),
                    countrysiteName = r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                    zoneName = r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                    sectionName = r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                    artificateName = r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName,
                    subSectionName = r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.SubSectionName,
                    documentName = r.ProjectWorkplaceSubSecArtificateDocument.DocumentName,
                    version = r.ProjectWorkplaceSubSecArtificateDocument.Version,
                    status = (WorkplaceStatus.SentForReview).GetDescription(),
                    statusId = WorkplaceStatus.SentForReview,
                    CreatedBy = r.CreatedBy,
                    CreatedDate = r.CreatedDate,
                    userName = _userRepository.Find(r.UserId).UserName,
                    userId = r.UserId,
                    Level = 5.2
                });
                result.AddRange(reviewerData);
                result.AddRange(subsecReviewData);
            }

            if (filters.statusId == null || (filters.statusId != null && filters.statusId == 2))
            {
                var approve = _context.ProjectArtificateDocumentApprover.Include(x => x.ProjectWorkplaceArtificatedDocument)
                .ThenInclude(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.ProjectWorkPlace)
                .ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace)
                .Include(x => x.ProjectWorkplaceArtificatedDocument).ThenInclude(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.EtmfArtificateMasterLbrary)
                .Include(x => x.ProjectWorkplaceArtificatedDocument).ThenInclude(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                .Include(x => x.ProjectWorkplaceArtificatedDocument).ThenInclude(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                .Where(x => workplaceartificatedocument.Contains(x.ProjectWorkplaceArtificatedDocumentId) && x.DeletedDate == null
                && x.UserId != x.ProjectWorkplaceArtificatedDocument.CreatedBy && x.IsApproved == null
                && (filters.userId == null || filters.userId == x.UserId)).ToList();

                var subsecApprove = _context.ProjectSubSecArtificateDocumentApprover.Include(x => x.ProjectWorkplaceSubSecArtificateDocument)
                    .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace)
                    .ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace)
                    .Include(x => x.ProjectWorkplaceSubSecArtificateDocument).ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                    .Include(x => x.ProjectWorkplaceSubSecArtificateDocument).ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.ProjectWorkPlace).ThenInclude(x => x.EtmfMasterLibrary)
                    .Where(x => subsecDocument.Contains(x.ProjectWorkplaceSubSecArtificateDocumentId) && x.DeletedDate == null
                    && x.UserId != x.ProjectWorkplaceSubSecArtificateDocument.CreatedBy && x.IsApproved == null
                    && (filters.userId == null || filters.userId == x.UserId)).ToList();

                var approvedata = approve.Select(r => new EtmfStudyReportDto
                {
                    Id = r.Id,
                    projectCode = ProjectCode,
                    artificateDocumentId = r.ProjectWorkplaceArtificatedDocument.Id,
                    projectId = r.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectId,
                    folderName = ((WorkPlaceFolder)r.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription(),
                    countrysiteName = r.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                    zoneName = r.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                    sectionName = r.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                    artificateName = r.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName,
                    documentName = r.ProjectWorkplaceArtificatedDocument.DocumentName,
                    version = r.ProjectWorkplaceArtificatedDocument.Version,
                    status = (WorkplaceStatus.SentForApprove).GetDescription(),
                    statusId = WorkplaceStatus.SentForApprove,
                    CreatedBy = r.CreatedBy,
                    CreatedDate = r.CreatedDate,
                    userName = _userRepository.Find(r.UserId).UserName,
                    userId = r.UserId,
                    Level = 6,
                }).ToList();

                var subsecApproveData = subsecApprove.Select(r => new EtmfStudyReportDto
                {
                    Id = r.Id,
                    projectCode = ProjectCode,
                    artificateDocumentId = r.ProjectWorkplaceSubSecArtificateDocument.Id,
                    projectId = r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectId,
                    folderName = ((WorkPlaceFolder)r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription(),
                    countrysiteName = r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                    zoneName = r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                    sectionName = r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                    artificateName = r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName,
                    subSectionName = r.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.SubSectionName,
                    documentName = r.ProjectWorkplaceSubSecArtificateDocument.DocumentName,
                    version = r.ProjectWorkplaceSubSecArtificateDocument.Version,
                    status = (WorkplaceStatus.SentForApprove).GetDescription(),
                    statusId = WorkplaceStatus.SentForApprove,
                    CreatedBy = r.CreatedBy,
                    CreatedDate = r.CreatedDate,
                    userName = _userRepository.Find(r.UserId).UserName,
                    userId = r.UserId,
                    Level = 5.2
                });

                result.AddRange(approvedata);
                result.AddRange(subsecApproveData);
            }

            return result.OrderByDescending(x => x.CreatedDate).ToList();
        }

        public void UpdateDocumentComment(int documentId, bool? isComment)
        {
            var doc = All.FirstOrDefault(x => x.Id == documentId);
            doc.IsReplyAllComment = isComment;
            Update(doc);
            _context.Save();
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

        public List<ProjectArtificateDocumentExpiryHistoryDto> GetDocumentHistory(int documentId)
        {
            var docHistory = (from history in _context.ProjectArtificateDocumentHistory.Where(q => q.ProjectWorkplaceArtificateDocumentId == documentId && q.ExpiryDate.HasValue)
                              join auditReasonTemp in _context.AuditTrail.Where(x => x.TableName == "ProjectArtificateDocumentHistory" && x.ColumnName == "Document Expiry Date")
                                      on history.Id equals auditReasonTemp.RecordId into auditReasonDto
                              from auditReason in auditReasonDto.DefaultIfEmpty()
                              select new ProjectArtificateDocumentExpiryHistoryDto()
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

