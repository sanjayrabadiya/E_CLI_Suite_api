﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.ProjectRight;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using GSC.Shared.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ETMFWorkplaceRepository : GenericRespository<ProjectWorkplace>, IETMFWorkplaceRepository
    {
        ProjectWorkplace projectWorkplace = new ProjectWorkplace();
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        List<ProjectWorkplaceDetail> ProjectWorkplaceDetailList = new List<ProjectWorkplaceDetail>();
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;
        public string Workplace = "AAAAAA";
        
        public ETMFWorkplaceRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
           IMapper mapper, IUploadSettingRepository uploadSettingRepository,
           IProjectRightRepository projectRightRepository)
           : base(context)
        {
            _context = context;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
        }

        public string Duplicate(int id)
        {
            if (All.Any(x => x.ProjectId == id && x.DeletedDate == null))
                return "Duplicate Workplace name";
            return "";
        }

        public List<TreeValue> GetTreeview(int id, EtmfChartType? chartType)
        {
            //chartType
            //1: core
            //2: Recommended
            //3: Missing
            //4: Pending Review
            //5: Pending Approve
            //6: Final
            //7: Incomplete
            //8: Not Required

            // Do Dynamic after complete task
            // Level 1 : Project
            // Level 2 : Country/Site/Trial Static Folder Name
            // Level 3 : Country/Site/Trial Name
            // Level 4 : Zone Name
            // Level 5 : Section Name      Level 5.1 : Sub Section Name    Level 5.2 : Sub Section Artifact Name
            // Level 6 : Artifact Name     

            var projectWorkplaces = _context.ProjectWorkplace.Where(t => t.DeletedBy == null && t.ProjectId == id)
                            .Include(x => x.ProjectWorkplaceDetail)
                            .ThenInclude(x => x.ProjectWorkPlaceZone)
                            .ThenInclude(x => x.ProjectWorkplaceSection)
                            .ThenInclude(x => x.ProjectWorkplaceArtificate)
                            .AsNoTracking().ToList();

            List<TreeValue> pvList = new List<TreeValue>();
            TreeValue pvListObj = new TreeValue();


            pvListObj.Id = projectWorkplaces.FirstOrDefault().Id;
            pvListObj.RandomId = Workplace;
            pvListObj.Text = _context.Project.Where(x => x.Id == projectWorkplaces.FirstOrDefault().ProjectId).FirstOrDefault().ProjectCode;
            pvListObj.Level = 1;
            pvListObj.Item = new List<TreeValue>();
            pvListObj.ParentMasterId = projectWorkplaces.FirstOrDefault().ProjectId;
            pvListObj.Icon = "las la-folder-open text-blue eicon";

            TreeValue TrialFol = new TreeValue();
            TrialFol.Id = 11111111;
            TrialFol.RandomId = "TTTTTT";
            TrialFol.Text = "Trial";
            TrialFol.Level = 2;
            TrialFol.Icon = "las la-folder-open text-blue eicon";
            TrialFol.WorkPlaceFolderId = WorkPlaceFolder.Trial;
            TrialFol.ExpandData = string.Join(",", pvListObj.RandomId, TrialFol.RandomId);

            TreeValue CountryFol = new TreeValue();
            CountryFol.Id = 11111112;
            CountryFol.RandomId = "CCCCCC";
            CountryFol.Text = "Country";
            CountryFol.Level = 2;
            CountryFol.Icon = "las la-folder-open text-blue eicon";
            CountryFol.WorkPlaceFolderId = WorkPlaceFolder.Country;
            CountryFol.ExpandData = string.Join(",", pvListObj.RandomId, CountryFol.RandomId);

            TreeValue SiteFol = new TreeValue();
            SiteFol.Id = 11111113;
            SiteFol.RandomId = "SSSSSS";
            SiteFol.Text = "Site";
            SiteFol.Level = 2;
            SiteFol.Icon = "las la-folder-open text-blue eicon";
            SiteFol.WorkPlaceFolderId = WorkPlaceFolder.Site;
            SiteFol.ExpandData = string.Join(",", pvListObj.RandomId, SiteFol.RandomId );

            CountryFol.Item = new List<TreeValue>();
            SiteFol.Item = new List<TreeValue>();
            foreach (var b in projectWorkplaces)
            {
                #region Get Country
                foreach (var c in b.ProjectWorkplaceDetail.Where(x => x.WorkPlaceFolderId == (int)WorkPlaceFolder.Country && x.DeletedBy == null))
                {
                    var rights = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId == c.Id && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();

                    if (rights != null ? rights.IsView : false)
                    {
                        TreeValue pvListdetaiObj = GetWorksplaceDetails(rights, c, CountryFol.RandomId);

                        List<TreeValue> pvListZoneList = new List<TreeValue>();
                        foreach (var d in c.ProjectWorkPlaceZone.Where(x => x.DeletedBy == null))
                        {
                            d.EtmfZoneMasterLibrary = _context.EtmfZoneMasterLibrary.Find(d.EtmfZoneMasterLibraryId);
                            // Get zone
                            TreeValue pvListZoneObj = GetZone(rights, c, d, b, pvListdetaiObj.ExpandData);

                            List<TreeValue> pvListSectionList = new List<TreeValue>();
                            foreach (var e in d.ProjectWorkplaceSection.Where(x => x.DeletedBy == null))
                            {
                                e.EtmfSectionMasterLibrary = _context.EtmfSectionMasterLibrary.Find(e.EtmfSectionMasterLibraryId);
                                // Get section
                                TreeValue pvListSectionObj = GetSection(e, WorkPlaceFolder.Country, rights, c, d, b, pvListZoneObj.ExpandData);
                                // Get artificate
                                List<TreeValue> pvListArtificateList = GetArtificate(e.ProjectWorkplaceArtificate, WorkPlaceFolder.Country, chartType, rights, c, d, e, b, pvListSectionObj.ExpandData);

                                pvListSectionList.Add(pvListSectionObj);
                                pvListSectionObj.Item = pvListArtificateList.OrderBy(x => x.Number).ToList();
                                pvListSectionObj = GetIcon(pvListSectionObj);
                            }

                            pvListZoneList.Add(pvListZoneObj);
                            pvListZoneObj.Item = pvListSectionList.OrderBy(x => x.Number).ToList();
                            pvListZoneObj = GetIcon(pvListZoneObj);
                            pvListdetaiObj.Item.Add(pvListZoneObj);
                        }

                        pvListdetaiObj.Item = pvListdetaiObj.Item.OrderBy(x => x.Number).ToList();
                        pvListdetaiObj = GetIcon(pvListdetaiObj);
                        CountryFol.Item.Add(pvListdetaiObj);
                        CountryFol = GetIcon(CountryFol);
                    }
                }

                #endregion

                #region Get Site
                foreach (var c in b.ProjectWorkplaceDetail.Where(x => x.WorkPlaceFolderId == (int)WorkPlaceFolder.Site && x.DeletedBy == null))
                {
                    var rights = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId == c.Id && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (rights != null ? rights.IsView : false)
                    {
                        TreeValue pvListdetaiObj = GetWorksplaceDetails(rights, c, SiteFol.RandomId);

                        List<TreeValue> pvListZoneList = new List<TreeValue>();
                        foreach (var d in c.ProjectWorkPlaceZone.Where(x => x.DeletedBy == null))
                        {
                            d.EtmfZoneMasterLibrary = _context.EtmfZoneMasterLibrary.Find(d.EtmfZoneMasterLibraryId);
                            // Get zone
                            TreeValue pvListZoneObj = GetZone(rights, c, d, b, pvListdetaiObj.ExpandData);

                            List<TreeValue> pvListSectionList = new List<TreeValue>();
                            foreach (var e in d.ProjectWorkplaceSection.Where(x => x.DeletedBy == null))
                            {
                                e.EtmfSectionMasterLibrary = _context.EtmfSectionMasterLibrary.Find(e.EtmfSectionMasterLibraryId);
                                // Get section
                                TreeValue pvListSectionObj = GetSection(e, WorkPlaceFolder.Site, rights, c, d, b, pvListZoneObj.ExpandData);
                                // Get artificate
                                List<TreeValue> pvListArtificateList = GetArtificate(e.ProjectWorkplaceArtificate, WorkPlaceFolder.Site, chartType, rights, c, d, e, b, pvListSectionObj.ExpandData);

                                pvListSectionList.Add(pvListSectionObj);
                                pvListSectionObj.Item = pvListArtificateList.OrderBy(x => x.Number).ToList();
                                pvListSectionObj = GetIcon(pvListSectionObj);
                            }

                            pvListZoneList.Add(pvListZoneObj);
                            pvListZoneObj.Item = pvListSectionList.OrderBy(x => x.Number).ToList();
                            pvListZoneObj = GetIcon(pvListZoneObj);
                            pvListdetaiObj.Item.Add(pvListZoneObj);
                        }
                        pvListdetaiObj.Item = pvListdetaiObj.Item.OrderBy(x => x.Number).ToList();
                        pvListdetaiObj = GetIcon(pvListdetaiObj);
                        SiteFol.Item.Add(pvListdetaiObj);
                        SiteFol = GetIcon(SiteFol);
                    }
                }

                #endregion

                #region Get Trial
                foreach (var c in b.ProjectWorkplaceDetail.Where(x => x.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial && x.DeletedBy == null))
                {
                    var rights = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId == c.Id && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (rights != null ? rights.IsView : false)
                    {
                        TreeValue pvListdetaiObj = GetWorksplaceDetails(rights, c, TrialFol.RandomId);
                        List<TreeValue> pvListZoneList = new List<TreeValue>();

                        foreach (var d in c.ProjectWorkPlaceZone.Where(x => x.DeletedBy == null))
                        {
                            d.EtmfZoneMasterLibrary = _context.EtmfZoneMasterLibrary.Find(d.EtmfZoneMasterLibraryId);
                            // Get zone
                            TreeValue pvListZoneObj = GetZone(rights, c, d, b, pvListdetaiObj.ExpandData);

                            List<TreeValue> pvListSectionList = new List<TreeValue>();
                            foreach (var e in d.ProjectWorkplaceSection.Where(x => x.DeletedBy == null))
                            {
                                e.EtmfSectionMasterLibrary = _context.EtmfSectionMasterLibrary.Find(e.EtmfSectionMasterLibraryId);
                                // Get section
                                TreeValue pvListSectionObj = GetSection(e, WorkPlaceFolder.Trial, rights, c, d, b, pvListZoneObj.ExpandData);
                                // Get Artificate
                                List<TreeValue> pvListArtificateList = GetArtificate(e.ProjectWorkplaceArtificate, WorkPlaceFolder.Trial, chartType, rights, c, d, e, b, pvListSectionObj.ExpandData);

                                pvListSectionList.Add(pvListSectionObj);
                                pvListSectionObj.Item = pvListArtificateList.OrderBy(x => x.Number).ToList();
                                pvListSectionObj = GetIcon(pvListSectionObj);
                            }
                            pvListZoneList.Add(pvListZoneObj);
                            pvListZoneObj.Item = pvListSectionList.OrderBy(x => x.Number).ToList();
                            pvListZoneObj = GetIcon(pvListZoneObj);
                        }

                        TrialFol.Item = pvListZoneList.OrderBy(x => x.Number).ToList();
                        TrialFol = GetIcon(TrialFol);
                    }
                }

                #endregion
                pvListObj.Item.Add(TrialFol);
                pvListObj.Item.Add(CountryFol);
                pvListObj.Item.Add(SiteFol);
            }
            pvListObj = GetIcon(pvListObj);
            pvList.Add(pvListObj);
            return pvList;
        }

        public TreeValue GetIcon(TreeValue data)
        {
            data.IconType = data.Item.Any(x => x.IconType == EtmfChartType.Missing) ? EtmfChartType.Missing :
                            data.Item.Any(x => x.IconType == EtmfChartType.Incomplete) ? EtmfChartType.Incomplete :
                            data.Item.Any(x => x.IconType == EtmfChartType.PendingReview) ? EtmfChartType.PendingReview :
                            data.Item.Any(x => x.IconType == EtmfChartType.PendingApprove) ? EtmfChartType.PendingApprove :
                            data.Item.Any(x => x.IconType == EtmfChartType.Final) ? EtmfChartType.Final :
                            data.Item.Any(x => x.IconType == EtmfChartType.NotRequired) ? EtmfChartType.NotRequired
                            : EtmfChartType.Nothing;
            data.Icon = data.IconType == EtmfChartType.Missing ? "las la-folder-open text-missing eicon" :
                        data.IconType == EtmfChartType.Incomplete ? "las la-folder-open text-incomeplete eicon" :
                        data.IconType == EtmfChartType.PendingReview ? "las la-folder-open text-pendingreview eicon" :
                        data.IconType == EtmfChartType.PendingApprove ? "las la-folder-open text-pendingapprove eicon" :
                        data.IconType == EtmfChartType.Final ? "las la-folder-open text-final eicon" :
                        data.IconType == EtmfChartType.NotRequired ? "las la-folder-open text-notreq eicon"
                        : "las la-folder-open text-blue eicon";
            return data;
        }
        public TreeValue GetWorksplaceDetails(EtmfUserPermission rights, ProjectWorkplaceDetail c, string Data)
        {
            TreeValue pvListdetaiObj = new TreeValue();
            pvListdetaiObj.Id = Convert.ToInt32(RandomPassword.CreateRandomNumericNumber(6));
            pvListdetaiObj.Item = new List<TreeValue>();
            pvListdetaiObj.RandomId = "WD" + ((WorkPlaceFolder)c.WorkPlaceFolderId).GetDescription().Substring(0, 2) + c.Id;
            pvListdetaiObj.Text = c.ItemName;
            pvListdetaiObj.Level = 3;
            pvListdetaiObj.Icon = "las la-folder-open text-blue eicon";
            pvListdetaiObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
            pvListdetaiObj.IsAdd = rights != null && rights.IsAdd;
            pvListdetaiObj.IsEdit = rights != null && rights.IsEdit;
            pvListdetaiObj.IsDelete = rights != null && rights.IsDelete;
            pvListdetaiObj.IsView = rights != null && rights.IsView;
            pvListdetaiObj.IsExport = rights != null && rights.IsExport;
            pvListdetaiObj.ExpandData = string.Join(",", Workplace, Data, pvListdetaiObj.RandomId);
            return pvListdetaiObj;
        }

        public TreeValue GetZone(EtmfUserPermission rights, ProjectWorkplaceDetail c, ProjectWorkPlaceZone d, ProjectWorkplace b, string data)
        {
            TreeValue pvListZoneObj = new TreeValue();
            pvListZoneObj.Id = d.Id;
            pvListZoneObj.RandomId = "ZO" + ((WorkPlaceFolder)c.WorkPlaceFolderId).GetDescription().Substring(0, 2) + d.Id;
            pvListZoneObj.Text = d.EtmfZoneMasterLibrary.ZonName;
            pvListZoneObj.Number = d.EtmfZoneMasterLibrary.ZoneNo;
            pvListZoneObj.Level = 4;
            pvListZoneObj.ParentMasterId = b.ProjectId;
            pvListZoneObj.Icon = "las la-folder-open text-blue eicon";
            pvListZoneObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
            pvListZoneObj.ZoneId = d.Id;
            pvListZoneObj.IsAdd = rights != null && rights.IsAdd;
            pvListZoneObj.IsEdit = rights != null && rights.IsEdit;
            pvListZoneObj.IsDelete = rights != null && rights.IsDelete;
            pvListZoneObj.IsView = rights != null && rights.IsView;
            pvListZoneObj.IsExport = rights != null && rights.IsExport;
            pvListZoneObj.ExpandData = string.Join(",", data, pvListZoneObj.RandomId);
            return pvListZoneObj;
        }

        public TreeValue GetSection(ProjectWorkplaceSection e, WorkPlaceFolder folderType, EtmfUserPermission rights, ProjectWorkplaceDetail c,
            ProjectWorkPlaceZone d, ProjectWorkplace b, string data)
        {
            TreeValue pvListSectionObj = new TreeValue();
            pvListSectionObj.Id = e.Id;
            pvListSectionObj.RandomId = "SE" + ((WorkPlaceFolder)c.WorkPlaceFolderId).GetDescription().Substring(0, 2) + e.Id;
            pvListSectionObj.Text = e.EtmfSectionMasterLibrary.SectionName;
            pvListSectionObj.Number = e.EtmfSectionMasterLibrary.Sectionno;
            pvListSectionObj.Level = 5;
            pvListSectionObj.ZoneId = d.Id;
            pvListSectionObj.CountryId = folderType == WorkPlaceFolder.Country ? c.Id : 0;
            pvListSectionObj.SiteId = folderType == WorkPlaceFolder.Site ? c.Id : 0;
            pvListSectionObj.ProjectDetailsId = c.Id;
            pvListSectionObj.SiteProjectId = folderType == WorkPlaceFolder.Site ? c.ItemId : 0;
            pvListSectionObj.ParentMasterId = b.ProjectId;
            pvListSectionObj.Icon = "las la-folder-open text-blue eicon";
            pvListSectionObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
            pvListSectionObj.SectionId = e.Id;
            pvListSectionObj.IsAdd = rights != null && rights.IsAdd;
            pvListSectionObj.IsEdit = rights != null && rights.IsEdit;
            pvListSectionObj.IsDelete = rights != null && rights.IsDelete;
            pvListSectionObj.IsView = rights != null && rights.IsView;
            pvListSectionObj.IsExport = rights != null && rights.IsExport;
            pvListSectionObj.ExpandData = string.Join(",", data, pvListSectionObj.RandomId);
            return pvListSectionObj;
        }

        public List<TreeValue> GetArtificate(List<ProjectWorkplaceArtificate> ArtificateList, WorkPlaceFolder folderType, EtmfChartType? chartType, EtmfUserPermission rights, ProjectWorkplaceDetail c,
            ProjectWorkPlaceZone d, ProjectWorkplaceSection e, ProjectWorkplace b, string data)
        {
            List<TreeValue> pvListArtificateList = new List<TreeValue>();
            foreach (var f in ArtificateList.Where(x => x.DeletedBy == null))
            {
                TreeValue pvListArtificateObj = new TreeValue();

                f.EtmfArtificateMasterLbrary = _context.EtmfArtificateMasterLbrary.Find(f.EtmfArtificateMasterLbraryId);
                var Document = _context.ProjectWorkplaceArtificatedocument.Include(x => x.ProjectArtificateDocumentApprover)
                                .Include(x => x.ProjectArtificateDocumentReview)
                                .Where(x => x.ProjectWorkplaceArtificateId == f.Id && x.DeletedDate == null).ToList();

                pvListArtificateObj.Id = f.Id;
                pvListArtificateObj.RandomId = "AR" + ((WorkPlaceFolder)c.WorkPlaceFolderId).GetDescription().Substring(0, 2) + f.Id;
                pvListArtificateObj.Text = f.EtmfArtificateMasterLbrary.ArtificateName;
                pvListArtificateObj.Number = f.EtmfArtificateMasterLbrary.ArtificateNo;
                pvListArtificateObj.Level = 6;
                pvListArtificateObj.CountryId = folderType == WorkPlaceFolder.Country ? c.Id : 0;
                pvListArtificateObj.SiteId = folderType == WorkPlaceFolder.Site ? c.Id : 0;
                pvListArtificateObj.ProjectDetailsId = c.Id;
                pvListArtificateObj.SiteProjectId = folderType == WorkPlaceFolder.Site ? c.ItemId : 0;
                pvListArtificateObj.ZoneId = d.Id;
                pvListArtificateObj.SectionId = e.Id;
                pvListArtificateObj.ParentMasterId = b.ProjectId;
                pvListArtificateObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                pvListArtificateObj.IsNotRequired = f.IsNotRequired;
                pvListArtificateObj.ArtificateId = f.Id;
                pvListArtificateObj.IsAdd = rights != null && rights.IsAdd;
                pvListArtificateObj.IsEdit = rights != null && rights.IsEdit;
                pvListArtificateObj.IsDelete = rights != null && rights.IsDelete;
                pvListArtificateObj.IsView = rights != null && rights.IsView;
                pvListArtificateObj.IsExport = rights != null && rights.IsExport;
                pvListArtificateObj.ExpandData = string.Join(",", data, pvListArtificateObj.RandomId);
                pvListArtificateObj.DocumentCount = Document.Count();

                pvListArtificateObj.Icon = Document.Count() == 0 && f.IsNotRequired == false ? "las la-file-alt text-missing eicon" :
                    Document.Where(x => x.ProjectArtificateDocumentReview.Where(y => y.DeletedDate == null && y.UserId != x.CreatedBy).Count() == 0).Count() != 0 ? "las la-file-alt text-incomeplete eicon" :
                    Document.Where(x => x.ProjectArtificateDocumentReview.Count() != 0 && x.ProjectArtificateDocumentReview.Where(x => x.DeletedDate == null).GroupBy(x => x.UserId).LastOrDefault().Where(y => y.IsSendBack == false && y.ModifiedDate == null && y.UserId != x.CreatedBy).Count() != 0).Count() != 0 ? "las la-file-alt text-pendingreview eicon" :
                    Document.Where(x => x.ProjectArtificateDocumentApprover.Count() != 0 && x.ProjectArtificateDocumentApprover.Any(c => c.IsApproved == null && c.DeletedDate == null)).Count() != 0 ? "las la-file-alt text-pendingapprove eicon" :
                    Document.Where(x => x.Status == ArtifactDocStatusType.Final).Count() != 0 ? "las la-file-alt text-final eicon" :
                    f.IsNotRequired == true ? "las la-file-alt text-notreq eicon"
                    : "las la-file-alt text-blue eicon";
                pvListArtificateObj.IconType = Document.Count() == 0 && f.IsNotRequired == false ? EtmfChartType.Missing :
                    Document.Where(x => x.ProjectArtificateDocumentReview.Where(y => y.DeletedDate == null && y.UserId != x.CreatedBy).Count() == 0).Count() != 0 ? EtmfChartType.Incomplete :
                    Document.Where(x => x.ProjectArtificateDocumentReview.Count() != 0 && x.ProjectArtificateDocumentReview.Where(x => x.DeletedDate == null).GroupBy(x => x.UserId).LastOrDefault().Where(y => y.IsSendBack == false && y.ModifiedDate == null && y.UserId != x.CreatedBy).Count() != 0).Count() != 0 ? EtmfChartType.PendingReview :
                    Document.Where(x => x.ProjectArtificateDocumentApprover.Count() != 0 && x.ProjectArtificateDocumentApprover.Any(c => c.IsApproved == null && c.DeletedDate == null)).Count() != 0 ? EtmfChartType.PendingApprove :
                    Document.Where(x => x.Status == ArtifactDocStatusType.Final).Count() != 0 ? EtmfChartType.Final :
                    f.IsNotRequired == true ? EtmfChartType.NotRequired
                    : EtmfChartType.Nothing;

                if (chartType == EtmfChartType.Nothing)
                {
                    pvListArtificateList.Add(pvListArtificateObj);
                }
                else if (chartType == EtmfChartType.Missing && Document.Count() == 0 && f.IsNotRequired == false)
                {
                    pvListArtificateList.Add(pvListArtificateObj);
                }
                else if (chartType == EtmfChartType.Incomplete && Document.Where(x => x.ProjectArtificateDocumentReview.Where(y => y.UserId != x.CreatedBy && y.DeletedDate == null).Count() == 0).Count() != 0)
                {
                    pvListArtificateList.Add(pvListArtificateObj);
                }
                else if (chartType == EtmfChartType.PendingReview && Document.Where(x => x.ProjectArtificateDocumentReview.Count() != 0 && x.ProjectArtificateDocumentReview.Where(x => x.DeletedDate == null).GroupBy(x => x.UserId).LastOrDefault().Where(y => y.IsSendBack == false && y.ModifiedDate == null && y.UserId != x.CreatedBy).Count() != 0).Count() != 0)
                {
                    pvListArtificateList.Add(pvListArtificateObj);
                }
                else if (chartType == EtmfChartType.PendingApprove && Document.Where(x => x.ProjectArtificateDocumentApprover.Count() != 0 && x.ProjectArtificateDocumentApprover.Any(c => c.IsApproved == null && c.DeletedDate == null)).Count() != 0)
                {
                    pvListArtificateList.Add(pvListArtificateObj);
                }
                else if (chartType == EtmfChartType.Final && Document.Where(x => x.Status == ArtifactDocStatusType.Final).Count() != 0)
                {
                    pvListArtificateList.Add(pvListArtificateObj);
                }
                else if (chartType == EtmfChartType.NotRequired && f.IsNotRequired == true)
                {
                    pvListArtificateList.Add(pvListArtificateObj);
                }
                else if (chartType == EtmfChartType.core && f.EtmfArtificateMasterLbrary.InclutionType == 2 && Document.Count() == 0)
                {
                    pvListArtificateList.Add(pvListArtificateObj);
                }
                else if (chartType == EtmfChartType.Recommended && f.EtmfArtificateMasterLbrary.InclutionType == 1 && Document.Count() == 0)
                {
                    pvListArtificateList.Add(pvListArtificateObj);
                }
            }

            #region Add sub section folder data
            var SectionData = _context.ProjectWorkplaceSubSection.Where(x => x.ProjectWorkplaceSectionId == e.Id && x.DeletedBy == null).ToList();
            foreach (var s in SectionData)
            {

                TreeValue pvListSubSectionObj = new TreeValue();
                pvListSubSectionObj.Id = s.Id;
                pvListSubSectionObj.RandomId = "SS" + ((WorkPlaceFolder)c.WorkPlaceFolderId).GetDescription().Substring(0, 2) + s.Id;
                pvListSubSectionObj.Text = s.SubSectionName;
                pvListSubSectionObj.Level = 5.1;
                pvListSubSectionObj.CountryId = folderType == WorkPlaceFolder.Country ? c.Id : 0;
                pvListSubSectionObj.SiteId = folderType == WorkPlaceFolder.Site ? c.Id : 0;
                pvListSubSectionObj.ProjectDetailsId = c.Id;
                pvListSubSectionObj.SiteProjectId = folderType == WorkPlaceFolder.Site ? c.ItemId : 0;
                pvListSubSectionObj.ZoneId = d.Id;
                pvListSubSectionObj.SectionId = e.Id;
                pvListSubSectionObj.SubSectionId = s.Id;
                pvListSubSectionObj.ParentMasterId = b.ProjectId;
                pvListSubSectionObj.Icon = "las la-folder-open text-blue eicon";
                pvListSubSectionObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                pvListSubSectionObj.IsAdd = rights != null && rights.IsAdd;
                pvListSubSectionObj.IsEdit = rights != null && rights.IsEdit;
                pvListSubSectionObj.IsDelete = rights != null && rights.IsDelete;
                pvListSubSectionObj.IsView = rights != null && rights.IsView;
                pvListSubSectionObj.IsExport = rights != null && rights.IsExport;
                pvListSubSectionObj.ExpandData = string.Join(",", data, pvListSubSectionObj.RandomId);
                #region MyRegion
                List<TreeValue> pvListartifactsubsectionList = new List<TreeValue>();
                var artifactsubSectionData = _context.ProjectWorkplaceSubSectionArtifact.Where(x => x.ProjectWorkplaceSubSectionId == s.Id && x.DeletedBy == null).ToList();
                foreach (var itemartifact in artifactsubSectionData)
                {
                    var Document = _context.ProjectWorkplaceSubSecArtificatedocument
                        .Include(x => x.ProjectSubSecArtificateDocumentReview).Include(x => x.ProjectSubSecArtificateDocumentApprover)
                        .Where(x => x.ProjectWorkplaceSubSectionArtifactId == itemartifact.Id).ToList();

                    TreeValue pvListartifactsubsectionobj = new TreeValue();
                    pvListartifactsubsectionobj.Id = itemartifact.Id;
                    pvListartifactsubsectionobj.RandomId = "SSA" + ((WorkPlaceFolder)c.WorkPlaceFolderId).GetDescription().Substring(0, 2) + itemartifact.Id;
                    pvListartifactsubsectionobj.Text = itemartifact.ArtifactName;
                    pvListartifactsubsectionobj.Level = 5.2;
                    pvListartifactsubsectionobj.CountryId = folderType == WorkPlaceFolder.Country ? c.Id : 0;
                    pvListartifactsubsectionobj.SiteId = folderType == WorkPlaceFolder.Site ? c.Id : 0;
                    pvListartifactsubsectionobj.ProjectDetailsId = c.Id;
                    pvListartifactsubsectionobj.SiteProjectId = folderType == WorkPlaceFolder.Site ? c.ItemId : 0;
                    pvListartifactsubsectionobj.ZoneId = d.Id;
                    pvListartifactsubsectionobj.SectionId = e.Id;
                    pvListartifactsubsectionobj.SubSectionId = s.Id;
                    pvListartifactsubsectionobj.ParentMasterId = b.ProjectId;
                    pvListartifactsubsectionobj.SubSectionArtificateId = itemartifact.Id;
                    pvListartifactsubsectionobj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                    pvListartifactsubsectionobj.IsNotRequired = itemartifact.IsNotRequired;
                    pvListartifactsubsectionobj.IsAdd = rights != null ? rights.IsAdd : false;
                    pvListartifactsubsectionobj.IsEdit = rights != null ? rights.IsEdit : false;
                    pvListartifactsubsectionobj.IsDelete = rights != null ? rights.IsDelete : false;
                    pvListartifactsubsectionobj.IsView = rights != null ? rights.IsView : false;
                    pvListartifactsubsectionobj.IsExport = rights != null ? rights.IsExport : false;
                    pvListartifactsubsectionobj.ExpandData = string.Join(",", pvListSubSectionObj.ExpandData, pvListSubSectionObj.RandomId);
                    pvListartifactsubsectionobj.DocumentCount = Document.Count();
                    //pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);

                    pvListartifactsubsectionobj.Icon = Document.Count() == 0 && itemartifact.IsNotRequired == false ? "las la-file-alt text-missing eicon" :
                    Document.Where(x => x.ProjectSubSecArtificateDocumentReview.Where(y => y.DeletedDate == null && y.UserId != x.CreatedBy).Count() == 0).Count() != 0 ? "las la-file-alt text-incomeplete eicon" :
                    Document.Where(x => x.ProjectSubSecArtificateDocumentReview.Count() != 0 && x.ProjectSubSecArtificateDocumentReview.Where(x => x.DeletedDate == null).GroupBy(x => x.UserId).LastOrDefault().Where(y => y.IsSendBack == false && y.ModifiedDate == null && y.UserId != x.CreatedBy).Count() != 0).Count() != 0 ? "las la-file-alt text-pendingreview eicon" :
                    Document.Where(x => x.ProjectSubSecArtificateDocumentApprover.Count() != 0 && x.ProjectSubSecArtificateDocumentApprover.Any(c => c.IsApproved == null && c.DeletedDate == null)).Count() != 0 ? "las la-file-alt text-pendingapprove eicon" :
                    Document.Where(x => x.Status == ArtifactDocStatusType.Final).Count() != 0 ? "las la-file-alt text-final eicon" :
                    itemartifact.IsNotRequired == true ? "las la-file-alt text-notreq eicon"
                    : "las la-file-alt text-blue eicon";

                    pvListartifactsubsectionobj.IconType = Document.Count() == 0 && itemartifact.IsNotRequired == false ? EtmfChartType.Missing :
                       Document.Where(x => x.ProjectSubSecArtificateDocumentReview.Where(y => y.DeletedDate == null && y.UserId != x.CreatedBy).Count() == 0).Count() != 0 ? EtmfChartType.Incomplete :
                       Document.Where(x => x.ProjectSubSecArtificateDocumentReview.Count() != 0 && x.ProjectSubSecArtificateDocumentReview.Where(x => x.DeletedDate == null).GroupBy(x => x.UserId).LastOrDefault().Where(y => y.IsSendBack == false && y.ModifiedDate == null && y.UserId != x.CreatedBy).Count() != 0).Count() != 0 ? EtmfChartType.PendingReview :
                       Document.Where(x => x.ProjectSubSecArtificateDocumentApprover.Count() != 0 && x.ProjectSubSecArtificateDocumentApprover.Any(c => c.IsApproved == null && c.DeletedDate == null)).Count() != 0 ? EtmfChartType.PendingApprove :
                       Document.Where(x => x.Status == ArtifactDocStatusType.Final).Count() != 0 ? EtmfChartType.Final :
                       itemartifact.IsNotRequired == true ? EtmfChartType.NotRequired
                        : EtmfChartType.Nothing;

                    if (chartType == EtmfChartType.Nothing)
                    {
                        pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);
                    }
                    else if (chartType == EtmfChartType.Missing && Document.Count() == 0 && itemartifact.IsNotRequired == false)
                    {
                        pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);
                    }
                    else if (chartType == EtmfChartType.Incomplete && Document.Where(x => x.ProjectSubSecArtificateDocumentReview.Where(y => y.UserId != x.CreatedBy && y.DeletedDate == null).Count() == 0).Count() != 0)
                    {
                        pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);
                    }
                    else if (chartType == EtmfChartType.PendingReview && Document.Where(x => x.ProjectSubSecArtificateDocumentReview.Count() != 0 && x.ProjectSubSecArtificateDocumentReview.Where(x => x.DeletedDate == null).GroupBy(x => x.UserId).LastOrDefault().Where(y => y.IsSendBack == false && y.ModifiedDate == null && y.UserId != x.CreatedBy).Count() != 0).Count() != 0)
                    {
                        pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);
                    }
                    else if (chartType == EtmfChartType.PendingApprove && Document.Where(x => x.ProjectSubSecArtificateDocumentApprover.Count() != 0 && x.ProjectSubSecArtificateDocumentApprover.Any(c => c.IsApproved == null && c.DeletedDate == null)).Count() != 0)
                    {
                        pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);
                    }
                    else if (chartType == EtmfChartType.Final && Document.Where(x => x.Status == ArtifactDocStatusType.Final).Count() != 0)
                    {
                        pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);
                    }
                    else if (chartType == EtmfChartType.NotRequired && itemartifact.IsNotRequired == true)
                    {
                        pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);
                    }
                }
                #endregion
                pvListArtificateList.Add(pvListSubSectionObj);
                pvListSubSectionObj.Item = pvListartifactsubsectionList;
                pvListSubSectionObj.IconType = pvListSubSectionObj.Item.Any(x => x.IconType == EtmfChartType.Missing) ? EtmfChartType.Missing :
                                               pvListSubSectionObj.Item.Any(x => x.IconType == EtmfChartType.Incomplete) ? EtmfChartType.Incomplete :
                                               pvListSubSectionObj.Item.Any(x => x.IconType == EtmfChartType.PendingReview) ? EtmfChartType.PendingReview :
                                               pvListSubSectionObj.Item.Any(x => x.IconType == EtmfChartType.PendingApprove) ? EtmfChartType.PendingApprove :
                                               pvListSubSectionObj.Item.Any(x => x.IconType == EtmfChartType.Final) ? EtmfChartType.Final :
                                               pvListSubSectionObj.Item.Any(x => x.IconType == EtmfChartType.NotRequired) ? EtmfChartType.NotRequired
                                               : EtmfChartType.Nothing;
                pvListSubSectionObj.Icon = pvListSubSectionObj.IconType == EtmfChartType.Missing ? "las la-folder-open text-missing eicon" :
                                        pvListSubSectionObj.IconType == EtmfChartType.Incomplete ? "las la-folder-open text-incomeplete eicon" :
                                        pvListSubSectionObj.IconType == EtmfChartType.PendingReview ? "las la-folder-open text-pendingreview eicon" :
                                        pvListSubSectionObj.IconType == EtmfChartType.PendingApprove ? "las la-folder-open text-pendingapprove eicon" :
                                        pvListSubSectionObj.IconType == EtmfChartType.Final ? "las la-folder-open text-final eicon" :
                                        pvListSubSectionObj.IconType == EtmfChartType.NotRequired ? "las la-folder-open text-notreq eicon"
                                        : "las la-folder-open text-blue eicon";
            }
            #endregion

            return pvListArtificateList;
        }

        public ProjectWorkplace SaveFolderStructure(Data.Entities.Master.Project projectDetail, List<ProjectDropDown> childProjectList, List<DropDownDto> countryList, List<MasterLibraryJoinDto> artificiteList, string docPath)
        {
            bool status = false;
            try
            {
                string projectPath = string.Empty;
                string countryPath = string.Empty;
                string sitePath = string.Empty;
                string trialPath = string.Empty;
                projectWorkplace = new ProjectWorkplace();
                ProjectWorkplaceDetailList = new List<ProjectWorkplaceDetail>();
                projectWorkplace.ProjectId = projectDetail.Id;
                projectPath = System.IO.Path.Combine(docPath, _jwtTokenAccesser.CompanyId.ToString(), projectDetail.ProjectCode.Replace("/", ""),FolderType.Etmf.GetDescription());
                //Set Path of country, site, trial
                countryPath = Path.Combine(projectPath, WorkPlaceFolder.Country.GetDescription());
                sitePath = Path.Combine(projectPath, WorkPlaceFolder.Site.GetDescription());
                trialPath = Path.Combine(projectPath, WorkPlaceFolder.Trial.GetDescription());

                bool projectPathExists = Directory.Exists(projectPath);
                if (!projectPathExists)
                {

                    // Create Project Directory
                    System.IO.Directory.CreateDirectory(Path.Combine(projectPath));

                    //create directiry of country, site, trial
                    System.IO.Directory.CreateDirectory(Path.Combine(projectPath, WorkPlaceFolder.Country.GetDescription()));
                    System.IO.Directory.CreateDirectory(Path.Combine(projectPath, WorkPlaceFolder.Site.GetDescription()));
                    System.IO.Directory.CreateDirectory(Path.Combine(projectPath, WorkPlaceFolder.Trial.GetDescription()));
                }


                //Create direcotry of child project inside of child folder
                if (countryList != null && countryList.Count > 0)
                {

                    foreach (var coountryp in countryList)
                    {
                        ProjectWorkplaceDetail projectWorkplaceobj = new ProjectWorkplaceDetail();
                        projectWorkplaceobj.WorkPlaceFolderId = (int)WorkPlaceFolder.Country;
                        projectWorkplaceobj.ItemId = coountryp.Id;
                        projectWorkplaceobj.ItemName = coountryp.Value;


                        bool countryPathExists = Directory.Exists(countryPath);
                        if (!countryPathExists)
                            System.IO.Directory.CreateDirectory(Path.Combine(countryPath));

                        System.IO.Directory.CreateDirectory(Path.Combine(countryPath, coountryp.Value));
                        string CountryNameCreatePath = Path.Combine(countryPath, coountryp.Value);

                        // Get CountryLevel Artificates

                        var CountryLevelArtificteData = artificiteList.Where(x => x.CountryLevelDoc == true).ToList();
                        CreateFolder(CountryLevelArtificteData, CountryNameCreatePath);
                        var aa = createDBSet(CountryLevelArtificteData);
                        projectWorkplaceobj.ProjectWorkPlaceZone = aa;
                        ProjectWorkplaceDetailList.Add(projectWorkplaceobj);

                    }
                    projectWorkplace.ProjectWorkplaceDetail = ProjectWorkplaceDetailList;
                }

                if (childProjectList != null && childProjectList.Count > 0)
                {
                    //Create direcotry of child project inside of child folder

                    foreach (var childp in childProjectList)
                    {
                        ProjectWorkplaceDetail projectWorkplaceobj = new ProjectWorkplaceDetail();
                        projectWorkplaceobj.WorkPlaceFolderId = (int)WorkPlaceFolder.Site;
                        projectWorkplaceobj.ItemId = childp.Id;
                        projectWorkplaceobj.ItemName = childp.Value;

                        bool sitePathExists = Directory.Exists(sitePath);
                        if (!sitePathExists)
                            System.IO.Directory.CreateDirectory(Path.Combine(sitePath));

                        System.IO.Directory.CreateDirectory(Path.Combine(sitePath, childp.Value));

                        var CountryLevelArtificteData = artificiteList.Where(x => x.SiteLevelDoc == true).ToList();
                        string CountryNameCreatePath = Path.Combine(sitePath, childp.Value);
                        CreateFolder(CountryLevelArtificteData, CountryNameCreatePath);
                        var aa = createDBSet(CountryLevelArtificteData);
                        projectWorkplaceobj.ProjectWorkPlaceZone = aa;
                        ProjectWorkplaceDetailList.Add(projectWorkplaceobj);
                    }
                    projectWorkplace.ProjectWorkplaceDetail = ProjectWorkplaceDetailList;
                }

                var TrialLevelArtificteData = artificiteList.Where(x => x.TrailLevelDoc == true).ToList();

                if (TrialLevelArtificteData.Count > 0)
                {
                    foreach (var triall in TrialLevelArtificteData)
                    {


                        bool trialPathExists = Directory.Exists(trialPath);
                        if (!trialPathExists)
                            System.IO.Directory.CreateDirectory(Path.Combine(trialPath));

                        System.IO.Directory.CreateDirectory(Path.Combine(trialPath));
                    }
                    ProjectWorkplaceDetail projectWorkplaceobj = new ProjectWorkplaceDetail();
                    projectWorkplaceobj.WorkPlaceFolderId = (int)WorkPlaceFolder.Trial;
                    CreateFolder(TrialLevelArtificteData, trialPath);
                    var aa = createDBSet(TrialLevelArtificteData);
                    projectWorkplaceobj.ProjectWorkPlaceZone = aa;
                    ProjectWorkplaceDetailList.Add(projectWorkplaceobj);
                    projectWorkplace.ProjectWorkplaceDetail = ProjectWorkplaceDetailList;
                }

                return projectWorkplace;
            }
            catch (Exception)
            {
                status = true;
                return null;

            }
        }

        public int CreateFolder(List<MasterLibraryJoinDto> masterLibraryJoinDtos, string CountryNameCreatePath)
        {

            foreach (var item in masterLibraryJoinDtos)
            {
                var childPath = Path.Combine(CountryNameCreatePath);
                bool PathExists = Directory.Exists(childPath);
                if (!PathExists) System.IO.Directory.CreateDirectory(Path.Combine(childPath));

                var ZonePath = Path.Combine(childPath, item.ZoneName);
                bool ZonePathExists = Directory.Exists(ZonePath);
                if (!ZonePathExists) System.IO.Directory.CreateDirectory(Path.Combine(ZonePath));

                var SectionPath = Path.Combine(ZonePath, item.SectionName);
                bool SectionExists = Directory.Exists(SectionPath);
                if (!SectionExists) System.IO.Directory.CreateDirectory(Path.Combine(SectionPath));

                var ArtifactPath = Path.Combine(SectionPath, item.ArtificateName);
                bool ArtifactExists = Directory.Exists(ArtifactPath);
                if (!ArtifactExists) System.IO.Directory.CreateDirectory(Path.Combine(ArtifactPath));
            }

            var aaa = projectWorkplace;

            return 1;
        }

        public List<ProjectWorkPlaceZone> createDBSet(List<MasterLibraryJoinDto> artificiteList)
        {
            List<ProjectWorkPlaceZone> zoneLibraryList = new List<ProjectWorkPlaceZone>();

            var objZone = artificiteList.GroupBy(u => u.ZoneId).ToList();
            foreach (var zoneObj in objZone)
            {

                ProjectWorkPlaceZone zoneLibraryObj = new ProjectWorkPlaceZone();
                if (zoneObj.Key > 0)
                {
                    zoneLibraryObj.EtmfZoneMasterLibraryId = zoneObj.Key;
                    zoneLibraryObj.ProjectWorkplaceSection = new List<ProjectWorkplaceSection>();
                    foreach (var sectionObj in zoneObj.GroupBy(x => x.SectionId).ToList())
                    {

                        ProjectWorkplaceSection sectionLibraryObj = new ProjectWorkplaceSection();
                        sectionLibraryObj.EtmfSectionMasterLibraryId = sectionObj.Key;

                        sectionLibraryObj.ProjectWorkplaceArtificate = new List<ProjectWorkplaceArtificate>();
                        foreach (var item in sectionObj)
                        {
                            ProjectWorkplaceArtificate artificateObj = new ProjectWorkplaceArtificate();
                            artificateObj.EtmfArtificateMasterLbraryId = item.ArtificateId;
                            artificateObj.ProjectWorkplaceSectionId = item.SectionId;
                            sectionLibraryObj.ProjectWorkplaceArtificate.Add(artificateObj);
                        }
                        zoneLibraryObj.ProjectWorkplaceSection.Add(sectionLibraryObj);
                    }
                    zoneLibraryList.Add(zoneLibraryObj);
                }
            }

            return zoneLibraryList;
        }

        public List<ETMFWorkplaceGridDto> GetETMFWorkplaceList(bool isDeleted)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            var childProjectList = _projectRightRepository.GetChildProjectRightIdList();
            projectList.AddRange(childProjectList);

            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && projectList.Any(c => c == x.ProjectId)).
                   ProjectTo<ETMFWorkplaceGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }

        public byte[] CreateZipFileOfWorkplace(int Id)
        {
            var ProjectWorkplace = All.Include(x => x.Project).Where(x => x.Id == Id).FirstOrDefault();
            var FolderPath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), ProjectWorkplace.Project.ProjectCode.Replace("/", ""));
            ZipFile.CreateFromDirectory(FolderPath, FolderPath + ".zip", CompressionLevel.Fastest, true);
            byte[] compressedBytes;
            var zipfolder = FolderPath + ".zip";

            var dataBytes = File.ReadAllBytes(zipfolder);
            var dataStream = new MemoryStream(dataBytes);
            compressedBytes = dataStream.ToArray();
            File.Delete(zipfolder);
            return compressedBytes.ToArray();
        }

        public ProjectWorkplace SaveSiteFolderStructure(Data.Entities.Master.Project projectDetail, List<int> childProjectList, List<DropDownDto> countryList, List<MasterLibraryJoinDto> artificiteList, string docPath)
        {
            bool status = false;
            try
            {
                string projectPath = string.Empty;
                string countryPath = string.Empty;
                string sitePath = string.Empty;
                projectWorkplace = All.Where(x => x.ProjectId == projectDetail.Id).FirstOrDefault();
                ProjectWorkplaceDetailList = new List<ProjectWorkplaceDetail>();
                projectPath = Path.Combine(docPath, _jwtTokenAccesser.CompanyId.ToString(), projectDetail.ProjectCode.Replace("/", ""));
                //Set Path of country, site, trial
                countryPath = Path.Combine(projectPath, WorkPlaceFolder.Country.GetDescription());
                sitePath = Path.Combine(projectPath, WorkPlaceFolder.Site.GetDescription());

                bool projectPathExists = Directory.Exists(projectPath);
                if (!projectPathExists)
                {

                    // Create Project Directory
                    Directory.CreateDirectory(Path.Combine(projectPath));

                    //create directiry of country, site, trial
                    Directory.CreateDirectory(Path.Combine(projectPath, WorkPlaceFolder.Country.GetDescription()));
                    Directory.CreateDirectory(Path.Combine(projectPath, WorkPlaceFolder.Site.GetDescription()));
                    Directory.CreateDirectory(Path.Combine(projectPath, WorkPlaceFolder.Trial.GetDescription()));
                }

                //Create direcotry of child project inside of child folder
                if (countryList != null && countryList.Count > 0)
                {

                    foreach (var coountryp in countryList)
                    {
                        ProjectWorkplaceDetail projectWorkplaceobj = new ProjectWorkplaceDetail();
                        projectWorkplaceobj.ProjectWorkplaceId = projectWorkplace.Id;
                        projectWorkplaceobj.WorkPlaceFolderId = (int)WorkPlaceFolder.Country;
                        projectWorkplaceobj.ItemId = coountryp.Id;
                        projectWorkplaceobj.ItemName = coountryp.Value;


                        bool countryPathExists = Directory.Exists(countryPath);
                        if (!countryPathExists)
                            Directory.CreateDirectory(Path.Combine(countryPath));

                        bool countryExists = Directory.Exists(Path.Combine(countryPath, coountryp.Value));
                        if (!countryExists)
                        {
                            Directory.CreateDirectory(Path.Combine(countryPath, coountryp.Value));
                            string CountryNameCreatePath = Path.Combine(countryPath, coountryp.Value);

                            // Get CountryLevel Artificates

                            var CountryLevelArtificteData = artificiteList.Where(x => x.CountryLevelDoc == true).ToList();
                            CreateFolder(CountryLevelArtificteData, CountryNameCreatePath);
                            var aa = createDBSet(CountryLevelArtificteData);
                            projectWorkplaceobj.ProjectWorkPlaceZone = aa;
                            ProjectWorkplaceDetailList.Add(projectWorkplaceobj);
                        }
                    }
                    projectWorkplace.ProjectWorkplaceDetail = ProjectWorkplaceDetailList;
                }

                if (childProjectList != null && childProjectList.Count > 0)
                {
                    //Create direcotry of child project inside of child folder

                    foreach (var item in childProjectList)
                    {
                        var childp = _context.Project.Where(x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                                     && x.DeletedDate == null && x.Id == item)
                                    .Select(c => new ProjectDropDown
                                    {
                                        Id = c.Id,
                                        Value = c.ProjectCode,
                                        CountryId = c.CountryId,
                                        Code = c.ProjectCode,
                                        IsStatic = c.IsStatic,
                                        ParentProjectId = c.ParentProjectId ?? 0
                                    }).OrderBy(o => o.Value).FirstOrDefault();

                        ProjectWorkplaceDetail projectWorkplaceobj = new ProjectWorkplaceDetail();
                        projectWorkplaceobj.ProjectWorkplaceId = projectWorkplace.Id;
                        projectWorkplaceobj.WorkPlaceFolderId = (int)WorkPlaceFolder.Site;
                        projectWorkplaceobj.ItemId = childp.Id;
                        projectWorkplaceobj.ItemName = childp.Value;

                        bool sitePathExists = Directory.Exists(sitePath);
                        if (!sitePathExists)
                            Directory.CreateDirectory(Path.Combine(sitePath));

                        bool siteExists = Directory.Exists(Path.Combine(sitePath, childp.Value));
                        if (!siteExists)
                        {
                            Directory.CreateDirectory(Path.Combine(sitePath, childp.Value));

                            var CountryLevelArtificteData = artificiteList.Where(x => x.SiteLevelDoc == true).ToList();
                            string CountryNameCreatePath = Path.Combine(sitePath, childp.Value);
                            CreateFolder(CountryLevelArtificteData, CountryNameCreatePath);
                            var aa = createDBSet(CountryLevelArtificteData);
                            projectWorkplaceobj.ProjectWorkPlaceZone = aa;
                            ProjectWorkplaceDetailList.Add(projectWorkplaceobj);
                        }
                    }
                    projectWorkplace.ProjectWorkplaceDetail = ProjectWorkplaceDetailList;
                }

                return projectWorkplace;
            }
            catch (Exception)
            {
                status = true;
                return null;
            }
        }

        public List<ChartReport> GetChartReport(int id, EtmfChartType? chartType)
        {
            var result = new List<ChartReport>();
            var projectWorkplaces = _context.ProjectWorkplace.Where(t => t.DeletedBy == null && t.ProjectId == id)
                            .Include(x => x.ProjectWorkplaceDetail)
                            .ThenInclude(x => x.ProjectWorkPlaceZone)
                            .ThenInclude(x => x.ProjectWorkplaceSection)
                            .ThenInclude(x => x.ProjectWorkplaceArtificate)
                            .AsNoTracking().ToList();
            var Project = _context.Project.Where(x => x.Id == id).FirstOrDefault();
            foreach (var b in projectWorkplaces)
            {
                #region Details
                foreach (var c in b.ProjectWorkplaceDetail.Where(x => x.DeletedBy == null))
                {
                    var rights = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId == c.Id && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();

                    if (rights != null && rights.IsView)
                    {
                        TreeValue pvListdetaiObj = GetWorksplaceDetails(rights, c, null);

                        foreach (var d in c.ProjectWorkPlaceZone.Where(x => x.DeletedBy == null))
                        {
                            d.EtmfZoneMasterLibrary = _context.EtmfZoneMasterLibrary.Find(d.EtmfZoneMasterLibraryId);
                            // Get zone
                            TreeValue pvListZoneObj = GetZone(rights, c, d, b, pvListdetaiObj.ExpandData);

                            foreach (var e in d.ProjectWorkplaceSection.Where(x => x.DeletedBy == null))
                            {
                                e.EtmfSectionMasterLibrary = _context.EtmfSectionMasterLibrary.Find(e.EtmfSectionMasterLibraryId);
                                // Get section
                                TreeValue pvListSectionObj = GetSection(e, (WorkPlaceFolder)c.WorkPlaceFolderId, rights, c, d, b, pvListZoneObj.ExpandData);
                                // Get artificate
                                List<TreeValue> pvListArtificateList = GetArtificate(e.ProjectWorkplaceArtificate, (WorkPlaceFolder)c.WorkPlaceFolderId, chartType, rights, c, d, e, b, pvListSectionObj.ExpandData);

                                foreach (var artificate in pvListArtificateList.Where(x => x.Level == 6).ToList())
                                {
                                    ChartReport obj = new ChartReport();
                                    obj.ProjectCode = Project.ProjectCode;
                                    obj.WorkPlaceFolderName = c.ItemName;
                                    obj.WorkPlaceFolderType = ((WorkPlaceFolder)c.WorkPlaceFolderId).GetDescription();
                                    obj.ZoneName = d.EtmfZoneMasterLibrary.ZonName;
                                    obj.SectionName = e.EtmfSectionMasterLibrary.SectionName;
                                    obj.ArtificateName = artificate.Text;
                                    result.Add(obj);
                                }

                                foreach (var artificate in pvListArtificateList.Where(x => x.Level == 5.1).ToList())
                                {
                                    foreach (var subData in artificate?.Item)
                                    {
                                        ChartReport obj = new ChartReport();
                                        obj.ProjectCode = Project.ProjectCode;
                                        obj.WorkPlaceFolderName = c.ItemName;
                                        obj.WorkPlaceFolderType = ((WorkPlaceFolder)c.WorkPlaceFolderId).GetDescription();
                                        obj.ZoneName = d.EtmfZoneMasterLibrary.ZonName;
                                        obj.SectionName = e.EtmfSectionMasterLibrary.SectionName;
                                        obj.SubSectionName = artificate.Text;
                                        obj.ArtificateName = subData.Text;
                                        result.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            return result;
        }

    }
    public class TreeValue
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Number { get; set; }
        public List<TreeValue> Item { get; set; }
        public double Level { get; set; }
        public int ParentMasterId { get; set; }
        public int CountryId { get; set; }
        public int SiteId { get; set; }
        public int ProjectDetailsId { get; set; }
        public int SiteProjectId { get; set; }
        public int ZoneId { get; set; }
        public int SectionId { get; set; }
        public int ArtificateId { get; set; }
        public int SubSectionId { get; set; }
        public int SubSectionArtificateId { get; set; }
        public WorkPlaceFolder WorkPlaceFolderId { get; set; }
        public string Icon { get; set; }
        public EtmfChartType IconType { get; set; }

        public string RandomId { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsView { get; set; }
        public bool IsDelete { get; set; }
        public bool IsExport { get; set; }
        public bool IsNotRequired { get; set; }
        public int DocumentCount { get; set; }
        public string ExpandData { get; set; }
    }
}
