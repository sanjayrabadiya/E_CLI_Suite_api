using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateLockUnlockRepository : GenericRespository<ScreeningTemplateLockUnlockAudit>, IScreeningTemplateLockUnlockRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public ScreeningTemplateLockUnlockRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public void Insert(ScreeningTemplateLockUnlockAudit screeningTemplateLockUnlock)
        {
            screeningTemplateLockUnlock.IpAddress = _jwtTokenAccesser.IpAddress;
            screeningTemplateLockUnlock.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            screeningTemplateLockUnlock.CreatedRoleBy = _jwtTokenAccesser.RoleId;
            Add(screeningTemplateLockUnlock);
        }

        public List<LockUnlockHistoryListDto> ProjectLockUnLockHistory(int projectId, int parentProjectId)
        {
            var ProjectCode = _context.Project.Find(parentProjectId).ProjectCode;

            var parent = _context.Project.Where(x => (x.Id == projectId) || (x.ParentProjectId == projectId)).Select(x => x.Id).ToList();

            var result = All.Where(y => parent.Contains(y.ProjectId)).Select(x => new LockUnlockHistoryListDto
            {
                Id = x.Id,
                ScreeningEntryId = x.ScreeningEntryId,
                VolunteerName = x.ScreeningEntry.RandomizationId != null ? x.ScreeningEntry.Randomization.Initial : x.ScreeningEntry.Attendance.Volunteer.FullName,
                VolunteerNumber = x.ScreeningEntry.RandomizationId != null ? x.ScreeningEntry.Randomization.ScreeningNumber : x.ScreeningEntry.Attendance.Volunteer.VolunteerNo,
                RandomizationNumber = x.ScreeningEntry.RandomizationId != null ? x.ScreeningEntry.Randomization.RandomizationNumber : x.ScreeningEntry.Attendance.ProjectSubject.Number,
                //AttendanceId = x.ScreeningEntry.Attendance.Id,
                ProjectDesignTemplateId = x.ScreeningTemplate.ScreeningVisit.Id,
                VisitId = x.ScreeningTemplate.ScreeningVisitId,
                // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                VisitName = x.ScreeningTemplate.ScreeningVisit.ScreeningVisitName + Convert.ToString(x.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + x.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber),
                // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                ProjectDesignTemplateName = x.ScreeningTemplate.RepeatSeqNo == null && x.ScreeningTemplate.ParentId == null ? x.ScreeningTemplate.ProjectDesignTemplate.DesignOrder + ". " + x.ScreeningTemplate.ScreeningTemplateName
                                            : x.ScreeningTemplate.ProjectDesignTemplate.DesignOrder+ "." + x.ScreeningTemplate.RepeatSeqNo + " " + x.ScreeningTemplate.ScreeningTemplateName,                
                DesignOrder = x.ScreeningTemplate.ProjectDesignTemplate.DesignOrder.ToString(),
                ScreeningTemplateParentId = x.ScreeningTemplate.ParentId,
                ScreeningTemplateId = x.ScreeningTemplateId,
                IsLocked = x.IsLocked,
                Locked = x.IsLocked ? "Locked" : "Unlocked",
                IpAddress = x.IpAddress,
                TimeZone = x.TimeZone,
                CreatedBy = x.CreatedBy,
                CreatedByName = _context.Users.Where(u => u.Id == x.CreatedBy).FirstOrDefault().UserName + " (" + _context.SecurityRole.Where(u => u.Id == x.CreatedRoleBy).FirstOrDefault().RoleShortName + ") ",
                CreatedRoleByName = _context.SecurityRole.Where(u => u.Id == x.CreatedRoleBy).FirstOrDefault().RoleShortName,
                CreatedRoleBy = x.CreatedRoleBy,
                CreatedDate = x.CreatedDate,
                AuditReasonComment = x.AuditReasonComment,
                AuditReasonId = x.AuditReasonId,
                AuditReasonName = _context.AuditReason.FirstOrDefault(y => y.Id == x.AuditReasonId).ReasonName,
                ProjectName = x.ScreeningEntry.Project.ParentProjectId != null ? x.ScreeningEntry.Project.ProjectCode : null,
                ProjectCode = ProjectCode,
                ParentProjectId = x.ScreeningEntry.Project.ParentProjectId,
                SeqNo = x.ScreeningTemplate.ProjectDesignTemplate.DesignOrder,
                DataEntryStatus = x.DataEntryStatus
            }).OrderByDescending(x => x.Id).ToList();            

            return result;
        }
    }
}
