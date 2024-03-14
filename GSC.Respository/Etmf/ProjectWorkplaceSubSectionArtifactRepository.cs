using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
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
    public class ProjectWorkplaceSubSectionArtifactRepository : GenericRespository<EtmfProjectWorkPlace>, IProjectWorkplaceSubSectionArtifactRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectWorkplaceSubSecArtificatedocumentRepository _projectWorkplaceSubSecArtificatedocumentRepository;
        public ProjectWorkplaceSubSectionArtifactRepository(IGSCContext context, IUploadSettingRepository uploadSettingRepository,
          IJwtTokenAccesser jwtTokenAccesser, IProjectWorkplaceSubSecArtificatedocumentRepository projectWorkplaceSubSecArtificatedocumentRepository)
          : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _uploadSettingRepository = uploadSettingRepository;
            _projectWorkplaceSubSecArtificatedocumentRepository = projectWorkplaceSubSecArtificatedocumentRepository;
        }


        public string Duplicate(EtmfProjectWorkPlace objSave)
        {
            if (All.Where(x => x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSectionArtifact).Include(x => x.ProjectWorkPlace).Any(x => x.Id != objSave.Id && x.ArtifactName == objSave.ArtifactName.Trim() && x.DeletedDate == null && x.ProjectWorkPlace.DeletedDate == null && x.EtmfProjectWorkPlaceId == objSave.EtmfProjectWorkPlaceId))
                return "Duplicate Artifact Name: " + objSave.ArtifactName;
            return "";
        }

        public EtmfProjectWorkPlaceDto getSectionDetail(EtmfProjectWorkPlaceDto projectWorkplaceSubSectionDto)
        {
            var data = (from subsection in _context.EtmfProjectWorkPlace.Where(x => x.Id == projectWorkplaceSubSectionDto.ProjectWorkplaceSubSectionId)
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
                            SubSectionName = subsection.SubSectionName,
                            ProjectWorkplaceSectionId = section.Id,
                            ProjectWorkplaceZoneId = workzone.Id,
                            ZonName = etmfZone.ZonName,
                            ProjectId = workdetail.ProjectId,
                            WorkPlaceFolderId = workdetail.WorkPlaceFolderId,
                            ChildName = workdetail.WorkPlaceFolderId == 1 ? country.CountryName :
                                        workdetail.WorkPlaceFolderId == 2 ? site.ProjectCode + " - " + site.ProjectName : null,
                            ProjectName = project.ProjectCode.Replace("/", "")

                        }).FirstOrDefault();

            string filePath = string.Empty;
            string path = string.Empty;

            if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Country.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), projectWorkplaceSubSectionDto.ArtifactName.Trim());
            else if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Site.GetDescription(),
                data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), projectWorkplaceSubSectionDto.ArtifactName.Trim());
            else if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Trial.GetDescription(),
                   data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), projectWorkplaceSubSectionDto.ArtifactName.Trim());
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), path);
            bool projectPathExists = Directory.Exists(filePath);
            if (!projectPathExists)
                System.IO.Directory.CreateDirectory(Path.Combine(filePath));

            return data;
        }

        public EtmfProjectWorkPlaceDto UpdateArtifactDetail(EtmfProjectWorkPlaceDto projectWorkplaceSubSectionDto)
        {
            var data = (from artifact in _context.EtmfProjectWorkPlace.Where(x => x.Id == projectWorkplaceSubSectionDto.Id)
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
                            SubSectionName = subsection.SubSectionName,
                            ProjectWorkplaceZoneId = workzone.Id,
                            ZonName = etmfZone.ZonName,
                            WorkPlaceFolderId = workdetail.WorkPlaceFolderId,
                            ChildName = workdetail.WorkPlaceFolderId == 1 ? country.CountryName :
                                        workdetail.WorkPlaceFolderId == 2 ? site.ProjectCode + " - " + site.ProjectName : null,
                            ProjectName = project.ProjectCode.Replace("/", ""),
                            ArtifactName = artifact.ArtifactName

                        }).FirstOrDefault();

            string filePath = string.Empty;
            string path = string.Empty;

            string OldfilePath = string.Empty;
            string Oldpath = string.Empty;

            if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Country)
            {
                Oldpath = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Country.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), data.ArtifactName.Trim());
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Country.GetDescription(),
                     data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), projectWorkplaceSubSectionDto.ArtifactName.Trim());
            }
            else if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
            {
                Oldpath = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Site.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), data.ArtifactName.Trim());
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Site.GetDescription(),
                 data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), projectWorkplaceSubSectionDto.ArtifactName.Trim());
            }
            else if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial)
            {
                Oldpath = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Trial.GetDescription(),
                 data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), data.ArtifactName.Trim());
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Trial.GetDescription(),
                    data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), projectWorkplaceSubSectionDto.ArtifactName.Trim());
            }
            OldfilePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), Oldpath);
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), path);
            bool projectPathExists = Directory.Exists(OldfilePath);
            if (projectPathExists)
                System.IO.Directory.Move(OldfilePath, filePath);


            #region Update document Detail
            var detailsArtifact = _projectWorkplaceSubSecArtificatedocumentRepository.FindBy(x => x.ProjectWorkplaceSubSectionArtifactId == projectWorkplaceSubSectionDto.Id).ToList();
            detailsArtifact.ForEach(b =>
            {
                b.DocPath = path;
                _projectWorkplaceSubSecArtificatedocumentRepository.Update(b);
                _context.Save();
            });

            #endregion
            return data;
        }

        public string DeletArtifactDetailFolder(int id)
        {
            var data = (from artifact in _context.EtmfProjectWorkPlace.Where(x => x.Id == id)
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
                            SubSectionName = subsection.SubSectionName,

                            ProjectWorkplaceZoneId = workzone.Id,
                            ZonName = etmfZone.ZonName,
                            WorkPlaceFolderId = workdetail.WorkPlaceFolderId,
                            ChildName = workdetail.WorkPlaceFolderId == 1 ? country.CountryName :
                                        workdetail.WorkPlaceFolderId == 2 ? site.ProjectCode + " - " + site.ProjectName : null,
                            ArtifactName = artifact.ArtifactName

                        }).FirstOrDefault();


            string OldfilePath = string.Empty;
            string Oldpath = string.Empty;

            if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Country)
            {
                Oldpath = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Country.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), data.ArtifactName.Trim());

            }
            else if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
            {
                Oldpath = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Site.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), data.ArtifactName.Trim());

            }
            else if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial)
            {
                Oldpath = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Trial.GetDescription(),
                data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), data.ArtifactName.Trim());

            }
            OldfilePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), Oldpath);
            bool projectPathExists = Directory.Exists(OldfilePath);
            if (projectPathExists)
                System.IO.Directory.Delete(OldfilePath, true);
            return "Success";
        }

        public List<DropDownDto> GetDrodDown(int subsectionId)
        {
            return All.Where(x =>
                    x.EtmfProjectWorkPlaceId == subsectionId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.ArtifactName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

    }
}
