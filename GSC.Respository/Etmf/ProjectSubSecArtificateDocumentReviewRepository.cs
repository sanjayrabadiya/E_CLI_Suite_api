using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EmailSender;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectSubSecArtificateDocumentReviewRepository : GenericRespository<ProjectSubSecArtificateDocumentReview>, IProjectSubSecArtificateDocumentReviewRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IProjectWorkplaceSubSecArtificatedocumentRepository _projectWorkplaceSubSecArtificatedocumentRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        private readonly IProjectSubSecArtificateDocumentHistoryRepository _projectSubSecArtificateDocumentHistoryRepository;
        private readonly IProjectRightRepository _projectRightRepository;

        public ProjectSubSecArtificateDocumentReviewRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
            IEmailSenderRespository emailSenderRespository,
            IUserRepository userRepository,
            IProjectWorkplaceSubSecArtificatedocumentRepository projectWorkplaceSubSecArtificatedocumentRepository,
            IProjectSubSecArtificateDocumentHistoryRepository projectSubSecArtificateDocumentHistoryRepository,
            IProjectRightRepository projectRightRepository
            )
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _projectSubSecArtificateDocumentHistoryRepository = projectSubSecArtificateDocumentHistoryRepository;
            _projectWorkplaceSubSecArtificatedocumentRepository = projectWorkplaceSubSecArtificatedocumentRepository;
            _projectRightRepository = projectRightRepository;
        }

        public List<ProjectSubSecArtificateDocumentReviewDto> UserRoles(int Id, int ProjectId, int ProjectDetailsId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone == true && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ProjectSubSecArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    IsSelected = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsSendBack == false),
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

        public void SaveDocumentReview(List<ProjectSubSecArtificateDocumentReviewDto> ProjectSubSecArtificateDocumentReviewDto)
        {
            foreach (var ReviewDto in ProjectSubSecArtificateDocumentReviewDto)
                if (ReviewDto.IsSelected)
                {
                    Add(new ProjectSubSecArtificateDocumentReview
                    {
                        ProjectWorkplaceSubSecArtificateDocumentId = ReviewDto.ProjectWorkplaceSubSecArtificateDocumentId,
                        UserId = ReviewDto.UserId,
                        IsSendBack = false,
                        Message = ReviewDto.Message,
                    });
                    if (_context.Save() < 0) throw new Exception("Artificate Send failed on save.");

                    SendMailToReviewer(ReviewDto);

                    var projectWorkplaceSubSecArtificatedocument = _projectWorkplaceSubSecArtificatedocumentRepository.Find(ReviewDto.ProjectWorkplaceSubSecArtificateDocumentId);
                    _projectSubSecArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceSubSecArtificatedocument, All.Max(p => p.Id), null);
                }
        }

        public void SendMailToReviewer(ProjectSubSecArtificateDocumentReviewDto ReviewDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                   .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == ReviewDto.ProjectWorkplaceSubSecArtificateDocumentId).FirstOrDefault();

            var ProjectName = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.Project.ProjectName;
            var document = project.ProjectWorkplaceSubSecArtificateDocument.DocumentName;
            var artificate = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName;
            var user = _userRepository.Find(ReviewDto.UserId);
            _emailSenderRespository.SendEmailOfReview(user.Email, user.UserName, document, artificate, ProjectName);
        }

        public void SendMailToSendBack(ProjectSubSecArtificateDocumentReview ReviewDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                   .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == ReviewDto.ProjectWorkplaceSubSecArtificateDocumentId).FirstOrDefault();

            var ProjectName = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.Project.ProjectName;
            var document = project.ProjectWorkplaceSubSecArtificateDocument.DocumentName;
            var artificate = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName;
            var user = _userRepository.Find((int)ReviewDto.CreatedBy);
            _emailSenderRespository.SendEmailOfSendBack(user.Email, user.UserName, document, artificate, ProjectName);
        }

        public void SaveByDocumentIdInReview(int ProjectWorkplaceSubSecArtificateDocumentId)
        {
            Add(new ProjectSubSecArtificateDocumentReview
            {
                ProjectWorkplaceSubSecArtificateDocumentId = ProjectWorkplaceSubSecArtificateDocumentId,
                UserId = _jwtTokenAccesser.UserId,
                RoleId = _jwtTokenAccesser.RoleId
            });

            _context.Save();
        }

        public List<ProjectSubSecArtificateDocumentReviewHistory> GetArtificateDocumentHistory(int Id)
        {
            //var result = All.Include(x => x.ProjectWorkplaceSubSecArtificateDocument).ThenInclude(x => x.ProjectSubSecArtificateDocumentHistory).Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == Id
            //              && x.UserId != x.ProjectWorkplaceSubSecArtificateDocument.CreatedBy)
            //    .Select(x => new ProjectSubSecArtificateDocumentReviewHistory
            //    {
            //        Id = x.Id,
            //        DocumentName = x.ProjectSubSecArtificateDocumentHistory.OrderByDescending(x => x.Id).FirstOrDefault().DocumentName,
            //        //DocumentName = x.ProjectArtificateDocumentHistory.Count() == 0 ? x.ProjectWorkplaceArtificatedDocument.DocumentName : x.ProjectArtificateDocumentHistory.OrderByDescending(x=>x.Id).FirstOrDefault().DocumentName,
            //        ProjectArtificateDocumentHistoryId = x.ProjectSubSecArtificateDocumentHistory.OrderByDescending(x => x.Id).FirstOrDefault().Id,
            //        UserName = _context.Users.Where(y => y.Id == x.UserId && y.DeletedDate == null).FirstOrDefault().UserName,
            //        IsSendBack = x.IsSendBack,
            //        UserId = x.UserId,
            //        ProjectWorkplaceSubSecArtificateDocumentId = x.ProjectWorkplaceSubSecArtificateDocumentId,
            //        CreatedDate = x.CreatedDate,
            //        CreatedByUser = _context.Users.Where(y => y.Id == x.CreatedBy && y.DeletedDate == null).FirstOrDefault().UserName,
            //        ModifiedDate = x.ModifiedDate,
            //        ModifiedByUser = _context.Users.Where(y => y.Id == x.ModifiedBy && y.DeletedDate == null).FirstOrDefault().UserName,
            //        SendBackDate = x.SendBackDate,
            //        Message = x.Message,
            //    }).OrderByDescending(x => x.Id).ToList();

            var result = (from review in _context.ProjectSubSecArtificateDocumentReview.Include(x => x.ProjectWorkplaceSubSecArtificateDocument).ThenInclude(x => x.ProjectSubSecArtificateDocumentHistory)
                          .Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == Id && x.UserId != x.ProjectWorkplaceSubSecArtificateDocument.CreatedBy)
                          join auditReasonTemp in _context.AuditTrail.Where(x => x.TableName == "ProjectSubSecArtificateDocumentReview" && x.ColumnName == "SendBack Date")
                          on review.Id equals auditReasonTemp.RecordId into auditReasonDto
                          from auditReason in auditReasonDto.DefaultIfEmpty()
                          select new ProjectSubSecArtificateDocumentReviewHistory
                          {
                              Id = review.Id,
                              DocumentName = review.ProjectSubSecArtificateDocumentHistory.OrderByDescending(x => x.Id).FirstOrDefault().DocumentName,
                              ProjectArtificateDocumentHistoryId = review.ProjectSubSecArtificateDocumentHistory.OrderByDescending(x => x.Id).FirstOrDefault().Id,
                              UserName = _context.Users.Where(y => y.Id == review.UserId && y.DeletedDate == null).FirstOrDefault().UserName,
                              IsSendBack = review.IsSendBack,
                              UserId = review.UserId,
                              ProjectWorkplaceSubSecArtificateDocumentId = review.ProjectWorkplaceSubSecArtificateDocumentId,
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
            var result = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                 .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact)
                 .ThenInclude(x => x.ProjectWorkPlace) // Sub Section
                 .ThenInclude(x => x.ProjectWorkPlace) // Section
                 .ThenInclude(x => x.EtmfMasterLibrary) // Etmf Section
                 .Include(x => x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace)
                 .ThenInclude(x => x.EtmfMasterLibrary)
                 .Include(x => x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace)
                .Where(x => x.DeletedDate == null && (x.UserId != x.ProjectWorkplaceSubSecArtificateDocument.CreatedBy && x.UserId == _jwtTokenAccesser.UserId)
                && x.ProjectWorkplaceSubSecArtificateDocument.DeletedDate == null
                && x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectId == ProjectId && x.IsSendBack == false
                && x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSectionArtifact)
                .Select(s => new DashboardDto
                {
                    Id = s.Id,
                    TaskInformation = ((WorkPlaceFolder)s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription() + " | " +
                    (s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName == null ? "" :
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName + " | ") +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.SectionName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.SubSectionName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.DocumentName,
                    ExtraData = s.ProjectWorkplaceSubSecArtificateDocumentId,
                    CreatedDate = s.CreatedDate,
                    CreatedByUser = _context.Users.Where(x => x.Id == s.CreatedBy).FirstOrDefault().UserName,
                    Module = MyTaskModule.ETMF.GetDescription(),
                    DataType = MyTaskMethodModule.Reviewed.GetDescription(),
                    Level = 5.2,
                    ControlType = DashboardMyTaskType.ETMFSubSecSendData
                }).OrderByDescending(x => x.CreatedDate).ToList();

            return result;
        }

        public List<DashboardDto> GetSendBackDocumentList(int ProjectId)
        {
            var result = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                 .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact)
                 .ThenInclude(x => x.ProjectWorkPlace) // Sub Section
                 .ThenInclude(x => x.ProjectWorkPlace) // Section
                 .ThenInclude(x => x.EtmfMasterLibrary) // Etmf Section
                 .Include(x => x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace)
                 .ThenInclude(x => x.EtmfMasterLibrary)
                 .Include(x => x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace)
                .Where(x => x.DeletedDate == null && (x.CreatedBy == x.ProjectWorkplaceSubSecArtificateDocument.CreatedBy && x.CreatedBy == _jwtTokenAccesser.UserId)
                && x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace
                .ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectId == ProjectId && x.IsSendBack == true
                && x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSectionArtifact)
                .Select(s => new DashboardDto
                {
                    Id = s.Id,
                    DocumentId = s.ProjectWorkplaceSubSecArtificateDocumentId,
                    TaskInformation = ((WorkPlaceFolder)s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription() + " | " +
                    (s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName == null ? "" :
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName + " | ") +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.SectionName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.DocumentName,
                    CreatedDate = s.CreatedDate,
                    CreatedByUser = _context.Users.Where(x => x.Id == s.UserId).FirstOrDefault().UserName,
                    Module = MyTaskModule.ETMF.GetDescription(),
                    DataType = MyTaskMethodModule.SendBack.GetDescription(),
                    ControlType = DashboardMyTaskType.ETMFSubSecSendBackData
                }).OrderByDescending(x => x.CreatedDate).ToList();

            result.ForEach(s =>
            {
                s.ExtraData = _context.ProjectSubSecArtificateDocumentApprover.Where(x => x.DeletedDate == null && x.ProjectWorkplaceSubSecArtificateDocumentId == s.DocumentId).Count();
            });

            return result.Where(x => Convert.ToInt32(x.ExtraData) == 0).ToList();
        }
    }
}
