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
    public class ProjectWorkplaceArtificatedocumentRepository : GenericRespository<ProjectWorkplaceArtificatedocument, GscContext>, IProjectWorkplaceArtificatedocumentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public ProjectWorkplaceArtificatedocumentRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser, IUploadSettingRepository uploadSettingRepository)
           : base(uow, jwtTokenAccesser)
        {
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public int deleteFile(int id)
        {
            string filename = string.Empty;
            var data = (from artifactdoc in Context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == id)
                        join artifact in Context.ProjectWorkplaceArtificate on artifactdoc.ProjectWorkplaceArtificateId equals artifact.Id
                        join etmfartifact in Context.EtmfArtificateMasterLbrary on artifact.EtmfArtificateMasterLbraryId equals etmfartifact.Id
                        join section in Context.ProjectWorkplaceSection on artifact.ProjectWorkplaceSectionId equals section.Id
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
                            Artificatename = etmfartifact.ArtificateName,
                            DocumentName = artifactdoc.DocumentName,
                        }).FirstOrDefault();

            string filePath = string.Empty;
            string path = string.Empty;

            if (data.FolderType == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(data.Projectname, WorkPlaceFolder.Country.GetDescription(),
                  data.Sitename.Trim(), data.Zonename.Trim(), data.Sectionname.Trim(), data.Artificatename);
            else if (data.FolderType == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(data.Projectname, WorkPlaceFolder.Site.GetDescription(),
                data.Sitename.Trim(), data.Zonename.Trim(), data.Sectionname.Trim(), data.Artificatename);
            else if (data.FolderType == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(data.Projectname, WorkPlaceFolder.Trial.GetDescription(),
                   data.Zonename.Trim(), data.Sectionname.Trim(), data.Artificatename);
            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path, data.DocumentName);
            System.IO.File.Delete(Path.Combine(filePath));

            return id;
        }
    }
}
