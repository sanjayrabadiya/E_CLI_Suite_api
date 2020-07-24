using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceSubSectionRepository : GenericRespository<ProjectWorkplaceSubSection, GscContext>, IProjectWorkplaceSubSectionRepository
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public ProjectWorkplaceSubSectionRepository(IUnitOfWork<GscContext> uow, IUploadSettingRepository uploadSettingRepository,
          IJwtTokenAccesser jwtTokenAccesser)
          : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public string Duplicate(ProjectWorkplaceSubSection objSave)
        {
            if (All.Any(x => x.ProjectWorkplaceSectionId == objSave.ProjectWorkplaceSectionId && x.Id != objSave.Id && x.SubSectionName == objSave.SubSectionName && x.DeletedDate == null))
                return "Duplicate Sub Section name : " + objSave.SubSectionName;
            return "";
        }

        public ProjectWorkplaceSubSectionDto getSectionDetail(ProjectWorkplaceSubSectionDto projectWorkplaceSubSectionDto)
        {
            var data = (from section in Context.ProjectWorkplaceSection.Where(x => x.Id == projectWorkplaceSubSectionDto.ProjectWorkplaceSectionId)
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
                        select new ProjectWorkplaceSubSectionDto
                        {
                            SectionName = etmfsection.SectionName,
                            ProjectWorkplaceSectionId = section.Id,
                            ProjectWorkplaceZoneId = workzone.Id,
                            ZonName = etmfZone.ZonName,
                            WorkPlaceFolderId = workdetail.WorkPlaceFolderId,
                            ChildName = workdetail.WorkPlaceFolderId == 1 ? country.CountryName :
                                        workdetail.WorkPlaceFolderId == 2 ? site.ProjectCode + " - " + site.ProjectName : null,
                            ProjectName = project.ProjectName + "-" + project.ProjectCode

                        }).FirstOrDefault();

            string filePath = string.Empty;
            string path = string.Empty;

            if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Country.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), projectWorkplaceSubSectionDto.SubSectionName.Trim());
            else if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Site.GetDescription(),
                data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), projectWorkplaceSubSectionDto.SubSectionName.Trim());
            else if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Trial.GetDescription(),
                   data.ZonName.Trim(), data.SectionName.Trim(), projectWorkplaceSubSectionDto.SubSectionName.Trim());
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path);
            bool projectPathExists = Directory.Exists(filePath);
            if (!projectPathExists)
                System.IO.Directory.CreateDirectory(Path.Combine(filePath));

            return data;
        }

        public ProjectWorkplaceSubSectionDto updateSectionDetailFolder(ProjectWorkplaceSubSectionDto projectWorkplaceSubSectionDto)
        {
            var data = (from subsection in Context.ProjectWorkplaceSubSection.Where(x => x.Id == projectWorkplaceSubSectionDto.Id)
                        join section in Context.ProjectWorkplaceSection on subsection.ProjectWorkplaceSectionId equals section.Id
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
                        select new ProjectWorkplaceSubSectionDto
                        {
                            SectionName = etmfsection.SectionName,
                            ProjectWorkplaceSectionId = section.Id,
                            ProjectWorkplaceZoneId = workzone.Id,
                            ZonName = etmfZone.ZonName,
                            WorkPlaceFolderId = workdetail.WorkPlaceFolderId,
                            ChildName = workdetail.WorkPlaceFolderId == 1 ? country.CountryName :
                                        workdetail.WorkPlaceFolderId == 2 ? site.ProjectCode + " - " + site.ProjectName : null,
                            ProjectName = project.ProjectName + "-" + project.ProjectCode,
                            SubSectionName = subsection.SubSectionName

                        }).FirstOrDefault();

            string filePath = string.Empty;
            string OldfilePath = string.Empty;
            string path = string.Empty;
            string Oldpath = string.Empty;
            if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Country)
            {
                Oldpath = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Country.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName);
                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Country.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), projectWorkplaceSubSectionDto.SubSectionName.Trim());
            }
            else if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
            {
                Oldpath = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Site.GetDescription(),
                data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName);
                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Site.GetDescription(),
                 data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), projectWorkplaceSubSectionDto.SubSectionName.Trim());
            }

            else if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial)
            {
                Oldpath = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Trial.GetDescription(),
                data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName);
                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Trial.GetDescription(),
                   data.ZonName.Trim(), data.SectionName.Trim(), projectWorkplaceSubSectionDto.SubSectionName.Trim());
            }
            OldfilePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), Oldpath);
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path);

            bool projectPathExists = Directory.Exists(OldfilePath);
            if (projectPathExists)
                System.IO.Directory.Move(OldfilePath, filePath);

            return data;
        }

        public string DeletSectionDetailFolder(int id)
        {
            var data = (from subsection in Context.ProjectWorkplaceSubSection.Where(x => x.Id == id)
                        join section in Context.ProjectWorkplaceSection on subsection.ProjectWorkplaceSectionId equals section.Id
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
                        select new ProjectWorkplaceSubSectionDto
                        {
                            SectionName = etmfsection.SectionName,
                            ProjectWorkplaceSectionId = section.Id,
                            ProjectWorkplaceZoneId = workzone.Id,
                            ZonName = etmfZone.ZonName,
                            WorkPlaceFolderId = workdetail.WorkPlaceFolderId,
                            ChildName = workdetail.WorkPlaceFolderId == 1 ? country.CountryName :
                                        workdetail.WorkPlaceFolderId == 2 ? site.ProjectCode + " - " + site.ProjectName : null,
                            ProjectName = project.ProjectName + "-" + project.ProjectCode,
                            SubSectionName = subsection.SubSectionName

                        }).FirstOrDefault();

            string filePath = string.Empty;
            string OldfilePath = string.Empty;
            string path = string.Empty;
            string Oldpath = string.Empty;
            if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Country)
            {
                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Country.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim());
            }
            else if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
            {
                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Site.GetDescription(),
                 data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim());
            }

            else if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial)
            {
                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Trial.GetDescription(),
                   data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim());
            }
             
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path);

            bool projectPathExists = Directory.Exists(filePath);
            if (projectPathExists)
                System.IO.Directory.Delete(filePath);
             
            return "success";
        }

        public List<DropDownDto> GetDrodDown(int SectionId)
        {
            return All.Where(x =>
                    x.ProjectWorkplaceSectionId == SectionId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.SubSectionName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}
