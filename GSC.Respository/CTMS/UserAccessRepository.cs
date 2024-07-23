using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using GSC.Respository.Master;
using GSC.Common.Common;

namespace GSC.Respository.CTMS
{
    public class UserAccessRepository : GenericRespository<UserAccess>, IUserAccessRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRepository _projectRepository;

        public UserAccessRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IProjectRepository projectRepository, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRepository = projectRepository;
        }

        /// <summary>
        /// Checks for duplicate user access entries based on the provided UserAccessDto.
        /// </summary>
        /// <param name="userAccessDto">UserAccessDto containing user access information.</param>
        /// <returns>A string message indicating duplicates, if any.</returns>
        public string Duplicate(UserAccessDto userAccessDto)
        {
            string msg = "";
            int count = 1;
            foreach (var siteAccess in userAccessDto.siteUserAccess)
            {
                foreach (var userAccess in siteAccess.multiUserAccess)
                {
                    if (IsDuplicateUserAccess(userAccessDto, siteAccess, userAccess.UserRoleId))
                    {
                        msg += $"{count++}) {GetUserName(userAccess.UserRoleId)} has already been given access to this {(userAccessDto.IsSite ? "site" : "study")} ";
                    }
                }
            }
            return msg;
        }

        /// <summary>
        /// Checks if there is a duplicate user access entry based on the provided parameters.
        /// </summary>
        /// <param name="userAccessDto">UserAccessDto containing user access information to be checked.</param>
        /// <param name="siteAccess">SiteUserAccessDto representing the site-specific access details.</param>
        /// <param name="userRoleId">The ID of the user role to check for duplicates.</param>
        /// <returns>True if a duplicate user access entry exists; otherwise, false.</returns>
        private bool IsDuplicateUserAccess(UserAccessDto userAccessDto, SiteUserAccessDto siteAccess, int userRoleId)
        {
            return All.Any(x => x.Id != userAccessDto.Id && x.ParentProjectId == userAccessDto.ParentProjectId &&
                                x.ProjectId == (userAccessDto.IsSite ? siteAccess.ProjectId : userAccessDto.ParentProjectId) &&
                                x.UserRoleId == userRoleId && x.DeletedDate == null);
        }

        private string GetUserName(int userRoleId)
        {
            return _context.UserRole.Include(x => x.User).Where(d => d.Id == userRoleId && d.DeletedByUser == null)
                                     .Select(m => m.User.UserName).FirstOrDefault();
        }

        public List<UserAccess> GetActive(UserAccessDto userAccessDto)
        {
            var userAccessList = new List<UserAccess>();
            foreach (var siteAccess in userAccessDto.siteUserAccess)
            {
                foreach (var userAccess in siteAccess.multiUserAccess)
                {
                    var existingAccess = GetDeletedUserAccess(userAccessDto, siteAccess.ProjectId, userAccess.UserRoleId);
                    if (existingAccess != null)
                    {
                        RevokeUserAccess(existingAccess);
                        userAccessList.Add(existingAccess);
                    }
                }
            }
            return userAccessList;
        }

        private UserAccess GetDeletedUserAccess(UserAccessDto userAccessDto, int projectId, int userRoleId)
        {
            return _context.UserAccess.FirstOrDefault(x => x.Id != userAccessDto.Id && x.ParentProjectId == userAccessDto.ParentProjectId &&
                                                           x.ProjectId == projectId && x.UserRoleId == userRoleId && x.DeletedDate != null);
        }

        private void RevokeUserAccess(UserAccess userAccess)
        {
            userAccess.DeletedBy = null;
            userAccess.DeletedDate = null;
            _context.UserAccess.Update(userAccess);

            var auditTrail = new AuditTrail
            {
                TableName = "UserAccess",
                RecordId = userAccess.Id,
                Action = "Modified",
                CreatedDate = DateTime.UtcNow,
                UserId = _jwtTokenAccesser.UserId,
                UserRole = _jwtTokenAccesser.RoleName
            };
            _context.AuditTrail.Add(auditTrail);

            _context.Save();
        }

        public List<UserAccessGridDto> GetUserAccessList(bool isDeleted, int studyId, int siteId)
        {
            var query = All.Where(x => x.ParentProjectId == studyId);

            if (siteId > 0)
                query = query.Where(x => x.ProjectId == siteId);

            var result = query.OrderByDescending(x => x.Id)
                              .ProjectTo<UserAccessGridDto>(_mapper.ConfigurationProvider).ToList();

            return result.Select(r => PopulateUserAccessGridDto(r, studyId)).ToList();
        }

        private UserAccessGridDto PopulateUserAccessGridDto(UserAccessGridDto dto, int studyId)
        {
            dto.Access = dto.DeletedDate == null ? "Grant" : "Revoke";
            dto.ProjectCode = _context.Project.Where(x => x.Id == dto.ParentProjectId && x.DeletedDate == null).Select(s => s.ProjectCode).FirstOrDefault();
            dto.SiteCode = _context.Project.Where(x => x.Id == dto.ProjectId && x.DeletedDate == null).Select(c => c.ProjectCode == null ? c.ManageSite.SiteName : c.ManageSiteId != null ? c.ProjectCode + " - " + c.ManageSite.SiteName : "").FirstOrDefault();
            dto.LoginUser = _jwtTokenAccesser.UserId;
            dto.projectCreatedBy = _context.Project.Where(x => x.Id == studyId && x.DeletedDate == null).Select(s => s.CreatedByUser.Id).FirstOrDefault();
            return dto;
        }

        public void AddProjectRight(int projectId, bool isCtms)
        {
            var projectRight = _context.ProjectRight.FirstOrDefault(s => s.UserId == _jwtTokenAccesser.UserId &&
                                                                         s.role.Id == _jwtTokenAccesser.RoleId &&
                                                                         s.CreatedBy == _jwtTokenAccesser.UserId &&
                                                                         s.DeletedBy == null && s.ProjectId == projectId);

            var userRoleId = GetUserRoleId();
            var ctmsOnData = _context.ProjectSettings.Include(d => d.Project)
                                                     .FirstOrDefault(s => s.DeletedBy == null && s.IsCtms == isCtms && projectRight != null && s.ProjectId == projectRight.ProjectId);

            if (isCtms && ctmsOnData != null)
                AddUserAccess(ctmsOnData.ProjectId, userRoleId);
            else
                RevokeUserAccess(projectId, userRoleId);

            _context.Save();

            // Add site in CTMS access table
            var siteProjects = _projectRepository.GetChildProjectDropDown(projectId);
            foreach (var siteProject in siteProjects)
                AddProjectSiteRight(projectId, siteProject.Id);
        }

        private int GetUserRoleId()
        {
            return _context.UserRole.Where(s => s.UserId == _jwtTokenAccesser.UserId && s.UserRoleId == _jwtTokenAccesser.RoleId && s.DeletedBy == null)
                                    .Select(r => r.Id).FirstOrDefault();
        }

        private void AddUserAccess(int projectId, int userRoleId)
        {
            var userAccess = new UserAccess
            {
                UserRoleId = userRoleId,
                ParentProjectId = projectId,
                ProjectId = projectId
            };
            _context.UserAccess.Add(userAccess);
        }

        private void RevokeUserAccess(int projectId, int userRoleId)
        {
            var userAccess = _context.UserAccess.FirstOrDefault(s => s.ProjectId == projectId && s.ParentProjectId == projectId && s.UserRoleId == userRoleId && s.DeletedBy == null);
            if (userAccess != null)
            {
                userAccess.DeletedBy = _jwtTokenAccesser.UserId;
                userAccess.DeletedDate = DateTime.UtcNow;
                _context.UserAccess.Update(userAccess);
            }
        }

        public void AddProjectSiteRight(int parentProjectId, int projectId)
        {
            var isCtms = _context.ProjectSettings.Where(x => x.ProjectId == parentProjectId).Select(s => s.IsCtms).FirstOrDefault();
            var projectRight = _context.ProjectRight.FirstOrDefault(s => s.UserId == _jwtTokenAccesser.UserId &&
                                                                         s.role.Id == _jwtTokenAccesser.RoleId &&
                                                                         s.CreatedBy == _jwtTokenAccesser.UserId &&
                                                                         s.DeletedBy == null && s.ProjectId == projectId);

            var userRoleId = GetUserRoleId();
            if (projectRight != null)
            {
                var ctmsOnData = _context.Project.FirstOrDefault(s => s.DeletedBy == null && s.Id == projectRight.ProjectId);

                if (isCtms && ctmsOnData != null)
                    AddUserAccess(ctmsOnData.Id, userRoleId);
                else
                    RevokeUserAccess(projectId, userRoleId);
            }

            _context.Save();
        }

        public void AddSiteUserAccess(UserAccessDto userAccessDto)
        {
            if (userAccessDto?.siteUserAccess == null || userAccessDto.siteUserAccess.Count == 0)
                return;

            foreach (var siteAccess in userAccessDto.siteUserAccess)
            {
                foreach (var userAccess in siteAccess.multiUserAccess)
                {
                    AddUserAccess(userAccessDto, siteAccess.ProjectId, userAccess.UserRoleId);
                }
            }
        }

        private void AddUserAccess(UserAccessDto userAccessDto, int projectId, int userRoleId)
        {
            var userAccessData = new UserAccess
            {
                Id = 0,
                UserRoleId = userRoleId,
                ProjectId = userAccessDto.IsSite ? projectId : userAccessDto.ParentProjectId,
                ParentProjectId = userAccessDto.ParentProjectId,
                IsSite = userAccessDto.IsSite
            };
            _context.UserAccess.Add(userAccessData);
            _context.Save();
        }


        public List<DropDownDto> GetRoleUserDropDown()
        {
            var rolePermissionIds = _context.RolePermission
                .Where(x => x.ScreenCode == "mnu_ctms" && x.DeletedDate == null && x.IsView && x.IsEdit)
                .Select(s => s.UserRoleId)
                .ToList();

            var data = _context.UserRole
                .Where(x => x.DeletedBy == null && x.User.DeletedBy == null && x.SecurityRole.DeletedBy == null && rolePermissionIds.Contains(x.SecurityRole.Id))
                .Select(c => new DropDownDto
                {
                    Id = c.Id,
                    Value = c.User.FirstName + " " + c.User.LastName,
                    ExtraData = c.SecurityRole.RoleName,
                    IsDeleted = c.DeletedDate != null
                })
                .OrderBy(o => o.Value)
                .ToList();

            return data;
        }
        public List<UserAccessHistoryDto> GetUserAccessHistory(int id)
        {
            var result = _context.AuditTrail.Where(x => x.RecordId == id && x.TableName == "UserAccess" && (x.Action == "Modified" || x.Action == "Deleted" || x.Action == "Added"))
                .Select(x => new UserAccessHistoryDto
                {
                    Id = x.Id,
                    TableName = x.TableName,
                    RecordId = x.RecordId,
                    Action = x.Action == "Deleted" ? "Revoke" : x.Action == "Added" ? "Grant" : x.Action == "Modified" ? "Grant" : "",
                    ReasonOth = x.ReasonOth,
                    ReasonName = x.Reason,
                    RevokeOn = x.Action == "Deleted" ? x.CreatedDate : null,
                    RevokeBy = x.Action == "Deleted" ? x.User.UserName : "",
                    RevokeByRole = x.Action == "Deleted" ? x.UserRole : "",
                    GrantOn = x.Action == "Added" ? x.CreatedDate : x.Action == "Modified" ? x.CreatedDate : null,
                    GrantBy = x.Action == "Added" ? x.User.UserName : x.Action == "Modified" ? x.User.UserName : "",
                    GrantByRole = x.Action == "Added" ? x.UserRole : x.Action == "Modified" ? x.UserRole : "",
                    TimeZone = x.TimeZone
                }).ToList();

            return result;
        }
    }
}
