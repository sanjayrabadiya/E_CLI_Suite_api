using AutoMapper;
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
        ProjectWorkplaceDto projectWorkplaceDto = new ProjectWorkplaceDto();
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        List<ProjectWorkplaceDetail> ProjectWorkplaceDetailList = new List<ProjectWorkplaceDetail>();
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;
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

        public List<TreeValue> GetTreeview(int id)
        {
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
                            .ThenInclude(x => x.ProjectWorkplaceArtificate).AsNoTracking().ToList();

            List<TreeValue> pvList = new List<TreeValue>();
            TreeValue pvListObj = new TreeValue();
            foreach (var b in projectWorkplaces)
            {

                pvListObj.Id = b.Id;
                pvListObj.RandomId = RandomPassword.CreateRandomPassword(6);
                pvListObj.Text = _context.Project.Where(x => x.Id == b.ProjectId).FirstOrDefault().ProjectCode;
                pvListObj.Level = 1;
                pvListObj.Item = new List<TreeValue>();
                pvListObj.ParentMasterId = b.ProjectId;
                pvListObj.Icon = "folder";

                TreeValue TrialFol = new TreeValue();
                TrialFol.Id = 11111111;
                TrialFol.RandomId = RandomPassword.CreateRandomPassword(6);
                TrialFol.Text = "Trial";
                TrialFol.Level = 2;
                TrialFol.Icon = "folder";
                TrialFol.WorkPlaceFolderId = WorkPlaceFolder.Trial;

                TreeValue CountryFol = new TreeValue();
                CountryFol.Id = 11111112;
                CountryFol.RandomId = RandomPassword.CreateRandomPassword(6);
                CountryFol.Text = "Country";
                CountryFol.Level = 2;
                CountryFol.Icon = "folder";
                CountryFol.WorkPlaceFolderId = WorkPlaceFolder.Country;

                TreeValue SiteFol = new TreeValue();
                SiteFol.Id = 11111113;
                SiteFol.RandomId = RandomPassword.CreateRandomPassword(6);
                SiteFol.Text = "Site";
                SiteFol.Level = 2;
                SiteFol.Icon = "folder";
                SiteFol.WorkPlaceFolderId = WorkPlaceFolder.Site;

                CountryFol.Item = new List<TreeValue>();
                SiteFol.Item = new List<TreeValue>();
                #region Get Country
                foreach (var c in b.ProjectWorkplaceDetail.Where(x => x.WorkPlaceFolderId == (int)WorkPlaceFolder.Country && x.DeletedBy == null))
                {
                    var rights = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId == c.Id && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();

                    TreeValue pvListdetaiObj = new TreeValue();
                    pvListdetaiObj.Item = new List<TreeValue>();
                    pvListdetaiObj.Id = 22222222;
                    pvListdetaiObj.RandomId = RandomPassword.CreateRandomPassword(6);
                    pvListdetaiObj.Text = c.ItemName;
                    pvListdetaiObj.Level = 3;
                    pvListdetaiObj.Icon = "folder";
                    pvListdetaiObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                    pvListdetaiObj.IsAdd = rights != null ? rights.IsAdd : false;
                    pvListdetaiObj.IsEdit = rights != null ? rights.IsEdit : false;
                    pvListdetaiObj.IsDelete = rights != null ? rights.IsDelete : false;
                    pvListdetaiObj.IsView = rights != null ? rights.IsView : false;
                    pvListdetaiObj.IsExport = rights != null ? rights.IsExport : false;

                    List<TreeValue> pvListZoneList = new List<TreeValue>();
                    foreach (var d in c.ProjectWorkPlaceZone.Where(x => x.DeletedBy == null))
                    {
                        d.EtmfZoneMasterLibrary = _context.EtmfZoneMasterLibrary.Find(d.EtmfZoneMasterLibraryId);

                        TreeValue pvListZoneObj = new TreeValue();
                        pvListZoneObj.Id = d.Id;
                        pvListZoneObj.RandomId = RandomPassword.CreateRandomPassword(6);
                        pvListZoneObj.Text = d.EtmfZoneMasterLibrary.ZonName;
                        pvListZoneObj.Number = d.EtmfZoneMasterLibrary.ZoneNo;
                        pvListZoneObj.Level = 4;
                        pvListZoneObj.ParentMasterId = b.ProjectId;
                        pvListZoneObj.Icon = "folder";
                        pvListZoneObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                        pvListZoneObj.ZoneId = d.Id;
                        pvListZoneObj.IsAdd = rights != null ? rights.IsAdd : false;
                        pvListZoneObj.IsEdit = rights != null ? rights.IsEdit : false;
                        pvListZoneObj.IsDelete = rights != null ? rights.IsDelete : false;
                        pvListZoneObj.IsView = rights != null ? rights.IsView : false;
                        pvListZoneObj.IsExport = rights != null ? rights.IsExport : false;

                        List<TreeValue> pvListSectionList = new List<TreeValue>();
                        foreach (var e in d.ProjectWorkplaceSection.Where(x => x.DeletedBy == null))
                        {
                            e.EtmfSectionMasterLibrary = _context.EtmfSectionMasterLibrary.Find(e.EtmfSectionMasterLibraryId);

                            TreeValue pvListSectionObj = new TreeValue();
                            pvListSectionObj.Id = e.Id;
                            pvListSectionObj.RandomId = RandomPassword.CreateRandomPassword(6);
                            pvListSectionObj.Text = e.EtmfSectionMasterLibrary.SectionName;
                            pvListSectionObj.Number = e.EtmfSectionMasterLibrary.Sectionno;
                            pvListSectionObj.Level = 5;
                            pvListSectionObj.ZoneId = d.Id;
                            pvListSectionObj.CountryId = c.Id;
                            pvListSectionObj.ParentMasterId = b.ProjectId;
                            pvListSectionObj.Icon = "folder";
                            pvListSectionObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                            pvListSectionObj.SectionId = e.Id;
                            pvListSectionObj.IsAdd = rights != null ? rights.IsAdd : false;
                            pvListSectionObj.IsEdit = rights != null ? rights.IsEdit : false;
                            pvListSectionObj.IsDelete = rights != null ? rights.IsDelete : false;
                            pvListSectionObj.IsView = rights != null ? rights.IsView : false;
                            pvListSectionObj.IsExport = rights != null ? rights.IsExport : false;
                            List<TreeValue> pvListArtificateList = new List<TreeValue>();
                            foreach (var f in e.ProjectWorkplaceArtificate.Where(x => x.DeletedBy == null))
                            {
                                f.EtmfArtificateMasterLbrary = _context.EtmfArtificateMasterLbrary.Find(f.EtmfArtificateMasterLbraryId);
                                TreeValue pvListArtificateObj = new TreeValue();
                                pvListArtificateObj.Id = f.Id;
                                pvListArtificateObj.RandomId = RandomPassword.CreateRandomPassword(6);
                                pvListArtificateObj.Text = f.EtmfArtificateMasterLbrary.ArtificateName;
                                pvListArtificateObj.Number = f.EtmfArtificateMasterLbrary.ArtificateNo;
                                pvListArtificateObj.Level = 6;

                                pvListArtificateObj.CountryId = c.Id;
                                pvListArtificateObj.SiteId = 0;
                                pvListArtificateObj.ZoneId = d.Id;
                                pvListArtificateObj.SectionId = e.Id;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.Icon = "las la-file-alt text-blue eicon";
                                pvListArtificateObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                                pvListArtificateObj.IsNotRequired = f.IsNotRequired;
                                pvListArtificateObj.ArtificateId = f.Id;
                                pvListArtificateObj.IsAdd = rights != null ? rights.IsAdd : false;
                                pvListArtificateObj.IsEdit = rights != null ? rights.IsEdit : false;
                                pvListArtificateObj.IsDelete = rights != null ? rights.IsDelete : false;
                                pvListArtificateObj.IsView = rights != null ? rights.IsView : false;
                                pvListArtificateObj.IsExport = rights != null ? rights.IsExport : false;
                                pvListArtificateList.Add(pvListArtificateObj);
                            }

                            #region Add sub section folder data
                            List<TreeValue> pvListsubsectionList = new List<TreeValue>();
                            var SectionData = _context.ProjectWorkplaceSubSection.Where(x => x.ProjectWorkplaceSectionId == e.Id && x.DeletedBy == null).ToList();
                            foreach (var s in SectionData)
                            {

                                TreeValue pvListArtificateObj = new TreeValue();
                                pvListArtificateObj.Id = s.Id;
                                pvListArtificateObj.RandomId = RandomPassword.CreateRandomPassword(6);
                                pvListArtificateObj.Text = s.SubSectionName;
                                pvListArtificateObj.Level = 5.1;

                                pvListArtificateObj.CountryId = c.Id;
                                pvListArtificateObj.SiteId = 0;
                                pvListArtificateObj.ZoneId = d.Id;
                                pvListArtificateObj.SectionId = e.Id;
                                pvListArtificateObj.SubSectionId = s.Id;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.Icon = "folder";
                                pvListArtificateObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                                pvListArtificateObj.IsAdd = rights != null ? rights.IsAdd : false;
                                pvListArtificateObj.IsEdit = rights != null ? rights.IsEdit : false;
                                pvListArtificateObj.IsDelete = rights != null ? rights.IsDelete : false;
                                pvListArtificateObj.IsView = rights != null ? rights.IsView : false;
                                pvListArtificateObj.IsExport = rights != null ? rights.IsExport : false;

                                List<TreeValue> pvListartifactsubsectionList = new List<TreeValue>();
                                var artifactsubSectionData = _context.ProjectWorkplaceSubSectionArtifact.Where(x => x.ProjectWorkplaceSubSectionId == s.Id && x.DeletedBy == null).ToList();
                                foreach (var itemartifact in artifactsubSectionData)
                                {
                                    TreeValue pvListartifactsubsectionobj = new TreeValue();
                                    pvListartifactsubsectionobj.Id = itemartifact.Id;
                                    pvListartifactsubsectionobj.RandomId = RandomPassword.CreateRandomPassword(6);
                                    pvListartifactsubsectionobj.Text = itemartifact.ArtifactName;
                                    pvListartifactsubsectionobj.Level = 5.2;
                                    pvListartifactsubsectionobj.CountryId = c.Id;
                                    pvListartifactsubsectionobj.SiteId = 0;
                                    pvListartifactsubsectionobj.ZoneId = d.Id;
                                    pvListartifactsubsectionobj.SectionId = e.Id;
                                    pvListartifactsubsectionobj.SubSectionId = s.Id;
                                    pvListartifactsubsectionobj.ParentMasterId = b.ProjectId;
                                    pvListartifactsubsectionobj.SubSectionArtificateId = itemartifact.Id;
                                    pvListartifactsubsectionobj.Icon = "las la-file-alt text-blue eicon";
                                    pvListartifactsubsectionobj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                                    pvListartifactsubsectionobj.IsNotRequired = itemartifact.IsNotRequired;
                                    pvListartifactsubsectionobj.IsAdd = rights != null ? rights.IsAdd : false;
                                    pvListartifactsubsectionobj.IsEdit = rights != null ? rights.IsEdit : false;
                                    pvListartifactsubsectionobj.IsDelete = rights != null ? rights.IsDelete : false;
                                    pvListartifactsubsectionobj.IsView = rights != null ? rights.IsView : false;
                                    pvListartifactsubsectionobj.IsExport = rights != null ? rights.IsExport : false;
                                    pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);
                                }
                                pvListArtificateList.Add(pvListArtificateObj);
                                pvListArtificateObj.Item = pvListartifactsubsectionList;
                            }
                            #endregion

                            pvListSectionList.Add(pvListSectionObj);
                            pvListSectionObj.Item = pvListArtificateList.OrderBy(x => x.Number).ToList();
                        }
                        pvListZoneList.Add(pvListZoneObj);
                        pvListZoneObj.Item = pvListSectionList.OrderBy(x => x.Number).ToList();
                        pvListdetaiObj.Item.Add(pvListZoneObj);
                    }

                    pvListdetaiObj.Item = pvListdetaiObj.Item.OrderBy(x => x.Number).ToList();
                    CountryFol.Item.Add(pvListdetaiObj);

                }

                #endregion

                #region Get Site
                foreach (var c in b.ProjectWorkplaceDetail.Where(x => x.WorkPlaceFolderId == (int)WorkPlaceFolder.Site && x.DeletedBy == null))
                {
                    var rights = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId == c.Id && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();

                    TreeValue pvListdetaiObj = new TreeValue();
                    pvListdetaiObj.Item = new List<TreeValue>();
                    pvListdetaiObj.Id = 232323232;
                    pvListdetaiObj.RandomId = RandomPassword.CreateRandomPassword(6);
                    pvListdetaiObj.Text = c.ItemName;
                    pvListdetaiObj.Level = 3;
                    pvListdetaiObj.Icon = "folder";
                    pvListdetaiObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                    pvListdetaiObj.IsAdd = rights != null ? rights.IsAdd : false;
                    pvListdetaiObj.IsEdit = rights != null ? rights.IsEdit : false;
                    pvListdetaiObj.IsDelete = rights != null ? rights.IsDelete : false;
                    pvListdetaiObj.IsView = rights != null ? rights.IsView : false;
                    pvListdetaiObj.IsExport = rights != null ? rights.IsExport : false;

                    List<TreeValue> pvListZoneList = new List<TreeValue>();
                    foreach (var d in c.ProjectWorkPlaceZone.Where(x => x.DeletedBy == null))
                    {
                        d.EtmfZoneMasterLibrary = _context.EtmfZoneMasterLibrary.Find(d.EtmfZoneMasterLibraryId);

                        TreeValue pvListZoneObj = new TreeValue();
                        pvListZoneObj.Id = d.Id;
                        pvListZoneObj.RandomId = RandomPassword.CreateRandomPassword(6);
                        pvListZoneObj.Text = d.EtmfZoneMasterLibrary.ZonName;
                        pvListZoneObj.Number = d.EtmfZoneMasterLibrary.ZoneNo;
                        pvListZoneObj.Level = 4;
                        pvListZoneObj.ParentMasterId = b.ProjectId;
                        pvListZoneObj.Icon = "folder";
                        pvListZoneObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                        pvListZoneObj.ZoneId = d.Id;
                        pvListZoneObj.IsAdd = rights != null ? rights.IsAdd : false;
                        pvListZoneObj.IsEdit = rights != null ? rights.IsEdit : false;
                        pvListZoneObj.IsDelete = rights != null ? rights.IsDelete : false;
                        pvListZoneObj.IsView = rights != null ? rights.IsView : false;
                        pvListZoneObj.IsExport = rights != null ? rights.IsExport : false;

                        List<TreeValue> pvListSectionList = new List<TreeValue>();
                        foreach (var e in d.ProjectWorkplaceSection.Where(x => x.DeletedBy == null))
                        {
                            e.EtmfSectionMasterLibrary = _context.EtmfSectionMasterLibrary.Find(e.EtmfSectionMasterLibraryId);

                            TreeValue pvListSectionObj = new TreeValue();
                            pvListSectionObj.Id = e.Id;
                            pvListSectionObj.RandomId = RandomPassword.CreateRandomPassword(6);
                            pvListSectionObj.Text = e.EtmfSectionMasterLibrary.SectionName;
                            pvListSectionObj.Number = e.EtmfSectionMasterLibrary.Sectionno;
                            pvListSectionObj.Level = 5;
                            pvListSectionObj.ZoneId = d.Id;
                            pvListSectionObj.SiteId = c.Id;
                            pvListSectionObj.SiteProjectId = c.ItemId;
                            pvListSectionObj.ParentMasterId = b.ProjectId;
                            pvListSectionObj.Icon = "folder";
                            pvListSectionObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                            pvListSectionObj.SectionId = e.Id;
                            pvListSectionObj.IsAdd = rights != null ? rights.IsAdd : false;
                            pvListSectionObj.IsEdit = rights != null ? rights.IsEdit : false;
                            pvListSectionObj.IsDelete = rights != null ? rights.IsDelete : false;
                            pvListSectionObj.IsView = rights != null ? rights.IsView : false;
                            pvListSectionObj.IsExport = rights != null ? rights.IsExport : false;

                            List<TreeValue> pvListArtificateList = new List<TreeValue>();
                            foreach (var f in e.ProjectWorkplaceArtificate.Where(x => x.DeletedBy == null))
                            {
                                f.EtmfArtificateMasterLbrary = _context.EtmfArtificateMasterLbrary.Find(f.EtmfArtificateMasterLbraryId);
                                TreeValue pvListArtificateObj = new TreeValue();
                                pvListArtificateObj.Id = f.Id;
                                pvListArtificateObj.RandomId = RandomPassword.CreateRandomPassword(6);
                                pvListArtificateObj.Text = f.EtmfArtificateMasterLbrary.ArtificateName;
                                pvListArtificateObj.Number = f.EtmfArtificateMasterLbrary.ArtificateNo;
                                pvListArtificateObj.Level = 6;

                                pvListArtificateObj.CountryId = 0;
                                pvListArtificateObj.SiteId = c.Id;
                                pvListArtificateObj.SiteProjectId = c.ItemId;
                                pvListArtificateObj.ZoneId = d.Id;
                                pvListArtificateObj.SectionId = e.Id;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.Icon = "las la-file-alt text-blue eicon";
                                pvListArtificateObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                                pvListArtificateObj.IsNotRequired = f.IsNotRequired;
                                pvListArtificateObj.ArtificateId = f.Id;
                                pvListArtificateObj.IsAdd = rights != null ? rights.IsAdd : false;
                                pvListArtificateObj.IsEdit = rights != null ? rights.IsEdit : false;
                                pvListArtificateObj.IsDelete = rights != null ? rights.IsDelete : false;
                                pvListArtificateObj.IsView = rights != null ? rights.IsView : false;
                                pvListArtificateObj.IsExport = rights != null ? rights.IsExport : false;
                                pvListArtificateList.Add(pvListArtificateObj);
                            }

                            #region Add sub section folder data
                            List<TreeValue> pvListsubsectionList = new List<TreeValue>();
                            var SectionData = _context.ProjectWorkplaceSubSection.Where(x => x.ProjectWorkplaceSectionId == e.Id && x.DeletedBy == null).ToList();
                            foreach (var s in SectionData)
                            {

                                TreeValue pvListArtificateObj = new TreeValue();
                                pvListArtificateObj.Id = s.Id;
                                pvListArtificateObj.RandomId = RandomPassword.CreateRandomPassword(6);
                                pvListArtificateObj.Text = s.SubSectionName;
                                pvListArtificateObj.Level = 5.1;

                                pvListArtificateObj.CountryId = 0;
                                pvListArtificateObj.SiteId = c.Id;
                                pvListArtificateObj.SiteProjectId = c.ItemId;
                                pvListArtificateObj.ZoneId = d.Id;
                                pvListArtificateObj.SectionId = e.Id;
                                pvListArtificateObj.SubSectionId = s.Id;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.Icon = "folder";
                                pvListArtificateObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                                pvListArtificateObj.IsAdd = rights != null ? rights.IsAdd : false;
                                pvListArtificateObj.IsEdit = rights != null ? rights.IsEdit : false;
                                pvListArtificateObj.IsDelete = rights != null ? rights.IsDelete : false;
                                pvListArtificateObj.IsView = rights != null ? rights.IsView : false;
                                pvListArtificateObj.IsExport = rights != null ? rights.IsExport : false;

                                List<TreeValue> pvListartifactsubsectionList = new List<TreeValue>();
                                var artifactsubSectionData = _context.ProjectWorkplaceSubSectionArtifact.Where(x => x.ProjectWorkplaceSubSectionId == s.Id && x.DeletedBy == null).ToList();
                                foreach (var itemartifact in artifactsubSectionData)
                                {
                                    TreeValue pvListartifactsubsectionobj = new TreeValue();
                                    pvListartifactsubsectionobj.Id = itemartifact.Id;
                                    pvListartifactsubsectionobj.RandomId = RandomPassword.CreateRandomPassword(6);
                                    pvListartifactsubsectionobj.Text = itemartifact.ArtifactName;
                                    pvListartifactsubsectionobj.Level = 5.2;

                                    pvListartifactsubsectionobj.CountryId = 0;
                                    pvListartifactsubsectionobj.SiteId = c.Id;
                                    pvListartifactsubsectionobj.SiteProjectId = c.ItemId;
                                    pvListartifactsubsectionobj.ZoneId = d.Id;
                                    pvListartifactsubsectionobj.SectionId = e.Id;
                                    pvListartifactsubsectionobj.SubSectionId = s.Id;
                                    pvListartifactsubsectionobj.ParentMasterId = b.ProjectId;
                                    pvListartifactsubsectionobj.SubSectionArtificateId = itemartifact.Id;
                                    pvListartifactsubsectionobj.Icon = "las la-file-alt text-blue eicon";
                                    pvListartifactsubsectionobj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                                    pvListartifactsubsectionobj.IsNotRequired = itemartifact.IsNotRequired;
                                    pvListartifactsubsectionobj.IsAdd = rights != null ? rights.IsAdd : false;
                                    pvListartifactsubsectionobj.IsEdit = rights != null ? rights.IsEdit : false;
                                    pvListartifactsubsectionobj.IsDelete = rights != null ? rights.IsDelete : false;
                                    pvListartifactsubsectionobj.IsView = rights != null ? rights.IsView : false;
                                    pvListartifactsubsectionobj.IsExport = rights != null ? rights.IsExport : false;
                                    pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);

                                }
                                pvListArtificateList.Add(pvListArtificateObj);
                                pvListArtificateObj.Item = pvListartifactsubsectionList;
                            }
                            #endregion
                            pvListSectionList.Add(pvListSectionObj);
                            pvListSectionObj.Item = pvListArtificateList.OrderBy(x => x.Number).ToList();
                        }

                        pvListZoneList.Add(pvListZoneObj);
                        pvListZoneObj.Item = pvListSectionList.OrderBy(x => x.Number).ToList();
                        pvListdetaiObj.Item.Add(pvListZoneObj);
                    }
                    pvListdetaiObj.Item = pvListdetaiObj.Item.OrderBy(x => x.Number).ToList();
                    SiteFol.Item.Add(pvListdetaiObj);
                }

                #endregion

                #region Get Trial
                foreach (var c in b.ProjectWorkplaceDetail.Where(x => x.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial && x.DeletedBy == null))
                {
                    var rights = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId == c.Id && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();

                    TreeValue pvListdetaiObj = new TreeValue();
                    pvListdetaiObj.Id = 333333333;
                    pvListdetaiObj.RandomId = RandomPassword.CreateRandomPassword(6);
                    pvListdetaiObj.Text = c.ItemName;
                    pvListdetaiObj.Level = 3;
                    pvListdetaiObj.Icon = "folder";
                    pvListdetaiObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                    pvListdetaiObj.IsAdd = rights != null ? rights.IsAdd : false;
                    pvListdetaiObj.IsEdit = rights != null ? rights.IsEdit : false;
                    pvListdetaiObj.IsDelete = rights != null ? rights.IsDelete : false;
                    pvListdetaiObj.IsView = rights != null ? rights.IsView : false;
                    pvListdetaiObj.IsExport = rights != null ? rights.IsExport : false;
                    List<TreeValue> pvListZoneList = new List<TreeValue>();

                    foreach (var d in c.ProjectWorkPlaceZone.Where(x => x.DeletedBy == null))
                    {
                        d.EtmfZoneMasterLibrary = _context.EtmfZoneMasterLibrary.Find(d.EtmfZoneMasterLibraryId);

                        TreeValue pvListZoneObj = new TreeValue();
                        pvListZoneObj.Id = d.Id;
                        pvListZoneObj.RandomId = RandomPassword.CreateRandomPassword(6);
                        pvListZoneObj.Text = d.EtmfZoneMasterLibrary.ZonName;
                        pvListZoneObj.Number = d.EtmfZoneMasterLibrary.ZoneNo;
                        pvListZoneObj.Level = 4;
                        pvListZoneObj.ParentMasterId = b.ProjectId;
                        pvListZoneObj.Icon = "folder";
                        pvListZoneObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                        pvListZoneObj.ZoneId = d.Id;
                        pvListZoneObj.IsAdd = rights != null ? rights.IsAdd : false;
                        pvListZoneObj.IsEdit = rights != null ? rights.IsEdit : false;
                        pvListZoneObj.IsDelete = rights != null ? rights.IsDelete : false;
                        pvListZoneObj.IsView = rights != null ? rights.IsView : false;
                        pvListZoneObj.IsExport = rights != null ? rights.IsExport : false;
                        List<TreeValue> pvListSectionList = new List<TreeValue>();
                        foreach (var e in d.ProjectWorkplaceSection.Where(x => x.DeletedBy == null))
                        {
                            e.EtmfSectionMasterLibrary = _context.EtmfSectionMasterLibrary.Find(e.EtmfSectionMasterLibraryId);

                            TreeValue pvListSectionObj = new TreeValue();
                            pvListSectionObj.Id = e.Id;
                            pvListSectionObj.RandomId = RandomPassword.CreateRandomPassword(6);
                            pvListSectionObj.Text = e.EtmfSectionMasterLibrary.SectionName;
                            pvListSectionObj.Number = e.EtmfSectionMasterLibrary.Sectionno;
                            pvListSectionObj.Level = 5;
                            pvListSectionObj.ZoneId = d.Id;
                            pvListSectionObj.ParentMasterId = b.ProjectId;
                            pvListSectionObj.Icon = "folder";
                            pvListSectionObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                            pvListSectionObj.SectionId = e.Id;
                            pvListSectionObj.IsAdd = rights != null ? rights.IsAdd : false;
                            pvListSectionObj.IsEdit = rights != null ? rights.IsEdit : false;
                            pvListSectionObj.IsDelete = rights != null ? rights.IsDelete : false;
                            pvListSectionObj.IsView = rights != null ? rights.IsView : false;
                            pvListSectionObj.IsExport = rights != null ? rights.IsExport : false;
                            List<TreeValue> pvListArtificateList = new List<TreeValue>();
                            foreach (var f in e.ProjectWorkplaceArtificate.Where(x => x.DeletedBy == null))
                            {
                                f.EtmfArtificateMasterLbrary = _context.EtmfArtificateMasterLbrary.Find(f.EtmfArtificateMasterLbraryId);
                                TreeValue pvListArtificateObj = new TreeValue();
                                pvListArtificateObj.Id = f.Id;
                                pvListArtificateObj.RandomId = RandomPassword.CreateRandomPassword(6);
                                pvListArtificateObj.Text = f.EtmfArtificateMasterLbrary.ArtificateName;
                                pvListArtificateObj.Number = f.EtmfArtificateMasterLbrary.ArtificateNo;
                                pvListArtificateObj.Level = 6;

                                pvListArtificateObj.CountryId = 0;
                                pvListArtificateObj.SiteId = 0;
                                pvListArtificateObj.ZoneId = d.Id;
                                pvListArtificateObj.SectionId = e.Id;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.Icon = "las la-file-alt text-blue eicon";
                                pvListArtificateObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                                pvListArtificateObj.IsNotRequired = f.IsNotRequired;
                                pvListArtificateObj.ArtificateId = f.Id;
                                pvListArtificateObj.IsAdd = rights != null ? rights.IsAdd : false;
                                pvListArtificateObj.IsEdit = rights != null ? rights.IsEdit : false;
                                pvListArtificateObj.IsDelete = rights != null ? rights.IsDelete : false;
                                pvListArtificateObj.IsView = rights != null ? rights.IsView : false;
                                pvListArtificateObj.IsExport = rights != null ? rights.IsExport : false;
                                pvListArtificateList.Add(pvListArtificateObj);
                            }

                            #region Add sub section folder data
                            List<TreeValue> pvListsubsectionList = new List<TreeValue>();
                            var SectionData = _context.ProjectWorkplaceSubSection.Where(x => x.ProjectWorkplaceSectionId == e.Id && x.DeletedBy == null).ToList();
                            foreach (var s in SectionData)
                            {

                                TreeValue pvListArtificateObj = new TreeValue();
                                pvListArtificateObj.Id = s.Id;
                                pvListArtificateObj.RandomId = RandomPassword.CreateRandomPassword(6);
                                pvListArtificateObj.Text = s.SubSectionName;
                                pvListArtificateObj.Level = 5.1;

                                pvListArtificateObj.CountryId = 0;
                                pvListArtificateObj.SiteId = 0;
                                pvListArtificateObj.ZoneId = d.Id;
                                pvListArtificateObj.SectionId = e.Id;
                                pvListArtificateObj.SubSectionId = s.Id;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.Icon = "folder";
                                pvListArtificateObj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                                pvListArtificateObj.IsAdd = rights != null ? rights.IsAdd : false;
                                pvListArtificateObj.IsEdit = rights != null ? rights.IsEdit : false;
                                pvListArtificateObj.IsDelete = rights != null ? rights.IsDelete : false;
                                pvListArtificateObj.IsView = rights != null ? rights.IsView : false;
                                pvListArtificateObj.IsExport = rights != null ? rights.IsExport : false;
                                #region MyRegion
                                List<TreeValue> pvListartifactsubsectionList = new List<TreeValue>();
                                var artifactsubSectionData = _context.ProjectWorkplaceSubSectionArtifact.Where(x => x.ProjectWorkplaceSubSectionId == s.Id && x.DeletedBy == null).ToList();
                                foreach (var itemartifact in artifactsubSectionData)
                                {
                                    TreeValue pvListartifactsubsectionobj = new TreeValue();
                                    pvListartifactsubsectionobj.Id = itemartifact.Id;
                                    pvListartifactsubsectionobj.RandomId = RandomPassword.CreateRandomPassword(6);
                                    pvListartifactsubsectionobj.Text = itemartifact.ArtifactName;
                                    pvListartifactsubsectionobj.Level = 5.2;
                                    pvListartifactsubsectionobj.CountryId = 0;
                                    pvListartifactsubsectionobj.SiteId = 0;
                                    pvListartifactsubsectionobj.ZoneId = d.Id;
                                    pvListartifactsubsectionobj.SectionId = e.Id;
                                    pvListartifactsubsectionobj.SubSectionId = s.Id;
                                    pvListartifactsubsectionobj.ParentMasterId = b.ProjectId;
                                    pvListartifactsubsectionobj.SubSectionArtificateId = itemartifact.Id;
                                    pvListartifactsubsectionobj.Icon = "las la-file-alt text-blue eicon";
                                    pvListartifactsubsectionobj.WorkPlaceFolderId = (WorkPlaceFolder)c.WorkPlaceFolderId;
                                    pvListartifactsubsectionobj.IsNotRequired = itemartifact.IsNotRequired;
                                    pvListartifactsubsectionobj.IsAdd = rights != null ? rights.IsAdd : false;
                                    pvListartifactsubsectionobj.IsEdit = rights != null ? rights.IsEdit : false;
                                    pvListartifactsubsectionobj.IsDelete = rights != null ? rights.IsDelete : false;
                                    pvListartifactsubsectionobj.IsView = rights != null ? rights.IsView : false;
                                    pvListartifactsubsectionobj.IsExport = rights != null ? rights.IsExport : false;
                                    pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);
                                }
                                #endregion
                                pvListArtificateList.Add(pvListArtificateObj);
                                pvListArtificateObj.Item = pvListartifactsubsectionList;

                            }
                            #endregion
                            pvListSectionList.Add(pvListSectionObj);
                            pvListSectionObj.Item = pvListArtificateList.OrderBy(x => x.Number).ToList();
                        }
                        pvListZoneList.Add(pvListZoneObj);
                        pvListZoneObj.Item = pvListSectionList.OrderBy(x => x.Number).ToList();
                    }

                    TrialFol.Item = pvListZoneList.OrderBy(x => x.Number).ToList();
                }

                #endregion
                pvListObj.Item.Add(TrialFol);
                pvListObj.Item.Add(CountryFol);
                pvListObj.Item.Add(SiteFol);
            }

            pvList.Add(pvListObj);

            return pvList;
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
                projectPath = System.IO.Path.Combine(docPath, FolderType.ProjectWorksplace.GetDescription(), projectDetail.ProjectCode.Replace("/", ""));
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
            var FolderPath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), ProjectWorkplace.Project.ProjectCode.Replace("/", ""));
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
                projectPath = Path.Combine(docPath, FolderType.ProjectWorksplace.GetDescription(), projectDetail.ProjectCode.Replace("/", ""));
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
        public int SiteProjectId { get; set; }
        public int ZoneId { get; set; }
        public int SectionId { get; set; }
        public int ArtificateId { get; set; }
        public int SubSectionId { get; set; }
        public int SubSectionArtificateId { get; set; }
        public WorkPlaceFolder WorkPlaceFolderId { get; set; }
        public string Icon { get; set; }
        public string RandomId { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsView { get; set; }
        public bool IsDelete { get; set; }
        public bool IsExport { get; set; }
        public bool IsNotRequired { get; set; }
    }
}
