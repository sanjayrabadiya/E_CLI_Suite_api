using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
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
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.Etmf
{
    public class ProjectSubSecArtificateDocumentReviewRepository : GenericRespository<ProjectSubSecArtificateDocumentReview>, IProjectSubSecArtificateDocumentReviewRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        private readonly IProjectSubSecArtificateDocumentHistoryRepository _projectSubSecArtificateDocumentHistoryRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IProjectWorkplaceArtificateRepository _projectWorkplaceArtificateRepository;
        private readonly IMapper _mapper;

        public ProjectSubSecArtificateDocumentReviewRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
            IEmailSenderRespository emailSenderRespository,
            IUserRepository userRepository,
            IProjectSubSecArtificateDocumentHistoryRepository projectSubSecArtificateDocumentHistoryRepository,
            IProjectRightRepository projectRightRepository,
            IProjectWorkplaceArtificateRepository projectWorkplaceArtificateRepository,
            IMapper mapper
            )
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _projectSubSecArtificateDocumentHistoryRepository = projectSubSecArtificateDocumentHistoryRepository;
            _projectRightRepository = projectRightRepository;
            _projectWorkplaceArtificateRepository = projectWorkplaceArtificateRepository;
            _mapper = mapper;
        }

        public List<ProjectSubSecArtificateDocumentReviewDto> UserRoles(int Id, int ProjectId, int ProjectDetailsId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ProjectSubSecArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    IsReview = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsReviewed),
                    SequenceNo = All.FirstOrDefault(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsSendBack)?.SequenceNo,
                    IsSelected = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null),
                }).Where(x => !x.IsSelected && !x.IsReview).ToList();

            users.ForEach(x =>
            {
                x.TempSeqNo = x.SequenceNo;
                var etmfUserPermissions = _context.EtmfUserPermission.Include(y => y.ProjectWorkplaceDetail)
                                        .Where(y => y.ProjectWorkplaceDetailId == ProjectDetailsId && y.DeletedDate == null && y.UserId == x.UserId)
                                        .OrderByDescending(x => x.Id).FirstOrDefault();
                x.IsRights = etmfUserPermissions?.IsAdd == true || etmfUserPermissions?.IsEdit == true || etmfUserPermissions?.IsView == true;
            });

            return users.Where(x => x.IsRights).ToList();
        }

        public void SaveDocumentReview(List<ProjectSubSecArtificateDocumentReviewDto> ProjectSubSecArtificateDocumentReviewDto)
        {
            foreach (var ReviewDto in ProjectSubSecArtificateDocumentReviewDto)
            {
                if (ReviewDto.IsSelected)
                {
                    Add(new ProjectSubSecArtificateDocumentReview
                    {
                        ProjectWorkplaceSubSecArtificateDocumentId = ReviewDto.ProjectWorkplaceSubSecArtificateDocumentId,
                        UserId = ReviewDto.UserId,
                        IsSendBack = false,
                        Message = ReviewDto.Message,
                        SequenceNo = ReviewDto.SequenceNo,
                        DueDate = ReviewDto.DueDate
                    });
                    if (_context.Save() < 0) throw new Exception("Artificate Send failed on save.");

                    var projectWorkplaceSubSecArtificatedocument = _context.ProjectWorkplaceSubSecArtificatedocument.Find(ReviewDto.ProjectWorkplaceSubSecArtificateDocumentId);
                    _projectSubSecArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceSubSecArtificatedocument, All.Max(p => p.Id), null);
                }
            }

            if (ProjectSubSecArtificateDocumentReviewDto.Count(x => x.SequenceNo == null && x.IsSelected) == ProjectSubSecArtificateDocumentReviewDto.Count(x => x.IsSelected))
            {
                foreach (var ReviewDto in ProjectSubSecArtificateDocumentReviewDto)
                {
                    if (ReviewDto.IsSelected)
                    {
                        SendMailToReviewer(ReviewDto);
                    }
                }
            }
            else
            {
                var firstRecord = ProjectSubSecArtificateDocumentReviewDto.Where(q => q.IsSelected).OrderBy(x => x.SequenceNo).First();
                if (!firstRecord.IsReview)
                    SendMailToReviewer(firstRecord);
            }



            var defaultUser = All.FirstOrDefault(x => x.ProjectWorkplaceSubSecArtificateDocumentId == ProjectSubSecArtificateDocumentReviewDto.FirstOrDefault().ProjectWorkplaceSubSecArtificateDocumentId
            && x.DeletedDate == null && x.UserId == x.CreatedBy && !x.IsReviewed);
            if (defaultUser != null)
            {
                defaultUser.SendBackDate = DateTime.Now;
                defaultUser.IsSendBack = true;
                defaultUser.IsReviewed = true;

                Update(defaultUser);
                _context.Save();
            }
        }

        public void SendMailToReviewer(ProjectSubSecArtificateDocumentReviewDto ReviewDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                   .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.Project)
                   .FirstOrDefault(x => x.ProjectWorkplaceSubSecArtificateDocumentId == ReviewDto.ProjectWorkplaceSubSecArtificateDocumentId);

            var ProjectName = project?.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.Project.ProjectName;
            var document = project?.ProjectWorkplaceSubSecArtificateDocument.DocumentName;
            var artificate = project?.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName;
            var user = _userRepository.Find(ReviewDto.UserId);
            _emailSenderRespository.SendEmailOfReview(user.Email, user.UserName, document, artificate, ProjectName);
        }

        public void SendMailToSendBack(ProjectSubSecArtificateDocumentReview ReviewDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceSubSecArtificateDocument)
                   .ThenInclude(x => x.ProjectWorkplaceSubSectionArtifact).ThenInclude(x => x.Project)
                   .FirstOrDefault(x => x.ProjectWorkplaceSubSecArtificateDocumentId == ReviewDto.ProjectWorkplaceSubSecArtificateDocumentId);

            var ProjectName = project?.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.Project.ProjectName;
            var document = project?.ProjectWorkplaceSubSecArtificateDocument.DocumentName;
            var artificate = project?.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ArtifactName;
            var user = _userRepository.Find((int)ReviewDto.CreatedBy);
            if (ReviewDto.IsReviewed)
            {
                _emailSenderRespository.SendEmailOfReviewed(user.Email, user.UserName, document, artificate, ProjectName);
            }
            else
            {
                _emailSenderRespository.SendEmailOfSendBack(user.Email, user.UserName, document, artificate, ProjectName);
            }
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
                && x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectId == ProjectId && !x.IsSendBack
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
                    Module = "e-TMF",
                    DueDate = s.DueDate,
                    IsDeleted = s.DueDate == null ? false : s.DueDate.Value.Date < DateTime.Now.Date && !s.IsReviewed,
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
                .ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ProjectId == ProjectId && x.IsSendBack
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
                    Module = "e-TMF",
                    DataType = MyTaskMethodModule.SendBack.GetDescription(),
                    ControlType = DashboardMyTaskType.ETMFSubSecSendBackData
                }).OrderByDescending(x => x.CreatedDate).ToList();

            result.ForEach(s =>
            {
                s.ExtraData = _context.ProjectSubSecArtificateDocumentApprover.Where(x => x.DeletedDate == null && x.ProjectWorkplaceSubSecArtificateDocumentId == s.DocumentId).Count();
            });

            return result.Where(x => Convert.ToInt32(x.ExtraData) == 0).ToList();
        }


        public List<ProjectSubSecArtificateDocumentReviewDto> GetUsers(int Id, int ProjectId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ProjectSubSecArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    IsReview = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsReviewed),
                    SequenceNo = All.FirstOrDefault(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsSendBack)?.SequenceNo,
                    IsSelected = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null),
                }).Where(x => !x.IsSelected && !x.IsReview).ToList();

            return users.ToList();
        }

        public int ReplaceUser(int documentId, int actualUserId, int replaceUserId)
        {
            var actualUsers = All.Where(q => q.UserId == actualUserId && q.ProjectWorkplaceSubSecArtificateDocumentId == documentId && q.DeletedDate == null && !q.IsReviewed).ToList();
            if (actualUsers.Any())
            {
                foreach (var user in actualUsers.Where(s => !s.IsSendBack))
                {
                    var replaceUser = new ProjectSubSecArtificateDocumentReview()
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
                        ProjectWorkplaceSubSecArtificateDocumentId = user.ProjectWorkplaceSubSecArtificateDocumentId
                    };
                    Add(replaceUser);

                    _context.Save();

                    var projectWorkplaceSubSecArtificatedocument = _context.ProjectWorkplaceSubSecArtificatedocument.Find(user.ProjectWorkplaceSubSecArtificateDocumentId);
                    _projectSubSecArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceSubSecArtificatedocument, All.Max(p => p.Id), null);

                    var replaceUserDto = _mapper.Map<ProjectSubSecArtificateDocumentReviewDto>(replaceUser);

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
                    var nulldata = All.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == documentId && x.DeletedDate == null && x.SequenceNo != null && !x.IsReviewed);
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
                        var result = sendBackReviewers.Exists(x => x.IsReviewed);
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
            var dueDate = All.Where(x => x.DeletedDate == null && !x.IsReviewed && x.ProjectWorkplaceSubSecArtificateDocumentId == documentId && x.SequenceNo != null).OrderByDescending(o => o.SequenceNo).FirstOrDefault();
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
            var defaultUser = All.FirstOrDefault(x => x.ProjectWorkplaceSubSecArtificateDocumentId == documentId
            && x.DeletedDate == null && x.UserId == x.CreatedBy && !x.IsReviewed);
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
            && x.UserId != x.CreatedBy && !x.IsReviewed
            && x.ProjectWorkplaceSubSecArtificateDocument.DeletedDate == null
            && x.DueDate.Value.Date >= DateTime.Now.Date).ToListAsync();
            foreach (var due in dueDates)
            {
                var user = await _context.Users.FindAsync(due.UserId);
                var document = await _context.ProjectWorkplaceArtificatedocument.FindAsync(due.ProjectWorkplaceSubSecArtificateDocumentId);
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
