using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.PropertyMapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Medra
{
    public class MeddraCodingAuditRepository : GenericRespository<MeddraCodingAudit, GscContext>, IMeddraCodingAuditRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public MeddraCodingAuditRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(uow, jwtTokenAccesser)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
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
            var result = (from MCA in Context.MeddraCodingAudit
                          join soc in Context.MeddraSocTerm on MCA.MeddraSocTermId equals soc.Id into socDto
                          from meddraSoc in socDto.DefaultIfEmpty()
                          join mllt in Context.MeddraLowLevelTerm on MCA.MeddraLowLevelTermId equals mllt.Id into mlltDto
                          from meddraLLT in mlltDto.DefaultIfEmpty()
                          join md in Context.MeddraMdHierarchy on meddraSoc.soc_code equals md.soc_code into mdDto
                          from meddraMD in mdDto.DefaultIfEmpty()
                          join reasonTemp in Context.AuditReason on MCA.ReasonId equals reasonTemp.Id into reasonDt
                          from reason in reasonDt.DefaultIfEmpty()
                          join users in Context.Users on MCA.CreatedBy equals users.Id into userDto
                          from user in userDto.DefaultIfEmpty()
                          join roles in Context.SecurityRole on MCA.UserRoleId equals roles.Id into roleDto
                          from role in roleDto.DefaultIfEmpty()
                          where MCA.MeddraCodingId == MeddraCodingId && meddraLLT.pt_code == meddraMD.pt_code
                          select new MeddraCodingAuditDto
                          {
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
                              ReasonOth = MCA.ReasonOth
                          }).OrderByDescending(o => o.CreatedDate).ToList();
            return result;
        }
    }
}
