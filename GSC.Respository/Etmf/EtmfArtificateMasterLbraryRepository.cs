using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared.Generic;
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
            var dtolist = (from zone in _context.EtmfMasterLibrary.Where(t => t.DeletedDate == null)
                           join section in _context.EtmfMasterLibrary.Where(t => t.DeletedDate == null) on zone.Id equals
                               section.EtmfMasterLibraryId
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

                               Version = zone.Version
                           }).ToList();

            return dtolist;

        }

        public List<MasterLibraryJoinDto> GetArtifcateWithAllListByVersion(int ParentProjectId)
        {
            var result = _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == ParentProjectId && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceZone)
                .Include(x => x.EtmfMasterLibrary).FirstOrDefault();

            var dtolist = (from zone in _context.EtmfMasterLibrary.Where(t => t.Version == result.EtmfMasterLibrary.Version)
                           join section in _context.EtmfMasterLibrary on zone.Id equals
                               section.EtmfMasterLibraryId
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

        public List<DropDownDto> GetArtificateDropDown(int EtmfSectionMasterLibraryId, int foldertype)
        {
            return All.Where(x => x.EtmfSectionMasterLibraryId == EtmfSectionMasterLibraryId && (foldertype == 1 ? x.CountryLevelDoc : (foldertype == 2 ? x.SiteLevelDoc : x.TrailLevelDoc)))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.ArtificateName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}