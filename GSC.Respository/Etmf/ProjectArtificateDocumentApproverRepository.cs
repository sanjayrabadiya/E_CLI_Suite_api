using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Respository.Etmf
{
    public class ProjectArtificateDocumentApproverRepository : GenericRespository<ProjectArtificateDocumentApprover>, IProjectArtificateDocumentApproverRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        //private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IProjectWorkplaceArtificateRepository _projectWorkplaceArtificateRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IProjectArtificateDocumentHistoryRepository _projectArtificateDocumentHistoryRepository;
        private readonly IAppSettingRepository _appSettingRepository;
        public ProjectArtificateDocumentApproverRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IMapper mapper,
            //IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IProjectWorkplaceArtificateRepository projectWorkplaceArtificateRepository,
            IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
            IEmailSenderRespository emailSenderRespository,
            IUserRepository userRepository,
            IUploadSettingRepository uploadSettingRepository,
            IProjectArtificateDocumentHistoryRepository projectArtificateDocumentHistoryRepository,
            IProjectRightRepository projectRightRepository,
            IAppSettingRepository appSettingRepository)
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
            //_projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
            _projectWorkplaceArtificateRepository = projectWorkplaceArtificateRepository;
            _etmfArtificateMasterLbraryRepository = etmfArtificateMasterLbraryRepository;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectRightRepository = projectRightRepository;
            _projectArtificateDocumentHistoryRepository = projectArtificateDocumentHistoryRepository;
            _appSettingRepository = appSettingRepository;
        }

        // Get UserName for approval
        public List<ProjectArtificateDocumentReviewDto> UserNameForApproval(int Id, int ProjectId, int ProjectDetailsId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone == true && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ProjectArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    SequenceNo = All.FirstOrDefault(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && (b.IsApproved == false || b.IsApproved == null))?.SequenceNo,
                    IsSelected = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null
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

        // Send mail for approver
        public void SendMailForApprover(ProjectArtificateDocumentApproverDto ProjectArtificateDocumentApproverDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                   .ThenInclude(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceArtificatedDocumentId == ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId).FirstOrDefault();
            var ProjectName = project.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.Project.ProjectName;

            //var document = _projectWorkplaceArtificatedocumentRepository.Find(ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId);
            var document = _context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId).FirstOrDefault();
            var artificate = _projectWorkplaceArtificateRepository.FindByInclude(x => x.Id == document.ProjectWorkplaceArtificateId, x => x.EtmfArtificateMasterLbrary).FirstOrDefault();
            var user = _userRepository.Find(ProjectArtificateDocumentApproverDto.UserId);

            _emailSenderRespository.SendApproverEmailOfArtificate(user.Email, user.UserName, document.DocumentName, artificate.EtmfArtificateMasterLbrary.ArtificateName, ProjectName);
        }

        public void SendMailForApprovedRejected(ProjectArtificateDocumentApprover ProjectArtificateDocumentApproverDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                   .ThenInclude(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceArtificatedDocumentId == ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId).FirstOrDefault();
            var ProjectName = project.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.Project.ProjectName;

            //var document = _projectWorkplaceArtificatedocumentRepository.Find(ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId);
            var document = _context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId).FirstOrDefault();
            var artificate = _projectWorkplaceArtificateRepository.FindByInclude(x => x.Id == document.ProjectWorkplaceArtificateId, x => x.EtmfArtificateMasterLbrary).FirstOrDefault();
            var user = _userRepository.Find((int)ProjectArtificateDocumentApproverDto.CreatedBy);
            if (ProjectArtificateDocumentApproverDto.IsApproved == true)
            {
                _emailSenderRespository.SendApprovedEmailOfArtificate(user.Email, user.UserName, document.DocumentName, artificate.EtmfArtificateMasterLbrary.ArtificateName, ProjectName);
            }
            if (ProjectArtificateDocumentApproverDto.IsApproved == false)
            {
                _emailSenderRespository.SendRejectedEmailOfArtificate(user.Email, user.UserName, document.DocumentName, artificate.EtmfArtificateMasterLbrary.ArtificateName, ProjectName);
            }
        }

        // Get data for mytasklist on dashboard
        public List<DashboardDto> GetEtmfMyTaskList(int ProjectId)
        {
            var result = All.Include(x => x.ProjectWorkplaceArtificatedDocument)
                .ThenInclude(x => x.ProjectWorkplaceArtificate)
                .ThenInclude(x => x.EtmfArtificateMasterLbrary)
                .Include(x => x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace)
                .ThenInclude(x => x.EtmfMasterLibrary)
                .Include(x => x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace)
                .ThenInclude(x => x.EtmfMasterLibrary)
                .Include(x => x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace).Where(x => x.DeletedDate == null && x.UserId == _jwtTokenAccesser.UserId && x.IsApproved == null && x.ProjectWorkplaceArtificatedDocument.DeletedDate == null
                 && x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectId == ProjectId && x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceArtificate)
                .Select(s => new DashboardDto
                {
                    Id = s.Id,
                    TaskInformation =
                    ((WorkPlaceFolder)s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.WorkPlaceFolderId).GetDescription() + " | " +
                    (s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ItemName == null ? "" :
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ItemName + " | ") +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.EtmfMasterLibrary.ZonName + " | " +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.EtmfMasterLibrary.SectionName + " | " +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName + " | " +
                    s.ProjectWorkplaceArtificatedDocument.DocumentName,
                    ExtraData = s.ProjectWorkplaceArtificatedDocumentId,
                    CreatedDate = s.CreatedDate,
                    CreatedByUser = _context.Users.Where(x => x.Id == s.CreatedBy).FirstOrDefault().UserName,
                    DueDate = s.DueDate,
                    IsDeleted = s.DueDate == null ? false : s.DueDate.Value.Date < DateTime.Now.Date && (s.IsApproved == null || s.IsApproved == false),
                    Module = "e-TMF",
                    DataType = MyTaskMethodModule.Approved.GetDescription(),
                    Level = 6,
                    ControlType = DashboardMyTaskType.ETMFApproveData
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

            var result = (from approver in _context.ProjectArtificateDocumentApprover.Include(x => x.ProjectArtificateDocumentHistory).Where(x => x.ProjectWorkplaceArtificatedDocumentId == Id && x.UserId != x.CreatedBy)
                          join auditReasonTemp in _context.AuditTrail.Where(x => x.TableName == "ProjectArtificateDocumentApprover" && x.ColumnName == "Is Approved")
                          on approver.Id equals auditReasonTemp.RecordId into auditReasonDto
                          from auditReason in auditReasonDto.DefaultIfEmpty()
                          select new ProjectArtificateDocumentApproverHistory
                          {
                              Id = approver.Id,
                              DocumentName = approver.ProjectArtificateDocumentHistory.Count <= 0 ? "" : approver.ProjectArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().DocumentName,
                              ProjectArtificateDocumentHistoryId = approver.ProjectArtificateDocumentHistory.Count <= 0 ? 0 : approver.ProjectArtificateDocumentHistory.OrderByDescending(y => y.Id).FirstOrDefault().Id,
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




        public List<ProjectArtificateDocumentReviewDto> GetUsers(int Id, int ProjectId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone == true && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ProjectArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    SequenceNo = All.FirstOrDefault(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && (b.IsApproved == false || b.IsApproved == null))?.SequenceNo,
                    IsSelected = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null
                    && (b.IsApproved == true || b.IsApproved == null)),
                }).Where(x => x.IsSelected == false).ToList();

            return users.ToList();
        }

        public int ReplaceUser(int documentId, int actualUserId, int replaceUserId)
        {
            var actualUsers = All.Where(q => q.UserId == actualUserId && q.ProjectWorkplaceArtificatedDocumentId == documentId && q.DeletedDate == null && (q.IsApproved == null || q.IsApproved == false)).ToList();
            if (actualUsers.Count() > 0)
            {
                foreach (var user in actualUsers.Where(s => s.IsApproved == null))
                {
                    var replaceUser = new ProjectArtificateDocumentApprover()
                    {
                        Id = 0,
                        UserId = replaceUserId,
                        CompanyId = user.CompanyId,
                        IsApproved = user.IsApproved,
                        SequenceNo = user.SequenceNo,
                        ProjectWorkplaceArtificatedDocumentId = user.ProjectWorkplaceArtificatedDocumentId
                    };
                    Add(replaceUser);
                    _context.Save();

                    //UpdateApproveDocument(user.ProjectWorkplaceArtificatedDocumentId, false);
                    var projectWorkplaceArtificatedocument = _context.ProjectWorkplaceArtificatedocument.Find(user.ProjectWorkplaceArtificatedDocumentId);
                    _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, All.Max(p => p.Id));

                    var replaceUserDto = _mapper.Map<ProjectArtificateDocumentApproverDto>(replaceUser);

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

        private void UpdateApproveDocument(int documentId, bool IsAccepted)
        {
            var document = _context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == documentId).FirstOrDefault();
            document.IsAccepted = IsAccepted;
            _context.ProjectWorkplaceArtificatedocument.Update(document);
            _context.Save();
        }

        public bool GetApprovePending(int documentId)
        {
            var reviewers = All.Where(x => x.ProjectWorkplaceArtificatedDocumentId == documentId && x.DeletedDate == null && x.SequenceNo == null);
            if (reviewers.Count() == All.Where(x => x.ProjectWorkplaceArtificatedDocumentId == documentId && x.DeletedDate == null).Count())
            {
                return false;
            }
            else
            {
                var reviewer = All.Where(x => x.ProjectWorkplaceArtificatedDocumentId == documentId && x.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null && x.SequenceNo != null).FirstOrDefault();
                if (reviewer == null)
                {
                    var nulldata = All.Where(x => x.ProjectWorkplaceArtificatedDocumentId == documentId && x.DeletedDate == null && x.SequenceNo != null && x.IsApproved == null);
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
                    var numArray = All.Where(x => x.ProjectWorkplaceArtificatedDocumentId == documentId && x.SequenceNo < reviewer.SequenceNo && x.DeletedDate == null && x.SequenceNo != null).Select(s => s.SequenceNo).ToList();
                    if (numArray.Count > 0)
                    {
                        var minseqno = _projectWorkplaceArtificateRepository.ClosestToNumber(numArray, reviewer.SequenceNo.Value);
                        var sendBackReviewers = All.Where(x => x.ProjectWorkplaceArtificatedDocumentId == documentId && x.DeletedDate == null && x.SequenceNo == minseqno).ToList();
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
            var dueDate = All.Where(x => x.DeletedDate == null && (x.IsApproved == null || x.IsApproved == false) && x.ProjectWorkplaceArtificatedDocumentId == documentId && x.SequenceNo != null).OrderByDescending(o => o.SequenceNo).FirstOrDefault();
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
            Add(new ProjectArtificateDocumentApprover
            {
                ProjectWorkplaceArtificatedDocumentId = projectWorkplaceArtificateDocumentId,
                UserId = _jwtTokenAccesser.UserId
            });

            _context.Save();
        }

        public int SkipDocumentApproval(int documentId, bool isApproval)
        {
            var defaultUser = All.FirstOrDefault(x => x.ProjectWorkplaceArtificatedDocumentId == documentId
            && x.DeletedDate == null && x.UserId == x.CreatedBy && x.IsApproved == null);
            if (defaultUser != null)
            {
                defaultUser.IsApproved = true;
                Update(defaultUser);
                if (isApproval)
                {
                    var document = _context.ProjectWorkplaceArtificatedocument.FirstOrDefault(x => x.Id == documentId);
                    document.IsAccepted = true;
                    _context.ProjectWorkplaceArtificatedocument.Update(document);
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
            && x.ProjectWorkplaceArtificatedDocument.DeletedDate == null
            && x.DueDate.Value.Date == compareDate).ToListAsync();
            foreach (var due in dueDates)
            {
                var user = await _context.Users.FindAsync((int)due.UserId);
                var document = await _context.ProjectWorkplaceArtificatedocument.FindAsync(due.ProjectWorkplaceArtificatedDocumentId);
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
