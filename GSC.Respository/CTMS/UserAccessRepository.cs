using System;
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
            //var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).
            // ProjectTo<UserAccessGridDto>(_mapper.ConfigurationProvider).ToList();
            //var data = result.Select(r =>
            //{
            //    r.ProjectCode = _context.Project.Where(x => x.Id == r.ParentProjectId).Select(s => s.ProjectCode).FirstOrDefault();
            //    r.SiteCode = isDeleted ? r.InactiveSiteCode : r.SiteCode;
            //    return r;
            //}).ToList();

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

            //if (userAccessDto.siteUserAccess != null && userAccessDto.siteUserAccess[0].ProjectId != 0)
            //{
            //    userAccessDto.siteUserAccess.ForEach(t =>
            //    {
            //            t.UserAccessId = userAccessDto.Id;
            //            t.DeletedDate = null;
            //            _context.SiteUserAccess.Add(t);
            //            _context.Save();
            //    });
            //}
            //else
            //{
            //    userAccessDto.siteUserAccess[0].UserAccessId = userAccessDto.Id;
            //    userAccessDto.siteUserAccess[0].DeletedDate = null;
            //    userAccessDto.siteUserAccess[0].ProjectId = userAccessDto.ParentProjectId;
            //    _context.SiteUserAccess.Add(userAccessDto.siteUserAccess[0]);
            //    _context.Save();
            //}
        }
        public void UpdateSiteUserAccess(UserAccess userAccess)
        {
            var userAccessdata = _context.SiteUserAccess.Where(x => x.UserAccessId == userAccess.Id && x.DeletedDate == null)
               .ToList();

            userAccessdata.ForEach(t =>
            {
                if (userAccessdata != null)
                {
                    _context.SiteUserAccess.Remove(t);
                }
            });
            //userAccess.siteUserAccess.ForEach(z =>
            //{
            //    _context.SiteUserAccess.Add(z);
            //});
        }
        public void DeleteSiteUserAccess(int id)
        {
            var siteUserAccessData = _context.SiteUserAccess.Where(x => x.UserAccessId == id && x.DeletedDate == null).ToList();
            siteUserAccessData.ForEach(t =>
            {
                if (siteUserAccessData != null)
                {
                    t.DeletedBy = _jwtTokenAccesser.UserId;
                    t.DeletedDate = DateTime.UtcNow;
                    _context.SiteUserAccess.Update(t);
                }
            });
        }
        public void ActiveSiteUserAccess(int id)
        {
            var siteUserAccessData = _context.SiteUserAccess.Where(x => x.UserAccessId == id && x.DeletedDate != null).ToList();
            siteUserAccessData.ForEach(t =>
            {
                if (siteUserAccessData != null)
                {
                    t.DeletedBy = null;
                    t.DeletedDate = null;
                    _context.SiteUserAccess.Update(t);
                }
            });
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
