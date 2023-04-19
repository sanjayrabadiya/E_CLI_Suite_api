using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Office2010.Excel;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.ProjectRight;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.ProjectRight
{
    public class ProjectDocumentReviewRepository : GenericRespository<ProjectDocumentReview>,
        IProjectDocumentReviewRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public ProjectDocumentReviewRepository(IGSCContext context, IUploadSettingRepository uploadSettingRepository, IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
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
                                ReviewDate = _jwtTokenAccesser.GetClientDate()// DateTime.Now.ToUniversalTime()
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
                    ParentProjectCode = _context.Project.Where(x => x.Id == project.ParentProjectId).FirstOrDefault().ProjectCode
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
                                             && _context.ProjectRight.Any(a => (a.project.ParentProjectId == x.ProjectId || a.ProjectId == x.ProjectId)
                                                                              && a.UserId == _jwtTokenAccesser.UserId &&
                                                                              a.RoleId == _jwtTokenAccesser.RoleId
                                                                              && a.DeletedDate == null &&
                                                                              a.RollbackReason == null) &&
                                             x.DeletedDate == null)
                .Select(c => new DropDownDto
                {
                    Id = c.ProjectId,
                    Value = c.Project.ProjectCode,
                    Code = c.Project.ProjectCode,
                    ExtraData = c.Project.ParentProjectId
                }).OrderBy(o => o.Value).Distinct().ToList();

            var childParentList = All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.Project.ParentProjectId != null
                                            && _context.ProjectRight.Any(a => (a.project.ParentProjectId == x.ProjectId || a.ProjectId == x.ProjectId)
                                                                             && a.UserId == _jwtTokenAccesser.UserId &&
                                                                             a.RoleId == _jwtTokenAccesser.RoleId
                                                                             && a.DeletedDate == null &&
                                                                             a.RollbackReason == null) &&
                                            x.DeletedDate == null).Select(c => new DropDownDto
                                            {
                                                Id = (int)c.Project.ParentProjectId,
                                                Value = _context.Project.Where(x => x.Id == c.Project.ParentProjectId).FirstOrDefault().ProjectCode,
                                                ExtraData = c.Project.ParentProjectId
                                            }).OrderBy(o => o.Value).Distinct().ToList();
            projectList.AddRange(childParentList);

            if (projectList == null || projectList.Count == 0) return null;
            return projectList.GroupBy(d => d.Id).Select(c => new DropDownDto
            {
                Id = c.FirstOrDefault().Id,
                Value = c.FirstOrDefault().Value,
                Code = c.FirstOrDefault().Code,
                ExtraData = c.FirstOrDefault().ExtraData
            }).OrderBy(o => o.Id).ToList();
        }


        public List<ProjectDropDown> GetChildProjectDropDownProjectRight(int ParentProjectId)
        {
            // changes by swati for child project
            var projectList = _context.Project.Include(x => x.ManageSite).Where(x => x.ParentProjectId == ParentProjectId
                                               && _context.ProjectRight.Any(a => a.ProjectId == x.Id
                                                                                && a.UserId == _jwtTokenAccesser.UserId &&
                                                                                a.RoleId == _jwtTokenAccesser.RoleId
                                                                                && a.DeletedDate == null &&
                                                                                a.RollbackReason == null) &&
                                               x.DeletedDate == null).Select(c => new ProjectDropDown
                                               {
                                                   Id = c.Id,
                                                   Value = c.ProjectCode == null ? c.ManageSite.SiteName : c.ProjectCode + " - " + c.ManageSite.SiteName,
                                                   Code = c.ProjectCode,
                                                   ParentProjectId = (int)c.ParentProjectId,
                                                   CountryId = c.ManageSite.City.State.CountryId
                                               }).OrderBy(o => o.Value).Distinct().ToList();

            if (projectList == null || projectList.Count == 0) return null;
            return projectList;
        }

        //public List<ProjectDropDown> GetChildProjectDropDownProjectRight(int ParentProjectId)
        //{
        //    // changes by swati for child project
        //    var projectList = All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.Project.ParentProjectId == ParentProjectId
        //                                     && _context.ProjectRight.Any(a => a.ProjectId == x.ProjectId
        //                                                                      && a.UserId == _jwtTokenAccesser.UserId &&
        //                                                                      a.RoleId == _jwtTokenAccesser.RoleId
        //                                                                      && a.DeletedDate == null &&
        //                                                                      a.RollbackReason == null) &&
        //                                     x.DeletedDate == null).Select(c => new ProjectDropDown
        //                                     {
        //                                         Id = c.ProjectId,
        //                                         Value = c.Project.ProjectCode,
        //                                         Code = c.Project.ProjectCode,
        //                                         ParentProjectId = (int)c.Project.ParentProjectId,
        //                                         CountryId = c.Project.ManageSite.City.State.CountryId
        //                                     }).OrderBy(o => o.Value).Distinct().ToList();

        //    if (projectList == null || projectList.Count == 0) return null;
        //    return projectList;
        //}

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

            projectDashBoardDto.ProjectReviewed = All.Count(x => x.UserId == _jwtTokenAccesser.UserId
                                                                 && x.IsReview && _context.ProjectRight.Any(a =>
                                                                     a.ProjectId == x.ProjectId
                                                                     && a.UserId == _jwtTokenAccesser.UserId &&
                                                                     a.RoleId == _jwtTokenAccesser.RoleId
                                                                     && x.DeletedDate == null) &&
                                                                 x.DeletedDate == null);

            return projectDashBoardDto;
        }


        // Get only count for pending review document for project right

        public int GetPendingProjectTrainingCount(int id)
        {
            return All.Where(x => x.UserId == _jwtTokenAccesser.UserId && !x.IsReview
                                                              && _context.ProjectRight.Any(a => a.ProjectId == x.ProjectId && a.UserId == _jwtTokenAccesser.UserId && x.DeletedDate == null)
                                                              && x.ProjectId == id && x.DeletedDate == null).Count();
        }


        //Add By Tinku Mahato for Dashboard Project List
        public List<DashboardProject> GetDashboardProjectList()
        {
            var projectList = All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.Project.ParentProjectId == null
                                             && _context.ProjectRight.Any(a => (a.project.ParentProjectId == x.ProjectId || a.ProjectId == x.ProjectId)
                                                                              && a.UserId == _jwtTokenAccesser.UserId
                                                                              && a.RoleId == _jwtTokenAccesser.RoleId
                                                                              && a.DeletedDate == null
                                                                              && a.RollbackReason == null)
                                             && x.DeletedDate == null)
                .Select(c => new DashboardProject
                {
                    ProjectId = c.ProjectId,
                    CreatedDate = c.CreatedDate.Value
                }).ToList();

            var childParentList = All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.Project.ParentProjectId != null
                                           && _context.ProjectRight.Any(a => (a.project.ParentProjectId == x.ProjectId || a.ProjectId == x.ProjectId)
                                                                            && a.UserId == _jwtTokenAccesser.UserId
                                                                            && a.RoleId == _jwtTokenAccesser.RoleId
                                                                            && a.DeletedDate == null
                                                                            && a.RollbackReason == null) &&
                                           x.DeletedDate == null)
                .Select(c => new DashboardProject
                {
                    ProjectId = (int)c.Project.ParentProjectId,
                    CreatedDate = c.CreatedDate.Value
                }).ToList();



            projectList.AddRange(childParentList);

            var projects = projectList.GroupBy(d => d.ProjectId).Select(c => new DashboardProject
            {
                ProjectId = c.FirstOrDefault().ProjectId,
                CreatedDate = c.FirstOrDefault().CreatedDate
            }).OrderBy(o => o.ProjectId).ToList();


            projects.ForEach(item =>
            {
                var temCountries = new List<string>();

                var countries = _context.Project
              .Where(x => x.DeletedDate == null && x.ParentProjectId == item.ProjectId && x.ManageSite != null).Select(r => new
              {
                  Id = (int)r.ManageSite.City.State.CountryId,
                  CountryName = r.ManageSite.City.State.Country.CountryName,
                  CountryCode = r.ManageSite.City.State.Country.CountryCode
              }).Distinct().OrderBy(o => o.CountryCode).ToList();



                //var countries = _context.Project.Include(i => i.Country).Where(x => x.DeletedDate == null && x.ParentProjectId == item.ProjectId && x.ManageSite != null);

                var project = _context.Project.Where(x => x.ParentProjectId == null && x.Id == item.ProjectId).
                    ProjectTo<ProjectGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).FirstOrDefault();
                foreach (var country in countries)
                {
                    temCountries.Add(country.CountryName);
                }

                item.CountriesName = temCountries.Distinct().ToList();
                item.CountCountry = temCountries.Distinct().Count();
                item.projectCode = project.ProjectCode;
                item.Project = project;
            });

            return projects;
        }

        //Add By Tinku on 07/06/2022 for New Dasboard Tranning Data
        public ProjectDashBoardDto GetNewDashboardTranningData(int projectId, int countryId, int siteId)
        {
            var projectDashBoardDto = new ProjectDashBoardDto();
            var projectIds = new List<int>()
            {
                projectId,
                siteId
            };


            var ids = _context.Project.Include(x => x.ManageSite).Where(x => projectIds.Contains(x.ParentProjectId.Value)
                                                          && _context.ProjectRight.Any(a => a.ProjectId == x.Id
                                                          && a.UserId == _jwtTokenAccesser.UserId
                                                          && a.RoleId == _jwtTokenAccesser.RoleId
                                                          && a.DeletedDate == null)
                                                          && x.DeletedDate == null).Select(s => s.Id).ToList();
            projectIds.AddRange(ids);
            projectIds = projectIds.Distinct().ToList();

            projectDashBoardDto.ProjectList = All.Include(x => x.Project)
                .Include(x => x.Project.ManageSite).Where(x => x.UserId == _jwtTokenAccesser.UserId
                                                                 && _context.ProjectRight.Any(a =>
                                                                   projectIds.Contains(a.ProjectId)
                                                                    && a.UserId == _jwtTokenAccesser.UserId
                                                                    && x.DeletedDate == null) &&
                                                                    (siteId > 0 ? x.ProjectId == siteId : projectIds.Contains(x.ProjectId)) &&
                                                                    (countryId > 0 ? x.Project.ManageSite.City.State.CountryId == countryId : true) &&
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
                       ParentProjectCode = _context.Project.Where(x => x.Id == projectId).FirstOrDefault().ProjectCode,
                       ReviewDate = c.ReviewDate,
                       TrainingTypeName = c.TrainingType == null ? !c.IsReview ? "" : "Not Applicable" : c.TrainingType.GetDescription(),
                       TrainerName = c.TrainerId == null ? "Not Applicable" : _context.Users.FirstOrDefault(x => x.Id == c.TrainerId).UserName
                   }).Distinct().ToList();


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

                //if (!projectDocumentReview.IsReview)
                //{
                var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
                projectDocumentReview.DocumentPath = documentUrl + projectDocumentReview.DocumentPath;
                //}
            });

            return projectDashBoardDto;
        }


        public int CountTranningNotification()
        {

            var countTraining = All.Where(x => x.UserId == _jwtTokenAccesser.UserId
                                                                 && !x.IsReview && _context.ProjectRight.Any(a =>
                                                                     a.ProjectId == x.ProjectId
                                                                     && a.UserId == _jwtTokenAccesser.UserId &&
                                                                     a.RoleId == _jwtTokenAccesser.RoleId
                                                                     && x.DeletedDate == null) &&
                                                                 x.DeletedDate == null).Distinct().Count();

            return countTraining;
        }




    }
}