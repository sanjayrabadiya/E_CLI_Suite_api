﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class SyncConfigurationMasterRepository : GenericRespository<SyncConfigurationMaster>, ISyncConfigurationMasterRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IProjectArtificateDocumentHistoryRepository _projectArtificateDocumentHistoryRepository;

        public SyncConfigurationMasterRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
           IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
           IProjectArtificateDocumentHistoryRepository projectArtificateDocumentHistoryRepository)
           : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
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
            if (!String.IsNullOrEmpty(validateMaster))
                return validateMaster;

            if (details.ProjectId > 0 && details.CountryId > 0 && details.SiteId > 0)
            {
                var validateDeails = ValidateFolderConfiguration(details.ProjectId, details.ReportCode, WorkPlaceFolder.Site);
                if (!String.IsNullOrEmpty(validateDeails))
                    return validateDeails;
                return "";
            }
            else if (details.ProjectId > 0 && details.SiteId > 0)
            {
                var validateDeails = ValidateFolderConfiguration(details.ProjectId, details.ReportCode, WorkPlaceFolder.Site);
                if (!String.IsNullOrEmpty(validateDeails))
                    return validateDeails;
                return "";
            }
            else if (details.ProjectId > 0 && details.CountryId > 0)
            {
                var validateDeails = ValidateFolderConfiguration(details.ProjectId, details.ReportCode, WorkPlaceFolder.Country);
                if (!String.IsNullOrEmpty(validateDeails))
                    return validateDeails;
                return "";
            }
            else
            {
                // save on trial
                var validateDeails = ValidateFolderConfiguration(details.ProjectId, details.ReportCode, WorkPlaceFolder.Trial);
                if (!String.IsNullOrEmpty(validateDeails))
                    return validateDeails;
                return "";
            }
            //return "";
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
                .Select(x => x.SyncConfigurationMasterDetails).FirstOrDefault();
            if (details.Any(x => x.WorkPlaceFolder == folder && x.ZoneMasterLibraryId > 0 && x.SectionMasterLibraryId > 0 && x.ArtificateMasterLbraryId > 0))
                return "";
            else
                return "please configuration Master details";
        }

        public string GetsyncConfigurationPath(SyncConfigurationParameterDto details, out int ProjectWorkplaceArtificateId)
        {
            if (details.ProjectId > 0 && details.CountryId > 0 && details.SiteId > 0)
            {
                string path = GetConfigurationPath(details, WorkPlaceFolder.Site, out ProjectWorkplaceArtificateId);
                return path;
            }
            else if (details.ProjectId > 0 && details.SiteId > 0)
            {
                string path = GetConfigurationPath(details, WorkPlaceFolder.Site, out ProjectWorkplaceArtificateId);
                return path;
            }
            else if (details.ProjectId > 0 && details.CountryId > 0)
            {
                string path = GetConfigurationPath(details, WorkPlaceFolder.Country, out ProjectWorkplaceArtificateId);
                return path;
            }
            else
            {
                string path = GetConfigurationPath(details, WorkPlaceFolder.Trial, out ProjectWorkplaceArtificateId);
                return path;
            }
        }

        private string GetConfigurationPath(SyncConfigurationParameterDto details, WorkPlaceFolder workplaceFolder, out int ProjectWorkplaceArtificateId)
        {
            int ReportScreenId = _context.ReportScreen.Where(x => x.ReportCode == details.ReportCode && x.DeletedDate == null).Select(x => x.Id).FirstOrDefault();
            string Version = _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == details.ProjectId && x.DeletedDate == null).Select(x => x.Version).FirstOrDefault();
            var syncConfigDetails = _context.SyncConfigurationMasterDetails.Where(x => x.SyncConfigurationMaster.ReportScreenId == ReportScreenId && x.SyncConfigurationMaster.Version == Version && x.WorkPlaceFolder == workplaceFolder && x.DeletedDate == null && x.SyncConfigurationMaster.DeletedDate == null).FirstOrDefault();

            var projectDetails = _context.EtmfProjectWorkPlace.Where(x =>
         x.ProjectWorkPlace.ProjectId == details.ProjectId
         && x.EtmfArtificateMasterLbraryId == syncConfigDetails.ArtificateMasterLbraryId && x.WorkPlaceFolderId == Convert.ToInt32(syncConfigDetails.WorkPlaceFolder)
         && x.EtmfMasterLibraryId == syncConfigDetails.SectionMasterLibraryId
         && x.EtmfMasterLibraryId == syncConfigDetails.ZoneMasterLibraryId
         && x.ProjectWorkPlace.DeletedDate == null
         // && syncConfigDetails.WorkPlaceFolder == WorkPlaceFolder.Site ? x.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemId == details.SiteId :(syncConfigDetails.WorkPlaceFolder == WorkPlaceFolder.Country ? x.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemId == details.CountryId : x.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemId == 0))
         && (workplaceFolder == WorkPlaceFolder.Site ? x.ItemId == details.SiteId : (syncConfigDetails.WorkPlaceFolder == WorkPlaceFolder.Country ? x.ItemId == details.CountryId : x.ItemId == 0)))
           .Select(x => new SyncConfigrationPathDetails
           {
               ProjectWorkplaceArtificateId = x.Id,
               ProjectCode = x.ProjectWorkPlace.Project.ProjectCode,
               WorkPlaceFolder = ((WorkPlaceFolder)x.WorkPlaceFolderId).GetDescription(),
               ItemName = x.ItemName,
               ZonName = x.EtmfMasterLibrary.ZonName,
               SectionName = x.EtmfMasterLibrary.SectionName,
               ArtificateName = x.EtmfArtificateMasterLbrary.ArtificateName,
           }).FirstOrDefault();
            if (projectDetails == null)
            {
                ProjectWorkplaceArtificateId = 0;
                return "";
            }
            ProjectWorkplaceArtificateId = projectDetails.ProjectWorkplaceArtificateId;
            string[] paths = { projectDetails.ProjectCode, FolderType.Etmf.GetDescription(), projectDetails.WorkPlaceFolder, projectDetails.ItemName != null ? projectDetails.ItemName : "", projectDetails.ZonName, projectDetails.SectionName, projectDetails.ArtificateName };
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
    }
}
