using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceSubSectionRepository : GenericRespository<EtmfProjectWorkPlace>, IProjectWorkplaceSubSectionRepository
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public ProjectWorkplaceSubSectionRepository(IGSCContext context, IUploadSettingRepository uploadSettingRepository,
          IJwtTokenAccesser jwtTokenAccesser)
          : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public string Duplicate(EtmfProjectWorkPlace objSave)
        {
            if (All.Any(x => x.EtmfProjectWorkPlaceId == objSave.EtmfProjectWorkPlaceId && x.Id != objSave.Id && x.SubSectionName == objSave.SubSectionName.Trim() && x.DeletedDate == null))
                return "Duplicate Sub Section name : " + objSave.SubSectionName;
            return "";
        }

        public EtmfProjectWorkPlaceDto getSectionDetail(EtmfProjectWorkPlaceDto projectWorkplaceSubSectionDto)
        {
            var data = (from section in _context.EtmfProjectWorkPlace.Where(x => x.Id == projectWorkplaceSubSectionDto.ProjectWorkplaceSectionId)
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
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), projectWorkplaceSubSectionDto.SubSectionName.Trim());
            else if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Site.GetDescription(),
                data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), projectWorkplaceSubSectionDto.SubSectionName.Trim());
            else if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Trial.GetDescription(),
                   data.ZonName.Trim(), data.SectionName.Trim(), projectWorkplaceSubSectionDto.SubSectionName.Trim());
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), path);
            bool projectPathExists = Directory.Exists(filePath);
            if (!projectPathExists)
                System.IO.Directory.CreateDirectory(Path.Combine(filePath));

            return data;
        }

        public EtmfProjectWorkPlaceDto updateSectionDetailFolder(EtmfProjectWorkPlaceDto projectWorkplaceSubSectionDto)
        {
            var data = (from subsection in _context.EtmfProjectWorkPlace.Where(x => x.Id == projectWorkplaceSubSectionDto.Id)
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
                            SubSectionName = subsection.SubSectionName

                        }).FirstOrDefault();

            string filePath = string.Empty;
            string OldfilePath = string.Empty;
            string path = string.Empty;
            string Oldpath = string.Empty;
            if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Country)
            {
                Oldpath = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Country.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName);
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Country.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), projectWorkplaceSubSectionDto.SubSectionName.Trim());
            }
            else if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
            {
                Oldpath = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Site.GetDescription(),
                data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName);
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Site.GetDescription(),
                 data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), projectWorkplaceSubSectionDto.SubSectionName.Trim());
            }

            else if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial)
            {
                Oldpath = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Trial.GetDescription(),
                data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName);
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Trial.GetDescription(),
                   data.ZonName.Trim(), data.SectionName.Trim(), projectWorkplaceSubSectionDto.SubSectionName.Trim());
            }
            OldfilePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), Oldpath);
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), path);

            bool projectPathExists = Directory.Exists(OldfilePath);
            if (projectPathExists)
                System.IO.Directory.Move(OldfilePath, filePath);

            return data;
        }

        public string DeletSectionDetailFolder(int id)
        {
            var data = (from subsection in _context.EtmfProjectWorkPlace.Where(x => x.Id == id)
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
                            SubSectionName = subsection.SubSectionName

                        }).FirstOrDefault();

            string filePath = string.Empty;
            string path = string.Empty;
            if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Country)
            {
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Country.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim());
            }
            else if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
            {
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Site.GetDescription(),
                 data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim());
            }

            else if (data?.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial)
            {
                path = System.IO.Path.Combine(data.ProjectName, FolderType.Etmf.GetDescription(), WorkPlaceFolder.Trial.GetDescription(),
                   data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim());
            }

            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), path);

            bool projectPathExists = Directory.Exists(filePath);
            if (projectPathExists)
                System.IO.Directory.Delete(filePath, true);

            return "success";
        }

        public List<DropDownDto> GetDrodDown(int zoneId)
        {
            return All.Where(x =>
                    x.EtmfProjectWorkPlaceId == zoneId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.SubSectionName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}
