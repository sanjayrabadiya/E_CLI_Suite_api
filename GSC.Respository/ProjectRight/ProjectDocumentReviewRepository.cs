using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.ProjectRight;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.ProjectRight
{
    public class ProjectDocumentReviewRepository : GenericRespository<ProjectDocumentReview>,
        IProjectDocumentReviewRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;

        public ProjectDocumentReviewRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public void SaveByUserId(int projectId, int userId)
        {
            var projectdetails = _context.Project.AsNoTracking().Where(x => x.Id == projectId).SingleOrDefault();
            var childprojectdetails =
                _context.Project.AsNoTracking().Where(x => x.ParentProjectId == projectId).ToList();
            if (projectdetails.ParentProjectId != null)
            {
                var documents = _context.ProjectDocument
                    .Where(x => x.ProjectId == projectdetails.ParentProjectId || x.ProjectId == projectId && x.DeletedDate == null).ToList();

                foreach (var item in documents)
                {
                    var documentReview = FindBy(x =>
                            x.ProjectDocumentId == item.Id && x.UserId == userId && x.ProjectId == projectId)
                        .FirstOrDefault();
                    var parentdocumentReview = FindBy(x =>
                        x.ProjectDocumentId == item.Id && x.UserId == userId &&
                        x.ProjectId == projectdetails.ParentProjectId).LastOrDefault();
                    if (documentReview == null)
                    {
                        Add(new ProjectDocumentReview
                        {
                            ProjectDocumentId = item.Id,
                            UserId = userId,
                            ProjectId = projectId,
                            IsReview = parentdocumentReview == null ? false : parentdocumentReview.IsReview,
                            TrainerId = parentdocumentReview == null ? null : parentdocumentReview.TrainerId,
                            TrainingType = parentdocumentReview == null ? null : parentdocumentReview.TrainingType,
                            TrainingDuration = parentdocumentReview == null
                                ? null
                                : parentdocumentReview.TrainingDuration,
                            ReviewNote = parentdocumentReview == null ? null : parentdocumentReview.ReviewNote,
                            ReviewDate = parentdocumentReview == null ? null : parentdocumentReview.ReviewDate
                        });
                    }
                    else
                    {
                        documentReview.DeletedDate = null;
                        documentReview.DeletedBy = null;
                        Update(documentReview);
                    }
                }
            }
            else
            {
                var documents = _context.ProjectDocument.Where(x => x.ProjectId == projectId && x.DeletedDate == null)
                    .ToList();

                foreach (var item in documents)
                {
                    var documentReview = FindBy(x =>
                            x.ProjectDocumentId == item.Id && x.UserId == userId && x.ProjectId == projectId)
                        .FirstOrDefault();
                    var childdocumentReview = childprojectdetails.Count == 0
                        ? null
                        : FindBy(x =>
                            x.ProjectDocumentId == item.Id && x.UserId == userId &&
                            x.ProjectId == childprojectdetails.LastOrDefault().Id).LastOrDefault();
                    if (documentReview == null)
                    {
                        Add(new ProjectDocumentReview
                        {
                            ProjectDocumentId = item.Id,
                            UserId = userId,
                            ProjectId = projectId,
                            IsReview = childdocumentReview == null ? false : childdocumentReview.IsReview,
                            TrainerId = childdocumentReview == null ? null : childdocumentReview.TrainerId,
                            TrainingType = childdocumentReview == null ? null : childdocumentReview.TrainingType,
                            TrainingDuration =
                                childdocumentReview == null ? null : childdocumentReview.TrainingDuration,
                            ReviewNote = childdocumentReview == null ? null : childdocumentReview.ReviewNote,
                            ReviewDate = childdocumentReview == null ? null : childdocumentReview.ReviewDate
                        });
                    }
                    else
                    {
                        documentReview.DeletedDate = null;
                        documentReview.DeletedBy = null;
                        Update(documentReview);
                    }
                }
            }
        }

        public void SaveByDocumentId(int documnetId, int projectId)
        {
            var allChild = new List<Data.Entities.Master.Project>();
            var projectParent = _context.Project.Where(x => x.Id == projectId).SingleOrDefault();
            allChild = _context.Project.Where(x => x.ParentProjectId == projectId
                                                  && x.DeletedDate == null).ToList();
            if (projectParent != null) allChild.Add(projectParent);

            foreach (var item in allChild)
            {
                var users = _context.ProjectRight.Where(x => x.ProjectId == item.Id && x.DeletedDate == null)
                    .Select(c => c.UserId).ToList().Distinct();

                foreach (var userId in users)
                {
                    var documentReview = FindBy(x => x.ProjectDocumentId == documnetId && x.UserId == userId)
                        .FirstOrDefault();
                    if (documentReview == null)
                    {
                        if (userId == item.CreatedBy)
                            Add(new ProjectDocumentReview
                            {
                                ProjectDocumentId = documnetId,
                                UserId = userId,
                                ProjectId = item.Id,
                                IsReview = true,
                                ReviewDate = DateTime.Now.ToUniversalTime()
                            });
                        else
                            Add(new ProjectDocumentReview
                            {
                                ProjectDocumentId = documnetId,
                                UserId = userId,
                                ProjectId = item.Id
                            });
                    }
                    else
                    {
                        documentReview.DeletedDate = null;
                        documentReview.DeletedBy = null;
                        Update(documentReview);
                    }
                }
            }

             _context.Save();
        }


        public void DeleteByUserId(int projectId, int userId)
        {
            var result = FindBy(x => x.ProjectId == projectId && x.UserId == userId)
                .ToList();
            foreach (var item in result)
            {
                item.DeletedBy = _jwtTokenAccesser.UserId;
                item.DeletedDate = DateTime.Now;
                Update(item);
            }
        }

        public void DeleteByDocumentId(int documnetId, int projectId)
        {
            var result = FindBy(x => x.ProjectDocumentId == documnetId)
                .ToList();
            foreach (var item in result)
            {
                item.DeletedDate = DateTime.Now;
                Update(item);
            }
        }

        public ProjectDashBoardDto GetProjectDashboard()
        {
            var projectDashBoardDto = new ProjectDashBoardDto();

            projectDashBoardDto.ProjectList = All.Where(x => x.UserId == _jwtTokenAccesser.UserId
                                                             && !x.IsReview && _context.ProjectRight.Any(a =>
                                                                 a.ProjectId == x.ProjectId
                                                                 && a.UserId == _jwtTokenAccesser.UserId &&
                                                                 a.RoleId == _jwtTokenAccesser.RoleId
                                                                 && x.DeletedDate == null) &&
                                                             _context.ProjectDocument.Any(a => a.ProjectId == x.ProjectId
                                                                                              && x.DeletedDate ==
                                                                                              null) &&
                                                             x.DeletedDate == null).Select(c =>
                new ProjectDocumentReviewDto
                {
                    Id = c.Id,
                    ProjectDocumentId = c.ProjectDocumentId,
                    ProjectId = c.ProjectId,
                    IsReview = c.IsReview,
                    UserId = c.UserId,
                    AssignedBy = c.User.UserName,
                    ProjectName = c.Project.ProjectName,
                    ProjectNumber = c.Project.ProjectCode,
                    DocumentPath = c.ProjectDocument.PathName
                }).ToList();

            projectDashBoardDto.ProjectPendingReview = projectDashBoardDto.ProjectList.Count;
            projectDashBoardDto.ProjectCount = _context.ProjectRight.Count(x => x.UserId == _jwtTokenAccesser.UserId
                                                                               && x.RoleId ==
                                                                               _jwtTokenAccesser.RoleId &&
                                                                               x.DeletedDate == null);

            projectDashBoardDto.ProjectReviewed = All.Count(x => x.UserId == _jwtTokenAccesser.UserId
                                                                 && x.IsReview && _context.ProjectRight.Any(a =>
                                                                     a.ProjectId == x.ProjectId
                                                                     && a.UserId == _jwtTokenAccesser.UserId &&
                                                                     a.RoleId == _jwtTokenAccesser.RoleId
                                                                     && x.DeletedDate == null) &&
                                                                 x.DeletedDate == null);

            return projectDashBoardDto;
        }

        public ProjectDashBoardDto GetProjectDashboardbyId(int id)
        {
            var projectDashBoardDto = new ProjectDashBoardDto();
            var project = _context.Project.Find(id);

            projectDashBoardDto.ProjectList = All.Where(x => x.UserId == _jwtTokenAccesser.UserId
                                                             && !x.IsReview && _context.ProjectRight.Any(a =>
                                                                 a.ProjectId == x.ProjectId
                                                                 && a.UserId == _jwtTokenAccesser.UserId
                                                                 && x.DeletedDate == null) &&
                                                             x.DeletedDate == null).Select(c =>
                new ProjectDocumentReviewDto
                {
                    Id = c.Id,
                    ProjectDocumentId = c.ProjectDocumentId,
                    ProjectId = c.ProjectId,
                    IsReview = c.IsReview,
                    UserId = c.UserId,
                    ProjectName = c.Project.ProjectName,
                    ProjectNumber = c.Project.ProjectCode,
                    DocumentPath = c.ProjectDocument.PathName,
                    FileName = c.ProjectDocument.FileName,
                    MimeType = c.ProjectDocument.MimeType,
                    ParentProjectCode = _context.Project.Where(x=>x.Id == project.ParentProjectId).FirstOrDefault().ProjectCode
                }).ToList();

            projectDashBoardDto.ProjectList.ForEach(projectDocumentReview =>
            {
                if (project.ParentProjectId == null)
                {
                    projectDocumentReview.ParentProjectCode = projectDocumentReview.ProjectNumber;
                    projectDocumentReview.ProjectNumber = null;                    
                }
                var projectRight = _context.ProjectRight.Where(a =>
                    a.ProjectId == projectDocumentReview.ProjectId
                    && a.UserId == _jwtTokenAccesser.UserId && a.RoleId == _jwtTokenAccesser.RoleId).FirstOrDefault();
                if (projectRight != null)
                {
                    projectDocumentReview.AssignedDate = projectRight.CreatedDate;
                    var createdByUser = _context.Users.Where(user => user.Id == projectRight.CreatedBy).FirstOrDefault();
                    if (createdByUser != null) projectDocumentReview.AssignedBy = createdByUser.UserName;
                }
            });

            projectDashBoardDto.ProjectList = projectDashBoardDto.ProjectList.Where(x => x.ProjectId == id).ToList();
            projectDashBoardDto.ProjectPendingReview = projectDashBoardDto.ProjectList.Count;
            projectDashBoardDto.ProjectCount = _context.ProjectRight.Count(x => x.UserId == _jwtTokenAccesser.UserId
                                                                               && x.RoleId ==
                                                                               _jwtTokenAccesser.RoleId &&
                                                                               x.DeletedDate == null);

            projectDashBoardDto.ProjectReviewed = All.Count(x => x.UserId == _jwtTokenAccesser.UserId
                                                                 && x.IsReview && _context.ProjectRight.Any(a =>
                                                                     a.ProjectId == x.ProjectId
                                                                     && a.UserId == _jwtTokenAccesser.UserId &&
                                                                     a.RoleId == _jwtTokenAccesser.RoleId
                                                                     && x.DeletedDate == null) &&
                                                                 x.DeletedDate == null);

            return projectDashBoardDto;
        }

        public List<DropDownDto> GetProjectDropDownProjectRight()
        {
            // changes by swati for child project
            var projectList = All.Where(x => x.UserId == _jwtTokenAccesser.UserId
                                             && _context.ProjectRight.Any(a => a.ProjectId == x.ProjectId
                                                                              && a.UserId == _jwtTokenAccesser.UserId &&
                                                                              a.RoleId == _jwtTokenAccesser.RoleId
                                                                              && !x.IsReview && a.DeletedDate == null &&
                                                                              a.RollbackReason == null) &&
                                             x.DeletedDate == null).Select(c => new DropDownDto
            {
                Id = c.ProjectId,
                Value = c.Project.ProjectCode,
                Code = c.Project.ProjectCode,
                ExtraData = c.Project.ParentProjectId
            }).OrderBy(o => o.Value).Distinct().ToList();

            if (projectList == null || projectList.Count == 0) return null;
            return projectList;
        }

        public List<DropDownDto> GetParentProjectDropDownProjectRight()
        {
            // changes by swati for child project
            var projectList = All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.Project.ParentProjectId == null
                                             && _context.ProjectRight.Any(a => a.ProjectId == x.ProjectId
                                                                              && a.UserId == _jwtTokenAccesser.UserId &&
                                                                              a.RoleId == _jwtTokenAccesser.RoleId
                                                                              && !x.IsReview && a.DeletedDate == null &&
                                                                              a.RollbackReason == null) &&
                                             x.DeletedDate == null).Select(c => new DropDownDto
                                             {
                                                 Id = c.ProjectId,
                                                 Value = c.Project.ProjectCode,
                                                 Code = c.Project.ProjectCode,
                                                 ExtraData = c.Project.ParentProjectId
                                             }).OrderBy(o => o.Value).Distinct().ToList();

            if (projectList == null || projectList.Count == 0) return null;
            return projectList;
        }

        public List<DropDownDto> GetChildProjectDropDownProjectRight(int ParentProjectId)
        {
            // changes by swati for child project
            var projectList = All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.Project.ParentProjectId == ParentProjectId
                                             && _context.ProjectRight.Any(a => a.ProjectId == x.ProjectId
                                                                              && a.UserId == _jwtTokenAccesser.UserId &&
                                                                              a.RoleId == _jwtTokenAccesser.RoleId
                                                                              && !x.IsReview && a.DeletedDate == null &&
                                                                              a.RollbackReason == null) &&
                                             x.DeletedDate == null).Select(c => new DropDownDto
                                             {
                                                 Id = c.ProjectId,
                                                 Value = c.Project.ProjectCode,
                                                 Code = c.Project.ProjectCode,
                                                 ExtraData = c.Project.ParentProjectId
                                             }).OrderBy(o => o.Value).Distinct().ToList();

            if (projectList == null || projectList.Count == 0) return null;
            return projectList;
        }

        public ProjectDashBoardDto GetCompleteTrainingDashboard(int id)
        {
            var projectDashBoardDto = new ProjectDashBoardDto();
            var project = _context.Project.Find(id);

            projectDashBoardDto.ProjectList = All.Where(x => x.UserId == _jwtTokenAccesser.UserId
                                                             && x.IsReview && _context.ProjectRight.Any(a =>
                                                                 a.ProjectId == x.ProjectId
                                                                 && a.UserId == _jwtTokenAccesser.UserId
                                                                 && x.DeletedDate == null) &&
                                                             x.DeletedDate == null).Select(c =>
                new ProjectDocumentReviewDto
                {
                    Id = c.Id,
                    ProjectDocumentId = c.ProjectDocumentId,
                    ProjectId = c.ProjectId,
                    IsReview = c.IsReview,
                    UserId = c.UserId,
                    ProjectName = c.Project.ProjectName,
                    ProjectNumber = c.Project.ProjectCode,
                    DocumentPath = c.ProjectDocument.PathName,
                    FileName = c.ProjectDocument.FileName,
                    MimeType = c.ProjectDocument.MimeType,
                    ParentProjectCode = _context.Project.Where(x => x.Id == project.ParentProjectId).FirstOrDefault().ProjectCode,
                    ReviewDate = c.ReviewDate
                }).ToList();

            projectDashBoardDto.ProjectList.ForEach(projectDocumentReview =>
            {
                var projectRight = _context.ProjectRight.Where(a =>
                    a.ProjectId == projectDocumentReview.ProjectId
                    && a.UserId == _jwtTokenAccesser.UserId && a.RoleId == _jwtTokenAccesser.RoleId).FirstOrDefault();
                if (projectRight != null)
                {
                    projectDocumentReview.AssignedDate = projectRight.CreatedDate;
                    var createdByUser = _context.Users.Where(user => user.Id == projectRight.CreatedBy).FirstOrDefault();
                    if (createdByUser != null) projectDocumentReview.AssignedBy = createdByUser.UserName;
                }
            });

            projectDashBoardDto.ProjectList = projectDashBoardDto.ProjectList.Where(x => x.ProjectId == id).ToList();
       
            projectDashBoardDto.ProjectReviewed = All.Count(x => x.UserId == _jwtTokenAccesser.UserId
                                                                 && x.IsReview && _context.ProjectRight.Any(a =>
                                                                     a.ProjectId == x.ProjectId
                                                                     && a.UserId == _jwtTokenAccesser.UserId &&
                                                                     a.RoleId == _jwtTokenAccesser.RoleId
                                                                     && x.DeletedDate == null) &&
                                                                 x.DeletedDate == null);

            return projectDashBoardDto;
        }

    }
}