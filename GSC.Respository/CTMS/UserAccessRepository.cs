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
using System.Linq.Dynamic.Core;
using GSC.Common.Common;

namespace GSC.Respository.CTMS
{
    public class UserAccessRepository : GenericRespository<UserAccess>, IUserAccessRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRepository _projectRepository;

        public UserAccessRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IProjectRepository projectRepository,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRepository = projectRepository;
        }

        //GRANT User Duplication Check
        public string Duplicate(UserAccessDto userAccessDto)
        {
            string msg = "";
            int count = 1;
            if (userAccessDto.siteUserAccess != null && userAccessDto.siteUserAccess[0].ProjectId != 0 && userAccessDto.IsSite)
            {
                userAccessDto.siteUserAccess.ForEach(t =>
                {
                    t.multiUserAccess.ForEach(s =>
                    {
                        if (All.Any(x => x.Id != userAccessDto.Id && x.ParentProjectId == userAccessDto.ParentProjectId && x.ProjectId == t.ProjectId && x.UserRoleId == s.UserRoleId && x.DeletedDate == null))
                            msg = msg + count++ + ") " + _context.UserRole.Include(x => x.User).Where(d => d.Id == s.UserRoleId && d.DeletedByUser == null).Select(m => m.User.UserName).FirstOrDefault() + " has already been given access to this site ";
                    });
                });
            }
            else
            {
                userAccessDto.siteUserAccess.ForEach(t =>
                {
                    t.multiUserAccess.ForEach(s =>
                    {
                        if (All.Any(x => x.Id != userAccessDto.Id && x.ParentProjectId == userAccessDto.ParentProjectId && x.ProjectId == userAccessDto.ParentProjectId && x.UserRoleId == s.UserRoleId && x.DeletedDate == null))
                            msg = msg + count++ + ") " + _context.UserRole.Include(x => x.User).Where(d => d.Id == s.UserRoleId && d.DeletedByUser == null).Select(m => m.User.UserName).FirstOrDefault() + " has already been given access to this study ";
                    });
                });
            }
            return msg;
        }
        //Add by mitul 06-12-2023 Revoke User same as add new thet time revoke user to convert Grant does not add new value
        public List<UserAccess> getActive(UserAccessDto userAccessDto)
        {
            List<UserAccess> UserAccessList = new List<UserAccess>();
            var userAccessData = new UserAccess();
            var auditTrailData = new AuditTrail();
            if (userAccessDto.siteUserAccess != null && userAccessDto.siteUserAccess[0].ProjectId != 0 && userAccessDto.IsSite)
            {
                userAccessDto.siteUserAccess.ForEach(t =>
                {
                    t.multiUserAccess.ForEach(s =>
                    {
                        userAccessData = _context.UserAccess.Where(x => x.Id != userAccessDto.Id && x.ParentProjectId == userAccessDto.ParentProjectId && x.ProjectId == t.ProjectId && x.UserRoleId == s.UserRoleId && x.DeletedDate != null).FirstOrDefault();
                        if (userAccessData != null)
                        {
                            userAccessData.DeletedBy = null;
                            userAccessData.DeletedDate = null;
                            _context.UserAccess.Update(userAccessData);

                            //Add by mitul on 08-12-2023 auditTrail Data add manually
                            auditTrailData.Id = 0;
                            auditTrailData.TableName = "UserAccess";
                            auditTrailData.RecordId = userAccessData.Id;
                            auditTrailData.Action = "Modified";
                            auditTrailData.CreatedDate = DateTime.UtcNow;
                            auditTrailData.UserId = _jwtTokenAccesser.UserId;
                            auditTrailData.UserRole = _jwtTokenAccesser.RoleName;
                            _context.AuditTrail.Add(auditTrailData);

                            _context.Save();
                            UserAccessList.Add(userAccessData);
                        }
                    });

                });

                return UserAccessList;
            }
            else
            {
                userAccessDto.siteUserAccess.ForEach(t =>
                {
                    t.multiUserAccess.ForEach(s =>
                    {
                        userAccessData = _context.UserAccess.Where(x => x.Id != userAccessDto.Id && x.ParentProjectId == userAccessDto.ParentProjectId && x.ProjectId == userAccessDto.ParentProjectId && x.UserRoleId == s.UserRoleId && x.DeletedDate != null).FirstOrDefault();
                        if (userAccessData != null)
                        {
                            userAccessData.DeletedBy = null;
                            userAccessData.DeletedDate = null;
                            _context.UserAccess.Update(userAccessData);

                            //Add by mitul on 08-12-2023 auditTrail Data add manually
                            auditTrailData.Id = 0;
                            auditTrailData.TableName = "UserAccess";
                            auditTrailData.RecordId = userAccessData.Id;
                            auditTrailData.Action = "Modified";
                            auditTrailData.CreatedDate = DateTime.UtcNow;
                            auditTrailData.UserId = _jwtTokenAccesser.UserId;
                            auditTrailData.UserRole = _jwtTokenAccesser.RoleName;
                            _context.AuditTrail.Add(auditTrailData);

                            _context.Save();
                            UserAccessList.Add(userAccessData);
                        }
                    });
                });
                return UserAccessList;
            }
        }
        public List<UserAccessGridDto> GetUserAccessList(bool isDeleted, int studyId, int siteId)
        {
            if (studyId > 0 && siteId == 0)
            {
                var result = All.Where(x => x.ParentProjectId == studyId).OrderByDescending(x => x.Id).
                ProjectTo<UserAccessGridDto>(_mapper.ConfigurationProvider).ToList();
                return result.Select(r =>
                {
                    r.Access = r.DeletedDate == null ? "Grant" : "Revoke";
                    r.ProjectCode = _context.Project.Where(x => x.Id == r.ParentProjectId && x.DeletedDate == null).Select(s => s.ProjectCode).FirstOrDefault();
                    r.SiteCode = _context.Project.Where(x => x.Id == r.ProjectId && x.DeletedDate == null).Select(c => c.ProjectCode == null ? c.ManageSite.SiteName : GetSiteName(c)).FirstOrDefault();
                    r.LoginUser = _jwtTokenAccesser.UserId;
                    r.projectCreatedBy = _context.Project.Where(x => x.Id == studyId && x.DeletedDate == null).Select(s => s.CreatedByUser.Id).FirstOrDefault();
                    return r;
                }).ToList();
            }
            else if (studyId > 0 && siteId > 0)
            {
                var result = All.Where(x => x.ParentProjectId == studyId && x.ProjectId == siteId).OrderByDescending(x => x.Id).
                ProjectTo<UserAccessGridDto>(_mapper.ConfigurationProvider).ToList();
               return result.Select(r =>
                {
                    r.Access = r.DeletedDate == null ? "Grant" : "Revoke";
                    r.ProjectCode = _context.Project.Where(x => x.Id == r.ParentProjectId).Select(s => s.ProjectCode).FirstOrDefault();
                    r.SiteCode = _context.Project.Where(x => x.Id == r.ProjectId).Select(c => c.ProjectCode == null ? c.ManageSite.SiteName : GetSiteName(c)).FirstOrDefault();
                    r.LoginUser = _jwtTokenAccesser.UserId;
                    r.projectCreatedBy = _context.Project.Where(x => x.Id == studyId && x.DeletedDate == null).Select(s => s.CreatedByUser.Id).FirstOrDefault();
                    return r;
                }).ToList();
            }
            else
            {
                var result = All.Where(x => x.ParentProjectId == studyId).OrderByDescending(x => x.Id).
                ProjectTo<UserAccessGridDto>(_mapper.ConfigurationProvider).ToList();
               return result.Select(r =>
                {
                    r.Access = r.DeletedDate == null ? "Grant" : "Revoke";
                    r.ProjectCode = _context.Project.Where(x => x.Id == r.ParentProjectId).Select(s => s.ProjectCode).FirstOrDefault();
                    r.SiteCode = _context.Project.Where(x => x.Id == r.ProjectId).Select(c => c.ProjectCode == null ? c.ManageSite.SiteName : GetSiteName(c)).FirstOrDefault();
                    r.LoginUser = _jwtTokenAccesser.UserId;
                    r.projectCreatedBy = _context.Project.Where(x => x.Id == studyId && x.DeletedDate == null).Select(s => s.CreatedByUser.Id).FirstOrDefault();
                    return r;
                }).ToList();
            }
        }
        public string GetSiteName(Data.Entities.Master.Project project)
        {
            if (project.ManageSiteId != null)
                return project.ProjectCode + " - " + project.ManageSite.SiteName;
            else
                return "";
        }
        //Add by Mitul On 09-11-2023 GS1-I3112 -> If CTMS On By default Add CTMS Access table.
        public void AddProjectRight(int projectId, bool isCtms)
        {
            var projectRightData = _context.ProjectRight.Where(s => s.UserId == _jwtTokenAccesser.UserId && s.role.Id == _jwtTokenAccesser.RoleId && s.CreatedBy == _jwtTokenAccesser.UserId && s.DeletedBy == null && s.ProjectId == projectId).FirstOrDefault();
            var userRoleData = _context.UserRole.Where(s => s.UserId == _jwtTokenAccesser.UserId && s.UserRoleId == _jwtTokenAccesser.RoleId && s.DeletedBy == null).Select(r => r.Id).FirstOrDefault();
            var ctmsOnData = _context.ProjectSettings.Include(d => d.Project).Where(s => s.DeletedBy == null && s.IsCtms == isCtms && s.ProjectId == projectRightData.ProjectId).FirstOrDefault();
            var userAccessData = new UserAccess();
            if (isCtms)
            {
                userAccessData.Id = 0;
                userAccessData.UserRoleId = userRoleData;
                userAccessData.ParentProjectId = ctmsOnData.ProjectId;
                userAccessData.ProjectId = ctmsOnData.ProjectId;
                _context.UserAccess.Add(userAccessData);
            }
            else
            {
                userAccessData = _context.UserAccess.Where(s => s.ProjectId == ctmsOnData.ProjectId && s.ParentProjectId == ctmsOnData.ProjectId && s.UserRoleId == userRoleData && s.DeletedBy == null).FirstOrDefault();
                if (userAccessData != null)
                {
                    userAccessData.DeletedBy = _jwtTokenAccesser.UserId;
                    userAccessData.DeletedDate = DateTime.UtcNow;
                    _context.UserAccess.Update(userAccessData);
                }
            }
            _context.Save();

            //Add site in ctms access table
            var sitProject = _projectRepository.GetChildProjectDropDown(projectId);
            foreach (var item in sitProject)
            {
                AddProjectSiteRight(projectId, item.Id);
            }

        }
        public void AddProjectSiteRight(int ParentProjectId, int projectId)
        {
            bool IsCtms = _context.ProjectSettings.Where(x => x.ProjectId == ParentProjectId).Select(s => s.IsCtms).FirstOrDefault();
            var projectRightData = _context.ProjectRight.Where(s => s.UserId == _jwtTokenAccesser.UserId && s.role.Id == _jwtTokenAccesser.RoleId && s.CreatedBy == _jwtTokenAccesser.UserId && s.DeletedBy == null && s.ProjectId == projectId).FirstOrDefault();
            var userRoleData = _context.UserRole.Where(s => s.UserId == _jwtTokenAccesser.UserId && s.UserRoleId == _jwtTokenAccesser.RoleId && s.DeletedBy == null).Select(r => r.Id).FirstOrDefault();
            if (projectRightData != null)
            {
                var ctmsOnData = _context.Project.Where(s => s.DeletedBy == null && s.Id == projectRightData.ProjectId).FirstOrDefault();
                var userAccessData = new UserAccess();
                if (IsCtms && ctmsOnData != null)
                {
                    userAccessData.Id = 0;
                    userAccessData.UserRoleId = userRoleData;
                    userAccessData.ParentProjectId = ParentProjectId;
                    userAccessData.ProjectId = ctmsOnData.Id;
                    _context.UserAccess.Add(userAccessData);
                }
                else
                {
                    userAccessData = _context.UserAccess.Where(s => s.ProjectId == projectId && s.ParentProjectId == ParentProjectId && s.UserRoleId == userRoleData && s.DeletedBy == null).FirstOrDefault();
                    if (userAccessData != null)
                    {
                        userAccessData.DeletedBy = _jwtTokenAccesser.UserId;
                        userAccessData.DeletedDate = DateTime.UtcNow;
                        _context.UserAccess.Update(userAccessData);
                    }
                }
            }
            _context.Save();
        }
        public void AddSiteUserAccesse(UserAccessDto userAccessDto)
        {
            if (userAccessDto.siteUserAccess != null && userAccessDto.siteUserAccess[0].ProjectId != 0 && userAccessDto.IsSite)
            {
                userAccessDto.siteUserAccess.ForEach(t =>
                {
                    t.multiUserAccess.ForEach(s =>
                    {
                        var userAccessData = new UserAccess();
                        userAccessData.Id = 0;
                        userAccessData.UserRoleId = s.UserRoleId;
                        userAccessData.ProjectId = t.ProjectId;
                        userAccessData.ParentProjectId = userAccessDto.ParentProjectId;
                        userAccessData.IsSite = userAccessDto.IsSite;
                        _context.UserAccess.Add(userAccessData);
                        _context.Save();
                    });
                });
            }
            else
            {
                userAccessDto.siteUserAccess.ForEach(t =>
                {
                    t.multiUserAccess.ForEach(s =>
                    {
                        var userAccessData = new UserAccess();
                        userAccessData.Id = 0;
                        userAccessData.UserRoleId = s.UserRoleId;
                        userAccessData.ProjectId = userAccessDto.ParentProjectId;
                        userAccessData.ParentProjectId = userAccessDto.ParentProjectId;
                        userAccessData.IsSite = userAccessDto.IsSite;
                        _context.UserAccess.Add(userAccessData);
                        _context.Save();
                    });
                });
            }
        }
        public List<DropDownDto> GetRollUserDropDown()
        {
            var data = _context.UserRole.Where(x => x.DeletedBy == null && x.User.DeletedBy == null && x.SecurityRole.DeletedBy == null)
                             .Select(c => new DropDownDto { Id = c.Id, Value = c.User.FirstName + ' ' + c.User.LastName, ExtraData = c.SecurityRole.RoleName, IsDeleted = c.DeletedDate != null })
                             .OrderBy(o => o.Value).ToList();
            return data;
        }
        //Add by Mitul on 06-12-2023 get History in AuditTrail Deleted=Revoke And Added,Modified=Gran
        public List<UserAccessHistoryDto> GetUserAccessHistory(int id)
        {
            var result = _context.AuditTrail.Where(x => x.RecordId == id && x.TableName == "UserAccess" && (x.Action == "Modified" || x.Action == "Deleted" || x.Action == "Added"))
                .Select(x => new UserAccessHistoryDto
                {
                    Id = x.Id,
                    TableName = x.TableName,
                    RecordId = x.RecordId,
                    Action = getAction(x.Action),
                    ReasonOth = x.ReasonOth,
                    ReasonName = x.Reason,
                    RevokeOn = x.Action == "Deleted" ? x.CreatedDate : null,
                    RevokeBy = x.Action == "Deleted" ? x.User.UserName : "",
                    RevokeByRole = x.Action == "Deleted" ? x.UserRole : "",
                    GrantOn = getCreatedDate(x),
                    GrantBy = getUserName(x),
                    GrantByRole = getUserRole(x),
                    TimeZone = x.TimeZone
                }).ToList();

            return result;
        }
        public string getAction(string action)
        {
            if (action == "Added" || action == "Modified")
                return "Grant";
            else
                return "Revoke";
        }
        public DateTime? getCreatedDate(AuditTrail x)
        {
            if (x.Action == "Added" || x.Action == "Modified")
                return x.CreatedDate;
            else
                return null;
        }
        public string getUserName(AuditTrail x)
        {
            if (x.Action == "Added" || x.Action == "Modified")
                return x.User.UserName;
            else
                return "";
        }
        public string getUserRole(AuditTrail x)
        {
            if (x.Action == "Added" || x.Action == "Modified")
                return x.UserRole;
            else
                return "";
        }
    }
}
