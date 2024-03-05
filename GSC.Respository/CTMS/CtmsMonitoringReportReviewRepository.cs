using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringReportReviewRepository : GenericRespository<CtmsMonitoringReportReview>, ICtmsMonitoringReportReviewRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IProjectRepository _projectRepository;

        public CtmsMonitoringReportReviewRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IProjectRightRepository projectRightRepository,
            IUserRepository userRepository, IEmailSenderRespository emailSenderRespository,
            IProjectRepository projectRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _projectRightRepository = projectRightRepository;
            _userRepository = userRepository;
            _emailSenderRespository = emailSenderRespository;
            _projectRepository = projectRepository;
        }

        public List<CtmsMonitoringReportReviewDto> UserRoles(int Id, int ProjectId)
        {
            //add by mitul on 30-11-2023 -> CTMS UserAccess wise user get
            var users = _context.UserAccess.Include(x => x.UserRole).ThenInclude(x => x.User).Where(s => s.ProjectId == ProjectId && s.DeletedDate == null && s.UserRole.UserId != _jwtTokenAccesser.UserId)
            .OrderByDescending(s => s.Id).Select(c => new CtmsMonitoringReportReviewDto
            {
                UserId = c.UserRole.UserId,
                Name = c.UserRole.User.UserName,
                IsSelected = All.Any(b => b.CtmsMonitoringReportId == Id && b.UserId == c.UserRole.UserId && b.DeletedDate == null && b.IsSendBack == false),
            }).ToList();

            return users;
        }

        public void SaveTemplateReview(List<CtmsMonitoringReportReviewDto> ctmsMonitoringReportReviewDtos)
        {
            foreach (var ReviewDto in ctmsMonitoringReportReviewDtos)
                if (ReviewDto.IsSelected)
                {
                    Add(new CtmsMonitoringReportReview
                    {
                        CtmsMonitoringReportId = ReviewDto.CtmsMonitoringReportId,
                        UserId = ReviewDto.UserId,
                        IsSendBack = false,
                        Message = ReviewDto.Message,
                    });
                    if (_context.Save() < 0) throw new Exception("Review Send failed on save.");

                    SendMailToReviewer(ReviewDto);
                }
        }

        // Send mail for review
        public void SendMailToReviewer(CtmsMonitoringReportReviewDto ReviewDto)
        {
            var Review = All.Include(x => x.CtmsMonitoringReport).ThenInclude(x => x.CtmsMonitoring).ThenInclude(x => x.Project)
                .Include(x => x.CtmsMonitoringReport).ThenInclude(x => x.CtmsMonitoring).ThenInclude(x => x.StudyLevelForm).ThenInclude(x => x.VariableTemplate)
                .Include(x => x.CtmsMonitoringReport).ThenInclude(x => x.CtmsMonitoring).ThenInclude(x => x.StudyLevelForm).ThenInclude(x => x.Activity)
                .Include(x => x.CtmsMonitoringReport).ThenInclude(x => x.CtmsMonitoring).ThenInclude(x => x.StudyLevelForm).ThenInclude(x => x.Activity).ThenInclude(x => x.CtmsActivity)
                .Where(x => x.CtmsMonitoringReportId == ReviewDto.CtmsMonitoringReportId)
                .FirstOrDefault();

            var ProjectName = Review.CtmsMonitoringReport.CtmsMonitoring.Project.ProjectName;
            var Activity = Review.CtmsMonitoringReport.CtmsMonitoring.StudyLevelForm.Activity.CtmsActivity.ActivityName;
            var Template = Review.CtmsMonitoringReport.CtmsMonitoring.StudyLevelForm.VariableTemplate.TemplateName;
            var User = _userRepository.Find(ReviewDto.UserId);

            _emailSenderRespository.SendEmailOfTemplateReview(User.Email, User.UserName, Activity, Template, ProjectName);
        }

        // Send mail for Approve
        public void SendMailForApproved(CtmsMonitoringReportReview ReviewDto)
        {
            var Review = All.Include(x => x.CtmsMonitoringReport).ThenInclude(x => x.CtmsMonitoring).ThenInclude(x => x.Project)
                .Include(x => x.CtmsMonitoringReport).ThenInclude(x => x.CtmsMonitoring).ThenInclude(x => x.StudyLevelForm).ThenInclude(x => x.VariableTemplate)
                .Include(x => x.CtmsMonitoringReport).ThenInclude(x => x.CtmsMonitoring).ThenInclude(x => x.StudyLevelForm).ThenInclude(x => x.Activity)
                .Include(x => x.CtmsMonitoringReport).ThenInclude(x => x.CtmsMonitoring).ThenInclude(x => x.StudyLevelForm).ThenInclude(x => x.Activity).ThenInclude(x => x.CtmsActivity)
                .Where(x => x.CtmsMonitoringReportId == ReviewDto.CtmsMonitoringReportId)
                .FirstOrDefault();

            var ProjectName = Review.CtmsMonitoringReport.CtmsMonitoring.Project.ProjectName;
            var Activity = Review.CtmsMonitoringReport.CtmsMonitoring.StudyLevelForm.Activity.CtmsActivity.ActivityName;
            var Template = Review.CtmsMonitoringReport.CtmsMonitoring.StudyLevelForm.VariableTemplate.TemplateName;
            var User = _userRepository.Find(ReviewDto.UserId);

            _emailSenderRespository.SendEmailOfTemplateApprove(User.Email, User.UserName, Activity, Template, ProjectName);
        }

        public bool GetReview(int CtmsMonitoringReportId)
        {
            var result = All.Where(x => x.CtmsMonitoringReportId == CtmsMonitoringReportId && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null)
                         .OrderByDescending(x => x.Id).FirstOrDefault();
            return result != null ? true : false;
        }

        public List<CtmsMonitoringReportReviewHistory> GetCtmsMonitoringReportReviewHistory(int id)
        {
            var result = (from review in _context.CtmsMonitoringReportReview.Include(x => x.CtmsMonitoringReport)
                          .Where(x => x.CtmsMonitoringReportId == id)
                          join auditreasontemp in _context.AuditTrail.Where(x => x.TableName == "CtmsMonitoringReportReview" && x.ColumnName == "Approve Date")
                          on review.Id equals auditreasontemp.RecordId into auditreasondto
                          from auditreason in auditreasondto.DefaultIfEmpty()
                          select new CtmsMonitoringReportReviewHistory
                          {
                              Id = review.Id,
                              CreatedDate = review.CreatedDate,
                              CreatedByUser = review.CreatedByUser.UserName,
                              Message = review.Message,
                              UserName = review.User.UserName,
                              ApproveDate = review.ApproveDate,
                              IsApproved = review.IsApproved,
                              Reason = auditreason.Reason,
                              ReasonOth = auditreason.ReasonOth,
                              ReportStatus = review.CtmsMonitoringReport.ReportStatus.ToString()
                          }).OrderByDescending(x => x.Id).ToList();

            return result;
        }

        public CtmsMonitoringReportReviewDto GetCtmsMonitoringReportReview(int id)
        {
            var result = All.Where(x => x.DeletedDate == null && x.CtmsMonitoringReportId == id && x.UserId == _jwtTokenAccesser.UserId)
                .Select(x => new CtmsMonitoringReportReviewDto
                {
                    Id = x.Id,
                    CtmsMonitoringReportId = x.CtmsMonitoringReportId,
                    UserId = x.UserId,
                    IsApproved = x.IsApproved,
                    ApproveDate = x.ApproveDate,
                }).FirstOrDefault();

            return result;
        }

        public bool isAnyReportReviewer(int id)
        {
            return All.Any(x => x.DeletedDate == null && x.CtmsMonitoringReportId == id);
        }

        public bool GetReviewSendToAnyone(int CtmsMonitoringReportId)
        {
            return All.Any(x => x.DeletedDate == null && x.CtmsMonitoringReportId == CtmsMonitoringReportId && x.ApproveDate == null);
        }
        public List<DashboardDto> GetSendTemplateList(int ProjectId, int? siteId)
        {
            var projectIds = new List<int>();

            if (siteId == 0)
            {
                projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == ProjectId
                                                           && _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                                                           && a.UserId == _jwtTokenAccesser.UserId
                                                           && a.RoleId == _jwtTokenAccesser.RoleId
                                                           && a.DeletedDate == null
                                                           && a.RollbackReason == null)
                                                           && x.DeletedDate == null).Select(x => x.Id).ToList();
            }
            else
            {
                projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == ProjectId
                                                        && _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                                                        && a.UserId == _jwtTokenAccesser.UserId
                                                        && a.RoleId == _jwtTokenAccesser.RoleId
                                                        && a.DeletedDate == null
                                                        && a.RollbackReason == null)
                                                        && x.Id == siteId
                                                        && x.DeletedDate == null).Select(x => x.Id).ToList();
            }

            var result = All.Include(t => t.CtmsMonitoringReport)
                        .ThenInclude(t => t.CtmsMonitoring)
                        .ThenInclude(t => t.Project)
                        .Include(z => z.CtmsMonitoringReport).ThenInclude(c => c.CtmsMonitoring).ThenInclude(x => x.StudyLevelForm).ThenInclude(x => x.VariableTemplate)

                        .Where(t => t.DeletedDate == null && projectIds.Contains(t.CtmsMonitoringReport.CtmsMonitoring.ProjectId)
                        && t.UserId == _jwtTokenAccesser.UserId && t.IsApproved == false && t.CtmsMonitoringReport.DeletedDate == null)
                        .Select(s => new DashboardDto
                        {
                            Id = s.Id,
                            TaskInformation = s.CtmsMonitoringReport.CtmsMonitoring.Project.ProjectCode + " - " + s.CtmsMonitoringReport.CtmsMonitoring.StudyLevelForm.VariableTemplate.TemplateName,
                            ExtraData = s.CtmsMonitoringReportId,
                            CreatedDate = s.CreatedDate,
                            CreatedByUser = s.CreatedByUser.UserName,
                            Module = MyTaskModule.CTMS.GetDescription(),
                            DataType = MyTaskMethodModule.Reviewed.GetDescription(),
                            Level = 6,
                            VariableTemplateId = s.CtmsMonitoringReport.CtmsMonitoring.StudyLevelForm.VariableTemplateId,
                            ControlType = DashboardMyTaskType.ManageMonitoringReportSendData,
                            CtmsMonitoringId = s.CtmsMonitoringReport.CtmsMonitoringId,
                            ActivityId = s.CtmsMonitoringReport.CtmsMonitoring.StudyLevelForm.ActivityId
                        }).OrderByDescending(x => x.CreatedDate).ToList();

            return result;
        }
        public List<DashboardDto> GetSendBackTemplateList(int ProjectId, int? siteId)
        {
            var projectIds = new List<int>();

            if (siteId == 0)
            {
                projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == ProjectId
                                                           && _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                                                           && a.UserId == _jwtTokenAccesser.UserId
                                                           && a.RoleId == _jwtTokenAccesser.RoleId
                                                           && a.DeletedDate == null
                                                           && a.RollbackReason == null)
                                                           && x.DeletedDate == null).Select(x => x.Id).ToList();
            }
            else
            {
                projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == ProjectId
                                                        && _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                                                        && a.UserId == _jwtTokenAccesser.UserId
                                                        && a.RoleId == _jwtTokenAccesser.RoleId
                                                        && a.DeletedDate == null
                                                        && a.RollbackReason == null)
                                                        && x.Id == siteId
                                                        && x.DeletedDate == null).Select(x => x.Id).ToList();
            }

            var result = All.Include(t => t.CtmsMonitoringReport)
                        .ThenInclude(t => t.CtmsMonitoring)
                        .ThenInclude(t => t.Project)
                        .Include(z => z.CtmsMonitoringReport).ThenInclude(c => c.CtmsMonitoring).ThenInclude(x => x.StudyLevelForm).ThenInclude(x => x.VariableTemplate)

                        .Where(t => t.DeletedDate == null && projectIds.Contains(t.CtmsMonitoringReport.CtmsMonitoring.ProjectId)
                        && t.UserId == _jwtTokenAccesser.UserId && t.IsSendBack == true && t.CtmsMonitoringReport.DeletedDate == null)
                        .Select(s => new DashboardDto
                        {
                            Id = s.Id,
                            TaskInformation = s.CtmsMonitoringReport.CtmsMonitoring.Project.ProjectCode + " - " + s.CtmsMonitoringReport.CtmsMonitoring.StudyLevelForm.VariableTemplate.TemplateName,
                            ExtraData = s.CtmsMonitoringReportId,
                            CreatedDate = s.CreatedDate,
                            CreatedByUser = s.CreatedByUser.UserName,
                            Module = MyTaskModule.CTMS.GetDescription(),
                            DataType = MyTaskMethodModule.Reviewed.GetDescription(),
                            Level = 6,
                            VariableTemplateId = s.CtmsMonitoringReport.CtmsMonitoring.StudyLevelForm.VariableTemplateId,
                            ControlType = DashboardMyTaskType.ManageMonitoringReportSendBackData,
                            CtmsMonitoringId = s.CtmsMonitoringReport.CtmsMonitoringId,
                            ActivityId = s.CtmsMonitoringReport.CtmsMonitoring.StudyLevelForm.ActivityId
                        }).OrderByDescending(x => x.CreatedDate).ToList();

            return result;
        }
    }
}