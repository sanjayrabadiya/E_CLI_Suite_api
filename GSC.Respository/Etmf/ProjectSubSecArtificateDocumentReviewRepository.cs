using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EmailSender;
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

        public ProjectSubSecArtificateDocumentReviewRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
            IEmailSenderRespository emailSenderRespository,
            IUserRepository userRepository,
            IProjectWorkplaceSubSecArtificatedocumentRepository projectWorkplaceSubSecArtificatedocumentRepository,
            IProjectSubSecArtificateDocumentHistoryRepository projectSubSecArtificateDocumentHistoryRepository
            )
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _projectSubSecArtificateDocumentHistoryRepository = projectSubSecArtificateDocumentHistoryRepository;
            _projectWorkplaceSubSecArtificatedocumentRepository = projectWorkplaceSubSecArtificatedocumentRepository;
        }

        public List<ProjectSubSecArtificateDocumentReviewDto> UserRoles(int Id)
        {
            var users = _context.Users.Where(x => x.DeletedDate == null && x.Id != _jwtTokenAccesser.UserId && x.UserType == UserMasterUserType.User).Select(c => new ProjectSubSecArtificateDocumentReviewDto
            {
                UserId = c.Id,
                Name = c.UserName,
                IsSelected = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.Id && b.DeletedDate == null && b.IsSendBack == false),
            }).Where(x => x.IsSelected == false).ToList();

            return users;
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
                   .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact)
                   .ThenInclude(x => x.ProjectWorkplaceSubSection).ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                   .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace).ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == ReviewDto.ProjectWorkplaceSubSecArtificateDocumentId).FirstOrDefault();

            var ProjectName = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectName;
            var document = project.ProjectWorkplaceSubSecArtificateDocument.DocumentName;
            var artificate = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName;
            var user = _userRepository.Find(ReviewDto.UserId);
            _emailSenderRespository.SendEmailOfReview(user.Email, user.UserName, document, artificate, ProjectName);
        }

        public void SendMailToSendBack(ProjectSubSecArtificateDocumentReview ReviewDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                   .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.ProjectWorkplaceSubSection)
                   .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                   .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace).ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == ReviewDto.ProjectWorkplaceSubSecArtificateDocumentId).FirstOrDefault();

            var ProjectName = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectName;
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
            var result = All.Include(x => x.ProjectWorkplaceSubSecArtificateDocument).ThenInclude(x => x.ProjectSubSecArtificateDocumentHistory).Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == Id
                          && x.UserId != x.ProjectWorkplaceSubSecArtificateDocument.CreatedBy)
                .Select(x => new ProjectSubSecArtificateDocumentReviewHistory
                {
                    Id = x.Id,
                    DocumentName = x.ProjectSubSecArtificateDocumentHistory.OrderByDescending(x => x.Id).FirstOrDefault().DocumentName,
                    //DocumentName = x.ProjectArtificateDocumentHistory.Count() == 0 ? x.ProjectWorkplaceArtificatedDocument.DocumentName : x.ProjectArtificateDocumentHistory.OrderByDescending(x=>x.Id).FirstOrDefault().DocumentName,
                    ProjectArtificateDocumentHistoryId = x.ProjectSubSecArtificateDocumentHistory.OrderByDescending(x => x.Id).FirstOrDefault().Id,
                    UserName = _context.Users.Where(y => y.Id == x.UserId && y.DeletedDate == null).FirstOrDefault().UserName,
                    IsSendBack = x.IsSendBack,
                    UserId = x.UserId,
                    ProjectWorkplaceSubSecArtificateDocumentId = x.ProjectWorkplaceSubSecArtificateDocumentId,
                    CreatedDate = x.CreatedDate,
                    CreatedByUser = _context.Users.Where(y => y.Id == x.CreatedBy && y.DeletedDate == null).FirstOrDefault().UserName,
                    ModifiedDate = x.ModifiedDate,
                    ModifiedByUser = _context.Users.Where(y => y.Id == x.ModifiedBy && y.DeletedDate == null).FirstOrDefault().UserName,
                    SendBackDate = x.SendBackDate,
                    Message = x.Message,
                }).OrderByDescending(x => x.Id).ToList();

            return result;
        }

        public List<DashboardDto> GetSendDocumentList(int ProjectId)
        {
            var result = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x=>x.ProjectWorkplaceSubSection)
                .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace)
                .Where(x => (x.UserId != x.ProjectWorkplaceSubSecArtificateDocument.CreatedBy && x.UserId == _jwtTokenAccesser.UserId)
                && x.ProjectWorkplaceSubSecArtificateDocument.DeletedDate == null
                && x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection
                .ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == ProjectId && x.IsSendBack == false)
                .Select(s => new DashboardDto
                {
                    Id = s.Id,
                    TaskInformation = ((WorkPlaceFolder)s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription() + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.SubSectionName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.DocumentName,
                    ExtraData = s.ProjectWorkplaceSubSecArtificateDocumentId,
                    CreatedDate = s.CreatedDate,
                    CreatedByUser = _context.Users.Where(x => x.Id == s.CreatedBy).FirstOrDefault().UserName,
                    Module = MyTaskModule.ETMF.GetDescription(),
                    DataType = MyTaskMethodModule.Reviewed.GetDescription(),
                    Level = 5.2
                }).OrderBy(x => x.Id).ToList();

            return result;
        }

        public List<DashboardDto> GetSendBackDocumentList(int ProjectId)
        {
            var result = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.ProjectWorkplaceSubSection)
                .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace)
                .Where(x => (x.CreatedBy == x.ProjectWorkplaceSubSecArtificateDocument.CreatedBy && x.CreatedBy == _jwtTokenAccesser.UserId)
                && x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection
                .ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == ProjectId && x.IsSendBack == true)
                .Select(s => new DashboardDto
                {
                    Id = s.Id,
                    TaskInformation = ((WorkPlaceFolder)s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription() + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.DocumentName,
                    CreatedDate = s.CreatedDate,
                    CreatedByUser = _context.Users.Where(x => x.Id == s.UserId).FirstOrDefault().UserName,
                    Module = MyTaskModule.ETMF.GetDescription(),
                    DataType = MyTaskMethodModule.SendBack.GetDescription()
                }).OrderBy(x => x.Id).ToList();

            return result;
        }
    }
}
