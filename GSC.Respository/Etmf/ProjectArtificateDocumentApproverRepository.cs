using AutoMapper;
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
    public class ProjectArtificateDocumentApproverRepository : GenericRespository<ProjectArtificateDocumentApprover>, IProjectArtificateDocumentApproverRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IProjectWorkplaceArtificateRepository _projectWorkplaceArtificateRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        public ProjectArtificateDocumentApproverRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IMapper mapper,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IProjectWorkplaceArtificateRepository projectWorkplaceArtificateRepository,
            IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
            IEmailSenderRespository emailSenderRespository,
            IUserRepository userRepository,
            IUploadSettingRepository uploadSettingRepository,
            IProjectRightRepository projectRightRepository)
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
            _projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
            _projectWorkplaceArtificateRepository = projectWorkplaceArtificateRepository;
            _etmfArtificateMasterLbraryRepository = etmfArtificateMasterLbraryRepository;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectRightRepository = projectRightRepository;
        }

        // Get UserName for approval
        public List<ProjectArtificateDocumentReviewDto> UserNameForApproval(int Id, int ProjectId)
        {
            //var users = _context.Users.Where(x => x.DeletedDate == null && x.Id != _jwtTokenAccesser.UserId && x.UserType == UserMasterUserType.User)
            //    .Select(c => new ProjectArtificateDocumentReviewDto
            //    {
            //        UserId = c.Id,
            //        Name = c.UserName,
            //        IsSelected = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.Id && b.DeletedDate == null
            //        && (b.IsApproved == true || b.IsApproved == null)),
            //    }).Where(x => x.IsSelected == false).ToList();

            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ProjectArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    IsSelected = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null
                     && (b.IsApproved == true || b.IsApproved == null)),
                }).Where(x => x.IsSelected == false).ToList();

            return users;
        }

        // Send mail for approver
        public void SendMailForApprover(ProjectArtificateDocumentApproverDto ProjectArtificateDocumentApproverDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                   .ThenInclude(x => x.ProjectWorkplaceArtificate)
                   .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                   .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace)
                   .ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceArtificatedDocumentId == ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId).FirstOrDefault();
            var ProjectName = project.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectName;

            var document = _projectWorkplaceArtificatedocumentRepository.Find(ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId);
            var artificate = _projectWorkplaceArtificateRepository.FindByInclude(x => x.Id == document.ProjectWorkplaceArtificateId, x => x.EtmfArtificateMasterLbrary).FirstOrDefault();
            var user = _userRepository.Find(ProjectArtificateDocumentApproverDto.UserId);

            _emailSenderRespository.SendApproverEmailOfArtificate(user.Email, user.UserName, document.DocumentName, artificate.EtmfArtificateMasterLbrary.ArtificateName, ProjectName);
        }

        // Get data for mytasklist on dashboard
        public List<DashboardDto> GetEtmfMyTaskList(int ProjectId)
        {
            var result = All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.IsApproved == null && x.ProjectWorkplaceArtificatedDocument.DeletedDate == null
                 && x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection
                .ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == ProjectId)
                .Select(s => new DashboardDto
                {
                    Id = s.Id,
                    TaskInformation =
                    ((WorkPlaceFolder)s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription() + " | " +
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
                    DataType = MyTaskMethodModule.Approved.GetDescription(),
                    Level = 6
                }).OrderByDescending(x => x.CreatedDate).ToList();

            return result;
        }

        // Get atrificate doc approver history
        public List<ProjectArtificateDocumentApproverHistory> GetArtificateDocumentApproverHistory(int Id)
        {
            //var result = All.Include(x => x.ProjectWorkplaceArtificatedDocument).Include(x => x.ProjectArtificateDocumentHistory)
            //    .Where(x => x.ProjectWorkplaceArtificatedDocumentId == Id)
            //    .Select(x => new ProjectArtificateDocumentApproverHistory
            //    {
            //        Id = x.Id,
            //        DocumentName = x.ProjectArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().DocumentName,
            //        ProjectArtificateDocumentHistoryId = x.ProjectArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().Id,
            //        UserName = _context.Users.Where(y => y.Id == x.UserId && y.DeletedDate == null).FirstOrDefault().UserName,
            //        UserId = x.UserId,
            //        IsApproved = x.IsApproved,
            //        ProjectWorkplaceArtificatedDocumentId = x.ProjectWorkplaceArtificatedDocumentId,
            //        CreatedDate = x.CreatedDate,
            //        CreatedByUser = x.CreatedByUser.UserName,
            //        ModifiedDate = x.ModifiedDate,
            //        ModifiedByUser = x.ModifiedByUser.UserName,
            //        Comment = x.Comment,
            //    }).OrderByDescending(x => x.Id).ToList();

            var result = (from approver in _context.ProjectArtificateDocumentApprover.Include(x => x.ProjectArtificateDocumentHistory).Where(x => x.ProjectWorkplaceArtificatedDocumentId == Id)
                          join auditReasonTemp in _context.AuditTrailCommon.Where(x => x.TableName == "ProjectArtificateDocumentApprover" && x.ColumnName == "Is Approved")
                          on approver.Id equals auditReasonTemp.RecordId into auditReasonDto
                          from auditReason in auditReasonDto.DefaultIfEmpty()
                          select new ProjectArtificateDocumentApproverHistory
                          {
                              Id = approver.Id,
                              DocumentName = approver.ProjectArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().DocumentName,
                              ProjectArtificateDocumentHistoryId = approver.ProjectArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().Id,
                              UserName = _context.Users.Where(y => y.Id == approver.UserId && y.DeletedDate == null).FirstOrDefault().UserName,
                              UserId = approver.UserId,
                              IsApproved = approver.IsApproved,
                              ProjectWorkplaceArtificatedDocumentId = approver.ProjectWorkplaceArtificatedDocumentId,
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

        // update approve document
        public void IsApproveDocument(int Id)
        {
            var DocumentApprover = All.Where(x => x.ProjectWorkplaceArtificatedDocumentId == Id
           && x.DeletedDate == null).OrderByDescending(x => x.Id).ToList().GroupBy(x => x.UserId).Select(x => new ProjectArtificateDocumentApprover
           {
               Id = x.FirstOrDefault().Id,
               IsApproved = x.FirstOrDefault().IsApproved,
               ProjectWorkplaceArtificatedDocumentId = x.FirstOrDefault().ProjectWorkplaceArtificatedDocumentId
           }).ToList();

            if (DocumentApprover.All(x => x.IsApproved == true))
            {
                _projectWorkplaceArtificatedocumentRepository.UpdateApproveDocument(Id, true);
            }
        }
    }
}
