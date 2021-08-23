using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Respository.PropertyMapping;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Barcode
{
    public class BarcodeAuditRepository : GenericRespository<BarcodeAudit>, IBarcodeAuditRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public BarcodeAuditRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(context)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public void Save(string tableName, AuditAction action, int recordId)
        {
            BarcodeAudit changes;

            int.TryParse(_jwtTokenAccesser.GetHeader("audit-reason-id"), out var reasonId);
            var reasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            changes = new BarcodeAudit
            {
                TableName = tableName,
                BarcodeId = recordId,
                Action = action.ToString(),
                AuditReasonId = action.ToString() == "Inserted" ? (int?)null : reasonId > 0 ? reasonId : (int?)null,
                Note = action.ToString() == "Inserted" ? null : reasonOth,
                UserId = _jwtTokenAccesser.UserId,
                CreatedDate = DateTime.Now,
                IpAddress = _jwtTokenAccesser.IpAddress,
                TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone")
            };

            Add(changes);
            _context.Save();
        }

        public IList<BarcodeAuditDto> GetBarcodeAuditDetails(int ProjectId)
        {
            var result = (from BA in _context.BarcodeAudit
                          join ABG in _context.AttendanceBarcodeGenerate on BA.BarcodeId equals ABG.Id into abgDt
                          from attBarcodeGenerate in abgDt.DefaultIfEmpty()
                          join ATD in _context.Attendance on attBarcodeGenerate.AttendanceId equals ATD.Id into attDt
                          from att in attDt.DefaultIfEmpty()
                          join volunteers in _context.Volunteer on att.VolunteerId equals volunteers.Id into volDt
                          from vol in volDt.DefaultIfEmpty()
                          join reasonTemp in _context.AuditReason on BA.AuditReasonId equals reasonTemp.Id into reasonDt
                          from reason in reasonDt.DefaultIfEmpty()
                          join users in _context.Users on BA.CreatedBy equals users.Id into userDto
                          from user in userDto.DefaultIfEmpty()
                          where att.ProjectId == ProjectId
                          select new BarcodeAuditDto
                          {
                              VolunteerNo = vol.VolunteerNo,
                              BarcodeString = attBarcodeGenerate.BarcodeString,
                              CreatedDate = BA.CreatedDate,
                              CreateUser = user.UserName,
                              IpAddress = BA.IpAddress,
                              TimeZone = BA.TimeZone,
                              Action = BA.Action,
                              Note = BA.Note,
                              ReasonName = reason.ReasonName
                          }).OrderByDescending(o => o.CreatedDate).ToList();
            return result;
        }
    }
}
