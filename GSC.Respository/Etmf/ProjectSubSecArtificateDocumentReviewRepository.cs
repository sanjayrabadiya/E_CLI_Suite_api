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
        //private readonly IProjectWorkplaceSubSecArtificatedocumentRepository _projectWorkplaceSubSecArtificatedocumentRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        private readonly IProjectSubSecArtificateDocumentHistoryRepository _projectSubSecArtificateDocumentHistoryRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IProjectWorkplaceArtificateRepository _projectWorkplaceArtificateRepository;
        private readonly IMapper _mapper;
        private readonly IAppSettingRepository _appSettingRepository;

        public ProjectSubSecArtificateDocumentReviewRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
            IEmailSenderRespository emailSenderRespository,
            IUserRepository userRepository,
            //IProjectWorkplaceSubSecArtificatedocumentRepository projectWorkplaceSubSecArtificatedocumentRepository,
            IProjectSubSecArtificateDocumentHistoryRepository projectSubSecArtificateDocumentHistoryRepository,
            IProjectRightRepository projectRightRepository,
            IProjectWorkplaceArtificateRepository projectWorkplaceArtificateRepository,
            IMapper mapper,
             IAppSettingRepository appSettingRepository
            )
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _projectSubSecArtificateDocumentHistoryRepository = projectSubSecArtificateDocumentHistoryRepository;
            //_projectWorkplaceSubSecArtificatedocumentRepository = projectWorkplaceSubSecArtificatedocumentRepository;
            _projectRightRepository = projectRightRepository;
            _projectWorkplaceArtificateRepository = projectWorkplaceArtificateRepository;
            _mapper = mapper;
            _appSettingRepository = appSettingRepository;
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
                    IsReview = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsReviewed == true),
                    SequenceNo = All.FirstOrDefault(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsSendBack == true)?.SequenceNo,
                    IsSelected = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null),
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

            if (ProjectSubSecArtificateDocumentReviewDto.Where(x => x.SequenceNo == null && x.IsSelected).Count() == ProjectSubSecArtificateDocumentReviewDto.Where(x => x.IsSelected).Count())
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
                var firstRecord = ProjectSubSecArtificateDocumentReviewDto.Where(q => q.IsSelected).OrderBy(x => x.SequenceNo).FirstOrDefault();
                if (firstRecord.IsReview == false)
                    SendMailToReviewer(firstRecord);
            }



            var defaultUser = All.FirstOrDefault(x => x.ProjectWorkplaceSubSecArtificateDocumentId == ProjectSubSecArtificateDocumentReviewDto.FirstOrDefault().ProjectWorkplaceSubSecArtificateDocumentId
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
                    Module = "e-TMF",
                    DueDate = s.DueDate,
                    IsDeleted = s.DueDate == null ? false : s.DueDate.Value.Date < DateTime.Now.Date && s.IsReviewed == false,
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
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && x.IsReviewDone == true && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null && x.UserId != _jwtTokenAccesser.UserId)
                .Select(c => new ProjectSubSecArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                    IsReview = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsReviewed == true),
                    SequenceNo = All.FirstOrDefault(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null && b.IsSendBack == true)?.SequenceNo,
                    IsSelected = All.Any(b => b.ProjectWorkplaceSubSecArtificateDocumentId == Id && b.UserId == c.UserId && b.DeletedDate == null),
                }).Where(x => x.IsSelected == false && x.IsReview == false).ToList();

            return users.ToList();
        }

        public int ReplaceUser(int documentId, int actualUserId, int replaceUserId)
        {
            var actualUsers = All.Where(q => q.UserId == actualUserId && q.ProjectWorkplaceSubSecArtificateDocumentId == documentId && q.DeletedDate == null && q.IsReviewed == false).ToList();
            if (actualUsers.Count() > 0)
            {
                foreach (var user in actualUsers.Where(s => s.IsSendBack == false))
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
                    var nulldata = All.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == documentId && x.DeletedDate == null && x.SequenceNo != null && x.IsReviewed == false);
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
            var dueDate = All.Where(x => x.DeletedDate == null && x.IsReviewed == false && x.ProjectWorkplaceSubSecArtificateDocumentId == documentId && x.SequenceNo != null).OrderByDescending(o => o.SequenceNo).FirstOrDefault();
            if (dueDate != null)
            {
                return dueDate.DueDate.Value.AddDays(1);
            }
            else
            {
                return DateTime.Now.Date;
            }
        }

        public int SkipDocumentReview(int documentId)
        {
            var defaultUser = All.FirstOrDefault(x => x.ProjectWorkplaceSubSecArtificateDocumentId == documentId
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
            var commonSettiongs = _appSettingRepository.Get<GeneralSettingsDto>(2);
            var compareDate = DateTime.Now.Date.AddDays(Convert.ToDouble(commonSettiongs.EtmfScheduleDueDate));
            var dueDates = await All.Where(x => x.DeletedDate == null && x.DueDate != null
            && x.UserId != x.CreatedBy && x.IsReviewed == false
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
                _emailSenderRespository.SendEmailOfReviewDue(user.Email, user.UserName, document.DocumentName, artificateName, project.ProjectCode);
            }
        }
    }
}
