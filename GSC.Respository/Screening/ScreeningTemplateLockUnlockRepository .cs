using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateLockUnlockRepository : GenericRespository<ScreeningTemplateLockUnlockAudit, GscContext>, IScreeningTemplateLockUnlockRepository
    {
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IScreeningTemplateValueEditCheckRepository _screeningTemplateValueEditCheckRepository;
        public ScreeningTemplateLockUnlockRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IUploadSettingRepository uploadSettingRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IScreeningTemplateValueEditCheckRepository screeningTemplateValueEditCheckRepository)
            : base(uow, jwtTokenAccesser)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateValueEditCheckRepository = screeningTemplateValueEditCheckRepository;
        }

        public void Insert(ScreeningTemplateLockUnlockAudit screeningTemplateLockUnlock)
        {
            screeningTemplateLockUnlock.IpAddress = _jwtTokenAccesser.IpAddress;
            screeningTemplateLockUnlock.TimeZone = _jwtTokenAccesser.GetHeader("timeZone");
            screeningTemplateLockUnlock.CreatedRoleBy = _jwtTokenAccesser.RoleId;
            Add(screeningTemplateLockUnlock);
        }

        public List<LockUnlockHistoryListDto> ProjectLockUnLockHistory(int projectId, int ParentProjectId)
        {
            var ProjectCode = Context.Project.Find(ParentProjectId).ProjectCode;

            var parent = Context.Project.Where(x => (x.Id == projectId) || (x.ParentProjectId == projectId)).Select(x => x.Id).ToList();

            var result = All.Where(y => parent.Contains(y.ProjectId)).Select(x => new LockUnlockHistoryListDto
            {
                Id = x.Id,
                ScreeningEntryId = x.ScreeningEntryId,
                VolunteerName = x.ScreeningEntry.Attendance.Volunteer == null ? x.ScreeningEntry.Attendance.NoneRegister.Initial : x.ScreeningEntry.Attendance.Volunteer.FullName,
                VolunteerNumber = x.ScreeningEntry.Attendance.Volunteer == null ? x.ScreeningEntry.Attendance.NoneRegister.ScreeningNumber : x.ScreeningEntry.Attendance.Volunteer.VolunteerNo,
                RandomizationNumber = x.ScreeningEntry.Attendance.Volunteer == null ? x.ScreeningEntry.Attendance.NoneRegister.RandomizationNumber : x.ScreeningEntry.Attendance.ProjectSubject.Number,
                AttendanceId = x.ScreeningEntry.Attendance.Id,
                ProjectDesignTemplateId = x.ScreeningTemplate.ScreeningVisit.Id,
                VisitId = x.ScreeningTemplate.ScreeningVisitId,
                VisitName = x.ScreeningTemplate.ScreeningVisit.ProjectDesignVisit.DisplayName + Convert.ToString(x.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + x.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber),
                ProjectDesignTemplateName = x.ScreeningTemplate.RepeatSeqNo == null && x.ScreeningTemplate.ParentId == null ? x.ScreeningTemplate.ProjectDesignTemplate.DesignOrder + " " + x.ScreeningTemplate.ProjectDesignTemplate.TemplateName
                                            : x.ScreeningTemplate.ProjectDesignTemplate.DesignOrder+ "." + x.ScreeningTemplate.RepeatSeqNo + " " + x.ScreeningTemplate.ProjectDesignTemplate.TemplateName,                
                DesignOrder = x.ScreeningTemplate.ProjectDesignTemplate.DesignOrder.ToString(),
                ScreeningTemplateParentId = x.ScreeningTemplate.ParentId,
                ScreeningTemplateId = x.ScreeningTemplateId,
                IsLocked = x.IsLocked,
                Locked = x.IsLocked ? "Locked" : "Unlocked",
                IpAddress = x.IpAddress,
                TimeZone = x.TimeZone,
                CreatedBy = x.CreatedBy,
                CreatedByName = Context.Users.Where(u => u.Id == x.CreatedBy).FirstOrDefault().UserName + " (" + Context.SecurityRole.Where(u => u.Id == x.CreatedRoleBy).FirstOrDefault().RoleShortName + ") ",
                CreatedRoleByName = Context.SecurityRole.Where(u => u.Id == x.CreatedRoleBy).FirstOrDefault().RoleShortName,
                CreatedRoleBy = x.CreatedRoleBy,
                CreatedDate = x.CreatedDate,
                AuditReasonComment = x.AuditReasonComment,
                AuditReasonId = x.AuditReasonId,
                AuditReasonName = Context.AuditReason.FirstOrDefault(y => y.Id == x.AuditReasonId).ReasonName,
                ProjectName = x.ScreeningEntry.Project.ParentProjectId != null ? x.ScreeningEntry.Project.ProjectCode : null,
                ProjectCode = ProjectCode,
                ParentProjectId = x.ScreeningEntry.Project.ParentProjectId,
                SeqNo = x.ScreeningTemplate.ProjectDesignTemplate.DesignOrder
            }).OrderByDescending(x => x.Id).ToList();            

            return result;
        }
    }
}
