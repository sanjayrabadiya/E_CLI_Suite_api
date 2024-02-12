using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
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
using System.Threading.Tasks;

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
        private readonly IMapper _mapper;
        private readonly IAppSettingRepository _appSettingRepository;

        public ProjectWorkplaceArtificateDocumentReviewRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
           IProjectWorkplaceArtificateRepository projectWorkplaceArtificateRepository,
           IEmailSenderRespository emailSenderRespository,
           IUserRepository userRepository,
            IMapper mapper,
           IProjectArtificateDocumentHistoryRepository projectArtificateDocumentHistoryRepository,
           IProjectRightRepository projectRightRepository,
            IAppSettingRepository appSettingRepository
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
            _mapper = mapper;
            _appSettingRepository = appSettingRepository;
        }

        public List<ProjectArtificateDocumentReviewDto> UserRoles(int Id, int ProjectId, int ProjectDetailsId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone == true && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users1 = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId);

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ProjectArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    IsReview = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsReviewed == true),
                    SequenceNo = All.FirstOrDefault(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsSendBack == true)?.SequenceNo,
                    IsSelected = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null),
                }).Where(x => x.IsSelected == false && x.IsReview == false).ToList();

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

        public void SaveDocumentReview(List<ProjectArtificateDocumentReviewDto> pojectArtificateDocumentReviewDto)
        {
            foreach (var ReviewDto in pojectArtificateDocumentReviewDto)
            {
                if (ReviewDto.IsSelected)
                {
                    Add(new ProjectArtificateDocumentReview
                    {
                        ProjectWorkplaceArtificatedDocumentId = ReviewDto.ProjectWorkplaceArtificatedDocumentId,
                        UserId = ReviewDto.UserId,
                        IsSendBack = false,
                        Message = ReviewDto.Message,
                        SequenceNo = ReviewDto.SequenceNo,
                        DueDate = ReviewDto.DueDate
                    });
                    if (_context.Save() < 0) throw new Exception("Artificate Send failed on save.");

                    //var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.Find(ReviewDto.ProjectWorkplaceArtificatedDocumentId);
                    var projectWorkplaceArtificatedocument = _context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == ReviewDto.ProjectWorkplaceArtificatedDocumentId && x.DeletedDate == null).FirstOrDefault();
                    _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, All.Max(p => p.Id), null);
                }
            }

            if (pojectArtificateDocumentReviewDto.Where(x => x.SequenceNo == null && x.IsSelected).Count() == pojectArtificateDocumentReviewDto.Where(x => x.IsSelected).Count())
            {
                foreach (var ReviewDto in pojectArtificateDocumentReviewDto)
                {
                    if (ReviewDto.IsSelected)
                    {
                        SendMailToReviewer(ReviewDto);
                    }
                }
            }
            else
            {
                var firstRecord = pojectArtificateDocumentReviewDto.Where(q => q.IsSelected).OrderBy(x => x.SequenceNo).FirstOrDefault();
                if (firstRecord.IsReview == false)
                    SendMailToReviewer(firstRecord);
            }


            var defaultUser = All.FirstOrDefault(x => x.ProjectWorkplaceArtificatedDocumentId == pojectArtificateDocumentReviewDto.FirstOrDefault().ProjectWorkplaceArtificatedDocumentId
            && x.DeletedDate == null && x.UserId == x.CreatedBy && x.IsReviewed == false);
            if (defaultUser != null)
            {
                defaultUser.SendBackDate = DateTime.Now;
                defaultUser.IsSendBack = true;
                defaultUser.IsReviewed = true;

                Update(defaultUser);
                _context.Save();
            }
        }

        // Send mail for review
        public void SendMailToReviewer(ProjectArtificateDocumentReviewDto ReviewDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                   .ThenInclude(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceArtificatedDocumentId == ReviewDto.ProjectWorkplaceArtificatedDocumentId).FirstOrDefault();
            var ProjectName = project.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.Project.ProjectName;
            //var document = _projectWorkplaceArtificatedocumentRepository.Find(ReviewDto.ProjectWorkplaceArtificatedDocumentId);
            var document = _context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == ReviewDto.ProjectWorkplaceArtificatedDocumentId && x.DeletedDate == null).FirstOrDefault();
            var artificate = _projectWorkplaceArtificateRepository.FindByInclude(x => x.Id == document.ProjectWorkplaceArtificateId, x => x.EtmfArtificateMasterLbrary).FirstOrDefault();
            var user = _userRepository.Find(ReviewDto.UserId);
            _emailSenderRespository.SendEmailOfReview(user.Email, user.UserName, document.DocumentName, artificate.EtmfArtificateMasterLbrary.ArtificateName, ProjectName);
        }

        // Send mail for sendback
        public void SendMailToSendBack(ProjectArtificateDocumentReview ReviewDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                   .ThenInclude(x => x.ProjectWorkplaceArtificate).ThenInclude(x => x.Project)
                   .Where(x => x.ProjectWorkplaceArtificatedDocumentId == ReviewDto.ProjectWorkplaceArtificatedDocumentId).FirstOrDefault();
            var ProjectName = project.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.Project.ProjectName;
            //var document = _projectWorkplaceArtificatedocumentRepository.Find(ReviewDto.ProjectWorkplaceArtificatedDocumentId);
            var document = _context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == ReviewDto.ProjectWorkplaceArtificatedDocumentId && x.DeletedDate == null).FirstOrDefault();
            var artificate = _projectWorkplaceArtificateRepository.FindByInclude(x => x.Id == document.ProjectWorkplaceArtificateId, x => x.EtmfArtificateMasterLbrary).FirstOrDefault();
            var user = _userRepository.Find((int)ReviewDto.CreatedBy);
            if (ReviewDto.IsReviewed)
            {
                _emailSenderRespository.SendEmailOfReviewed(user.Email, user.UserName, document.DocumentName, artificate.EtmfArtificateMasterLbrary.ArtificateName, ProjectName);
            }
            else
            {
                _emailSenderRespository.SendEmailOfSendBack(user.Email, user.UserName, document.DocumentName, artificate.EtmfArtificateMasterLbrary.ArtificateName, ProjectName);
            }
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
            var result = All.Include(x => x.ProjectWorkplaceArtificatedDocument)
                .ThenInclude(x => x.ProjectWorkplaceArtificate)
                .ThenInclude(x => x.EtmfArtificateMasterLbrary)
                .Include(x => x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace)
                .ThenInclude(x => x.EtmfMasterLibrary)
                .Include(x => x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace)
                .ThenInclude(x => x.EtmfMasterLibrary)
                .Include(x => x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace)
                           .Where(x => x.DeletedDate == null && (x.UserId != x.ProjectWorkplaceArtificatedDocument.CreatedBy && x.UserId == _jwtTokenAccesser.UserId)
                           && x.ProjectWorkplaceArtificatedDocument.DeletedDate == null
                           && x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectId == ProjectId && x.IsSendBack == false && x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceArtificate)
                           .Select(s => new DashboardDto
                           {
                               Id = s.Id,
                               TaskInformation = ((WorkPlaceFolder)s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription() + " | " +
                               (s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName == null ? "" :
                               s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName + " | ") +
                               s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName + " | " +
                               s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.EtmfMasterLibrary.SectionName + " | " +
                               s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName + " | " +
                               s.ProjectWorkplaceArtificatedDocument.DocumentName,
                               ExtraData = s.ProjectWorkplaceArtificatedDocumentId,
                               CreatedDate = s.CreatedDate,
                               CreatedByUser = _context.Users.Where(x => x.Id == s.CreatedBy).FirstOrDefault().UserName,
                               Module = "e-TMF",
                               DueDate = s.DueDate,
                               IsDeleted = s.DueDate == null ? false : s.DueDate.Value.Date < DateTime.Now.Date && s.IsReviewed == false,
                               DataType = MyTaskMethodModule.Reviewed.GetDescription(),
                               Level = 6,
                               ControlType = DashboardMyTaskType.ETMFSendData
                           }).OrderByDescending(x => x.CreatedDate).ToList();

            return result;
        }

        public List<DashboardDto> GetSendBackDocumentList(int ProjectId)
        {
            var result = All.Include(x => x.ProjectWorkplaceArtificatedDocument)
                .ThenInclude(x => x.ProjectWorkplaceArtificate)
                .ThenInclude(x => x.EtmfArtificateMasterLbrary)
                .Include(x => x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace)
                .ThenInclude(x => x.EtmfMasterLibrary)
                .Include(x => x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace)
                .ThenInclude(x => x.EtmfMasterLibrary)
                .Include(x => x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace)
                .Where(x => x.DeletedDate == null && (x.CreatedBy == x.ProjectWorkplaceArtificatedDocument.CreatedBy && x.CreatedBy == _jwtTokenAccesser.UserId)
                && x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectId == ProjectId && x.IsSendBack == true && x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceArtificate)
                .Select(s => new DashboardDto
                {
                    Id = s.Id,
                    DocumentId = s.ProjectWorkplaceArtificatedDocumentId,
                    TaskInformation = ((WorkPlaceFolder)s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId).GetDescription() + " | " +
                    (s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName == null ? "" :
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName + " | ") +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName + " | " +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.EtmfMasterLibrary.SectionName + " | " +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName + " | " +
                    s.ProjectWorkplaceArtificatedDocument.DocumentName,
                    CreatedDate = s.CreatedDate,
                    CreatedByUser = _context.Users.Where(x => x.Id == s.UserId).FirstOrDefault().UserName,
                    Module = "e-TMF",
                    DataType = MyTaskMethodModule.SendBack.GetDescription(),
                    ControlType = DashboardMyTaskType.ETMFSendBackData
                }).OrderByDescending(x => x.CreatedDate).ToList();

            result.ForEach(s =>
            {
                s.ExtraData = _context.ProjectArtificateDocumentApprover.Where(x => x.DeletedDate == null && x.ProjectWorkplaceArtificatedDocumentId == s.DocumentId).Count();
            });

            return result.Where(x => Convert.ToInt32(x.ExtraData) == 0).ToList();
        }

        public List<ProjectArtificateDocumentReviewDto> GetUsers(int Id, int ProjectId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone == true && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            //var users1 = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId);

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ProjectArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    IsReview = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsReviewed == true),
                    SequenceNo = All.FirstOrDefault(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsSendBack == true)?.SequenceNo,
                    IsSelected = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null),
                }).Where(x => x.IsSelected == false && x.IsReview == false).ToList();

            return users.ToList();
        }

        public int ReplaceUser(int documentId, int actualUserId, int replaceUserId)
        {
            var actualUsers = All.Where(q => q.UserId == actualUserId && q.ProjectWorkplaceArtificatedDocumentId == documentId && q.DeletedDate == null && q.IsReviewed == false).ToList();
            if (actualUsers.Count() > 0)
            {
                foreach (var user in actualUsers.Where(s => s.IsSendBack == false))
                {
                    var replaceUser = new ProjectArtificateDocumentReview()
                    {
                        Id = 0,
                        UserId = replaceUserId,
                        CompanyId = user.CompanyId,
                        IsReviewed = user.IsReviewed,
                        IsSendBack = user.IsSendBack,
                        Message = user.Message,
                        RoleId = user.RoleId,
                        SendBackDate = user.SendBackDate,
                        SequenceNo = user.SequenceNo,
                        ProjectWorkplaceArtificatedDocumentId = user.ProjectWorkplaceArtificatedDocumentId
                    };
                    Add(replaceUser);

                    _context.Save();

                    var projectWorkplaceArtificatedocument = _context.ProjectWorkplaceArtificatedocument.Where(x => x.Id == user.ProjectWorkplaceArtificatedDocumentId && x.DeletedDate == null).FirstOrDefault();
                    _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, All.Max(p => p.Id), null);

                    var replaceUserDto = _mapper.Map<ProjectArtificateDocumentReviewDto>(replaceUser);

                    SendMailToReviewer(replaceUserDto);
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

        public bool GetReviewPending(int documentId)
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
                    var nulldata = All.Where(x => x.ProjectWorkplaceArtificatedDocumentId == documentId && x.DeletedDate == null && x.SequenceNo != null && x.IsReviewed == false);
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
                        var result = sendBackReviewers.Any(x => x.IsReviewed);
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
            var dueDate = All.Where(x => x.DeletedDate == null && x.IsReviewed == false && x.ProjectWorkplaceArtificatedDocumentId == documentId && x.SequenceNo != null).OrderByDescending(o => o.SequenceNo).FirstOrDefault();
            if (dueDate != null)
            {
                if (dueDate.DueDate != null)
                    return dueDate.DueDate.Value.AddDays(1);
                else
                    return DateTime.Now.Date;
            }
            else
            {
                return DateTime.Now.Date;
            }
        }

        public int SkipDocumentReview(int documentId)
        {
            var defaultUser = All.FirstOrDefault(x => x.ProjectWorkplaceArtificatedDocumentId == documentId
            && x.DeletedDate == null && x.UserId == x.CreatedBy && x.IsReviewed == false);
            if (defaultUser != null)
            {
                defaultUser.SendBackDate = DateTime.Now;
                defaultUser.IsSendBack = true;
                defaultUser.IsReviewed = true;

                Update(defaultUser);
                return _context.Save();
            }

            return 0;
        }


        public async Task SendDueReviewEmail()
        {
            var dueDates = await All.Where(x => x.DeletedDate == null && x.DueDate != null
            && x.UserId != x.CreatedBy && x.IsReviewed == false
            && x.ProjectWorkplaceArtificatedDocument.DeletedDate == null
            && x.DueDate.Value.Date >= DateTime.Now.Date).ToListAsync();
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
                _emailSenderRespository.SendEmailOfReviewDue(user.Email, user.UserName, document.DocumentName, artificateName, project.ProjectCode, due.DueDate);
            }
        }
    }
}
