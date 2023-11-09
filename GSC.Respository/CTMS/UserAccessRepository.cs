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

namespace GSC.Respository.CTMS
{
   public  class UserAccessRepository : GenericRespository<UserAccess>, IUserAccessRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public UserAccessRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context=context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
        public string Duplicate(UserAccessDto userAccessDto)
        {
            string msg = "";
            int count = 1;
            if (userAccessDto.siteUserAccess != null && userAccessDto.siteUserAccess[0].ProjectId != 0 && userAccessDto.IsSite == true){
                userAccessDto.siteUserAccess.ForEach(t =>
                {
                    t.multiUserAccess.ForEach(s =>
                    {
                        if (All.Any(x => x.Id != userAccessDto.Id && x.ParentProjectId == userAccessDto.ParentProjectId && x.ProjectId == t.ProjectId && x.UserRoleId == s.UserRoleId && x.DeletedDate == null))
                            msg = msg + count++ +") "+ _context.UserRole.Include(x=>x.User).Where(d=>d.Id == s.UserRoleId && d.DeletedByUser ==null).Select(m=>m.User.UserName).FirstOrDefault() + " has already been given access to this site ";
                    });
                });
            }
            else{
                userAccessDto.siteUserAccess.ForEach(t =>
                {
                    t.multiUserAccess.ForEach(s =>
                    {
                        if (All.Any(x => x.Id != userAccessDto.Id && x.ParentProjectId == userAccessDto.ParentProjectId && x.UserRoleId == s.UserRoleId && x.DeletedDate == null))
                            msg = msg + count++ + ") " + _context.UserRole.Include(x => x.User).Where(d => d.Id == s.UserRoleId && d.DeletedByUser == null).Select(m => m.User.UserName).FirstOrDefault() + " has already been given access to this study ";
                    });
                });
            }
            return msg;
        }
        public string DuplicateIActive(UserAccess userAccessDto)
        {
                        if (All.Any(x => x.Id != userAccessDto.Id && x.ParentProjectId == userAccessDto.ParentProjectId && x.ProjectId == userAccessDto.ProjectId && x.UserRoleId == userAccessDto.UserRoleId && x.DeletedDate == null))
                         return "This user has already been given access";
   
            return "";
        }
        public List<UserAccessGridDto> GetUserAccessList(bool isDeleted, int studyId, int siteId)
        {
            if (studyId > 0 && siteId == 0)
            {
                var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ParentProjectId == studyId).OrderByDescending(x => x.Id).
             ProjectTo<UserAccessGridDto>(_mapper.ConfigurationProvider).ToList();
                var data = result.Select(r =>
                {
                    r.ProjectCode = _context.Project.Where(x => x.Id == r.ParentProjectId).Select(s => s.ProjectCode).FirstOrDefault();
                    r.SiteCode = _context.Project.Where(x => x.Id == r.ProjectId).Select(c => c.ProjectCode == null ? c.ManageSite.SiteName : c.ProjectCode + " - " + c.ManageSite.SiteName).FirstOrDefault();
                    return r;
                }).ToList();
                return result;
            }
            else if(studyId > 0 && siteId > 0){
                var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ParentProjectId == studyId && x.ProjectId == siteId).OrderByDescending(x => x.Id).
                 ProjectTo<UserAccessGridDto>(_mapper.ConfigurationProvider).ToList();
                var data = result.Select(r =>
                {
                    r.ProjectCode = _context.Project.Where(x => x.Id == r.ParentProjectId).Select(s => s.ProjectCode).FirstOrDefault();
                    r.SiteCode = _context.Project.Where(x => x.Id == r.ProjectId).Select(c => c.ProjectCode == null ? c.ManageSite.SiteName : c.ProjectCode + " - " + c.ManageSite.SiteName).FirstOrDefault();
                    return r;
                }).ToList();
                return result;
            }
            else
            {
                var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).
                ProjectTo<UserAccessGridDto>(_mapper.ConfigurationProvider).ToList();
                var data = result.Select(r =>
                {
                    r.ProjectCode = _context.Project.Where(x => x.Id == r.ParentProjectId).Select(s => s.ProjectCode).FirstOrDefault();
                    r.SiteCode = _context.Project.Where(x => x.Id == r.ProjectId).Select(c => c.ProjectCode == null ? c.ManageSite.SiteName : c.ProjectCode + " - " + c.ManageSite.SiteName).FirstOrDefault();
                    return r;
                }).ToList();
                return result;
            }
        }
        //Add by Mitul On 09-11-2023 GS1-I3112 -> If CTMS On By default Add CTMS Access table.
        public void AddProjectRight(int ProjectId, bool isCtms)
        {
            var projectRightData = _context.ProjectRight.Where(s=>s.UserId == _jwtTokenAccesser.UserId && s.role.Id == _jwtTokenAccesser.RoleId && s.CreatedBy== _jwtTokenAccesser.UserId && s.DeletedBy==null && s.ProjectId== ProjectId).FirstOrDefault();
            var userRoleData= _context.UserRole.Where(s=> s.UserId == _jwtTokenAccesser.UserId && s.UserRoleId== _jwtTokenAccesser.RoleId).Select(r => r.Id).FirstOrDefault();
            var ctmsOnData= _context.ProjectSettings.Include(d=>d.Project).Where(s=>s.DeletedBy == null && s.IsCtms == isCtms && s.ProjectId== projectRightData.ProjectId).FirstOrDefault();
            var userAccessData = new UserAccess();
            if (isCtms){ 
                userAccessData.Id = 0;
                userAccessData.UserRoleId = userRoleData;
                userAccessData.ParentProjectId = ctmsOnData.ProjectId;
                userAccessData.ProjectId = ctmsOnData.ProjectId;
                _context.UserAccess.Add(userAccessData);
            }
            else{
                userAccessData = _context.UserAccess.Where(s => s.ProjectId == ctmsOnData.ProjectId && s.ParentProjectId == ctmsOnData.ProjectId && s.UserRoleId == userRoleData && s.DeletedBy == null).FirstOrDefault();
                userAccessData.DeletedBy = _jwtTokenAccesser.UserId;
                userAccessData.DeletedDate = DateTime.UtcNow;
                _context.UserAccess.Update(userAccessData);
            }
            _context.Save();

        }
        public void AddSiteUserAccesse(UserAccessDto userAccessDto)
        {
            if (userAccessDto.siteUserAccess != null && userAccessDto.siteUserAccess[0].ProjectId != 0 && userAccessDto.IsSite == true)
            {
                userAccessDto.siteUserAccess.ForEach(t =>
                {
                    t.multiUserAccess.ForEach(s =>
                    {
                        var userAccessData = new UserAccess();
                        userAccessData.Id = 0;
                        userAccessData.UserRoleId = s.UserRoleId;
                        userAccessData.ProjectId=t.ProjectId;
                        userAccessData.ParentProjectId = userAccessDto.ParentProjectId;
                        userAccessData.IsSite = userAccessDto.IsSite;
                        _context.UserAccess.Add(userAccessData);
                        _context.Save();
                        
                    });
                });
            }else
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
        public List<DropDownDto> GetRollUserDropDown(int projectId)
        {
           var data =_context.UserRole.Where(x => x.DeletedBy == null && x.User.DeletedBy==null && x.SecurityRole.DeletedBy==null)
                            .Select(c => new DropDownDto { Id = c.Id, Value = c.User.FirstName + ' ' + c.User.LastName, ExtraData = c.SecurityRole.RoleName,IsDeleted = c.DeletedDate != null })
                            .OrderBy(o => o.Value).ToList();
            return data;
        }
    }
}
