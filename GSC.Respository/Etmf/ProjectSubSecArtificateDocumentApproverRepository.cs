using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
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
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Etmf
{
    public class ProjectSubSecArtificateDocumentApproverRepository : GenericRespository<ProjectSubSecArtificateDocumentApprover>, IProjectSubSecArtificateDocumentApproverRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectWorkplaceSubSecArtificatedocumentRepository _projectWorkplaceSubSecArtificatedocumentRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        private readonly IGSCContext _context;
        private readonly IProjectRightRepository _projectRightRepository;
        public ProjectSubSecArtificateDocumentApproverRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
           IProjectWorkplaceSubSecArtificatedocumentRepository projectWorkplaceSubSecArtificatedocumentRepository,
           IEmailSenderRespository emailSenderRespository,
           IUserRepository userRepository, IProjectRightRepository projectRightRepository)
           : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectWorkplaceSubSecArtificatedocumentRepository = projectWorkplaceSubSecArtificatedocumentRepository;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _projectRightRepository = projectRightRepository;
        }

        public List<ProjectSubSecArtificateDocumentReviewDto> UserNameForApproval(int Id, int ProjectId, int ProjectDetailsId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone == true && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ProjectSubSecArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    IsSelected = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null
                     && (b.IsApproved == true || b.IsApproved == null)),
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

        public void SendMailForApprover(ProjectSubSecArtificateDocumentApproverDto ProjectSubSecArtificateDocumentApproverDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                   .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.ProjectWorkplaceSubSection)
                   .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                   .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace)
                   .ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == ProjectSubSecArtificateDocumentApproverDto.ProjectWorkplaceSubSecArtificateDocumentId).FirstOrDefault();

            var ProjectName = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectName;
            var Document = project.ProjectWorkplaceSubSecArtificateDocument.DocumentName;
            var Artificate = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName;
            var User = _userRepository.Find(ProjectSubSecArtificateDocumentApproverDto.UserId);

            _emailSenderRespository.SendApproverEmailOfArtificate(User.Email, User.UserName, Document, Artificate, ProjectName);
        }

        public List<DashboardDto> GetEtmfMyTaskList(int ProjectId)
        {
            var result = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.ProjectWorkplaceSubSection)
                .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace)
                .Where(x => x.DeletedDate == null && x.UserId == _jwtTokenAccesser.UserId && x.IsApproved == null && x.ProjectWorkplaceSubSecArtificateDocument.DeletedDate == null
                && x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection
                .ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == ProjectId)
                .Select(s => new DashboardDto
                {
                    Id = s.Id,
                    TaskInformation =
                    ((WorkPlaceFolder)s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription() + " | " +
                    (s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemName == null ? "" :
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ItemName + " | ") +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkplaceSubSection.SubSectionName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName + " | " +
                    s.ProjectWorkplaceSubSecArtificateDocument.DocumentName,
                    ExtraData = s.ProjectWorkplaceSubSecArtificateDocumentId,
                    CreatedDate = s.CreatedDate,
                    CreatedByUser = _context.Users.Where(x => x.Id == s.CreatedBy).FirstOrDefault().UserName,
                    Module = MyTaskModule.ETMF.GetDescription(),
                    DataType = MyTaskMethodModule.Approved.GetDescription(),
                    Level = 5.2
                }).OrderByDescending(x => x.CreatedDate).ToList();

            return result;
        }

        public List<ProjectSubSecArtificateDocumentApproverHistory> GetArtificateDocumentApproverHistory(int Id)
        {
            //var result = All.Include(x => x.ProjectWorkplaceSubSecArtificateDocument).Include(x => x.ProjectSubSecArtificateDocumentHistory).Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == Id)
            //    .Select(x => new ProjectSubSecArtificateDocumentApproverHistory
            //    {
            //        Id = x.Id,
            //        DocumentName = x.ProjectSubSecArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().DocumentName,
            //        ProjectArtificateDocumentHistoryId = x.ProjectSubSecArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().Id,
            //        UserName = _context.Users.Where(y => y.Id == x.UserId && y.DeletedDate == null).FirstOrDefault().UserName,
            //        UserId = x.UserId,
            //        IsApproved = x.IsApproved,
            //        ProjectWorkplaceSubSecArtificateDocumentId = x.ProjectWorkplaceSubSecArtificateDocumentId,
            //        CreatedDate = x.CreatedDate,
            //        CreatedByUser = _context.Users.Where(y => y.Id == x.CreatedBy && y.DeletedDate == null).FirstOrDefault().UserName,
            //        ModifiedDate = x.ModifiedDate,
            //        ModifiedByUser = _context.Users.Where(y => y.Id == x.ModifiedBy && y.DeletedDate == null).FirstOrDefault().UserName,
            //        Comment = x.Comment
            //    }).OrderByDescending(x => x.Id).ToList();

            var result = (from approver in _context.ProjectSubSecArtificateDocumentApprover.Include(x => x.ProjectWorkplaceSubSecArtificateDocument).Include(x => x.ProjectSubSecArtificateDocumentHistory)
                          .Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == Id)
                          join auditReasonTemp in _context.AuditTrail.Where(x => x.TableName == "ProjectSubSecArtificateDocumentApprover" && x.ColumnName == "Is Approved")
                          on approver.Id equals auditReasonTemp.RecordId into auditReasonDto
                          from auditReason in auditReasonDto.DefaultIfEmpty()
                          select new ProjectSubSecArtificateDocumentApproverHistory
                          {
                              Id = approver.Id,
                              DocumentName = approver.ProjectSubSecArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().DocumentName,
                              ProjectArtificateDocumentHistoryId = approver.ProjectSubSecArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().Id,
                              UserName = _context.Users.Where(y => y.Id == approver.UserId && y.DeletedDate == null).FirstOrDefault().UserName,
                              UserId = approver.UserId,
                              IsApproved = approver.IsApproved,
                              ProjectWorkplaceSubSecArtificateDocumentId = approver.ProjectWorkplaceSubSecArtificateDocumentId,
                              CreatedDate = approver.CreatedDate,
                              CreatedByUser = approver.CreatedByUser.UserName,
                              ModifiedDate = approver.ModifiedDate,
                              ModifiedByUser = approver.ModifiedByUser.UserName,
                              Comment = approver.Comment,
                              Reason = auditReason.Reason,
                              ReasonOth = auditReason.ReasonOth
                          }).OrderByDescending(x => x.Id).ToList();

            return result;
        }

        public void IsApproveDocument(int Id)
        {
            var DocumentApprover = All.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == Id
            && x.DeletedDate == null).OrderByDescending(x => x.Id).ToList().GroupBy(x => x.UserId).Select(x => new ProjectSubSecArtificateDocumentApprover
            {
                Id = x.FirstOrDefault().Id,
                IsApproved = x.FirstOrDefault().IsApproved,
                ProjectWorkplaceSubSecArtificateDocumentId = x.FirstOrDefault().ProjectWorkplaceSubSecArtificateDocumentId
            }).ToList();

            if (DocumentApprover.All(x => x.IsApproved == true))
            {
                _projectWorkplaceSubSecArtificatedocumentRepository.UpdateApproveDocument(Id, true);
            }
        }
    }
}
