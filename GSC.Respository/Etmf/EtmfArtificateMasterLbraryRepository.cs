using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class EtmfArtificateMasterLbraryRepository : GenericRespository<EtmfArtificateMasterLbrary>, IEtmfArtificateMasterLbraryRepository
    {
        private readonly IGSCContext _context;
        public EtmfArtificateMasterLbraryRepository(IGSCContext context,
         IJwtTokenAccesser jwtTokenAccesser)
         : base(context)
        {
            _context = context;
        }

        public List<MasterLibraryJoinDto> GetArtifcateWithAllList()
        {
            var dtolist = (from zone in _context.EtmfZoneMasterLibrary.Where(t => t.DeletedDate == null)
                           join section in _context.EtmfSectionMasterLibrary.Where(t => t.DeletedDate == null) on zone.Id equals
                               section.EtmfZoneMasterLibraryId
                           join artificate in _context.EtmfArtificateMasterLbrary.Where(t => t.DeletedDate == null) on section.Id equals
                               artificate.EtmfSectionMasterLibraryId
                           select new MasterLibraryJoinDto
                           {
                               ZoneId = zone.Id,
                               ZoneName = zone.ZonName,
                               Zoneno = zone.ZoneNo,

                               SectionId = section.Id,
                               SectionName = section.SectionName,
                               SectionNo = section.Sectionno,

                               ArtificateId = artificate.Id,
                               ArtificateName = artificate.ArtificateName,
                               ArtificateNo = artificate.ArtificateNo,

                               TrailLevelDoc = artificate.TrailLevelDoc,
                               SiteLevelDoc = artificate.SiteLevelDoc,
                               CountryLevelDoc = artificate.CountryLevelDoc,

                           }).ToList();

            return dtolist;

        }

        public List<MasterLibraryJoinDto> GetArtifcateWithAllListByVersion(int ParentProjectId)
        {
            var result = _context.ProjectWorkplace.Include(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkPlaceZone)
                                        .ThenInclude(x => x.EtmfZoneMasterLibrary)
                                        .Where(x => x.ProjectId == ParentProjectId).FirstOrDefault();

            var dtolist = (from zone in _context.EtmfZoneMasterLibrary.Where(t => t.Version == result.ProjectWorkplaceDetail.FirstOrDefault().ProjectWorkPlaceZone.FirstOrDefault().EtmfZoneMasterLibrary.Version)
                           join section in _context.EtmfSectionMasterLibrary on zone.Id equals
                               section.EtmfZoneMasterLibraryId
                           join artificate in _context.EtmfArtificateMasterLbrary on section.Id equals
                               artificate.EtmfSectionMasterLibraryId
                           select new MasterLibraryJoinDto
                           {
                               ZoneId = zone.Id,
                               ZoneName = zone.ZonName,
                               Zoneno = zone.ZoneNo,

                               SectionId = section.Id,
                               SectionName = section.SectionName,
                               SectionNo = section.Sectionno,

                               ArtificateId = artificate.Id,
                               ArtificateName = artificate.ArtificateName,
                               ArtificateNo = artificate.ArtificateNo,

                               TrailLevelDoc = artificate.TrailLevelDoc,
                               SiteLevelDoc = artificate.SiteLevelDoc,
                               CountryLevelDoc = artificate.CountryLevelDoc,

                           }).ToList();

            return dtolist;

        }
    }
}