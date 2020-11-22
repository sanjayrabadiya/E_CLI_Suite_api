using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class EtmfArtificateMasterLbraryRepository : GenericRespository<EtmfArtificateMasterLbrary, GscContext>, IEtmfArtificateMasterLbraryRepository
    {
        public EtmfArtificateMasterLbraryRepository(IUnitOfWork<GscContext> uow,
         IJwtTokenAccesser jwtTokenAccesser)
         : base(uow, jwtTokenAccesser)
        {
        }

        public List<MasterLibraryJoinDto> GetArtifcateWithAllList()
        {
            var dtolist = (from zone in Context.EtmfZoneMasterLibrary.Where(t => t.DeletedDate == null)
                                 join section in Context.EtmfSectionMasterLibrary.Where(t => t.DeletedDate == null) on zone.Id equals
                                     section.EtmfZoneMasterLibraryId
                                 join artificate in Context.EtmfArtificateMasterLbrary.Where(t => t.DeletedDate == null) on section.Id equals
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