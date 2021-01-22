using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Respository.PropertyMapping;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Medra
{
    public class MeddraCodingAuditRepository : GenericRespository<MeddraCodingAudit>, IMeddraCodingAuditRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public MeddraCodingAuditRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(context)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public MeddraCodingAudit SaveAudit(string note, int meddraCodingId, int? meddraLowLevelTermId, int? meddraSocTermId, string Action, int? ReasonId, string ReasonOth)
        {
            var meddraCodingAudit = new MeddraCodingAudit();
            meddraCodingAudit.UserRoleId = _jwtTokenAccesser.RoleId;
            meddraCodingAudit.MeddraCodingId = meddraCodingId;
            meddraCodingAudit.MeddraLowLevelTermId = meddraLowLevelTermId;
            meddraCodingAudit.MeddraSocTermId = meddraSocTermId;
            meddraCodingAudit.Action = Action;
            meddraCodingAudit.Note = note;
            meddraCodingAudit.ReasonId = ReasonId;
            meddraCodingAudit.ReasonOth = ReasonOth;
            meddraCodingAudit.IpAddress = _jwtTokenAccesser.IpAddress;
            meddraCodingAudit.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            meddraCodingAudit.CreatedDate = DateTime.Now;
            Add(meddraCodingAudit);
            return meddraCodingAudit;
        }


        public IList<MeddraCodingAuditDto> GetMeddraAuditDetails(int MeddraCodingId)
        {
            var result = (from MCA in _context.MeddraCodingAudit
                          join medraConding in _context.MeddraCoding on MCA.MeddraCodingId equals medraConding.Id
                          join medraConfig in _context.MedraConfig on medraConding.MeddraConfigId equals medraConfig.Id
                          join soc in _context.MeddraSocTerm on MCA.MeddraSocTermId equals soc.Id into socDto
                          from meddraSoc in socDto.DefaultIfEmpty()
                          join mllt in _context.MeddraLowLevelTerm on MCA.MeddraLowLevelTermId equals mllt.Id into mlltDto
                          from meddraLLT in mlltDto.DefaultIfEmpty()
                          join md in _context.MeddraMdHierarchy on meddraSoc.soc_code equals md.soc_code into mdDto
                          from meddraMD in mdDto.DefaultIfEmpty()
                          join reasonTemp in _context.AuditReason on MCA.ReasonId equals reasonTemp.Id into reasonDt
                          from reason in reasonDt.DefaultIfEmpty()
                          join users in _context.Users on MCA.CreatedBy equals users.Id into userDto
                          from user in userDto.DefaultIfEmpty()
                          join roles in _context.SecurityRole on MCA.UserRoleId equals roles.Id into roleDto
                          from role in roleDto.DefaultIfEmpty()
                          where MCA.MeddraCodingId == MeddraCodingId && meddraLLT.pt_code == meddraMD.pt_code
                          && meddraLLT.MedraConfigId == medraConfig.Id && meddraSoc.MedraConfigId == medraConfig.Id
                          && meddraMD.MedraConfigId == medraConfig.Id
                          select new MeddraCodingAuditDto
                          {
                              Code = meddraLLT.llt_name,
                              LLT = meddraLLT.llt_code,
                              Value = meddraLLT.llt_name,
                              CreatedDate = MCA.CreatedDate,
                              CreateUser = user.UserName + " (" + role.RoleName + ")",
                              PT = meddraMD.pt_name,
                              HLT = meddraMD.hlt_name,
                              HLGT = meddraMD.hlgt_name,
                              SOCValue = meddraMD.soc_name,
                              SocCode = meddraMD.pt_code.ToString(),
                              IpAddress = MCA.IpAddress,
                              TimeZone = MCA.TimeZone,
                              Action = MCA.Action,
                              Note = MCA.Note,
                              ReasonName = reason.ReasonName,
                              ReasonOth = MCA.ReasonOth,
                              PrimarySoc = meddraMD.primary_soc_fg,
                          }).OrderByDescending(o => o.CreatedDate).ToList();
            return result;
        }
    }
}
