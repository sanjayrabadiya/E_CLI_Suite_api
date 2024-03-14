using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class SyncConfigurationMasterRepository : GenericRespository<SyncConfigurationMaster>, ISyncConfigurationMasterRepository
    {
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IProjectArtificateDocumentHistoryRepository _projectArtificateDocumentHistoryRepository;

        public SyncConfigurationMasterRepository(IGSCContext context, IMapper mapper, IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
           IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
           IProjectArtificateDocumentHistoryRepository projectArtificateDocumentHistoryRepository)
           : base(context)
        {
            _context = context;
            _mapper = mapper;
            _projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _projectArtificateDocumentHistoryRepository = projectArtificateDocumentHistoryRepository;
        }

        public List<SyncConfigurationMasterGridDto> GetSyncConfigurationMastersList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).
                    ProjectTo<SyncConfigurationMasterGridDto>(_mapper.ConfigurationProvider).ToList();
        }

        public string Duplicate(SyncConfigurationMaster objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.DeletedDate == null && x.ReportScreenId == objSave.ReportScreenId && x.Version == objSave.Version))
            {
                return "Duplicate Report";
            }
            return "";
        }
        public List<SyncConfigurationAuditDto> GetAudit()
        {
            var result = _context.SyncConfigurationMasterDetailsAudit.Select(x => new SyncConfigurationAuditDto
            {
                Key = x.SyncConfigurationMasterDetails.SyncConfigurationMaster.Id,
                ReportName = x.ReportScreen.ReportName,
                Version = x.Version,
                ZonName = x.ZoneMasterLibraryId == null ? "" : x.EtmfZoneMasterLibrary.ZonName,
                SectionName = x.SectionMasterLibraryId == null ? "" : x.EtmfSectionMasterLibrary.SectionName,
                ArtificateName = x.ArtificateMasterLbraryId == null ? "" : x.EtmfArtificateMasterLbrary.ArtificateName,
                ReasonName = x.ReasonId == null ? "" : x.AuditReason.ReasonName,
                Notes = x.Note,
                IpAddress = x.IpAddress,
                TimeZone = x.TimeZone,
                Activity = x.Activity,
                ActivityBy = x.CreatedByUser.UserName,
                ActivityDate = x.CreatedDate
            }).ToList();
            return result;
        }

        public string ValidateMasterConfiguration(SyncConfigurationParameterDto details)
        {
            var validateMaster = ValidateSyncConfigurationMasterDetails(details.ProjectId, details.ReportCode);
            if (!string.IsNullOrEmpty(validateMaster))
                return validateMaster;

            WorkPlaceFolder folder;

            if (details.ProjectId > 0)
            {
                if (details.CountryId > 0 && details.SiteId > 0)
                    folder = WorkPlaceFolder.Site;
                else if (details.SiteId > 0)
                    folder = WorkPlaceFolder.Site;
                else if (details.CountryId > 0)
                    folder = WorkPlaceFolder.Country;
                else
                    folder = WorkPlaceFolder.Trial;
            }
            else
            {
                folder = WorkPlaceFolder.Trial;
            }

            var validateDetails = ValidateFolderConfiguration(details.ProjectId, details.ReportCode, folder);
            return string.IsNullOrEmpty(validateDetails) ? "" : validateDetails;
        }


        public string ValidateSyncConfigurationMasterDetails(int ProjectId, string ReportCode)
        {
            int ReportScreenId = _context.ReportScreen.Where(x => x.ReportCode == ReportCode && x.DeletedDate == null).Select(x => x.Id).FirstOrDefault();
            string Version = _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null).Select(x => x.Version).FirstOrDefault();
            if (All.Any(x => x.ReportScreenId == ReportScreenId && x.DeletedDate == null && x.Version == Version))
                return "";
            else
                return "Version is Not Configuration in Sync Master";
        }

        public string ValidateFolderConfiguration(int ProjectId, string ReportCode, WorkPlaceFolder folder)
        {
            int ReportScreenId = _context.ReportScreen.Where(x => x.ReportCode == ReportCode && x.DeletedDate == null).Select(x => x.Id).FirstOrDefault();
            string Version = _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null).Select(x => x.Version).FirstOrDefault();
            List<SyncConfigurationMasterDetails> details = All.Where(x => x.ReportScreenId == ReportScreenId && x.DeletedDate == null && x.Version == Version)
                .Select(x => x.SyncConfigurationMasterDetails).First();
            if (details.Exists(x => x.WorkPlaceFolder == folder && x.ZoneMasterLibraryId > 0 && x.SectionMasterLibraryId > 0 && x.ArtificateMasterLbraryId > 0))
                return "";
            else
                return "please configuration Master details";
        }

        public string GetsyncConfigurationPath(SyncConfigurationParameterDto details, out int ProjectWorkplaceArtificateId)
        {
            WorkPlaceFolder folder;

            if (details.ProjectId > 0)
            {
                if (details.CountryId > 0 && details.SiteId > 0)
                    folder = WorkPlaceFolder.Site;
                else if (details.SiteId > 0)
                    folder = WorkPlaceFolder.Site;
                else if (details.CountryId > 0)
                    folder = WorkPlaceFolder.Country;
                else
                    folder = WorkPlaceFolder.Trial;
            }
            else
            {
                folder = WorkPlaceFolder.Trial;
            }

            string path = GetConfigurationPath(details, folder, out ProjectWorkplaceArtificateId);
            return path;
        }

        private string GetConfigurationPath(SyncConfigurationParameterDto details, WorkPlaceFolder workplaceFolder, out int ProjectWorkplaceArtificateId)
        {
            int ReportScreenId = _context.ReportScreen.Where(x => x.ReportCode == details.ReportCode && x.DeletedDate == null).Select(x => x.Id).FirstOrDefault();
            string Version = _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == details.ProjectId && x.DeletedDate == null).Select(x => x.Version).FirstOrDefault();
            var syncConfigDetails = _context.SyncConfigurationMasterDetails.Where(x => x.SyncConfigurationMaster.ReportScreenId == ReportScreenId && x.SyncConfigurationMaster.Version == Version && x.WorkPlaceFolder == workplaceFolder && x.DeletedDate == null && x.SyncConfigurationMaster.DeletedDate == null).FirstOrDefault();


            var projectDetails = _context.EtmfProjectWorkPlace.Where(x =>
           x.ProjectId == details.ProjectId
           && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceArtificate
           && x.EtmfArtificateMasterLbraryId == syncConfigDetails.ArtificateMasterLbraryId
           && x.ProjectWorkPlace.DeletedDate == null).Where(q => q.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId == (int)syncConfigDetails.WorkPlaceFolder
              && q.ProjectWorkPlace.EtmfMasterLibraryId == syncConfigDetails.SectionMasterLibraryId
              && q.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibraryId == syncConfigDetails.ZoneMasterLibraryId
              && (workplaceFolder == WorkPlaceFolder.Site ? q.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemId == details.SiteId : (syncConfigDetails.WorkPlaceFolder == WorkPlaceFolder.Country ? q.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemId == details.CountryId : q.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemId == 0)))
             .Select(x => new SyncConfigrationPathDetails
             {
                 ProjectWorkplaceArtificateId = x.Id,
                 ProjectCode = x.ProjectWorkPlace.Project.ProjectCode,
                 WorkPlaceFolder = ((WorkPlaceFolder)x.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription(),
                 ItemName = x.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                 ZonName = x.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                 SectionName = x.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                 ArtificateName = x.EtmfArtificateMasterLbrary.ArtificateName,
             }).FirstOrDefault();
            if (projectDetails == null)
            {
                ProjectWorkplaceArtificateId = 0;
                return "";
            }
            var strProjectName = projectDetails.ProjectCode.Replace("/", "");
            ProjectWorkplaceArtificateId = projectDetails.ProjectWorkplaceArtificateId;
            string[] paths = { strProjectName, FolderType.Etmf.GetDescription(), projectDetails.WorkPlaceFolder, projectDetails.ItemName != null ? projectDetails.ItemName : "", projectDetails.ZonName, projectDetails.SectionName, projectDetails.ArtificateName };
            var fullPath = Path.Combine(paths);
            return fullPath;
        }

        public string SaveArtifactDocument(string DocumentName, SyncConfigurationParameterDto details)
        {
            int ProjectWorkplaceArtificateId;
            string DocumentPath = GetsyncConfigurationPath(details, out ProjectWorkplaceArtificateId);
            ProjectWorkplaceArtificatedocument projectWorkplaceArtificatedocument = new ProjectWorkplaceArtificatedocument();
            projectWorkplaceArtificatedocument.DocumentName = DocumentName;
            projectWorkplaceArtificatedocument.DocPath = DocumentPath;
            projectWorkplaceArtificatedocument.ProjectWorkplaceArtificateId = ProjectWorkplaceArtificateId;
            projectWorkplaceArtificatedocument.Status = ArtifactDocStatusType.Draft;
            projectWorkplaceArtificatedocument.Version = "1.0";
            _projectWorkplaceArtificatedocumentRepository.Add(projectWorkplaceArtificatedocument);
            if (_context.Save() <= 0) throw new Exception("Creating Document failed on save.");
            _projectWorkplaceArtificateDocumentReviewRepository.SaveByDocumentIdInReview(projectWorkplaceArtificatedocument.Id);
            _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, null);
            return DocumentPath;
        }

        public List<DropDownDto> GetReportScreen()
        {
            var reportList = _context.SyncConfigurationMaster.Where(q => q.DeletedDate == null)
                .Include(x => x.ReportScreen).Select(s => new DropDownDto()
                {
                    Id = s.ReportScreen.Id,
                    Code = s.ReportScreen.ReportCode,
                    Value = s.ReportScreen.ReportName,
                }).Distinct().ToList();

            DropDownDto reportScreenDto = new DropDownDto()
            {
                Id = 0,
                Code = "crf",
                Value = "CRF"
            };

            reportList.Insert(0, reportScreenDto);

            return reportList;
        }
    }
}
