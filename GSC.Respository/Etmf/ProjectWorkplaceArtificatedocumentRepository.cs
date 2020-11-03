using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
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

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceArtificatedocumentRepository : GenericRespository<ProjectWorkplaceArtificatedocument, GscContext>, IProjectWorkplaceArtificatedocumentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IEtmfZoneMasterLibraryRepository _etmfZoneMasterLibraryRepository;
        private readonly IEtmfSectionMasterLibraryRepository _etmfSectionMasterLibraryRepository;
        public ProjectWorkplaceArtificatedocumentRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser, IUploadSettingRepository uploadSettingRepository,
           IUserRepository userRepository,
           IMapper mapper,
           IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
           IProjectRepository projectRepository,
           IEtmfZoneMasterLibraryRepository etmfZoneMasterLibraryRepository,
           IEtmfSectionMasterLibraryRepository etmfSectionMasterLibraryRepository
           )
           : base(uow, jwtTokenAccesser)
        {
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _uow = uow;
            _userRepository = userRepository;
            _etmfArtificateMasterLbraryRepository = etmfArtificateMasterLbraryRepository;
            _projectRepository = projectRepository;
            _etmfSectionMasterLibraryRepository = etmfSectionMasterLibraryRepository;
            _etmfZoneMasterLibraryRepository = etmfZoneMasterLibraryRepository;
        }

        public int deleteFile(int id)
        {
            string filename = string.Empty;
            var data = (from artifactdoc in Context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == id)
                        join artifact in Context.ProjectWorkplaceArtificate on artifactdoc.ProjectWorkplaceArtificateId equals artifact.Id
                        join etmfartifact in Context.EtmfArtificateMasterLbrary on artifact.EtmfArtificateMasterLbraryId equals etmfartifact.Id
                        join section in Context.ProjectWorkplaceSection on artifact.ProjectWorkplaceSectionId equals section.Id
                        join etmfsection in Context.EtmfSectionMasterLibrary on section.EtmfSectionMasterLibraryId equals etmfsection.Id
                        join workzone in Context.ProjectWorkPlaceZone on section.ProjectWorkPlaceZoneId equals workzone.Id
                        join etmfZone in Context.EtmfZoneMasterLibrary on workzone.EtmfZoneMasterLibraryId equals etmfZone.Id
                        join workdetail in Context.ProjectWorkplaceDetail on workzone.ProjectWorkplaceDetailId equals workdetail.Id
                        join work in Context.ProjectWorkplace on workdetail.ProjectWorkplaceId equals work.Id
                        join project in Context.Project on work.ProjectId equals project.Id

                        join countryleft in Context.Country on workdetail.ItemId equals countryleft.Id into countryl
                        from country in countryl.DefaultIfEmpty()
                        join projectsite in Context.Project on workdetail.ItemId equals projectsite.Id into siteleft
                        from site in siteleft.DefaultIfEmpty()
                        select new ProjectWorkplaceSubSecArtificatedocumentDto
                        {
                            Sectionname = etmfsection.SectionName,


                            Zonename = etmfZone.ZonName,
                            FolderType = workdetail.WorkPlaceFolderId,
                            Sitename = workdetail.WorkPlaceFolderId == 1 ? country.CountryName :
                                        workdetail.WorkPlaceFolderId == 2 ? site.ProjectCode + " - " + site.ProjectName : null,
                            Projectname = project.ProjectName + "-" + project.ProjectCode,
                            Artificatename = etmfartifact.ArtificateName,
                            DocumentName = artifactdoc.DocumentName,
                        }).FirstOrDefault();

            string filePath = string.Empty;
            string path = string.Empty;

            if (data.FolderType == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(data.Projectname, WorkPlaceFolder.Country.GetDescription(),
                  data.Sitename.Trim(), data.Zonename.Trim(), data.Sectionname.Trim(), data.Artificatename);
            else if (data.FolderType == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(data.Projectname, WorkPlaceFolder.Site.GetDescription(),
                data.Sitename.Trim(), data.Zonename.Trim(), data.Sectionname.Trim(), data.Artificatename);
            else if (data.FolderType == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(data.Projectname, WorkPlaceFolder.Trial.GetDescription(),
                   data.Zonename.Trim(), data.Sectionname.Trim(), data.Artificatename);
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path, data.DocumentName);
            System.IO.File.Delete(Path.Combine(filePath));

            return id;
        }

        public void UpdateApproveDocument(int documentId, bool IsAccepted)
        {
            var document = All.Where(x => x.Id == documentId).FirstOrDefault();
            document.IsAccepted = IsAccepted;
            Update(document);
            _uow.Save();
        }

        public List<CommonArtifactDocumentDto> GetDocumentList(int id)
        {
            List<CommonArtifactDocumentDto> dataList = new List<CommonArtifactDocumentDto>();
            var reviewdocument = Context.ProjectArtificateDocumentReview.Where(c => c.DeletedDate == null && c.UserId == _jwtTokenAccesser.UserId
                                  //  && c.RoleId == _jwtTokenAccesser.RoleId
                                  ).Select(x => x.ProjectWorkplaceArtificatedDocumentId).ToList();
            if (reviewdocument == null || reviewdocument.Count == 0) return dataList;

            var documentList = All.Include(x => x.ProjectWorkplaceArtificate).Where(x => x.ProjectWorkplaceArtificateId == id && x.DeletedDate == null
             && reviewdocument.Any(c => c == x.Id)
            ).ToList();

            foreach (var item in documentList)
            {
                var reviewerList = Context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id && x.UserId != item.CreatedBy).Select(z => z.UserId).Distinct().ToList();
                var users = new List<string>();
                reviewerList.ForEach(r =>
                {
                    var username = _userRepository.Find(r).UserName;
                    users.Add(username);
                });

                var Review = Context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id
                && x.UserId != item.CreatedBy && x.DeletedDate == null).ToList();

                var ApproveList = Context.ProjectArtificateDocumentApprover.Where(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id).OrderByDescending(x => x.Id).ToList()
                    .GroupBy(v => v.UserId).Select(y => new ProjectArtificateDocumentApprover
                    {
                        Id = y.FirstOrDefault().Id,
                        UserId = y.Key,
                        ProjectWorkplaceArtificatedDocumentId = y.FirstOrDefault().ProjectWorkplaceArtificatedDocumentId,
                        IsApproved = y.FirstOrDefault().IsApproved
                    }).ToList();

                var ApproverName = new List<string>();
                ApproveList.ForEach(r =>
                {
                    var username = _userRepository.Find(r.UserId).UserName;
                    ApproverName.Add(username);
                });

                CommonArtifactDocumentDto obj = new CommonArtifactDocumentDto();
                obj.Id = item.Id;
                obj.ProjectWorkplaceSubSectionArtifactId = item.ProjectWorkplaceArtificateId;
                obj.ProjectWorkplaceArtificateId = item.ProjectWorkplaceArtificateId;
                obj.Artificatename = _etmfArtificateMasterLbraryRepository.Find(item.ProjectWorkplaceArtificate.EtmfArtificateMasterLbraryId).ArtificateName;
                obj.DocumentName = item.DocumentName;
                obj.ExtendedName = item.DocumentName.Contains('_') ? item.DocumentName.Substring(0, item.DocumentName.LastIndexOf('_')) : item.DocumentName;
                obj.DocPath = System.IO.Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), FolderType.ProjectWorksplace.GetDescription(), item.DocPath, item.DocumentName);
                obj.CreatedByUser = _userRepository.Find((int)item.CreatedBy).UserName;
                obj.Reviewer = string.Join(", ", users);
                obj.CreatedDate = item.CreatedDate;
                obj.Version = item.Version;
                obj.StatusName = item.Status.GetDescription();
                obj.Status = (int)item.Status;
                obj.Level = 6;
                obj.SendBy = !(item.CreatedBy == _jwtTokenAccesser.UserId);
                obj.ReviewStatus = Review.Count() == 0 ? "" : Review.All(z => z.IsSendBack) ? "Send Back" : "Send";
                obj.IsReview = Review.Count() == 0 ? false : Review.All(z => z.IsSendBack) ? true : false;
                obj.IsSendBack = Context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id && x.UserId == _jwtTokenAccesser.UserId).OrderByDescending(x => x.Id).Select(z => z.IsSendBack).FirstOrDefault();
                obj.IsAccepted = item.IsAccepted;
                obj.ApprovedStatus = ApproveList.Count() == 0 ? "" : ApproveList.Any(x => x.IsApproved == false) ? "Reject" : ApproveList.All(x => x.IsApproved == true) ? "Approved"
                    : "Send For Approval";
                obj.Approver = string.Join(", ", ApproverName);
                obj.EtmfArtificateMasterLbraryId = item.ProjectWorkplaceArtificate.EtmfArtificateMasterLbraryId;
                obj.IsApproveDoc = ApproveList.Any(x => x.UserId == _jwtTokenAccesser.UserId && x.IsApproved == null) ? true : false;
                dataList.Add(obj);
            }
            return dataList;
        }

        public CommonArtifactDocumentDto GetDocument(int id)
        {
            var document = All.Include(x => x.ProjectWorkplaceArtificate).Where(x => x.Id == id && x.DeletedDate == null).FirstOrDefault();

            CommonArtifactDocumentDto obj = new CommonArtifactDocumentDto();
            obj.Id = document.Id;
            obj.ProjectWorkplaceSubSectionArtifactId = document.ProjectWorkplaceArtificateId;
            obj.ProjectWorkplaceArtificateId = document.ProjectWorkplaceArtificateId;
            obj.DocumentName = document.DocumentName;
            obj.ExtendedName = document.DocumentName.Contains('_') ? document.DocumentName.Substring(0, document.DocumentName.LastIndexOf('_')) : document.DocumentName;
            obj.DocPath = System.IO.Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), FolderType.ProjectWorksplace.GetDescription(), document.DocPath, document.DocumentName);
            obj.CreatedByUser = _userRepository.Find((int)document.CreatedBy).UserName;
            obj.CreatedDate = document.CreatedDate;
            obj.Version = document.Version;
            obj.StatusName = document.Status.GetDescription();
            obj.Status = (int)document.Status;
            obj.Level = 6;
            obj.SendBy = !(document.CreatedBy == _jwtTokenAccesser.UserId);
            obj.IsSendBack = Context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == document.Id && x.UserId == _jwtTokenAccesser.UserId).OrderByDescending(x => x.Id).Select(z => z.IsSendBack).FirstOrDefault();
            obj.IsAccepted = document.IsAccepted;
            obj.ApprovedStatus = document.IsAccepted == null ? "" : document.IsAccepted == true ? "Approved" : "Rejected";
            obj.EtmfArtificateMasterLbraryId = document.ProjectWorkplaceArtificate.EtmfArtificateMasterLbraryId;

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
            var Projectname = Project.ProjectName + "-" + Project.ProjectCode;

            string filePath = string.Empty;
            string path = string.Empty;

            if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(Projectname, WorkPlaceFolder.Country.GetDescription(),
                  projectWorkplaceArtificatedocumentDto.Countryname.Trim(), projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());
            else if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(Projectname, WorkPlaceFolder.Site.GetDescription(),
                 projectWorkplaceArtificatedocumentDto.Sitename.Trim(), projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());
            else if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(Projectname, WorkPlaceFolder.Trial.GetDescription(),
                   projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());

            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path);
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

        public List<DropDownDto> GetEtmfZoneDropdown()
        {
            return _etmfZoneMasterLibraryRepository.FindBy(x => x.DeletedDate == null)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.ZonName }).OrderBy(o => o.Value)
               .ToList();
        }

        public List<DropDownDto> GetEtmfSectionDropdown(int zoneId)
        {
            return _etmfSectionMasterLibraryRepository.FindBy(x => x.EtmfZoneMasterLibraryId == zoneId && x.DeletedDate == null)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.SectionName }).OrderBy(o => o.Value)
               .ToList();
        }

        public List<DropDownDto> GetEtmfArtificateDropdown(int sectionId)
        {
            return _etmfArtificateMasterLbraryRepository.FindBy(x => x.EtmfSectionMasterLibraryId == sectionId && x.DeletedDate == null)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.ArtificateName }).OrderBy(o => o.Value)
               .ToList();
        }

        public IList<EtmfAuditLogReportDto> GetEtmfAuditLogReport(EtmfAuditLogReportSearchDto filters)
        {
            var workplace = Context.ProjectWorkplace.Where(x => x.ProjectId == filters.projectId).ToList().FirstOrDefault();
            var workplacedetail = new List<int>();
            if (filters.folderId != null)
            {
                workplacedetail = Context.ProjectWorkplaceDetail.Where(x => x.ProjectWorkplaceId == workplace.Id && x.WorkPlaceFolderId == filters.folderId).Select(y => y.Id).ToList();
            }
            else
            {
                workplacedetail = Context.ProjectWorkplaceDetail.Where(x => x.ProjectWorkplaceId == workplace.Id).Select(y => y.Id).ToList();
            }
            var workplacezone = new List<int>();
            if (filters.zoneId != null)
            {
                workplacezone = Context.ProjectWorkPlaceZone.Where(x => x.EtmfZoneMasterLibraryId == filters.zoneId && workplacedetail.Contains(x.ProjectWorkplaceDetailId)).Select(y => y.Id).ToList();
            }
            else
            {
                workplacezone = Context.ProjectWorkPlaceZone.Where(x => workplacedetail.Contains(x.ProjectWorkplaceDetailId)).Select(y => y.Id).ToList();
            }
            var workplacesection = new List<int>();
            if (filters.sectionId != null)
            {
                workplacesection = Context.ProjectWorkplaceSection.Where(x => x.EtmfSectionMasterLibraryId == filters.sectionId && workplacezone.Contains(x.ProjectWorkPlaceZoneId)).Select(y => y.Id).ToList();
            }
            else
            {
                workplacesection = Context.ProjectWorkplaceSection.Where(x => workplacezone.Contains(x.ProjectWorkPlaceZoneId)).Select(y => y.Id).ToList();
            }
            var workplaceartificate = new List<int>();
            if (filters.artificateId != null)
            {
                workplaceartificate = Context.ProjectWorkplaceArtificate.Where(x => x.EtmfArtificateMasterLbraryId == filters.artificateId && workplacesection.Contains(x.ProjectWorkplaceSectionId)).Select(y => y.Id).ToList();
            }
            else
            {
                workplaceartificate = Context.ProjectWorkplaceArtificate.Where(x => workplacesection.Contains(x.ProjectWorkplaceSectionId)).Select(y => y.Id).ToList();
            }
            var workplaceartificatedocument = new List<int>();
            workplaceartificatedocument = FindByInclude(x => workplaceartificate.Contains(x.ProjectWorkplaceArtificateId)).Select(y => y.Id).ToList();

            var projectWorkplaceArtificatedocuments = All.Include(x => x.ProjectWorkplaceArtificate).
                ThenInclude(x => x.EtmfArtificateMasterLbrary).
                Include(x => x.ProjectWorkplaceArtificate).
                ThenInclude(x => x.ProjectWorkplaceSection).
                ThenInclude(x => x.ProjectWorkPlaceZone).
                ThenInclude(x => x.ProjectWorkplaceDetail).
                ThenInclude(x => x.ProjectWorkplace).
                ThenInclude(x => x.Project).
                Include(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.EtmfSectionMasterLibrary).
                Include(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone).ThenInclude(x => x.EtmfZoneMasterLibrary)
                .Where(x => workplaceartificate.Contains(x.ProjectWorkplaceArtificateId)).ToList();
            var projectWorkplaceArtificatedocumentreviews = Context.ProjectArtificateDocumentReview.Where(x => workplaceartificatedocument.Contains(x.ProjectWorkplaceArtificatedDocumentId)).ToList();
            var projectWorkplaceArtificatedocumentapprover = Context.ProjectArtificateDocumentApprover.Where(x => workplaceartificatedocument.Contains(x.ProjectWorkplaceArtificatedDocumentId)).ToList();

            var cretaedData = projectWorkplaceArtificatedocuments.Select(r => new EtmfAuditLogReportDto
            {
                projectCode = r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectCode,
                folderName = ((WorkPlaceFolder)r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription(),
                zoneName = r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName,
                sectionName = r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName,
                artificateName = r.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName,
                documentName = r.DocumentName,
                version = r.Version,
                status = ((ArtifactDocStatusType)r.Status).GetDescription(),
                action = "Created",
                userName = _userRepository.Find((int)r.CreatedBy).UserName,
                actionDate = r.CreatedDate
            }).ToList();

            var sendData = (from doc in projectWorkplaceArtificatedocuments
                            join review in projectWorkplaceArtificatedocumentreviews on doc.Id equals review.ProjectWorkplaceArtificatedDocumentId
                            select new EtmfAuditLogReportDto
                            {
                                projectCode = doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectCode,
                                folderName = ((WorkPlaceFolder)doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription(),
                                zoneName = doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName,
                                sectionName = doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName,
                                artificateName = doc.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName,
                                documentName = doc.DocumentName,
                                version = doc.Version,
                                status = ((ArtifactDocStatusType)doc.Status).GetDescription(),
                                action = "Send",
                                userName = _userRepository.Find((int)review.UserId).UserName,
                                actionDate = review.CreatedDate
                            }).ToList();

            var sendBackData = (from doc in projectWorkplaceArtificatedocuments
                                join review in projectWorkplaceArtificatedocumentreviews on doc.Id equals review.ProjectWorkplaceArtificatedDocumentId
                                where review.IsSendBack == true
                                select new EtmfAuditLogReportDto
                                {
                                    projectCode = doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectCode,
                                    folderName = ((WorkPlaceFolder)doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription(),
                                    zoneName = doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName,
                                    sectionName = doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName,
                                    artificateName = doc.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName,
                                    documentName = doc.DocumentName,
                                    version = doc.Version,
                                    status = ((ArtifactDocStatusType)doc.Status).GetDescription(),
                                    action = "Send Back",
                                    userName = _userRepository.Find((int)review.UserId).UserName,
                                    actionDate = review.SendBackDate
                                }).ToList();

            var sendforApproveData = (from doc in projectWorkplaceArtificatedocuments
                                      join approve in projectWorkplaceArtificatedocumentapprover on doc.Id equals approve.ProjectWorkplaceArtificatedDocumentId
                                      select new EtmfAuditLogReportDto
                                      {
                                          projectCode = doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectCode,
                                          folderName = ((WorkPlaceFolder)doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription(),
                                          zoneName = doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName,
                                          sectionName = doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName,
                                          artificateName = doc.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName,
                                          documentName = doc.DocumentName,
                                          version = doc.Version,
                                          status = ((ArtifactDocStatusType)doc.Status).GetDescription(),
                                          action = "Send for Approve",
                                          userName = _userRepository.Find((int)approve.UserId).UserName,
                                          actionDate = approve.CreatedDate
                                      }).ToList();

            var ApprovedData = (from doc in projectWorkplaceArtificatedocuments
                                join approve in projectWorkplaceArtificatedocumentapprover on doc.Id equals approve.ProjectWorkplaceArtificatedDocumentId
                                where approve.IsApproved != null
                                select new EtmfAuditLogReportDto
                                {
                                    projectCode = doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectCode,
                                    folderName = ((WorkPlaceFolder)doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription(),
                                    zoneName = doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName,
                                    sectionName = doc.ProjectWorkplaceArtificate.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName,
                                    artificateName = doc.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName,
                                    documentName = doc.DocumentName,
                                    version = doc.Version,
                                    status = ((ArtifactDocStatusType)doc.Status).GetDescription(),
                                    action = approve.IsApproved == true ? "Approved" : "Rejected",
                                    userName = _userRepository.Find((int)approve.UserId).UserName,
                                    actionDate = approve.ModifiedDate
                                }).ToList();


            var deletedData = projectWorkplaceArtificatedocuments.Where(x => x.DeletedDate != null).Select(r => new EtmfAuditLogReportDto
            {
                projectCode = r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectCode,
                folderName = ((WorkPlaceFolder)r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription(),
                zoneName = r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName,
                sectionName = r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName,
                artificateName = r.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName,
                documentName = r.DocumentName,
                version = r.Version,
                status = ((ArtifactDocStatusType)r.Status).GetDescription(),
                action = "Delete",
                userName = _userRepository.Find((int)r.DeletedBy).UserName,
                actionDate = r.DeletedDate
            }).ToList();

            var supersededata = projectWorkplaceArtificatedocuments.Where(x => x.ParentDocumentId != null).Select(r => new EtmfAuditLogReportDto
            {
                projectCode = r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectCode,
                folderName = ((WorkPlaceFolder)r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription(),
                zoneName = r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName,
                sectionName = r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName,
                artificateName = r.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName,
                documentName = r.DocumentName,
                version = r.Version,
                status = ((ArtifactDocStatusType)r.Status).GetDescription(),
                action = "Supersede",
                userName = _userRepository.Find((int)r.CreatedBy).UserName,
                actionDate = r.ModifiedDate
            }).ToList();

            var finaldata = projectWorkplaceArtificatedocuments.Where(x => x.Status == ArtifactDocStatusType.Final).Select(r => new EtmfAuditLogReportDto
            {
                projectCode = r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectCode,
                folderName = ((WorkPlaceFolder)r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription(),
                zoneName = r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName,
                sectionName = r.ProjectWorkplaceArtificate.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName,
                artificateName = r.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName,
                documentName = r.DocumentName,
                version = r.Version,
                status = ((ArtifactDocStatusType)r.Status).GetDescription(),
                action = "Final",
                userName = _userRepository.Find((int)r.CreatedBy).UserName,
                actionDate = r.ModifiedDate
            }).ToList();

            return cretaedData.Union(sendData).Union(sendBackData).Union(sendforApproveData).Union(ApprovedData).Union(deletedData).Union(supersededata).Union(finaldata).OrderBy(x => x.actionDate).ToList();
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
            var upload = Context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var fileName = projectWorkplaceArtificatedocument.DocumentName.Contains('_') ? projectWorkplaceArtificatedocument.DocumentName.Substring(0, projectWorkplaceArtificatedocument.DocumentName.LastIndexOf('_')) : projectWorkplaceArtificatedocument.DocumentName;
            var docName = fileName + "_" + DateTime.Now.Ticks + ".docx";
            filePath = System.IO.Path.Combine(upload.DocumentPath, FolderType.ProjectWorksplace.GetDescription(), projectWorkplaceArtificatedocument.DocPath, docName);

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


    }
}
