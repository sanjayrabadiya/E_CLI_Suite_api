using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.ProjectRight
{
    public class ProjectRightRepository : GenericRespository<Data.Entities.ProjectRight.ProjectRight>,
        IProjectRightRepository
    {
        private readonly IProjectDocumentReviewRepository _documentReviewRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;

        public ProjectRightRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IProjectDocumentReviewRepository documentReviewRepository
            ) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _documentReviewRepository = documentReviewRepository;
        }

        public List<ProjectRightListDto> GetProjectRights()
        {
            var projects = _context.Project.Where(x =>
                x.DeletedDate == null && _context.ProjectRight.Any(c => c.DeletedDate == null
                                                                       && c.ProjectId == x.Id
                                                                       && c.UserId == _jwtTokenAccesser.UserId
                                                                       && c.RoleId == _jwtTokenAccesser.RoleId));

            return projects.Select(x => new ProjectRightListDto
            {
                ProjectId = x.Id,
                ProjectCode = x.ProjectCode,
                ProjectName = x.ProjectName,
                Users = string.Join(", ",
                    _context.ProjectRight.Include(a => a.User).Where(c => c.ProjectId == x.Id && x.DeletedDate == null)
                        .Select(a => a.User.UserName).ToList()),
                Documents = string.Join(", ",
                    _context.ProjectDocument.Where(c => c.ProjectId == x.Id && x.DeletedDate == null)
                        .Select(a => a.FileName).ToList())
            }).ToList();
        }

        public List<ProjectRightDto> GetProjectRightByProjectId(int projectId)
        {
            var roles = _context.SecurityRole.Where(x => x.DeletedDate == null).Select(c => new ProjectRightDto
            {
                RoleId = c.Id,
                Name = c.RoleName,
                users = _context.UserRole.Where(a => a.UserRoleId == c.Id && a.User.DeletedDate == null
                                                                         && a.DeletedDate == null).Select(r =>
                    new ProjectRightDto
                    {
                        RoleId = c.Id,
                        UserId = r.UserId,
                        Name = r.User.UserName,
                        IsSelected = All.Any(b => b.ProjectId == projectId && b.RoleId == c.Id && b.UserId == r.UserId && b.DeletedDate == null)
                    }).Where(x => x.IsSelected == false).ToList()
            }).ToList();

            return roles.Where(x => x.users.Count() != 0).ToList();
        }


        public void SaveProjectAccessRight(List<ProjectRightDto> projectRightDto, int projectId)
        {
            var rights = projectRightDto.SelectMany(x =>
                x.users.Select(c => new ProjectRightDto
                { UserId = c.UserId, RoleId = c.RoleId, IsSelected = c.IsSelected })).Distinct().ToList();

            rights = rights.Distinct().ToList();

            var userlist = rights.Select(c => new { c.UserId, c.IsSelected }).Distinct();
            foreach (var userDto in userlist)
                if (userDto.IsSelected)
                    _documentReviewRepository.SaveByUserId(projectId, userDto.UserId);

            foreach (var itemDto in rights)
                if (itemDto.IsSelected)
                {
                    var isDocumentFound = _documentReviewRepository.All.Any(x => x.ProjectId == projectId
                                                                                 && x.UserId == itemDto.UserId &&
                                                                                 x.DeletedDate == null
                                                                                 && !x.IsReview);

                    var checkparentTrainig = _context.Project.Where(x => x.Id == projectId).FirstOrDefault().ParentProjectId;
                    var checkchildTrainig = _context.Project.Where(x => x.ParentProjectId == projectId).ToList();

                    var isProjectRightFound = new Data.Entities.ProjectRight.ProjectRight();
                    var isProjectRightUserFound = new Data.Entities.ProjectRight.ProjectRight();

                    if (checkparentTrainig != null)
                    {
                        isProjectRightFound = All.Where(x =>
                            x.ProjectId == checkparentTrainig && x.UserId == itemDto.UserId).OrderByDescending(x => x.Id).FirstOrDefault();
                        isProjectRightUserFound = All.Where(x => x.ProjectId == projectId && x.UserId == itemDto.UserId).OrderBy(x => x.Id).FirstOrDefault();
                    }
                    else
                    {
                        var lstIsProjectRightFound = new List<Data.Entities.ProjectRight.ProjectRight>();
                        foreach (var childProject in checkchildTrainig)
                        {
                            var projectright = All.Where(x =>
                                x.ProjectId == childProject.Id && x.UserId == itemDto.UserId &&
                                x.RoleId == itemDto.RoleId).OrderByDescending(x => x.Id).FirstOrDefault();

                            if (projectright != null) lstIsProjectRightFound.Add(projectright);
                        }

                        var isExist = lstIsProjectRightFound.Any(l => l.IsReviewDone == false);
                        if (isExist)
                            isProjectRightFound = null;
                        else
                            isProjectRightFound = lstIsProjectRightFound.FirstOrDefault();

                        isProjectRightUserFound = All.Where(x => x.ProjectId == projectId && x.UserId == itemDto.UserId).OrderByDescending(x => x.Id).FirstOrDefault();
                    }


                    if (isProjectRightFound != null)
                        Add(new Data.Entities.ProjectRight.ProjectRight
                        {
                            ProjectId = projectId,
                            UserId = itemDto.UserId,
                            RoleId = itemDto.RoleId,
                            IsReviewDone = isProjectRightFound.IsReviewDone
                        });
                    else if (isProjectRightUserFound != null)
                        Add(new Data.Entities.ProjectRight.ProjectRight
                        {
                            ProjectId = projectId,
                            UserId = itemDto.UserId,
                            RoleId = itemDto.RoleId,
                            IsReviewDone = isProjectRightUserFound.IsReviewDone
                        });
                    else
                        Add(new Data.Entities.ProjectRight.ProjectRight
                        {
                            ProjectId = projectId,
                            UserId = itemDto.UserId,
                            RoleId = itemDto.RoleId,
                            IsReviewDone = isDocumentFound
                        });
                }
        }

        public void SaveProjectRollbackRight(List<ProjectRightDto> projectRightDto, int projectId, int[] ids)
        {
            var rights = projectRightDto.SelectMany(x =>
                x.users.Select(c => new ProjectRightDto
                { UserId = c.UserId, RoleId = c.RoleId, IsSelected = c.IsSelected })).ToList();

            rights = rights.Distinct().ToList();

            foreach (var id in ids)
            {
                var projectRightuser = FindBy(x => x.Id == id).FirstOrDefault();
                var userByproject = FindBy(x => x.UserId == projectRightuser.UserId).ToList()
                    .Any(a => a.DeletedDate == null);
                if (userByproject == false)
                    if (projectRightuser != null)
                        _documentReviewRepository.DeleteByUserId(projectId, projectRightuser.UserId);
            }

            foreach (var itemDto in ids)
            {
                var projectRight = FindBy(x => x.Id == itemDto).ToList();

                foreach (var item in projectRight)
                {
                    item.DeletedBy = _jwtTokenAccesser.UserId;
                    item.DeletedDate = DateTime.Now;
                    item.RollbackReason = projectRightDto[0].RollbackReason;
                    item.AuditReasonId = projectRightDto[0].AuditReasonId;
                    Update(item);
                }
            }
        }

        public void UpdateIsReviewDone(int projectId)
        {
            List<Data.Entities.Master.Project> allChild;

            var projectParent = _context.Project.AsNoTracking().Where(x => x.Id == projectId).SingleOrDefault();
            if (projectParent != null && projectParent.ParentProjectId != null)
            {
                allChild = _context.Project.AsNoTracking().Where(x => x.ParentProjectId == projectParent.ParentProjectId
                                                                     && _context.ProjectRight.AsNoTracking().Any(a =>
                                                                         a.ProjectId == x.Id
                                                                         && a.UserId == _jwtTokenAccesser.UserId
                                                                         && x.DeletedDate == null) &&
                                                                     x.DeletedDate == null).ToList();

                var parentProjectExist = _context.Project.AsNoTracking().Where(x => x.Id == projectParent.ParentProjectId
                                                                                   && _context.ProjectRight
                                                                                       .AsNoTracking().Any(a =>
                                                                                           a.ProjectId == x.Id
                                                                                           && a.UserId ==
                                                                                           _jwtTokenAccesser.UserId
                                                                                           && x.DeletedDate == null) &&
                                                                                   x.DeletedDate == null).ToList();
                if (parentProjectExist.Count() > 0) allChild.AddRange(parentProjectExist);
            }
            else
            {
                allChild = _context.Project.AsNoTracking().Where(x => x.ParentProjectId == projectId
                                                                     && _context.ProjectRight.AsNoTracking().Any(a =>
                                                                         a.ProjectId == x.Id
                                                                         && a.UserId == _jwtTokenAccesser.UserId
                                                                         && x.DeletedDate == null) &&
                                                                     x.DeletedDate == null).ToList();

                var parentProjectExist = _context.Project.AsNoTracking().Where(x => x.Id == projectParent.Id
                                                                                   && _context.ProjectRight
                                                                                       .AsNoTracking().Any(a =>
                                                                                           a.ProjectId == x.Id
                                                                                           && a.UserId ==
                                                                                           _jwtTokenAccesser.UserId
                                                                                           && x.DeletedDate == null) &&
                                                                                   x.DeletedDate == null).ToList();
                if (parentProjectExist.Count() > 0) allChild.AddRange(parentProjectExist);
            }

            var isSave = false;
            foreach (var allData in allChild)
            {
                var projectRights = All.AsNoTracking().Where(x => x.ProjectId == allData.Id && x.DeletedDate == null)
                    .ToList();

                foreach (var projectRight in projectRights)
                {
                    if (_context.ProjectDocumentReview.AsNoTracking().Any(x =>
                        x.ProjectId == allData.Id && x.UserId == projectRight.UserId &&
                        x.IsReview == false && x.DeletedDate == null))
                    {
                        if (!projectRight.IsPrimary)
                            projectRight.IsReviewDone = false;
                        else
                            projectRight.IsReviewDone = true;
                    }
                    else
                    {
                        projectRight.IsReviewDone = true;
                    }

                    isSave = true;

                    Update(projectRight);
                }

                if (isSave) _context.Save();
            }
        }

        public List<int> GetProjectRightIdList()
        {
            return All.Where(c => c.DeletedDate == null && c.UserId == _jwtTokenAccesser.UserId &&
                                  c.RoleId == _jwtTokenAccesser.RoleId
                                  && c.IsReviewDone).Select(x => x.ProjectId).ToList();
        }

        public List<ProjectDocumentReviewDto> GetProjectRightDetailsByProjectId(int projectId)
        {
            var projectListbyId = All.Where(x => x.ProjectId == projectId).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId, c.RoleId }, (key, group) => group.First());

            var result = latestProjectRight.Select(x => new ProjectDocumentReviewDto
            {
                Id = x.Id,
                Project = _context.Project.Where(p => p.Id == x.ProjectId).FirstOrDefault(),
                ProjectName = _context.Project.Where(p => p.Id == x.ProjectId).Select(r => r.ProjectName).FirstOrDefault(),
                //Add By vipul on 14092020 for get study and site display on grid project access
                ProjectNumber = _context.Project.Where(p => p.Id == x.ProjectId).Select(r => r.ParentProjectId).FirstOrDefault() == null ? "" : _context.Project.Where(p => p.Id == x.ProjectId).Select(r => r.ProjectCode).FirstOrDefault(),
                ParentProjectCode = _context.Project.Where(p => p.Id == x.ProjectId).FirstOrDefault().ParentProjectId == null
                ? _context.Project.Where(p => p.Id == x.ProjectId).FirstOrDefault().ProjectCode
                : _context.Project.Where(p => p.Id == _context.Project.Where(p => p.Id == x.ProjectId).FirstOrDefault().ParentProjectId).FirstOrDefault().ProjectCode,

                ProjectId = x.ProjectId,
                UserId = x.UserId,
                UserName = _context.Users.Where(p => p.Id == x.UserId).Select(r => r.UserName).FirstOrDefault(),
                AccessType = x.AuditReasonId != null ? "Revoke" : "Grant",
                RoleId = x.RoleId,
                RoleName = _context.ProjectRight.Where(c => c.ProjectId == x.ProjectId && c.UserId == x.UserId && c.RoleId == x.RoleId).Select(a => a.role.RoleName).FirstOrDefault(),
                TotalReview = _context.ProjectDocumentReview.Where(a => a.DeletedDate == null && a.ProjectId == x.ProjectId
                                          && a.UserId == x.UserId && a.IsReview).Select(b => new ReviewDeteail
                                          {
                                              DocumentPath = b.ProjectDocument.FileName,
                                              ReviewDate = b.ReviewDate,
                                              ReviewNote = b.ReviewNote,
                                              TrainingDuration = b.TrainingDuration,
                                              TrainingType = b.TrainingType == null ? "" : ((TrainigType)b.TrainingType).GetDescription(),
                                              TrainerName = _context.Users.Where(c => c.Id == b.TrainerId).Select(a => a.UserName).FirstOrDefault()
                                          }).ToList().OrderByDescending(k => k.AssignedDate).ToList(),
                PendingReview = _context.ProjectDocumentReview.Where(a => a.DeletedDate == null && a.ProjectId == x.ProjectId
                                          && a.UserId == x.UserId && !a.IsReview).Select(b => new ReviewDeteail
                                          {
                                              DocumentPath = b.ProjectDocument.FileName
                                          }).ToList().OrderByDescending(y => y.AssignedDate).ToList(),
                ProjectCreatedBy = x.CreatedBy
            }).ToList();

            //Changes by vipul for assign detail

            result.ForEach(r =>
            {
                var createdByUser = _context.Users.Where(user => user.Id == r.TrainerId).FirstOrDefault();
                if (createdByUser != null) r.RollabackBy = createdByUser.UserName;
            });

            result.ForEach(r =>
            {
                r.TotalReviewName = r.TotalReview == null ? "" : "Complete (" + r.TotalReview.Count() + ")";
                if (r.AccessType == "Grant" && r.ProjectCreatedBy == r.UserId) r.TotalReviewName = "N/AP";

                if (r.TotalReview != null)
                {
                    r.TotalReview.ForEach(collection =>
                    {
                        var projectRights = _context.ProjectRight.Where(a => a.ProjectId == r.ProjectId
                                                                                     && a.UserId == r.UserId).FirstOrDefault();
                        if (projectRights != null)
                        {
                            collection.AssignedDate = projectRights.CreatedDate;
                            var createdByUser = _context.Users.Where(user => user.Id == projectRights.CreatedBy).FirstOrDefault();
                            if (createdByUser != null) collection.AssignedBy = createdByUser.UserName;
                        }
                    });
                }
            });

            result.ForEach(r =>
            {
                r.PendingReviewName = r.PendingReview == null ? "" : "Pending (" + r.PendingReview.Count() + ")";
                if (r.AccessType == "Grant" && r.ProjectCreatedBy == r.UserId) r.PendingReviewName = "N/AP";

                if (r.PendingReview != null)
                {
                    r.PendingReview.ForEach(collection =>
                    {
                        var projectRights = _context.ProjectRight.Where(a => a.ProjectId == r.ProjectId
                                                                                     && a.UserId == r.UserId).FirstOrDefault();
                        if (projectRights != null)
                        {
                            collection.AssignedDate = projectRights.CreatedDate;
                            var createdByUser = _context.Users.Where(user => user.Id == projectRights.CreatedBy).FirstOrDefault();
                            if (createdByUser != null) collection.AssignedBy = createdByUser.UserName;
                        }
                    });
                }
            });

            return result;
        }

        public ProjectDocumentHistory GetProjectRightHistory(int projectId, int userId, int roleId)
        {
            var objdochistory = new ProjectDocumentHistory();

            var projectrightlist = All.Where(x => x.ProjectId == projectId && x.UserId == userId && x.RoleId == roleId)
                .ToList();

            var result = projectrightlist.Select(x => new ProjectDocumentReviewDto
            {
                ProjectName = _context.Project.Where(p => p.Id == x.ProjectId).Select(c => c.ProjectName).FirstOrDefault(),
                ProjectNumber = _context.Project.Where(p => p.Id == x.ProjectId).Select(c => c.ProjectCode).FirstOrDefault(),
                ProjectId = x.ProjectId,
                UserId = x.UserId,
                UserName = _context.Users.Where(p => p.Id == x.UserId).Select(c => c.UserName).FirstOrDefault(),
                IsTraning = x.IsReviewDone ? "Yes" : "No",
                RoleName = string.Join(", ", _context.ProjectRight.Where(c => c.ProjectId == x.ProjectId
                                                                             && c.UserId == x.UserId)
                    .Select(a => a.role.RoleName).ToList().Distinct()),
                AuditReasonID = x.AuditReasonId,
                AuditReason = x.AuditReasonId == null
                    ? null
                    : _context.AuditReason.Where(y => y.Id == x.AuditReasonId).FirstOrDefault().ReasonName,
                RollbackReason = x.RollbackReason,
                TrainerId = x.DeletedBy,
                RollbackOn = x.DeletedDate,
                AccessType = x.AuditReasonId == null ? "Grant" : "Revoke",
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            }).OrderByDescending(x => x.RollbackOn).OrderBy(q => q.AccessType).ToList();

            result.ForEach(r =>
            {
                r.CreatedDate = r.AccessType == "Revoke" ? null : r.CreatedDate;
                r.CreatedBy = r.AccessType == "Revoke" ? null : r.CreatedBy;
                var createdByUser = _context.Users.Where(user => user.Id == r.TrainerId).FirstOrDefault();
                if (createdByUser != null) r.RollabackBy = createdByUser.UserName;
            });

            result.ForEach(r =>
            {
                var createdByUser = _context.Users.Where(user => user.Id == r.CreatedBy).FirstOrDefault();
                if (createdByUser != null) r.CreatedByName = createdByUser.UserName;
            });

            var rollbackresult = projectrightlist.Where(x => x.AuditReasonId != null).ToList();
            var grantresult = rollbackresult.Select(x => new ProjectDocumentReviewDto
            {
                AccessType = "Grant",
                RollbackReason = null,
                RollbackOn = null,
                RollabackBy = null,
                AuditReason = null,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            }).OrderByDescending(x => x.CreatedDate).OrderBy(q => q.AccessType).ToList();

            grantresult.ForEach(r =>
            {
                var createdByUser = _context.Users.Where(user => user.Id == r.CreatedBy).FirstOrDefault();
                if (createdByUser != null) r.CreatedByName = createdByUser.UserName;
            });

            result.AddRange(grantresult);

            objdochistory.RollbackRights = result;
            objdochistory.TotalReview = _context.ProjectDocumentReview.Where(a => a.ProjectId == projectId
                                                                                 && a.UserId == userId).Select(b =>
                new ReviewDeteail
                {
                    DocumentPath = b.ProjectDocument.FileName,
                    ReviewDate = b.ReviewDate,
                    ReviewNote = b.ReviewNote,
                    TrainingDuration = b.TrainingDuration,
                    TrainingType = b.TrainingType == null ? "" : ((TrainigType)b.TrainingType).GetDescription(),
                    TrainerName = _context.Users.Where(c => c.Id == b.TrainerId).Select(a => a.UserName).FirstOrDefault()
                }).ToList().OrderByDescending(k => k.AssignedDate).ToList();

            objdochistory.TotalReview.ForEach(collection =>
            {
                var projectRights = _context.ProjectRight.Where(a => a.ProjectId == projectId
                                                                             && a.UserId == userId).FirstOrDefault();
                if (projectRights != null)
                {
                    collection.AssignedDate = projectRights.CreatedDate;
                    var createdByUser = _context.Users.Where(user => user.Id == projectRights.CreatedBy).FirstOrDefault();
                    if (createdByUser != null) collection.AssignedBy = createdByUser.UserName;
                }
            });


            objdochistory.PendingReview = _context.ProjectDocumentReview.Where(a => a.ProjectId == projectId
                                                                                   && a.UserId == userId && !a.IsReview)
                .Select(b => new ReviewDeteail
                {
                    DocumentPath = b.ProjectDocument.FileName,
                    IsDeleted = b.ProjectDocument.DeletedDate.HasValue ? "Yes" : "No"
                }).ToList().OrderByDescending(y => y.AssignedDate).ToList();

            objdochistory.PendingReview.ForEach(collection =>
            {
                var projectRights = _context.ProjectRight.FirstOrDefault(a => a.ProjectId == projectId
                                                                             && a.UserId == userId);
                if (projectRights != null)
                {
                    collection.AssignedDate = projectRights.CreatedDate;
                    var createdByUser = _context.Users.FirstOrDefault(user => user.Id == projectRights.CreatedBy);
                    if (createdByUser != null) collection.AssignedBy = createdByUser.UserName;
                }
            });

            return objdochistory;
        }

        public IList<ProjectTrainingDto> GetRoles(int ProjectId)
        {
            var queryDtos = (from projectRight in _context.ProjectRight.Where(t => t.ProjectId == ProjectId)
                             join roleTemp in _context.SecurityRole on projectRight.RoleId equals roleTemp.Id into roleDto
                             from role in roleDto
                             group new { role, projectRight } by new { role.Id, role.RoleName } into g
                             select new ProjectTrainingDto
                             {
                                 Id = g.Key.Id,
                                 RoleName = g.Key.RoleName,
                             }).OrderBy(x => x.Id).ToList();
            return queryDtos;
        }

        public IList<ProjectTrainingDto> GetUsers(int ProjectId)
        {
            var queryDtos = (from projectRight in _context.ProjectRight.Where(t => t.ProjectId == ProjectId)
                             join userTemp in _context.Users on projectRight.UserId equals userTemp.Id into userDto
                             from user in userDto
                             group new { user, projectRight } by new { user.Id, user.UserName } into g
                             select new ProjectTrainingDto
                             {
                                 Id = g.Key.Id,
                                 UserName = g.Key.UserName,
                             }).OrderBy(x => x.Id).ToList();
            return queryDtos;
        }

        public IList<ProjectAccessDto> GetProjectAccessReportList(ProjectTrainigAccessSearchDto filters)
        {
            var parent = _context.Project.Where(x => x.Id == filters.ProjectId).FirstOrDefault().ParentProjectId;

            var queryDtos = (from projectRight in _context.ProjectRight.Where(t => t.ProjectId == filters.ProjectId
                             && (filters.UserIds == null || filters.UserIds.Contains(t.UserId))
                             && (filters.RoleIds == null || filters.RoleIds.Contains(t.RoleId)))
                             join project in _context.Project on projectRight.ProjectId equals project.Id
                             join auditReasonTemp in _context.AuditReason on projectRight.AuditReasonId equals auditReasonTemp.Id into auditReasonTempDto
                             from auditReason in auditReasonTempDto.DefaultIfEmpty()
                             join projectDocument in _context.ProjectDocument.Where(x => x.DeletedDate == null) on parent != null ? parent : projectRight.ProjectId equals projectDocument.ProjectId
                             join projectDocumentReviewTemp in _context.ProjectDocumentReview on new { x = projectDocument.Id, y = projectRight.UserId, z = projectDocument.ProjectId } equals new { x = projectDocumentReviewTemp.ProjectDocumentId, y = projectDocumentReviewTemp.UserId, z = projectDocumentReviewTemp.ProjectId } into projectDocumentReviewDto
                             from projectDocumentReview in projectDocumentReviewDto.DefaultIfEmpty()
                             where projectDocumentReview.DeletedDate == null
                             join trainerTemp in _context.Users on projectDocumentReview.TrainerId equals trainerTemp.Id into trainerDto
                             from trainer in trainerDto.DefaultIfEmpty()
                             join userTemp in _context.Users on projectRight.UserId equals userTemp.Id into userDto
                             from user in userDto.DefaultIfEmpty()
                             join roleTemp in _context.SecurityRole on projectRight.RoleId equals roleTemp.Id into roleDto
                             from role in roleDto.DefaultIfEmpty()
                             select new ProjectAccessDto
                             {
                                 Id = project.Id,
                                 RoleName = role.RoleName,
                                 UserName = user.UserName,
                                 SiteName = string.IsNullOrEmpty(project.SiteName) ? project.ProjectName : project.SiteName,
                                 AssignedDate = projectRight.CreatedDate.UtcDateTime(),
                                 AssignedBy = user.UserName,
                                 DocumentName = projectDocument.FileName,
                                 AccessType = projectRight.AuditReasonId == null ? "Grant" : "Revoke",
                                 RollbackReason = projectRight.RollbackReason,
                                 RollbackOn = projectRight.DeletedDate.UtcDateTime(),
                                 CreatedDate = projectRight.CreatedDate.UtcDateTime(),
                                 TrainerId = projectDocumentReview.DeletedBy,
                                 CreatedByName = _context.Users.FirstOrDefault(user => user.Id == projectRight.CreatedBy).UserName,
                                 AuditReason = auditReason.ReasonName,
                                 RollabackBy = _context.Users.FirstOrDefault(user => user.Id == projectRight.DeletedBy && projectRight.AuditReasonId != null).UserName,
                             }).OrderBy(x => x.Id).ToList();

            var Rollbackresult = queryDtos.Where(x => x.AuditReason != null).ToList();
            var Grantresult = Rollbackresult.Select(x => new ProjectAccessDto
            {
                AccessType = "Grant",
                RollbackReason = null,
                RollbackOn = null,
                RollabackBy = null,
                AuditReason = null,
                RoleName = x.RoleName,
                UserName = x.UserName,
                SiteName = x.SiteName,
                DocumentName = x.DocumentName,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy,
                CreatedByName = x.CreatedByName,
            }).OrderByDescending(x => x.CreatedDate).OrderBy(q => q.AccessType).ToList();

            queryDtos.ForEach(r =>
            {
                r.CreatedDate = r.AccessType == "Revoke" ? null : r.CreatedDate;
                r.CreatedBy = r.AccessType == "Revoke" ? null : r.CreatedBy;
                r.CreatedByName = r.AccessType == "Revoke" ? null : r.CreatedByName;
                var createdByUser = _context.Users.FirstOrDefault(user => user.Id == r.TrainerId);
                if (r.RollabackBy == null)
                {
                    r.RollabackBy = "N/AP";
                }
                if (createdByUser != null)
                {
                    r.RollabackBy = createdByUser.UserName;
                }
            });

            queryDtos.ForEach(r =>
            {
                var createdByUser = _context.Users.FirstOrDefault(user => user.Id == r.CreatedBy);
                if (createdByUser != null)
                {
                    r.CreatedByName = createdByUser.UserName;
                }
            });

            Grantresult.ForEach(r =>
            {
                var createdByUser = _context.Users.FirstOrDefault(user => user.Id == r.CreatedBy);
                if (createdByUser != null)
                {
                    r.CreatedByName = createdByUser.UserName;
                }
            });

            queryDtos.AddRange(Grantresult);
            return queryDtos;
        }

        public IList<ProjectTrainingDto> GetProjectTrainingReportList(ProjectTrainigAccessSearchDto filters)
        {
            var parent = _context.Project.Where(x => x.Id == filters.ProjectId).FirstOrDefault().ParentProjectId;
            var queryDtos = (from projectRight in _context.ProjectRight.Where(t => t.ProjectId == filters.ProjectId
                                     && (filters.UserIds == null || filters.UserIds.Contains(t.UserId))
                                     && (filters.RoleIds == null || filters.RoleIds.Contains(t.RoleId)))
                             join project in _context.Project on projectRight.ProjectId equals project.Id
                             join auditReasonTemp in _context.AuditReason on projectRight.AuditReasonId equals auditReasonTemp.Id into auditReasonTempDto
                             from auditReason in auditReasonTempDto.DefaultIfEmpty()
                             join projectDocument in _context.ProjectDocument.Where(x => x.DeletedDate == null) on parent != null ? parent : projectRight.ProjectId equals projectDocument.ProjectId
                             join projectDocumentReviewTemp in _context.ProjectDocumentReview on new { x = projectDocument.Id, y = projectRight.UserId, z = projectRight.ProjectId } equals new { x = projectDocumentReviewTemp.ProjectDocumentId, y = projectDocumentReviewTemp.UserId, z = projectDocumentReviewTemp.ProjectId } into projectDocumentReviewDto
                             from projectDocumentReview in projectDocumentReviewDto.DefaultIfEmpty()
                             where projectDocumentReview.DeletedDate == null
                             join trainerTemp in _context.Users on projectDocumentReview.TrainerId equals trainerTemp.Id into trainerDto
                             from trainer in trainerDto.DefaultIfEmpty()
                             join userTemp in _context.Users on projectRight.UserId equals userTemp.Id into userDto
                             from user in userDto.DefaultIfEmpty()
                             join roleTemp in _context.SecurityRole on projectRight.RoleId equals roleTemp.Id into roleDto
                             from role in roleDto.DefaultIfEmpty()
                             select new ProjectTrainingDto
                             {
                                 Id = project.Id,
                                 ProjectId = project.Id,
                                 SiteName = string.IsNullOrEmpty(project.SiteName) ? project.ProjectName : project.SiteName,
                                 UserName = user.UserName,
                                 RoleName = role.RoleName,
                                 AssignedBy = _context.ProjectRight.FirstOrDefault(a => a.CreatedBy == user.Id).User.UserName,
                                 AssignedDate = _context.ProjectRight.FirstOrDefault(a => a.ProjectId == (project.ParentProjectId == null ? project.Id : project.ParentProjectId)).CreatedDate.UtcDateTime(),
                                 DocumentName = projectDocument.FileName,
                                 ReviewDate = projectDocumentReview.ReviewDate,
                                 TrainerName = trainer.UserName,
                                 TrainingType = _context.ProjectDocumentReview.FirstOrDefault(a => (a.ProjectDocumentId == projectDocument.Id) && (a.UserId == projectRight.UserId) && (a.ProjectId == projectDocument.ProjectId)).TrainingType.ToString(),
                                 TrainingDuration = _context.ProjectDocumentReview.Where(c => c.ProjectId == projectRight.ProjectId && c.UserId == projectRight.UserId && c.ProjectDocumentId == projectDocument.Id).FirstOrDefault().TrainingDuration,
                                 ReviewNote = projectDocumentReview.ReviewNote,
                             }).OrderBy(x => x.Id).ToList();

            return queryDtos;
        }
        public IList<UserReportDto> GetUserReportList(UserReportSearchDto filters)
        {
            if (filters.UserIds != null && filters.UserIds.ToList().Count <= 0)
                filters.UserIds = null;

            var results = (from user in _context.Users.Where(t => ((t.DeletedBy != null && filters.UserId == 2) && (filters.UserIds == null || filters.UserIds.Contains(t.Id))) ||
                           ((t.DeletedBy == null && filters.UserId == 3) && (filters.UserIds == null || filters.UserIds.Contains(t.Id))) || ((t.IsLogin == true && filters.UserId == 1) && (filters.UserIds == null || filters.UserIds.Contains(t.Id)))
                          )
                           select new UserReportDto
                           {
                               UserName = _context.Users.FirstOrDefault(b => b.Id == user.Id).UserName,
                               Session = "Active",
                               CreatedBy = _context.Users.FirstOrDefault(b => b.Id == user.CreatedBy).UserName,
                               DeletedBy = _context.Users.FirstOrDefault(b => b.Id == user.DeletedBy).UserName,
                               CreatedDate = user.CreatedDate,
                               DeletedDate = user.DeletedDate,
                               LoginTime = user.LastLoginDate.UtcDateTime(),
                               LastIpAddress = user.LastIpAddress,
                               RoleName = string.Join(" ,", _context.Users.FirstOrDefault(b => b.Id == user.Id).UserRoles.Where(x => x.DeletedDate == null).Select(s => s.SecurityRole.RoleName).ToList())
                           }).ToList();//OrderBy(x => x.Id).ToList();
            results = results.OrderBy(x => x.Id).ToList();

            results.ForEach(r =>
            {
                if (filters.UserId == 1)
                {
                    r.RoleName = _context.SecurityRole.FirstOrDefault(a => a.Id == _jwtTokenAccesser.RoleId).RoleName;
                }
            });

            return results;
        }
        public IList<UserReportDto> GetLoginLogoutReportList(UserReportSearchDto filters)
        {
            if (filters.UserIds != null && filters.UserIds.ToList().Count <= 0)
                filters.UserIds = null;

            //var parent = _context.Project.Where(x => (x.Id == filters.ProjectId) || (x.ParentProjectId == filters.ProjectId)).Select(x => x.Id).ToList();

            //var userlis = _context.ProjectRight.Where(u => filters.ProjectId == null || parent.Contains(u.ProjectId)).ToList();


            //var queryDtos = (from user in _context.UserLoginReport.Where(t => t.DeletedBy == null && filters.UserId == 4 && (filters.UserIds == null || filters.UserIds.Contains(t.UserId)))
            //                 join projectRight in _context.ProjectRight.Where(u => filters.ProjectId == null || parent.Contains(u.ProjectId))
            //                 on user.UserId equals projectRight.UserId
            //                 join project in _context.Project on projectRight.ProjectId equals project.Id
            //                 select new UserReportDto
            //                 {
            //                     UserName = user.LoginName,
            //                     ParentProjectId = project.Id,
            //                     SiteName = string.IsNullOrEmpty(project.SiteName) ? project.ProjectName : project.SiteName,
            //                     LoginTime = _context.UserLoginReport.FirstOrDefault(a => a.UserId == projectRight.UserId).LoginTime.UtcDateTime(),
            //                     LogOutTime = user.LoginTime.UtcDateTime(),
            //                     RoleName = string.Join(" ,", _context.Users.FirstOrDefault(b => b.Id == user.UserId).UserRoles.Where(x => x.DeletedDate == null).Select(s => s.SecurityRole.RoleName).ToList())
            //                 }).ToList();//OrderBy(x => x.Id).ToList();

           var queryDtos = _context.UserLoginReport.Where(t => t.DeletedBy == null && filters.UserId == 4 && (filters.UserIds == null || filters.UserIds.Contains(t.UserId))).Select(
                x => new UserReportDto
                {
                    Id = x.Id,
                    UserName = x.LoginName,
                    LoginTime = x.LoginTime,
                    LogOutTime = x.LogoutTime,
                }).ToList();
            queryDtos = queryDtos.OrderByDescending(x => x.Id).ToList();
           
            return queryDtos;
        }

        public List<ProjectDocumentReviewDto> EtmfUserDropDown(int projectId, int? userId)
        {
            var projectListbyId = All.Where(x => x.ProjectId == projectId).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId, c.RoleId }, (key, group) => group.First());

            var etmf = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == projectId).ToList();
            var etmfresult = etmf.GroupBy(x => x.UserId).Select(x => new EtmfUserPermissionDto
            {
                Id = x.FirstOrDefault().Id,
                UserId = x.Key,
                IsRevoke = x.LastOrDefault().DeletedDate == null ? false : true,
                CreatedDate = x.FirstOrDefault().CreatedDate
            }).ToList();

            var result = latestProjectRight.GroupBy(x => x.UserId).Select(x => new ProjectDocumentReviewDto
            {
                Id = x.FirstOrDefault().Id,
                ProjectId = x.FirstOrDefault().ProjectId,
                UserId = x.Key,
                UserName = _context.Users.Where(p => p.Id == x.Key).Select(r => r.UserName).FirstOrDefault(),
                RoleName = _context.ProjectRight.Where(c => c.ProjectId == x.FirstOrDefault().ProjectId && c.UserId == x.Key
                && c.RoleId == x.FirstOrDefault().RoleId).Select(a => a.role.RoleName).FirstOrDefault(),
                IsRevoke = etmfresult.Where(y => y.UserId == x.Key).Count() > 0 ? etmfresult.Where(y => y.UserId == x.Key).FirstOrDefault().IsRevoke : true
            }).ToList();

            if (userId <= 0)
                result = result.Where(x => x.IsRevoke == true).ToList();

            return result;
        }
    }
}