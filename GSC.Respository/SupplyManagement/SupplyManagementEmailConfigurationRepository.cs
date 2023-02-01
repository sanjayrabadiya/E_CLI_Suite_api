using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementEmailConfigurationRepository : GenericRespository<SupplyManagementEmailConfiguration>, ISupplyManagementEmailConfigurationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        private readonly IGSCContext _context;

        public SupplyManagementEmailConfigurationRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,

        IMapper mapper)
            : base(context)
        {


            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<SupplyManagementEmailConfigurationGridDto> GetSupplyManagementEmailConfigurationList(int projectId, bool isDeleted)
        {
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectId == projectId).
                   ProjectTo<SupplyManagementEmailConfigurationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(x =>
            {
                if (x.SiteId > 0)
                {
                    x.SiteCode = _context.Project.Where(z => z.Id == x.SiteId).FirstOrDefault().ProjectCode;
                }
                if (x.AuditReasonId > 0)
                {
                    x.Reason = _context.AuditReason.Where(z => z.Id == x.AuditReasonId).FirstOrDefault().ReasonName;
                }

                //x.Roles = string.Join(", ", x.SupplyManagementEmailConfigurationDetail.Select(c => c.SecurityRole.RoleName).ToList().Distinct());
                //x.Users = string.Join(", ", x.SupplyManagementEmailConfigurationDetail.Select(c => c.Users.UserName).ToList().Distinct());
            });
            return data;
        }

        public List<ProjectRightDto> GetProjectRightsIWRS(int projectId)
        {
            List<ProjectRightDto> roles = new List<ProjectRightDto>();
            var projectrights = _context.ProjectRight.Include(x => x.User).Where(x => x.ProjectId == projectId && x.DeletedDate == null)
                              .Select(x => new { RoleId = x.RoleId, Name = x.role.RoleName }).Distinct().ToList();

            if (projectrights != null && projectrights.Count > 0)
            {
                foreach (var item in projectrights)
                {
                    ProjectRightDto obj = new ProjectRightDto();
                    obj.RoleId = item.RoleId;
                    obj.Name = item.Name;
                    obj.users = _context.ProjectRight.Where(a => a.DeletedDate == null && a.RoleId == item.RoleId && a.ProjectId == projectId).Select(r =>
                        new ProjectRightDto
                        {
                            RoleId = item.RoleId,
                            UserId = r.UserId,
                            Name = r.User.UserName
                        }).ToList();

                    roles.Add(obj);
                }
                return roles.Where(x => x.users.Count() != 0).ToList();
            }
            return new List<ProjectRightDto>();

        }

        public string Duplicate(SupplyManagementEmailConfiguration obj)
        {
            if (obj.Id > 0)
            {
                if (obj.SiteId > 0)
                {
                    var data = _context.SupplyManagementEmailConfiguration.Where(x => x.Id != obj.Id && x.ProjectId == obj.ProjectId && x.SiteId == obj.SiteId && x.DeletedDate == null && x.Triggers == obj.Triggers).FirstOrDefault();
                    if (data != null)
                    {
                        return "trigger already created!";
                    }
                    var data1 = _context.SupplyManagementEmailConfiguration.Where(x => x.Id != obj.Id && x.ProjectId == obj.ProjectId && x.SiteId == null && x.DeletedDate == null && x.Triggers == obj.Triggers).FirstOrDefault();
                    if (data1 != null)
                    {
                        return "trigger already created!";
                    }

                }
                else
                {
                    var data1 = _context.SupplyManagementEmailConfiguration.Where(x => x.Id != obj.Id && x.ProjectId == obj.ProjectId && x.DeletedDate == null && x.Triggers == obj.Triggers).FirstOrDefault();
                    if (data1 != null)
                    {
                        return "trigger already created!";
                    }
                }

            }
            else
            {
                if (obj.SiteId > 0)
                {
                    var data = _context.SupplyManagementEmailConfiguration.Where(x => x.ProjectId == obj.ProjectId && x.SiteId == obj.SiteId && x.DeletedDate == null && x.Triggers == obj.Triggers).FirstOrDefault();
                    if (data != null)
                    {
                        return "trigger already created!";
                    }
                    var data1 = _context.SupplyManagementEmailConfiguration.Where(x => x.ProjectId == obj.ProjectId && x.SiteId == null && x.DeletedDate == null && x.Triggers == obj.Triggers).FirstOrDefault();
                    if (data1 != null)
                    {
                        return "trigger already created!";
                    }
                    if (obj.Triggers == Helper.SupplyManagementEmailTriggers.Threshold)
                    {
                        return "You cannot configure site for threshold!";
                    }

                }
                else
                {
                    var data1 = _context.SupplyManagementEmailConfiguration.Where(x => x.ProjectId == obj.ProjectId && x.DeletedDate == null && x.Triggers == obj.Triggers).FirstOrDefault();
                    if (data1 != null)
                    {
                        return "trigger already created!";
                    }
                }
            }

            return "";
        }
        public void ChildEmailUserAdd(SupplyManagementEmailConfigurationDto obj, int id)
        {
            if (obj.SupplyManagementEmailConfigurationDetail.Count > 0)
            {
                foreach (var item in obj.SupplyManagementEmailConfigurationDetail)
                {
                    var data = _context.SupplyManagementEmailConfigurationDetail.Where(x => x.SupplyManagementEmailConfigurationId == id && x.DeletedDate == null && x.RoleId == item.RoleId && x.UserId == item.UserId).FirstOrDefault();
                    if (data == null)
                    {
                        SupplyManagementEmailConfigurationDetail detail = new SupplyManagementEmailConfigurationDetail();
                        detail.UserId = item.UserId;
                        detail.RoleId = item.RoleId;
                        detail.SupplyManagementEmailConfigurationId = id;
                        _context.SupplyManagementEmailConfigurationDetail.Add(detail);
                        _context.Save();
                    }
                    else
                    {
                        var data1 = _context.SupplyManagementEmailConfigurationDetail.Where(x => x.SupplyManagementEmailConfigurationId == id && x.DeletedDate != null && x.RoleId == item.RoleId && x.UserId == item.UserId).FirstOrDefault();
                        if (data1 != null)
                        {
                            data1.DeletedBy = null;
                            data1.DeletedDate = null;
                            _context.SupplyManagementEmailConfigurationDetail.Add(data1);
                            _context.Save();
                        }
                    }

                }
            }
        }
        public void DeleteChildEmailUser(int id)
        {
            var SupplyManagementEmailConfigurationDetail = _context.SupplyManagementEmailConfigurationDetail.Where(x => x.SupplyManagementEmailConfigurationId == id).ToList();
            _context.SupplyManagementEmailConfigurationDetail.AddRange(SupplyManagementEmailConfigurationDetail);
            _context.Save();
        }

        public List<SupplyManagementEmailConfigurationDetailGridDto> GetSupplyManagementEmailConfigurationDetailList(int id)
        {
            var data = _context.SupplyManagementEmailConfigurationDetail.Where(x => x.SupplyManagementEmailConfigurationId == id).
                   ProjectTo<SupplyManagementEmailConfigurationDetailGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            return data;
        }
    }
}
