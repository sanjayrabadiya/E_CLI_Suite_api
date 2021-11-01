using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.CTMS
{
    public class ManageMonitoringReportReviewRepository : GenericRespository<ManageMonitoringReportReview>, IManageMonitoringReportReviewRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        private readonly IProjectRightRepository _projectRightRepository;

        public ManageMonitoringReportReviewRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
           IEmailSenderRespository emailSenderRespository,
           IUserRepository userRepository,
           IProjectRightRepository projectRightRepository
            )
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _projectRightRepository = projectRightRepository;
        }

        public List<ManageMonitoringReportReviewDto> UserRoles(int Id, int ProjectId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone == true && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ManageMonitoringReportReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    IsSelected = All.Any(b => b.ManageMonitoringReportId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsSendBack == false),
                }).Where(x => x.IsSelected == false).ToList();

            return users;
        }

        public void SaveTemplateReview(List<ManageMonitoringReportReviewDto> manageMonitoringReportReviewDto)
        {
            foreach (var ReviewDto in manageMonitoringReportReviewDto)
                if (ReviewDto.IsSelected)
                {
                    Add(new ManageMonitoringReportReview
                    {
                        ManageMonitoringReportId = ReviewDto.ManageMonitoringReportId,
                        UserId = ReviewDto.UserId,
                        IsSendBack = false,
                        Message = ReviewDto.Message,
                    });
                    if (_context.Save() < 0) throw new Exception("Review Send failed on save.");

                    SendMailToReviewer(ReviewDto);

                    //var projectWorkplaceArtificatedocument = _context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == ReviewDto.ProjectWorkplaceArtificatedDocumentId && x.DeletedDate == null).FirstOrDefault();
                    //_projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, All.Max(p => p.Id), null);
                }
        }

        // Send mail for review
        public void SendMailToReviewer(ManageMonitoringReportReviewDto ReviewDto)
        {
            var Review = All.Include(x => x.ManageMonitoringReport).ThenInclude(x => x.ManageMonitoringVisit).ThenInclude(x => x.Project)
                .Include(x => x.ManageMonitoringReport).ThenInclude(x => x.VariableTemplate)
                .Include(x => x.ManageMonitoringReport).ThenInclude(x => x.ManageMonitoringVisit).ThenInclude(x => x.Activity)
                .Where(x => x.ManageMonitoringReportId == ReviewDto.ManageMonitoringReportId)
                .FirstOrDefault();

            var ProjectName = Review.ManageMonitoringReport.ManageMonitoringVisit.Project.ProjectCode;
            var Activity = Review.ManageMonitoringReport.ManageMonitoringVisit.Activity.ActivityCode;
            var Template = Review.ManageMonitoringReport.VariableTemplate.TemplateName;
            var User = _userRepository.Find(ReviewDto.UserId);

            _emailSenderRespository.SendEmailOfTemplateReview(User.Email, User.UserName, Activity, Template, ProjectName);
        }

        // Send mail for sendback
        public void SendMailToSendBack(ManageMonitoringReportReview ReviewDto)
        {
            var Review = All.Include(x => x.ManageMonitoringReport).ThenInclude(x => x.ManageMonitoringVisit).ThenInclude(x => x.Project)
                .Include(x => x.ManageMonitoringReport).ThenInclude(x => x.VariableTemplate)
                .Include(x => x.ManageMonitoringReport).ThenInclude(x => x.ManageMonitoringVisit).ThenInclude(x => x.Activity)
                .Where(x => x.ManageMonitoringReportId == ReviewDto.ManageMonitoringReportId)
                .FirstOrDefault();

            var ProjectName = Review.ManageMonitoringReport.ManageMonitoringVisit.Project.ProjectCode;
            var Activity = Review.ManageMonitoringReport.ManageMonitoringVisit.Activity.ActivityCode;
            var Template = Review.ManageMonitoringReport.VariableTemplate.TemplateName;
            var User = _userRepository.Find(ReviewDto.UserId);
            
            _emailSenderRespository.SendEmailOfTemplateSendBack(User.Email, User.UserName, Activity, Template, ProjectName);
        }

        //public void SaveByDocumentIdInReview(int projectWorkplaceArtificateDocumentId)
        //{
        //    Add(new ProjectArtificateDocumentReview
        //    {
        //        ProjectWorkplaceArtificatedDocumentId = projectWorkplaceArtificateDocumentId,
        //        UserId = _jwtTokenAccesser.UserId,
        //        RoleId = _jwtTokenAccesser.RoleId
        //    });

        //    _context.Save();
        //}

        //public List<ProjectArtificateDocumentReviewHistory> GetArtificateDocumentHistory(int Id)
        //{

        //    var result = (from review in _context.ProjectArtificateDocumentReview.Include(x => x.ProjectWorkplaceArtificatedDocument).ThenInclude(x => x.ProjectArtificateDocumentHistory)
        //                  .Where(x => x.ProjectWorkplaceArtificatedDocumentId == Id && x.UserId != x.ProjectWorkplaceArtificatedDocument.CreatedBy)
        //                  join auditReasonTemp in _context.AuditTrail.Where(x => x.TableName == "ProjectArtificateDocumentReview" && x.ColumnName == "SendBack Date")
        //                  on review.Id equals auditReasonTemp.RecordId into auditReasonDto
        //                  from auditReason in auditReasonDto.DefaultIfEmpty()
        //                  select new ProjectArtificateDocumentReviewHistory
        //                  {
        //                      Id = review.Id,
        //                      DocumentName = review.ProjectArtificateDocumentHistory.OrderByDescending(x => x.Id).FirstOrDefault().DocumentName,
        //                      ProjectArtificateDocumentHistoryId = review.ProjectArtificateDocumentHistory.OrderByDescending(x => x.Id).FirstOrDefault().Id,
        //                      UserName = _context.Users.Where(y => y.Id == review.UserId && y.DeletedDate == null).FirstOrDefault().UserName,
        //                      IsSendBack = review.IsSendBack,
        //                      UserId = review.UserId,
        //                      ProjectWorkplaceArtificatedDocumentId = review.ProjectWorkplaceArtificatedDocumentId,
        //                      CreatedDate = review.CreatedDate,
        //                      CreatedByUser = review.CreatedByUser.UserName,
        //                      ModifiedDate = review.ModifiedDate,
        //                      ModifiedByUser = review.ModifiedByUser.UserName,
        //                      SendBackDate = review.SendBackDate,
        //                      Message = review.Message,
        //                      Reason = auditReason.Reason,
        //                      ReasonOth = auditReason.ReasonOth
        //                  }).OrderByDescending(x => x.Id).ToList();

        //    return result;
        //}

        public List<DashboardDto> GetSendTemplateList(int ProjectId)
        {
            var result = All.Include(t => t.ManageMonitoringReport)
                        .ThenInclude(t => t.ManageMonitoringVisit)
                        .ThenInclude(t => t.Project)
                        .Where(t => t.DeletedDate == null && t.ManageMonitoringReport.ManageMonitoringVisit.ProjectId == ProjectId
                        && t.CreatedBy == _jwtTokenAccesser.UserId && t.IsSendBack == false && t.ManageMonitoringReport.DeletedDate == null)
                        .Select(s => new DashboardDto
                        {
                            Id = s.Id,
                            TaskInformation = s.ManageMonitoringReport.VariableTemplate.TemplateName,
                            ExtraData = s.ManageMonitoringReportId,
                            CreatedDate = s.CreatedDate,
                            CreatedByUser = _context.Users.Where(x => x.Id == s.CreatedBy).FirstOrDefault().UserName,
                            Module = MyTaskModule.CTMS.GetDescription(),
                            DataType = MyTaskMethodModule.Reviewed.GetDescription(),
                            Level = 6
                        }).OrderByDescending(x => x.CreatedDate).ToList();

            return result;
        }

        public List<DashboardDto> GetSendBackTemplateList(int ProjectId)
        {
            var result = All.Include(t => t.ManageMonitoringReport)
                        .ThenInclude(t => t.ManageMonitoringVisit)
                        .ThenInclude(t => t.Project)
                        .Where(t => t.DeletedDate == null && t.ManageMonitoringReport.ManageMonitoringVisit.ProjectId == ProjectId
                        && t.CreatedBy == _jwtTokenAccesser.UserId && t.IsSendBack == false && t.ManageMonitoringReport.DeletedDate == null)
                        .Select(s => new DashboardDto
                        {
                            Id = s.Id,
                            TaskInformation = s.ManageMonitoringReport.VariableTemplate.TemplateName,
                            ExtraData = s.ManageMonitoringReportId,
                            CreatedDate = s.CreatedDate,
                            CreatedByUser = _context.Users.Where(x => x.Id == s.CreatedBy).FirstOrDefault().UserName,
                            Module = MyTaskModule.CTMS.GetDescription(),
                            DataType = MyTaskMethodModule.Reviewed.GetDescription(),
                            Level = 6
                        }).OrderByDescending(x => x.CreatedDate).ToList();

            return result;
        }
    }
}
