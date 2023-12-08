using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
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
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Respository.Etmf
{
    public class ProjectSubSecArtificateDocumentApproverRepository : GenericRespository<ProjectSubSecArtificateDocumentApprover>, IProjectSubSecArtificateDocumentApproverRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        //private readonly IProjectWorkplaceSubSecArtificatedocumentRepository _projectWorkplaceSubSecArtificatedocumentRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        private readonly IGSCContext _context;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IProjectWorkplaceArtificateRepository _projectWorkplaceArtificateRepository;
        private readonly IProjectSubSecArtificateDocumentHistoryRepository _projectSubSecArtificateDocumentHistoryRepository;
        private readonly IMapper _mapper;
        private readonly IAppSettingRepository _appSettingRepository;
        public ProjectSubSecArtificateDocumentApproverRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
           //IProjectWorkplaceSubSecArtificatedocumentRepository projectWorkplaceSubSecArtificatedocumentRepository,
           IEmailSenderRespository emailSenderRespository,
           IProjectWorkplaceArtificateRepository projectWorkplaceArtificateRepository,
           IProjectSubSecArtificateDocumentHistoryRepository projectSubSecArtificateDocumentHistoryRepository,
           IMapper mapper,
           IUserRepository userRepository, IProjectRightRepository projectRightRepository, IAppSettingRepository appSettingRepository)
           : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            //_projectWorkplaceSubSecArtificatedocumentRepository = projectWorkplaceSubSecArtificatedocumentRepository;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _projectRightRepository = projectRightRepository;
            _projectWorkplaceArtificateRepository = projectWorkplaceArtificateRepository;
            _mapper = mapper;
            _projectSubSecArtificateDocumentHistoryRepository = projectSubSecArtificateDocumentHistoryRepository;
            _appSettingRepository = appSettingRepository;
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
                    SequenceNo = All.FirstOrDefault(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && (b.IsApproved == false || b.IsApproved == null))?.SequenceNo,
                    IsSelected = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null
                    && (b.IsApproved == true || b.IsApproved == null)),
                }).Where(x => x.IsSelected == false).ToList();

            users.ForEach(x =>
            {
                x.TempSeqNo = x.SequenceNo;
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
                   .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact)
                   .ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == ProjectSubSecArtificateDocumentApproverDto.ProjectWorkplaceSubSecArtificateDocumentId).FirstOrDefault();

            var ProjectName = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.Project.ProjectName;
            var Document = project.ProjectWorkplaceSubSecArtificateDocument.DocumentName;
            var Artificate = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName;
            var User = _userRepository.Find(ProjectSubSecArtificateDocumentApproverDto.UserId);

            _emailSenderRespository.SendApproverEmailOfArtificate(User.Email, User.UserName, Document, Artificate, ProjectName);
        }
        public void SendMailForApprovedRejected(ProjectSubSecArtificateDocumentApprover ProjectSubSecArtificateDocumentApproverDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                  .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact)
                  .ThenInclude(x => x.Project)
                  .Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == ProjectSubSecArtificateDocumentApproverDto.ProjectWorkplaceSubSecArtificateDocumentId).FirstOrDefault();

            var ProjectName = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.Project.ProjectName;
            var Document = project.ProjectWorkplaceSubSecArtificateDocument.DocumentName;
            var Artificate = project.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName;
            var user = _userRepository.Find((int)ProjectSubSecArtificateDocumentApproverDto.CreatedBy);
            if (ProjectSubSecArtificateDocumentApproverDto.IsApproved == true)
            {
                _emailSenderRespository.SendApprovedEmailOfArtificate(user.Email, user.UserName, Document, Artificate, ProjectName);
            }
            if (ProjectSubSecArtificateDocumentApproverDto.IsApproved == false)
            {
                _emailSenderRespository.SendRejectedEmailOfArtificate(user.Email, user.UserName, Document, Artificate, ProjectName);
            }
        }

        public List<DashboardDto> GetEtmfMyTaskList(int ProjectId)
        {
            var result = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                 .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact)
                 .ThenInclude(x => x.ProjectWorkPlace) // Sub Section
                 .ThenInclude(x => x.ProjectWorkPlace) // Section
                 .ThenInclude(x => x.EtmfMasterLibrary) // Etmf Section
                 .Include(x => x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace)
                 .ThenInclude(x => x.EtmfMasterLibrary)
                 .Include(x => x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace)
                 .Where(x => x.DeletedDate == null && x.UserId == _jwtTokenAccesser.UserId && x.IsApproved == null && x.ProjectWorkplaceSubSecArtificateDocument.DeletedDate == null
                 && x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectId == ProjectId && x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceSubSectionArtifact)
                 .Select(s => new DashboardDto
                 {
                     Id = s.Id,
                     TaskInformation =
                     ((WorkPlaceFolder)s.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription() + " | " +
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
                     Module = "e-TMF",
                     DataType = MyTaskMethodModule.Approved.GetDescription(),
                     Level = 5.2,
                     DueDate = s.DueDate,
                     IsDeleted = s.DueDate == null ? false : s.DueDate.Value.Date < DateTime.Now.Date && (s.IsApproved == null || s.IsApproved == false),
                     ControlType = DashboardMyTaskType.ETMFSubSecApproveData
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
                              //DocumentName = approver.ProjectSubSecArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().DocumentName,
                              //ProjectArtificateDocumentHistoryId = approver.ProjectSubSecArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().Id,
                              //UserName = _context.Users.Where(y => y.Id == approver.UserId && y.DeletedDate == null).FirstOrDefault().UserName,
                              DocumentName = approver.ProjectSubSecArtificateDocumentHistory.Count <= 0 ? "" : approver.ProjectSubSecArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().DocumentName,
                              ProjectArtificateDocumentHistoryId = approver.ProjectSubSecArtificateDocumentHistory.Count <= 0 ? 0 : approver.ProjectSubSecArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().Id,
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

        public List<ProjectSubSecArtificateDocumentReviewDto> GetUsers(int Id, int ProjectId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone == true && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ProjectSubSecArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    SequenceNo = All.FirstOrDefault(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && (b.IsApproved == false || b.IsApproved == null))?.SequenceNo,
                    IsSelected = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null
                    && (b.IsApproved == true || b.IsApproved == null)),
                }).Where(x => x.IsSelected == false).ToList();

            return users.ToList();
        }

        public int ReplaceUser(int documentId, int actualUserId, int replaceUserId)
        {
            var actualUsers = All.Where(q => q.UserId == actualUserId && q.ProjectWorkplaceSubSecArtificateDocumentId == documentId && q.DeletedDate == null && (q.IsApproved == null || q.IsApproved == false)).ToList();
            if (actualUsers.Count() > 0)
            {
                foreach (var user in actualUsers.Where(s => s.IsApproved == null))
                {
                    var replaceUser = new ProjectSubSecArtificateDocumentApprover()
                    {
                        Id = 0,
                        UserId = replaceUserId,
                        CompanyId = user.CompanyId,
                        IsApproved = user.IsApproved,
                        SequenceNo = user.SequenceNo,
                        ProjectWorkplaceSubSecArtificateDocumentId = user.ProjectWorkplaceSubSecArtificateDocumentId
                    };
                    Add(replaceUser);
                    _context.Save();

                    //_projectWorkplaceSubSecArtificatedocumentRepository.UpdateApproveDocument(user.ProjectWorkplaceSubSecArtificateDocumentId, false);
                    var projectWorkplaceArtificatedocument = _context.ProjectWorkplaceSubSecArtificatedocument.Find(user.ProjectWorkplaceSubSecArtificateDocumentId);
                    _projectSubSecArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, All.Max(p => p.Id));

                    var replaceUserDto = _mapper.Map<ProjectSubSecArtificateDocumentApproverDto>(replaceUser);

                    SendMailForApprover(replaceUserDto);
                }

                foreach (var user in actualUsers)
                {
                    Delete(user);
                }

                _context.Save();

                return 1;
            }

            return 0;
        }

        public bool GetApprovePending(int documentId)
        {
            var reviewers = All.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == documentId && x.DeletedDate == null && x.SequenceNo == null);
            if (reviewers.Count() == All.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == documentId && x.DeletedDate == null).Count())
            {
                return false;
            }
            else
            {
                var reviewer = All.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == documentId && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null && x.SequenceNo != null).FirstOrDefault();
                if (reviewer == null)
                {
                    var nulldata = All.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == documentId && x.DeletedDate == null && x.SequenceNo != null && x.IsApproved == null);
                    if (nulldata.Count() > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var numArray = All.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == documentId && x.SequenceNo < reviewer.SequenceNo && x.DeletedDate == null && x.SequenceNo != null).Select(s => s.SequenceNo).ToList();
                    if (numArray.Count > 0)
                    {
                        var minseqno = _projectWorkplaceArtificateRepository.ClosestToNumber(numArray, reviewer.SequenceNo.Value);
                        var sendBackReviewers = All.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == documentId && x.DeletedDate == null && x.SequenceNo == minseqno).ToList();
                        var result = sendBackReviewers.Any(x => x.IsApproved == true);
                        return (!result);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }


        public DateTime? GetMaxDueDate(int documentId)
        {
            var dueDate = All.Where(x => x.DeletedDate == null && (x.IsApproved == null || x.IsApproved == false) && x.ProjectWorkplaceSubSecArtificateDocumentId == documentId && x.SequenceNo != null).OrderByDescending(o => o.SequenceNo).FirstOrDefault();
            if (dueDate != null)
            {
                return dueDate.DueDate.Value.AddDays(1);
            }
            else
            {
                return DateTime.Now.Date;
            }
        }

        public void SaveByDocumentIdInApprove(int projectWorkplaceArtificateDocumentId)
        {
            Add(new ProjectSubSecArtificateDocumentApprover
            {
                ProjectWorkplaceSubSecArtificateDocumentId = projectWorkplaceArtificateDocumentId,
                UserId = _jwtTokenAccesser.UserId
            });

            _context.Save();
        }

        public int SkipDocumentApproval(int documentId, bool isApproval)
        {
            var defaultUser = All.FirstOrDefault(x => x.ProjectWorkplaceSubSecArtificateDocumentId == documentId
            && x.DeletedDate == null && x.UserId == x.CreatedBy && x.IsApproved == null);
            if (defaultUser != null)
            {
                defaultUser.IsApproved = true;
                Update(defaultUser);
                if (isApproval)
                {
                    var document = _context.ProjectWorkplaceSubSecArtificatedocument.FirstOrDefault(x => x.Id == documentId);
                    document.IsAccepted = true;
                    _context.ProjectWorkplaceSubSecArtificatedocument.Update(document);
                }
                return _context.Save();
            }

            return 0;
        }

        public async Task SendDueApproveEmail()
        {
            var commonSettiongs = _appSettingRepository.Get<GeneralSettingsDto>(2);
            var compareDate = DateTime.Now.Date.AddDays(Convert.ToDouble(commonSettiongs.EtmfScheduleDueDate));
            var dueDates = await All.Where(x => x.DeletedDate == null && x.DueDate != null
            && x.UserId != x.CreatedBy && (x.IsApproved == false || x.IsApproved == null)
            && x.ProjectWorkplaceSubSecArtificateDocument.DeletedDate == null
            && x.DueDate.Value.Date == compareDate).ToListAsync();
            foreach (var due in dueDates)
            {
                var user = await _context.Users.FindAsync((int)due.UserId);
                var document = await _context.ProjectWorkplaceArtificatedocument.FindAsync(due.ProjectWorkplaceSubSecArtificateDocumentId);
                var artificate = await _context.EtmfProjectWorkPlace.FindAsync(document.ProjectWorkplaceArtificateId);
                string artificateName = artificate.ArtifactName;
                if (artificate.EtmfArtificateMasterLbraryId > 0)
                {
                    var EtfArtificate = await _context.EtmfArtificateMasterLbrary.FindAsync(artificate.EtmfArtificateMasterLbraryId);
                    artificateName = EtfArtificate.ArtificateName;
                }
                var project = await _context.Project.FindAsync(artificate.ProjectId);
                _emailSenderRespository.SendEmailOfApproveDue(user.Email, user.UserName, document.DocumentName, artificateName, project.ProjectCode);
            }
        }
    }
}
