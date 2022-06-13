using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Etmf;
using GSC.Data.Entities.UserMgt;
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

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceArtificateDocumentReviewRepository : GenericRespository<ProjectArtificateDocumentReview>, IProjectWorkplaceArtificateDocumentReviewRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IProjectWorkplaceArtificateRepository _projectWorkplaceArtificateRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        private readonly IProjectArtificateDocumentHistoryRepository _projectArtificateDocumentHistoryRepository;
        private readonly IProjectRightRepository _projectRightRepository;

        public ProjectWorkplaceArtificateDocumentReviewRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
           IProjectWorkplaceArtificateRepository projectWorkplaceArtificateRepository,
           IEmailSenderRespository emailSenderRespository,
           IUserRepository userRepository,
           IProjectArtificateDocumentHistoryRepository projectArtificateDocumentHistoryRepository,
           IProjectRightRepository projectRightRepository
            )
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _projectWorkplaceArtificateRepository = projectWorkplaceArtificateRepository;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _projectArtificateDocumentHistoryRepository = projectArtificateDocumentHistoryRepository;
            _projectRightRepository = projectRightRepository;
        }

        public List<ProjectArtificateDocumentReviewDto> UserRoles(int Id, int ProjectId, int ProjectDetailsId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone == true && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ProjectArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    IsSelected = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsSendBack == false),
                }).Where(x => x.IsSelected == false).ToList();

            users.ForEach(x =>
            {
                var etmfUserPermissions = _context.EtmfUserPermission.Include(y => y.ProjectWorkplaceDetail)
                                        .Where(y => y.ProjectWorkplaceDetailId == ProjectDetailsId && y.DeletedDate == null && y.UserId == x.UserId)
                                        .OrderByDescending(x => x.Id).FirstOrDefault();
                x.IsRights = etmfUserPermissions != null ? etmfUserPermissions.IsAdd || etmfUserPermissions.IsEdit || etmfUserPermissions.IsView : false;
            });

            return users.Where(x => x.IsRights == true).ToList();
        }

        public void SaveDocumentReview(List<ProjectArtificateDocumentReviewDto> pojectArtificateDocumentReviewDto)
        {
            foreach (var ReviewDto in pojectArtificateDocumentReviewDto)
                if (ReviewDto.IsSelected)
                {
                    Add(new ProjectArtificateDocumentReview
                    {
                        ProjectWorkplaceArtificatedDocumentId = ReviewDto.ProjectWorkplaceArtificatedDocumentId,
                        UserId = ReviewDto.UserId,
                        IsSendBack = false,
                        Message = ReviewDto.Message,
                    });
                    if (_context.Save() < 0) throw new Exception("Artificate Send failed on save.");

                    SendMailToReviewer(ReviewDto);

                    //var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.Find(ReviewDto.ProjectWorkplaceArtificatedDocumentId);
                    var projectWorkplaceArtificatedocument = _context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == ReviewDto.ProjectWorkplaceArtificatedDocumentId && x.DeletedDate == null).FirstOrDefault();
                    _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, All.Max(p => p.Id), null);
                }
        }

        // Send mail for review
        public void SendMailToReviewer(ProjectArtificateDocumentReviewDto ReviewDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                   .ThenInclude(x => x.ProjectWorkplaceArtificate)
                   .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                   .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace).ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceArtificatedDocumentId == ReviewDto.ProjectWorkplaceArtificatedDocumentId).FirstOrDefault();
            var ProjectName = project.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectName;
            //var document = _projectWorkplaceArtificatedocumentRepository.Find(ReviewDto.ProjectWorkplaceArtificatedDocumentId);
            var document = _context.ProjectWorkplaceArtificatedocument.Where(x=>x.Id==ReviewDto.ProjectWorkplaceArtificatedDocumentId && x.DeletedDate==null).FirstOrDefault();
            var artificate = _projectWorkplaceArtificateRepository.FindByInclude(x => x.Id == document.ProjectWorkplaceArtificateId, x => x.EtmfArtificateMasterLbrary).FirstOrDefault();
            var user = _userRepository.Find(ReviewDto.UserId);
            _emailSenderRespository.SendEmailOfReview(user.Email, user.UserName, document.DocumentName, artificate.EtmfArtificateMasterLbrary.ArtificateName, ProjectName);
        }

        // Send mail for sendback
        public void SendMailToSendBack(ProjectArtificateDocumentReview ReviewDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                   .ThenInclude(x => x.ProjectWorkplaceArtificate)
                   .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                   .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace).ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceArtificatedDocumentId == ReviewDto.ProjectWorkplaceArtificatedDocumentId).FirstOrDefault();
            var ProjectName = project.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectName;
            //var document = _projectWorkplaceArtificatedocumentRepository.Find(ReviewDto.ProjectWorkplaceArtificatedDocumentId);
            var document = _context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == ReviewDto.ProjectWorkplaceArtificatedDocumentId && x.DeletedDate==null).FirstOrDefault();
            var artificate = _projectWorkplaceArtificateRepository.FindByInclude(x => x.Id == document.ProjectWorkplaceArtificateId, x => x.EtmfArtificateMasterLbrary).FirstOrDefault();
            var user = _userRepository.Find((int)ReviewDto.CreatedBy);
            _emailSenderRespository.SendEmailOfSendBack(user.Email, user.UserName, document.DocumentName, artificate.EtmfArtificateMasterLbrary.ArtificateName, ProjectName);
        }

        public void SaveByDocumentIdInReview(int projectWorkplaceArtificateDocumentId)
        {
            Add(new ProjectArtificateDocumentReview
            {
                ProjectWorkplaceArtificatedDocumentId = projectWorkplaceArtificateDocumentId,
                UserId = _jwtTokenAccesser.UserId,
                RoleId = _jwtTokenAccesser.RoleId
            });

            _context.Save();
        }

        public List<ProjectArtificateDocumentReviewHistory> GetArtificateDocumentHistory(int Id)
        {
            //var result = All.Include(x => x.ProjectWorkplaceArtificatedDocument).ThenInclude(x => x.ProjectArtificateDocumentHistory).Where(x => x.ProjectWorkplaceArtificatedDocumentId == Id
            //              && x.UserId != x.ProjectWorkplaceArtificatedDocument.CreatedBy)
            //    .Select(x => new ProjectArtificateDocumentReviewHistory
            //    {
            //        Id = x.Id,
            //        DocumentName = x.ProjectArtificateDocumentHistory.OrderByDescending(x => x.Id).FirstOrDefault().DocumentName,
            //        //DocumentName = x.ProjectArtificateDocumentHistory.Count() == 0 ? x.ProjectWorkplaceArtificatedDocument.DocumentName : x.ProjectArtificateDocumentHistory.OrderByDescending(x=>x.Id).FirstOrDefault().DocumentName,
            //        ProjectArtificateDocumentHistoryId = x.ProjectArtificateDocumentHistory.OrderByDescending(x => x.Id).FirstOrDefault().Id,
            //        UserName = _context.Users.Where(y => y.Id == x.UserId && y.DeletedDate == null).FirstOrDefault().UserName,
            //        IsSendBack = x.IsSendBack,
            //        UserId = x.UserId,
            //        ProjectWorkplaceArtificatedDocumentId = x.ProjectWorkplaceArtificatedDocumentId,
            //        CreatedDate = x.CreatedDate,
            //        CreatedByUser = _context.Users.Where(y => y.Id == x.CreatedBy && y.DeletedDate == null).FirstOrDefault().UserName,
            //        ModifiedDate = x.ModifiedDate,
            //        ModifiedByUser = _context.Users.Where(y => y.Id == x.ModifiedBy && y.DeletedDate == null).FirstOrDefault().UserName,
            //        SendBackDate = x.SendBackDate,
            //        Message = x.Message,
            //    }).OrderByDescending(x => x.Id).ToList();

            var result = (from review in _context.ProjectArtificateDocumentReview.Include(x => x.ProjectWorkplaceArtificatedDocument).ThenInclude(x => x.ProjectArtificateDocumentHistory)
                          .Where(x => x.ProjectWorkplaceArtificatedDocumentId == Id && x.UserId != x.ProjectWorkplaceArtificatedDocument.CreatedBy)
                          join auditReasonTemp in _context.AuditTrail.Where(x => x.TableName == "ProjectArtificateDocumentReview" && x.ColumnName == "SendBack Date")
                          on review.Id equals auditReasonTemp.RecordId into auditReasonDto
                          from auditReason in auditReasonDto.DefaultIfEmpty()
                          select new ProjectArtificateDocumentReviewHistory
                          {
                              Id = review.Id,
                              DocumentName = review.ProjectArtificateDocumentHistory.OrderByDescending(x => x.Id).FirstOrDefault().DocumentName,
                              ProjectArtificateDocumentHistoryId = review.ProjectArtificateDocumentHistory.OrderByDescending(x => x.Id).FirstOrDefault().Id,
                              UserName = _context.Users.Where(y => y.Id == review.UserId && y.DeletedDate == null).FirstOrDefault().UserName,
                              IsSendBack = review.IsSendBack,
                              UserId = review.UserId,
                              ProjectWorkplaceArtificatedDocumentId = review.ProjectWorkplaceArtificatedDocumentId,
                              CreatedDate = review.CreatedDate,
                              CreatedByUser = review.CreatedByUser.UserName,
                              ModifiedDate = review.ModifiedDate,
                              ModifiedByUser = review.ModifiedByUser.UserName,
                              SendBackDate = review.SendBackDate,
                              Message = review.Message,
                              Reason = auditReason.Reason,
                              ReasonOth = auditReason.ReasonOth
                          }).OrderByDescending(x => x.Id).ToList();

            return result;
        }

        public List<DashboardDto> GetSendDocumentList(int ProjectId)
        {
            var result = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                .ThenInclude(x => x.ProjectWorkplaceArtificate)
                .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace)
                .Where(x => x.DeletedDate == null && (x.UserId != x.ProjectWorkplaceArtificatedDocument.CreatedBy && x.UserId == _jwtTokenAccesser.UserId)
                && x.ProjectWorkplaceArtificatedDocument.DeletedDate == null
                && x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection
                .ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == ProjectId && x.IsSendBack == false)
                .Select(s => new DashboardDto
                {
                    Id = s.Id,
                    TaskInformation = ((WorkPlaceFolder)s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription() + " | " +
                    (s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemName == null ? "" :
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemName + " | ") +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName + " | " +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName + " | " +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName + " | " +
                    s.ProjectWorkplaceArtificatedDocument.DocumentName,
                    ExtraData = s.ProjectWorkplaceArtificatedDocumentId,
                    CreatedDate = s.CreatedDate,
                    CreatedByUser = _context.Users.Where(x => x.Id == s.CreatedBy).FirstOrDefault().UserName,
                    Module = MyTaskModule.ETMF.GetDescription(),
                    DataType = MyTaskMethodModule.Reviewed.GetDescription(),
                    Level = 6,
                    ControlType=DashboardMyTaskType.ETMFSendData
                }).OrderByDescending(x => x.CreatedDate).ToList();

            return result;
        }

        public List<DashboardDto> GetSendBackDocumentList(int ProjectId)
        {
            var result = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                .ThenInclude(x => x.ProjectWorkplaceArtificate)
                .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace)
                .Where(x => x.DeletedDate == null && (x.CreatedBy == x.ProjectWorkplaceArtificatedDocument.CreatedBy && x.CreatedBy == _jwtTokenAccesser.UserId)
                && x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection
                .ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == ProjectId && x.IsSendBack == true)
                .Select(s => new DashboardDto
                {
                    Id = s.Id,
                    DocumentId = s.ProjectWorkplaceArtificatedDocumentId,
                    TaskInformation = ((WorkPlaceFolder)s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription() + " | " +
                    (s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemName == null ? "" :
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemName + " | ") +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName + " | " +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName + " | " +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName + " | " +
                    s.ProjectWorkplaceArtificatedDocument.DocumentName,
                    CreatedDate = s.CreatedDate,
                    CreatedByUser = _context.Users.Where(x => x.Id == s.UserId).FirstOrDefault().UserName,
                    Module = MyTaskModule.ETMF.GetDescription(),
                    DataType = MyTaskMethodModule.SendBack.GetDescription(),
                    ControlType=DashboardMyTaskType.ETMFSendBackData
                }).OrderByDescending(x => x.CreatedDate).ToList();

            result.ForEach(s =>
            {
                s.ExtraData = _context.ProjectArtificateDocumentApprover.Where(x => x.DeletedDate == null && x.ProjectWorkplaceArtificatedDocumentId == s.DocumentId).Count();
            });

            return result.Where(x => Convert.ToInt32(x.ExtraData) == 0).ToList();
        }
    }
}
