using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Etmf;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.UserMgt;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceArtificateDocumentReviewRepository : GenericRespository<ProjectArtificateDocumentReview, GscContext>, IProjectWorkplaceArtificateDocumentReviewRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IProjectWorkplaceArtificateRepository _projectWorkplaceArtificateRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        public ProjectWorkplaceArtificateDocumentReviewRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser, IUploadSettingRepository uploadSettingRepository,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IProjectWorkplaceArtificateRepository projectWorkplaceArtificateRepository,
            IEmailSenderRespository emailSenderRespository,
            IUserRepository userRepository)
           : base(uow, jwtTokenAccesser)
        {
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
            _projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
            _projectWorkplaceArtificateRepository = projectWorkplaceArtificateRepository;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
        }

        public List<ProjectArtificateDocumentReviewDto> UserRoles(int Id)
        {
            var users = Context.Users.Where(x => x.DeletedDate == null && x.Id != _jwtTokenAccesser.UserId).Select(c => new ProjectArtificateDocumentReviewDto
            {
                UserId = c.Id,
                Name = c.UserName,
                IsSelected = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.Id && b.DeletedDate == null && b.IsSendBack == false),
            }).Where(x => x.IsSelected == false).ToList();

            return users;
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
                    });
                    if (_uow.Save() < 0) throw new Exception("Artificate Send failed on save.");

                    SendMailToReviewer(ReviewDto);
                }
        }

        public void SendMailToReviewer(ProjectArtificateDocumentReviewDto ReviewDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                   .ThenInclude(x => x.ProjectWorkplaceArtificate)
                   .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                   .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace).ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceArtificatedDocumentId == ReviewDto.ProjectWorkplaceArtificatedDocumentId).FirstOrDefault();
            var ProjectName = project.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectName;
            var document = _projectWorkplaceArtificatedocumentRepository.Find(ReviewDto.ProjectWorkplaceArtificatedDocumentId);
            var artificate = _projectWorkplaceArtificateRepository.FindByInclude(x => x.Id == document.ProjectWorkplaceArtificateId, x => x.EtmfArtificateMasterLbrary).FirstOrDefault();
            var user = _userRepository.Find(ReviewDto.UserId);
            _emailSenderRespository.SendEmailOfReview(user.Email, user.UserName, document.DocumentName, artificate.EtmfArtificateMasterLbrary.ArtificateName, ProjectName);
        }

        public void SendMailToSendBack(ProjectArtificateDocumentReview ReviewDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                   .ThenInclude(x => x.ProjectWorkplaceArtificate)
                   .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                   .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace).ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceArtificatedDocumentId == ReviewDto.ProjectWorkplaceArtificatedDocumentId).FirstOrDefault();
            var ProjectName = project.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectName;
            var document = _projectWorkplaceArtificatedocumentRepository.Find(ReviewDto.ProjectWorkplaceArtificatedDocumentId);
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

            _uow.Save();
        }

        public List<ProjectArtificateDocumentReviewHistory> GetArtificateDocumentHistory(int Id)
        {
            var result = All.Include(x => x.ProjectWorkplaceArtificatedDocument).Where(x => x.ProjectWorkplaceArtificatedDocumentId == Id).
                Select(x => new ProjectArtificateDocumentReviewHistory
                {
                    Id = x.Id,
                    DocumentName = x.ProjectWorkplaceArtificatedDocument.DocumentName,
                    UserName = Context.Users.Where(y => y.Id == x.UserId && y.DeletedDate == null).FirstOrDefault().UserName,
                    //UserName = _userRepository.Find(x.UserId).UserName,
                    IsSendBack = x.IsSendBack,
                    UserId = x.UserId,
                    ProjectWorkplaceArtificatedDocumentId = x.ProjectWorkplaceArtificatedDocumentId,
                    CreatedDate = x.CreatedDate,
                    CreatedByUser = Context.Users.Where(y => y.Id == x.CreatedBy && y.DeletedDate == null).FirstOrDefault().UserName,
                    ModifiedDate = x.ModifiedDate,
                    ModifiedByUser = Context.Users.Where(y => y.Id == x.ModifiedBy && y.DeletedDate == null).FirstOrDefault().UserName,
                    SendBackDate = x.SendBackDate,
                }).OrderByDescending(x => x.Id).ToList();

            return result;
        }
    }
}
