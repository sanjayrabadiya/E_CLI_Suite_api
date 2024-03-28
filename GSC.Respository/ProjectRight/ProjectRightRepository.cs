using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
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
            var roles = _context.SecurityRole.Where(x => x.DeletedDate == null && x.Id != 2).Select(c => new ProjectRightDto
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
                    }).Where(x => !x.IsSelected).ToList()
            }).ToList();

            return roles.Where(x => x.users.Count != 0).ToList();
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
                    var project = _context.Project.Where(x => x.Id == projectId).FirstOrDefault();
                    if (project != null)
                    {
                        var checkparentTrainig = project.ParentProjectId;
                        var checkchildTrainig = _context.Project.Where(x => x.ParentProjectId == projectId).ToList();



                        if (checkparentTrainig != null)
                        {
                            var isProjectRightFound = All.Where(x =>
                                 x.ProjectId == checkparentTrainig && x.UserId == itemDto.UserId).OrderByDescending(x => x.Id).FirstOrDefault();
                            var isProjectRightUserFound = All.Where(x => x.ProjectId == projectId && x.UserId == itemDto.UserId).OrderBy(x => x.Id).FirstOrDefault();

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

                            var isExist = lstIsProjectRightFound.Exists(l => !l.IsReviewDone);

                            var isProjectRightUserFound = All.Where(x => x.ProjectId == projectId && x.UserId == itemDto.UserId).OrderByDescending(x => x.Id).FirstOrDefault();
                            var last = lstIsProjectRightFound.FirstOrDefault();
                            if (!isExist && last != null)
                            {
                                Add(new Data.Entities.ProjectRight.ProjectRight
                                {
                                    ProjectId = projectId,
                                    UserId = itemDto.UserId,
                                    RoleId = itemDto.RoleId,
                                    IsReviewDone = last.IsReviewDone
                                });
                            }
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
                }
        }

        public void SaveProjectRollbackRight(List<ProjectRightDto> projectRightDto, int projectId, int[] ids)
        {


            foreach (var id in ids)
            {
                var projectRightuser = FindBy(x => x.Id == id).FirstOrDefault();
                var userByproject = FindBy(x => x.UserId == projectRightuser.UserId).ToList()
                    .Exists(a => a.DeletedDate == null);
                if (!userByproject && projectRightuser != null)
                    _documentReviewRepository.DeleteByUserId(projectId, projectRightuser.UserId);
            }

            foreach (var itemDto in ids)
            {
                var projectRight = FindBy(x => x.Id == itemDto).ToList();

                foreach (var item in projectRight)
                {
                    item.DeletedBy = _jwtTokenAccesser.UserId;
                    item.DeletedDate = _jwtTokenAccesser.GetClientDate();
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
                if (parentProjectExist.Count > 0) allChild.AddRange(parentProjectExist);
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
                if (parentProjectExist.Count > 0) allChild.AddRange(parentProjectExist);
            }

            var isSave = false;
            foreach (var allData in allChild.Select(s => s.Id))
            {
                var projectRights = All.AsNoTracking().Where(x => x.ProjectId == allData && x.DeletedDate == null)
                    .ToList();

                foreach (var projectRight in projectRights)
                {
                    if (_context.ProjectDocumentReview.AsNoTracking().Any(x =>
                        x.ProjectId == allData && x.UserId == projectRight.UserId && !x.IsReview && x.DeletedDate == null))
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

        public List<int> GetEtmfProjectRightIdList()
        {
            return All.Include(x => x.project).Where(c => c.DeletedDate == null && c.UserId == _jwtTokenAccesser.UserId &&
                                  c.RoleId == _jwtTokenAccesser.RoleId
                                  && c.IsReviewDone && c.project.DeletedDate == null).Select(x => x.ProjectId).ToList();
        }

        public List<int> GetEtmfChildProjectRightIdList()
        {
            return All.Include(x => x.project).Where(c => c.DeletedDate == null && c.UserId == _jwtTokenAccesser.UserId &&
                                  c.project.ParentProjectId != null &&
                                  c.RoleId == _jwtTokenAccesser.RoleId
                                  && c.IsReviewDone && c.project.DeletedDate == null).Select(x => (int)x.project.ParentProjectId).ToList();
        }

        public List<int> GetProjectRightIdList()
        {
            return All.Where(c => c.DeletedDate == null && c.UserId == _jwtTokenAccesser.UserId &&
                                  c.RoleId == _jwtTokenAccesser.RoleId
                                  && c.IsReviewDone).Select(x => x.ProjectId).ToList();
        }

        public List<int> GetParentProjectRightIdList()
        {
            return All.Where(c => c.DeletedDate == null && c.UserId == _jwtTokenAccesser.UserId &&
                                  c.RoleId == _jwtTokenAccesser.RoleId
                                  && c.IsReviewDone).Select(x => x.project.ParentProjectId ?? x.project.Id).Distinct().ToList();
        }
        //Add by Mitul On 09-11-2023 GS1-I3112 -> All study code get as based on User Access for CTMS
        public List<int> GetProjectCTMSRightIdList()
        {
            var userRoleId = _context.UserRole.Where(s => s.UserId == _jwtTokenAccesser.UserId && s.UserRoleId == _jwtTokenAccesser.RoleId && s.DeletedDate == null)
                             .Select(d => d.Id).FirstOrDefault();
            return _context.UserAccess.Where(c => c.DeletedDate == null && c.UserRoleId == userRoleId).Select(x => x.ParentProjectId).Distinct().ToList();
        }
        //Add by Mitul On 09-11-2023 GS1-I3112 -> All Site code get as based on User Access for CTMS
        public List<int> GetProjectChildCTMSRightIdList()
        {
            var userRoleId = _context.UserRole.Where(s => s.UserId == _jwtTokenAccesser.UserId && s.UserRoleId == _jwtTokenAccesser.RoleId && s.DeletedDate == null)
                             .Select(d => d.Id).FirstOrDefault();
            return _context.UserAccess.Where(c => c.DeletedDate == null && c.UserRoleId == userRoleId).Select(x => x.ProjectId).Distinct().ToList();
        }

        public List<int> GetChildProjectRightIdList()
        {
            return All.Where(c => c.DeletedDate == null && c.UserId == _jwtTokenAccesser.UserId &&
                                  c.project.ParentProjectId != null &&
                                  c.RoleId == _jwtTokenAccesser.RoleId
                                  && c.IsReviewDone).Select(x => (int)x.project.ParentProjectId).ToList();
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
                ParentProjectCode = GetProjectCode(x.ProjectId),
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
                                          }).AsEnumerable().OrderByDescending(k => k.AssignedDate).ToList(),
                PendingReview = _context.ProjectDocumentReview.Where(a => a.DeletedDate == null && a.ProjectId == x.ProjectId
                                          && a.UserId == x.UserId && !a.IsReview).Select(b => new ReviewDeteail
                                          {
                                              DocumentPath = b.ProjectDocument.FileName
                                          }).AsEnumerable().OrderByDescending(y => y.AssignedDate).ToList(),
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
                r.TotalReviewName = r.TotalReview == null ? "" : "Complete (" + r.TotalReview.Count + ")";
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
                r.PendingReviewName = r.PendingReview == null ? "" : "Pending (" + r.PendingReview.Count + ")";
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
        private string GetProjectCode(int projectId)
        {
            var project = _context.Project.Where(p => p.Id == projectId).FirstOrDefault();
            if (project != null && project.ParentProjectId == null)
            {
                return project.ProjectCode;
            }
            if (project != null && project.ParentProjectId != null)
            {
                var parentProject = _context.Project.Where(p => p.Id == project.ParentProjectId).FirstOrDefault();
                if (parentProject != null)
                    return parentProject.ProjectCode;
            }
            return "";
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
                    .Select(a => a.role.RoleName).AsEnumerable().Distinct()),
                AuditReasonID = x.AuditReasonId,
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
                var audit = _context.AuditReason.Where(y => y.Id == r.AuditReasonID).FirstOrDefault();
                if (audit != null)
                    r.AuditReason = audit.ReasonName;

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
                }).AsEnumerable().OrderByDescending(k => k.AssignedDate).ToList();

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

            var sites = new List<int>();
            if (filters.SiteId != null)
            {
                sites = _context.Project.Where(x => x.Id == filters.SiteId).Select(x => x.Id).ToList();
            }
            else
            {
                sites = _context.Project.Where(x => x.Id == filters.ProjectId && !x.IsTestSite).Select(x => x.Id).ToList();
            }

            var queryDtos = (from projectRight in _context.ProjectRight.Where(t =>
                            (filters.SiteId != null ? t.ProjectId == filters.SiteId : sites.Contains(t.ProjectId))
                             && (filters.UserIds == null || filters.UserIds.Contains(t.UserId))
                             && (filters.RoleIds == null || filters.RoleIds.Contains(t.RoleId)))
                             join project in _context.Project on projectRight.ProjectId equals project.Id
                             join auditReasonTemp in _context.AuditReason on projectRight.AuditReasonId equals auditReasonTemp.Id into auditReasonTempDto
                             from auditReason in auditReasonTempDto.DefaultIfEmpty()
                             join projectDocument in _context.ProjectDocument.Where(x => x.DeletedDate == null) on projectRight.ProjectId equals projectDocument.ProjectId
                             join projectDocumentReviewTemp in _context.ProjectDocumentReview on new { x = projectDocument.Id, y = projectRight.UserId, z = projectDocument.ProjectId }
                             equals new { x = projectDocumentReviewTemp.ProjectDocumentId, y = projectDocumentReviewTemp.UserId, z = projectDocumentReviewTemp.ProjectId }
                             into projectDocumentReviewDto
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
                                 ProjectCode = filters.SiteId != null ? _context.Project.Where(x => x.Id == filters.ProjectId).FirstOrDefault().ProjectCode : project.ProjectCode,
                                 SiteCode = filters.SiteId != null ? project.ProjectCode : null,
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
                ProjectCode = x.ProjectCode,
                SiteCode = x.SiteCode,
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
            var sites = new List<int>();
            if (filters.SiteId != null)
            {
                sites = _context.Project.Where(x => x.Id == filters.SiteId).Select(x => x.Id).ToList();
            }
            else
            {
                sites = _context.Project.Where(x => x.Id == filters.ProjectId && !x.IsTestSite).Select(x => x.Id).ToList();
            }

            var queryDtos = (from projectRight in _context.ProjectRight.Where(t => (filters.SiteId != null ? t.ProjectId == filters.SiteId : sites.Contains(t.ProjectId))
                                     && (filters.UserIds == null || filters.UserIds.Contains(t.UserId))
                                     && (filters.RoleIds == null || filters.RoleIds.Contains(t.RoleId)))
                             join project in _context.Project on projectRight.ProjectId equals project.Id
                             join auditReasonTemp in _context.AuditReason on projectRight.AuditReasonId equals auditReasonTemp.Id into auditReasonTempDto
                             from auditReason in auditReasonTempDto.DefaultIfEmpty()
                             join projectDocument in _context.ProjectDocument.Where(x => x.DeletedDate == null) on projectRight.ProjectId equals projectDocument.ProjectId
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
                                 ProjectCode = filters.SiteId != null ? _context.Project.Where(x => x.Id == filters.ProjectId).FirstOrDefault().ProjectCode : project.ProjectCode,
                                 SiteCode = filters.SiteId != null ? project.ProjectCode : null,
                                 UserName = user.UserName,
                                 RoleName = role.RoleName,
                                 AssignedBy = _context.Users.FirstOrDefault(a => a.Id == projectRight.CreatedBy).UserName,
                                 AssignedDate = projectRight.CreatedDate,
                                 DocumentName = projectDocument.FileName,
                                 ReviewDate = projectDocumentReview.ReviewDate,
                                 TrainerName = trainer.UserName,
                                 TrainingType = projectDocumentReview.TrainingType.GetDescription(),
                                 TrainingDuration = projectDocumentReview.TrainingDuration,
                                 ReviewNote = projectDocumentReview.ReviewNote,
                             }).OrderBy(x => x.Id).ToList();

            return queryDtos;
        }
        public IList<UserReportDto> GetUserReportList(UserReportSearchDto filters)
        {
            var results = (from user in _context.Users.Where(t => (filters.UserId == 2 && (t.DeletedBy != null || (t.ValidFrom.HasValue && t.ValidFrom.Value > DateTime.Now
                            || t.ValidTo.HasValue && t.ValidTo.Value < DateTime.Now)))
                            || (t.DeletedBy == null && filters.UserId == 3)
                            || (filters.UserId == 1))
                           select new UserReportDto
                           {
                               Id = user.Id,
                               UserName = user.UserName,
                               Session = "Active",
                               CreatedBy = user.CreatedByUser.UserName,
                               DeletedBy = user.DeletedByUser.UserName,
                               CreatedDate = user.CreatedDate,
                               DeletedDate = user.DeletedDate,
                               LoginTime = user.LastLoginDate,
                               LastIpAddress = user.LastIpAddress,
                               RoleName = string.Join(", ", user.UserRoles.Where(x => x.DeletedDate == null).Select(s => s.SecurityRole.RoleName).ToList()),
                               UserType = user.UserType
                           }).ToList();

            results = results.Where(x => x.UserType == Shared.Generic.UserMasterUserType.User).OrderBy(x => x.Id).ToList();

            if (filters.UserId == 1)
            {
                results = results.Select(x => { x.RoleName = _context.UserLoginReport.Where(a => a.UserId == x.Id).OrderByDescending(x => x.Id).Select(x => x.SecurityRole.RoleName).FirstOrDefault(); return x; }).ToList();
            }

            return results;
        }
        public IList<UserReportDto> GetLoginLogoutReportList(UserReportSearchDto filters)
        {
            var result = _context.UserLoginReport.Where(t => t.DeletedBy == null && filters.UserId == 4)
                .Select(x => new UserReportDto
                {
                    Id = x.Id,
                    UserName = x.LoginName,
                    LoginTime = x.LoginTime,
                    LogOutTime = x.LogoutTime,
                    SecurityRoleId = x.SecurityRoleId,
                    RoleName = x.SecurityRole.RoleName
                }).OrderByDescending(x => x.Id).ToList();

            return result;
        }

        public List<ProjectDocumentReviewDto> EtmfUserDropDown(int projectId, int? userId)
        {
            var projectListbyId = All.Where(x => x.ProjectId == projectId).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId, c.RoleId }, (key, group) => group.First());

            var etmf = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetail.ProjectWorkPlace.ProjectId == projectId).ToList();
            var etmfresult = etmf.OrderByDescending(x => x.Id).GroupBy(x => x.UserId).Select(x => new EtmfUserPermissionDto
            {
                Id = x.Select(s => s.Id).FirstOrDefault(),
                UserId = x.Key,
                IsRevoke = (x.Select(s => s.DeletedDate).FirstOrDefault() != null),
                CreatedDate = x.Select(s => s.CreatedDate).FirstOrDefault()
            }).ToList();

            var result = latestProjectRight.GroupBy(x => x.UserId).Select(x => new ProjectDocumentReviewDto
            {
                Id = x.Select(s => s.Id).FirstOrDefault(),
                ProjectId = x.Select(s => s.ProjectId).FirstOrDefault(),
                UserId = x.Key,
                RoleId = x.Select(s => s.RoleId).FirstOrDefault(),
                UserName = _context.Users.Where(p => p.Id == x.Key).Select(r => r.UserName).FirstOrDefault(),
                RoleName = _context.ProjectRight.Where(c => c.ProjectId == x.FirstOrDefault().ProjectId && c.UserId == x.Key
                && c.RoleId == x.FirstOrDefault().RoleId).Select(a => a.role.RoleName).FirstOrDefault(),
                IsRevoke = etmfresult.Exists(y => y.UserId == x.Key) ? etmfresult.Find(y => y.UserId == x.Key).IsRevoke : !etmfresult.Exists(y => y.UserId == x.Key)
            }).ToList();

            if (userId <= 0)
                result = result.Where(x => x.IsRevoke).ToList();

            return result;
        }
    }
}