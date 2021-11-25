using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class SyncConfigurationMasterRepository : GenericRespository<SyncConfigurationMaster>, ISyncConfigurationMasterRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;

        public SyncConfigurationMasterRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
           : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<SyncConfigurationMasterGridDto> GetSyncConfigurationMastersList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).
                    ProjectTo<SyncConfigurationMasterGridDto>(_mapper.ConfigurationProvider).ToList();
        }

        public string Duplicate(SyncConfigurationMaster objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.DeletedDate == null && x.ReportScreenId == objSave.ReportScreenId))
            {
                return "Duplicate Report";
            }
            return "";
        }
        public List<SyncConfigurationAuditDto> GetAudit()
        {
            var result = _context.SyncConfigurationMasterDetailsAudit.Select(x => new SyncConfigurationAuditDto
            {
                Key = x.SyncConfigurationMasterDetails.SyncConfigurationMaster.Id,
                ReportName = x.ReportScreen.ReportName,
                Version = x.Version,
                ZonName = x.ZoneMasterLibraryId == null ? "" : x.EtmfZoneMasterLibrary.ZonName,
                SectionName = x.SectionMasterLibraryId == null ? "" : x.EtmfSectionMasterLibrary.SectionName,
                ArtificateName = x.ArtificateMasterLbraryId == null ? "" : x.EtmfArtificateMasterLbrary.ArtificateName,
                ReasonName = x.ReasonId == null ? "" : x.AuditReason.ReasonName,
                Notes = x.Note,
                IpAddress = x.IpAddress,
                TimeZone = x.TimeZone,
                Activity = x.Activity,
                ActivityBy = x.CreatedByUser.UserName,
                ActivityDate = x.CreatedDate
            }).ToList();
            return result;
        }
    }
}
