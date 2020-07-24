using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceSubSecArtificatedocumentRepository : GenericRespository<ProjectWorkplaceSubSecArtificatedocument, GscContext>, IProjectWorkplaceSubSecArtificatedocumentRepository
    {

        private readonly IUploadSettingRepository _uploadSettingRepository;
        public ProjectWorkplaceSubSecArtificatedocumentRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser, IUploadSettingRepository uploadSettingRepository)
           : base(uow, jwtTokenAccesser)
        {
            _uploadSettingRepository = uploadSettingRepository;
        }

        public string getArtifactSectionDetail(ProjectWorkplaceSubSecArtificatedocumentDto projectWorkplaceSubSectionDto)
        {
            var data = (from artifact in Context.ProjectWorkplaceSubSectionArtifact.Where(x => x.Id == projectWorkplaceSubSectionDto.ProjectWorkplaceSubSectionArtifactId)
                        join subsection in Context.ProjectWorkplaceSubSection on artifact.ProjectWorkplaceSubSectionId equals subsection.Id
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
                            SubSectionName = subsection.SubSectionName,
                            SubSectionArtifactName = artifact.ArtifactName
                        }).FirstOrDefault();

            string filePath = string.Empty;
            string path = string.Empty;

            if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Country.GetDescription(),
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), data.SubSectionArtifactName.Trim());
            else if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Site.GetDescription(),
                data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), data.SubSectionArtifactName.Trim());
            else if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Trial.GetDescription(),
                   data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), data.SubSectionArtifactName.Trim());
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path);
            bool projectPathExists = Directory.Exists(filePath);
            if (!projectPathExists)
                System.IO.Directory.CreateDirectory(Path.Combine(filePath));

            return path;
        }

        public int deleteSubsectionArtifactfile(int id)
        {
            var data = (from artifactdoc in Context.ProjectWorkplaceSubSecArtificatedocument.Where(x => x.Id == id)
                        join artifact in Context.ProjectWorkplaceSubSectionArtifact on artifactdoc.ProjectWorkplaceSubSectionArtifactId equals artifact.Id
                        join subsection in Context.ProjectWorkplaceSubSection on artifact.ProjectWorkplaceSubSectionId equals subsection.Id
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
                        select new ProjectWorkplaceSubSecArtificatedocumentDto
                        {
                            Sectionname = etmfsection.SectionName,

                            Zonename = etmfZone.ZonName,
                            FolderType = workdetail.WorkPlaceFolderId,
                            Sitename = workdetail.WorkPlaceFolderId == 1 ? country.CountryName :
                                        workdetail.WorkPlaceFolderId == 2 ? site.ProjectCode + " - " + site.ProjectName : null,
                            Projectname = project.ProjectName + "-" + project.ProjectCode,
                            SubsectionName = subsection.SubSectionName,
                            Artificatename = artifact.ArtifactName,
                            DocumentName = artifactdoc.DocumentName

                        }).FirstOrDefault();

            string filePath = string.Empty;
            string path = string.Empty;

            if (data.FolderType == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(data.Projectname, WorkPlaceFolder.Country.GetDescription(),
                  data.Sitename.Trim(), data.Zonename.Trim(), data.Sectionname.Trim(), data.SubsectionName.Trim(), data.Artificatename.Trim());
            else if (data.FolderType == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(data.Projectname, WorkPlaceFolder.Site.GetDescription(),
                data.Sitename.Trim(), data.Zonename.Trim(), data.Sectionname.Trim(), data.SubsectionName.Trim(), data.Artificatename.Trim());
            else if (data.FolderType == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(data.Projectname, WorkPlaceFolder.Trial.GetDescription(),
                   data.Zonename.Trim(), data.Sectionname.Trim(), data.SubsectionName.Trim(), data.Artificatename.Trim());
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path, data.DocumentName);

            System.IO.File.Delete(Path.Combine(filePath));

            return id;
        }
    }
}
