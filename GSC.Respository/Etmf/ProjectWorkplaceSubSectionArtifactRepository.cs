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
    public class ProjectWorkplaceSubSectionArtifactRepository : GenericRespository<ProjectWorkplaceSubSectionArtifact, GscContext>, IProjectWorkplaceSubSectionArtifactRepository
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public ProjectWorkplaceSubSectionArtifactRepository(IUnitOfWork<GscContext> uow, IUploadSettingRepository uploadSettingRepository,
          IJwtTokenAccesser jwtTokenAccesser)
          : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uploadSettingRepository = uploadSettingRepository;
        }


        public string Duplicate(ProjectWorkplaceSubSectionArtifact objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ArtifactName == objSave.ArtifactName && x.DeletedDate == null))
                return "Duplicate Artifact Name: " + objSave.ArtifactName;
            return "";
        }

        public ProjectWorkplaceSubSectionDto getSectionDetail(ProjectWorkplaceSubSectionArtifactDto projectWorkplaceSubSectionDto)
        {
            var data = (from subsection in Context.ProjectWorkplaceSubSection.Where(x => x.Id == projectWorkplaceSubSectionDto.ProjectWorkplaceSubSectionId)
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
                            SubSectionName = subsection.SubSectionName,
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
                  data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), projectWorkplaceSubSectionDto.ArtifactName.Trim());
            else if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Site.GetDescription(),
                data.ChildName.Trim(), data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), projectWorkplaceSubSectionDto.ArtifactName.Trim());
            else if (data.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(data.ProjectName, WorkPlaceFolder.Trial.GetDescription(),
                   data.ZonName.Trim(), data.SectionName.Trim(), data.SubSectionName.Trim(), projectWorkplaceSubSectionDto.ArtifactName.Trim());
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path);
            bool projectPathExists = Directory.Exists(filePath);
            if (!projectPathExists)
                System.IO.Directory.CreateDirectory(Path.Combine(filePath));

            return data;
        }

    }
}
