using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Respository.EmailSender;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringReportReviewRepository : GenericRespository<CtmsMonitoringReportReview>, ICtmsMonitoringReportReviewRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;

        public CtmsMonitoringReportReviewRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IProjectRightRepository projectRightRepository,
            IUserRepository userRepository, IEmailSenderRespository emailSenderRespository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _projectRightRepository = projectRightRepository;
            _userRepository = userRepository;
            _emailSenderRespository = emailSenderRespository;
        }


        public List<CtmsMonitoringReportReviewDto> UserRoles(int Id, int ProjectId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone == true && x.DeletedDate == null && x.User.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new CtmsMonitoringReportReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    IsSelected = All.Any(b => b.CtmsMonitoringReportId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsSendBack == false),
                }).Where(x => x.IsSelected == false).ToList();

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

            var ProjectName = Review.CtmsMonitoringReport.CtmsMonitoring.Project.ProjectCode;
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
                .Where(x => x.CtmsMonitoringReportId == ReviewDto.CtmsMonitoringReportId)
                .FirstOrDefault();

            var ProjectName = Review.CtmsMonitoringReport.CtmsMonitoring.Project.ProjectCode;
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
                              ReasonOth = auditreason.ReasonOth
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
    }
}