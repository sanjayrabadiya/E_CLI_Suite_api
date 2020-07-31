using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ETMFWorkplaceRepository : GenericRespository<ProjectWorkplace, GscContext>, IETMFWorkplaceRepository
    {
        ProjectWorkplace projectWorkplace = new ProjectWorkplace();
        ProjectWorkplaceDto projectWorkplaceDto = new ProjectWorkplaceDto();
        List<ProjectWorkplaceDetail> ProjectWorkplaceDetailList = new List<ProjectWorkplaceDetail>();
        public ETMFWorkplaceRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser)
           : base(uow, jwtTokenAccesser)
        {
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

            var projectWorkplaces = Context.ProjectWorkplace.Where(t => t.DeletedBy == null && t.ProjectId == id)
                            .Include(x => x.ProjectWorkplaceDetail)
                            .ThenInclude(x => x.ProjectWorkPlaceZone)
                            .ThenInclude(x => x.ProjectWorkplaceSection)
                            .ThenInclude(x => x.ProjectWorkplaceArtificate).AsNoTracking().ToList();

            List<TreeValue> pvList = new List<TreeValue>();
            TreeValue pvListObj = new TreeValue();
            foreach (var b in projectWorkplaces)
            {

                pvListObj.Id = b.Id;
                pvListObj.Text = Context.Project.Where(x => x.Id == b.ProjectId).FirstOrDefault().ProjectName;
                pvListObj.Level = 1;
                pvListObj.Item = new List<TreeValue>();
                pvListObj.ParentMasterId = b.ProjectId;
                pvListObj.Icon = "folder";

                TreeValue TrialFol = new TreeValue();
                TrialFol.Id = 11111111;
                TrialFol.Text = "Trial";
                TrialFol.Level = 2;
                TrialFol.Icon = "folder";

                TreeValue CountryFol = new TreeValue();
                CountryFol.Id = 11111112;
                CountryFol.Text = "Country";
                CountryFol.Level = 2;
                CountryFol.Icon = "folder";

                TreeValue SiteFol = new TreeValue();
                SiteFol.Id = 11111113;
                SiteFol.Text = "Site";
                SiteFol.Level = 2;
                SiteFol.Icon = "folder";

                CountryFol.Item = new List<TreeValue>();
                SiteFol.Item = new List<TreeValue>();
                #region Get Country
                foreach (var c in b.ProjectWorkplaceDetail.Where(x => x.WorkPlaceFolderId == (int)WorkPlaceFolder.Country && x.DeletedBy == null))
                {
                    TreeValue pvListdetaiObj = new TreeValue();
                    pvListdetaiObj.Item = new List<TreeValue>();
                    pvListdetaiObj.Id = 22222222;
                    pvListdetaiObj.Text = c.ItemName;
                    pvListdetaiObj.Level = 3;
                    pvListdetaiObj.Icon = "folder";
                    List<TreeValue> pvListZoneList = new List<TreeValue>();
                    foreach (var d in c.ProjectWorkPlaceZone.Where(x => x.DeletedBy == null))
                    {
                        d.EtmfZoneMasterLibrary = Context.EtmfZoneMasterLibrary.Find(d.EtmfZoneMasterLibraryId);

                        TreeValue pvListZoneObj = new TreeValue();
                        pvListZoneObj.Id = d.Id;
                        pvListZoneObj.Text = d.EtmfZoneMasterLibrary.ZonName;
                        pvListZoneObj.Level = 4;
                        pvListZoneObj.ParentMasterId = b.ProjectId;
                        pvListZoneObj.Icon = "folder";
                        List<TreeValue> pvListSectionList = new List<TreeValue>();
                        foreach (var e in d.ProjectWorkplaceSection.Where(x => x.DeletedBy == null))
                        {
                            e.EtmfSectionMasterLibrary = Context.EtmfSectionMasterLibrary.Find(e.EtmfSectionMasterLibraryId);

                            TreeValue pvListSectionObj = new TreeValue();
                            pvListSectionObj.Id = e.Id;
                            pvListSectionObj.Text = e.EtmfSectionMasterLibrary.SectionName;
                            pvListSectionObj.Level = 5;
                            pvListSectionObj.ZoneId = d.Id;
                            pvListSectionObj.CountryId = c.Id;
                            pvListSectionObj.ParentMasterId = b.ProjectId;
                            pvListSectionObj.Icon = "folder";
                            List<TreeValue> pvListArtificateList = new List<TreeValue>();
                            foreach (var f in e.ProjectWorkplaceArtificate.Where(x => x.DeletedBy == null))
                            {
                                f.EtmfArtificateMasterLbrary = Context.EtmfArtificateMasterLbrary.Find(f.EtmfArtificateMasterLbraryId);
                                TreeValue pvListArtificateObj = new TreeValue();
                                pvListArtificateObj.Id = f.Id;
                                pvListArtificateObj.Text = f.EtmfArtificateMasterLbrary.ArtificateName;
                                pvListArtificateObj.Level = 6;

                                pvListArtificateObj.CountryId = c.Id;
                                pvListArtificateObj.SiteId = 0;
                                pvListArtificateObj.ZoneId = d.Id;
                                pvListArtificateObj.SectionId = e.Id;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.Icon = "audio";
                                pvListArtificateList.Add(pvListArtificateObj);
                            }

                            #region Add sub section folder data
                            List<TreeValue> pvListsubsectionList = new List<TreeValue>();
                            var SectionData = Context.ProjectWorkplaceSubSection.Where(x => x.ProjectWorkplaceSectionId == e.Id && x.DeletedBy == null).ToList();
                            foreach (var s in SectionData)
                            {

                                TreeValue pvListArtificateObj = new TreeValue();
                                pvListArtificateObj.Id = s.Id;
                                pvListArtificateObj.Text = s.SubSectionName;
                                pvListArtificateObj.Level = 5.1;

                                pvListArtificateObj.CountryId = c.Id;
                                pvListArtificateObj.SiteId = 0;
                                pvListArtificateObj.ZoneId = d.Id;
                                pvListArtificateObj.SectionId = e.Id;
                                pvListArtificateObj.SubSectionId = s.Id;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.Icon = "folder";
                                List<TreeValue> pvListartifactsubsectionList = new List<TreeValue>();
                                var artifactsubSectionData = Context.ProjectWorkplaceSubSectionArtifact.Where(x => x.ProjectWorkplaceSubSectionId == s.Id && x.DeletedBy == null).ToList();
                                foreach (var itemartifact in artifactsubSectionData)
                                {
                                    TreeValue pvListartifactsubsectionobj = new TreeValue();
                                    pvListartifactsubsectionobj.Id = itemartifact.Id;
                                    pvListartifactsubsectionobj.Text = itemartifact.ArtifactName;
                                    pvListartifactsubsectionobj.Level = 5.2;
                                    pvListartifactsubsectionobj.CountryId = c.Id;
                                    pvListartifactsubsectionobj.SiteId = 0;
                                    pvListartifactsubsectionobj.ZoneId = d.Id;
                                    pvListartifactsubsectionobj.SectionId = e.Id;
                                    pvListartifactsubsectionobj.SubSectionId = s.Id;
                                    pvListartifactsubsectionobj.ParentMasterId = b.ProjectId;
                                    pvListartifactsubsectionobj.Icon = "audio";
                                    pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);
                                }
                                pvListArtificateList.Add(pvListArtificateObj);
                                pvListArtificateObj.Item = pvListartifactsubsectionList;
                            }
                            #endregion

                            pvListSectionList.Add(pvListSectionObj);
                            pvListSectionObj.Item = pvListArtificateList;
                        }
                        pvListZoneList.Add(pvListZoneObj);
                        pvListZoneObj.Item = pvListSectionList;
                        pvListdetaiObj.Item.Add(pvListZoneObj);
                    }

                    CountryFol.Item.Add(pvListdetaiObj);
                }

                #endregion

                #region Get Site
                foreach (var c in b.ProjectWorkplaceDetail.Where(x => x.WorkPlaceFolderId == (int)WorkPlaceFolder.Site && x.DeletedBy == null))
                {
                    TreeValue pvListdetaiObj = new TreeValue();
                    pvListdetaiObj.Item = new List<TreeValue>();
                    pvListdetaiObj.Id = 232323232;
                    pvListdetaiObj.Text = c.ItemName;
                    pvListdetaiObj.Level = 3;
                    pvListdetaiObj.Icon = "folder";
                    List<TreeValue> pvListZoneList = new List<TreeValue>();
                    foreach (var d in c.ProjectWorkPlaceZone.Where(x => x.DeletedBy == null))
                    {
                        d.EtmfZoneMasterLibrary = Context.EtmfZoneMasterLibrary.Find(d.EtmfZoneMasterLibraryId);

                        TreeValue pvListZoneObj = new TreeValue();
                        pvListZoneObj.Id = d.Id;
                        pvListZoneObj.Text = d.EtmfZoneMasterLibrary.ZonName;
                        pvListZoneObj.Level = 4;
                        pvListZoneObj.ParentMasterId = b.ProjectId;
                        pvListZoneObj.Icon = "folder";
                        List<TreeValue> pvListSectionList = new List<TreeValue>();
                        foreach (var e in d.ProjectWorkplaceSection.Where(x => x.DeletedBy == null))
                        {
                            e.EtmfSectionMasterLibrary = Context.EtmfSectionMasterLibrary.Find(e.EtmfSectionMasterLibraryId);

                            TreeValue pvListSectionObj = new TreeValue();
                            pvListSectionObj.Id = e.Id;
                            pvListSectionObj.Text = e.EtmfSectionMasterLibrary.SectionName;
                            pvListSectionObj.Level = 5;
                            pvListSectionObj.ZoneId = d.Id;
                            pvListSectionObj.SiteId = c.Id;
                            pvListSectionObj.ParentMasterId = b.ProjectId;
                            pvListSectionObj.Icon = "folder";
                            List<TreeValue> pvListArtificateList = new List<TreeValue>();
                            foreach (var f in e.ProjectWorkplaceArtificate.Where(x => x.DeletedBy == null))
                            {
                                f.EtmfArtificateMasterLbrary = Context.EtmfArtificateMasterLbrary.Find(f.EtmfArtificateMasterLbraryId);
                                TreeValue pvListArtificateObj = new TreeValue();
                                pvListArtificateObj.Id = f.Id;
                                pvListArtificateObj.Text = f.EtmfArtificateMasterLbrary.ArtificateName;
                                pvListArtificateObj.Level = 6;

                                pvListArtificateObj.CountryId = 0;
                                pvListArtificateObj.SiteId = c.Id;
                                pvListArtificateObj.ZoneId = d.Id;
                                pvListArtificateObj.SectionId = e.Id;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.Icon = "audio";
                                pvListArtificateList.Add(pvListArtificateObj);
                            }

                            #region Add sub section folder data
                            List<TreeValue> pvListsubsectionList = new List<TreeValue>();
                            var SectionData = Context.ProjectWorkplaceSubSection.Where(x => x.ProjectWorkplaceSectionId == e.Id && x.DeletedBy == null).ToList();
                            foreach (var s in SectionData)
                            {

                                TreeValue pvListArtificateObj = new TreeValue();
                                pvListArtificateObj.Id = s.Id;
                                pvListArtificateObj.Text = s.SubSectionName;
                                pvListArtificateObj.Level = 5.1;

                                pvListArtificateObj.CountryId = 0;
                                pvListArtificateObj.SiteId = c.Id;
                                pvListArtificateObj.ZoneId = d.Id;
                                pvListArtificateObj.SectionId = e.Id;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.Icon = "folder";
                                List<TreeValue> pvListartifactsubsectionList = new List<TreeValue>();
                                var artifactsubSectionData = Context.ProjectWorkplaceSubSectionArtifact.Where(x => x.ProjectWorkplaceSubSectionId == s.Id && x.DeletedBy == null).ToList();
                                foreach (var itemartifact in artifactsubSectionData)
                                {
                                    TreeValue pvListartifactsubsectionobj = new TreeValue();
                                    pvListartifactsubsectionobj.Id = itemartifact.Id;
                                    pvListartifactsubsectionobj.Text = itemartifact.ArtifactName;
                                    pvListartifactsubsectionobj.Level = 5.2;

                                    pvListartifactsubsectionobj.CountryId = 0;
                                    pvListartifactsubsectionobj.SiteId = c.Id;
                                    pvListartifactsubsectionobj.ZoneId = d.Id;
                                    pvListartifactsubsectionobj.SectionId = e.Id;
                                    pvListartifactsubsectionobj.SubSectionId = s.Id;
                                    pvListartifactsubsectionobj.ParentMasterId = b.ProjectId;
                                    pvListartifactsubsectionobj.Icon = "audio";
                                    pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);
                                    
                                }
                                pvListArtificateList.Add(pvListArtificateObj);
                                pvListArtificateObj.Item = pvListartifactsubsectionList;
                            }
                            #endregion
                            pvListSectionList.Add(pvListSectionObj);
                            pvListSectionObj.Item = pvListArtificateList;
                        }

                        pvListZoneList.Add(pvListZoneObj);
                        pvListZoneObj.Item = pvListSectionList;
                        pvListdetaiObj.Item.Add(pvListZoneObj);
                    }
                    SiteFol.Item.Add(pvListdetaiObj);
                }

                #endregion

                #region Get Trial
                foreach (var c in b.ProjectWorkplaceDetail.Where(x => x.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial && x.DeletedBy == null))
                {
                    TreeValue pvListdetaiObj = new TreeValue();
                    pvListdetaiObj.Id = 333333333;
                    pvListdetaiObj.Text = c.ItemName;
                    pvListdetaiObj.Level = 3;
                    pvListdetaiObj.Icon = "folder";
                    List<TreeValue> pvListZoneList = new List<TreeValue>();

                    foreach (var d in c.ProjectWorkPlaceZone.Where(x => x.DeletedBy == null))
                    {
                        d.EtmfZoneMasterLibrary = Context.EtmfZoneMasterLibrary.Find(d.EtmfZoneMasterLibraryId);

                        TreeValue pvListZoneObj = new TreeValue();
                        pvListZoneObj.Id = d.Id;
                        pvListZoneObj.Text = d.EtmfZoneMasterLibrary.ZonName;
                        pvListZoneObj.Level = 4;
                        pvListZoneObj.ParentMasterId = b.ProjectId;
                        pvListZoneObj.Icon = "folder";
                        List<TreeValue> pvListSectionList = new List<TreeValue>();
                        foreach (var e in d.ProjectWorkplaceSection.Where(x => x.DeletedBy == null))
                        {
                            e.EtmfSectionMasterLibrary = Context.EtmfSectionMasterLibrary.Find(e.EtmfSectionMasterLibraryId);

                            TreeValue pvListSectionObj = new TreeValue();
                            pvListSectionObj.Id = e.Id;
                            pvListSectionObj.Text = e.EtmfSectionMasterLibrary.SectionName;
                            pvListSectionObj.Level = 5;
                            pvListSectionObj.ZoneId = d.Id;
                            pvListSectionObj.ParentMasterId = b.ProjectId;
                            pvListSectionObj.Icon = "folder";
                            List<TreeValue> pvListArtificateList = new List<TreeValue>();
                            foreach (var f in e.ProjectWorkplaceArtificate.Where(x => x.DeletedBy == null))
                            {
                                f.EtmfArtificateMasterLbrary = Context.EtmfArtificateMasterLbrary.Find(f.EtmfArtificateMasterLbraryId);
                                TreeValue pvListArtificateObj = new TreeValue();
                                pvListArtificateObj.Id = f.Id;
                                pvListArtificateObj.Text = f.EtmfArtificateMasterLbrary.ArtificateName;
                                pvListArtificateObj.Level = 6;

                                pvListArtificateObj.CountryId = 0;
                                pvListArtificateObj.SiteId = 0;
                                pvListArtificateObj.ZoneId = d.Id;
                                pvListArtificateObj.SectionId = e.Id;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.Icon = "audio";
                                pvListArtificateList.Add(pvListArtificateObj);
                            }

                            #region Add sub section folder data
                            List<TreeValue> pvListsubsectionList = new List<TreeValue>();
                            var SectionData = Context.ProjectWorkplaceSubSection.Where(x => x.ProjectWorkplaceSectionId == e.Id && x.DeletedBy == null).ToList();
                            foreach (var s in SectionData)
                            {

                                TreeValue pvListArtificateObj = new TreeValue();
                                pvListArtificateObj.Id = s.Id;
                                pvListArtificateObj.Text = s.SubSectionName;
                                pvListArtificateObj.Level = 5.1;

                                pvListArtificateObj.CountryId = 0;
                                pvListArtificateObj.SiteId = 0;
                                pvListArtificateObj.ZoneId = d.Id;
                                pvListArtificateObj.SectionId = e.Id;
                                pvListArtificateObj.ParentMasterId = b.ProjectId;
                                pvListArtificateObj.Icon = "folder";
                                #region MyRegion
                                List<TreeValue> pvListartifactsubsectionList = new List<TreeValue>();
                                var artifactsubSectionData = Context.ProjectWorkplaceSubSectionArtifact.Where(x => x.ProjectWorkplaceSubSectionId == s.Id && x.DeletedBy == null).ToList();
                                foreach (var itemartifact in artifactsubSectionData)
                                {
                                    TreeValue pvListartifactsubsectionobj = new TreeValue();
                                    pvListartifactsubsectionobj.Id = itemartifact.Id;
                                    pvListartifactsubsectionobj.Text = itemartifact.ArtifactName;
                                    pvListartifactsubsectionobj.Level = 5.2;
                                    pvListartifactsubsectionobj.CountryId = 0;
                                    pvListartifactsubsectionobj.SiteId = 0;
                                    pvListartifactsubsectionobj.ZoneId = d.Id;
                                    pvListartifactsubsectionobj.SectionId = e.Id;
                                    pvListartifactsubsectionobj.SubSectionId = s.Id;
                                    pvListartifactsubsectionobj.ParentMasterId = b.ProjectId;
                                    pvListartifactsubsectionobj.Icon = "audio";
                                    pvListartifactsubsectionList.Add(pvListartifactsubsectionobj);
                                }
                                #endregion
                                pvListArtificateList.Add(pvListArtificateObj);
                                pvListArtificateObj.Item = pvListartifactsubsectionList;

                            }
                            #endregion
                            pvListSectionList.Add(pvListSectionObj);
                            pvListSectionObj.Item = pvListArtificateList;
                        }
                        pvListZoneList.Add(pvListZoneObj);
                        pvListZoneObj.Item = pvListSectionList;
                    }

                    TrialFol.Item = pvListZoneList;
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
                projectPath = System.IO.Path.Combine(docPath, FolderType.ProjectWorksplace.GetDescription(), projectDetail.ProjectName + "-" + projectDetail.ProjectCode);
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
    }
    public class TreeValue
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<TreeValue> Item { get; set; }
        public double Level { get; set; }
        public int ParentMasterId { get; set; }
        public int CountryId { get; set; }
        public int SiteId { get; set; }
        public int ZoneId { get; set; }
        public int SectionId { get; set; }
        public int SubSectionId { get; set; }

        public string Icon { get; set; }
    }
}
