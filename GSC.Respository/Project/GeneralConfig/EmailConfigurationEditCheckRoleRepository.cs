using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Generalconfig;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.GeneralConfig
{
    public class EmailConfigurationEditCheckRoleRepository : GenericRespository<EmailConfigurationEditCheckRole>, IEmailConfigurationEditCheckRoleRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IEmailConfigurationEditCheckRepository _emailConfigurationEditCheckRepository;
        public EmailConfigurationEditCheckRoleRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IEmailConfigurationEditCheckRepository emailConfigurationEditCheckRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _emailConfigurationEditCheckRepository = emailConfigurationEditCheckRepository;
        }

        public void AddChileRecord(EmailConfigurationEditCheckRoleDto emailConfigurationEditCheckRoleDto)
        {
            var roles = All.Where(x => x.EmailConfigurationEditCheckId == emailConfigurationEditCheckRoleDto.EmailConfigurationEditCheckId).ToList();
            if (roles.Count > 0)
            {
                _context.EmailConfigurationEditCheckRole.RemoveRange(roles);
                _context.Save();
            }
            var obj = _emailConfigurationEditCheckRepository.Find(emailConfigurationEditCheckRoleDto.EmailConfigurationEditCheckId);
            if (emailConfigurationEditCheckRoleDto.RoleId.Count > 0)
            {
                foreach (var item in emailConfigurationEditCheckRoleDto.RoleId)
                {
                    var data = All.Where(x => x.EmailConfigurationEditCheckId == emailConfigurationEditCheckRoleDto.EmailConfigurationEditCheckId && x.DeletedDate == null && x.RoleId == item).FirstOrDefault();
                    if (data == null)
                    {
                        EmailConfigurationEditCheckRole detail = new EmailConfigurationEditCheckRole();
                        detail.RoleId = item;
                        detail.EmailConfigurationEditCheckId = emailConfigurationEditCheckRoleDto.EmailConfigurationEditCheckId;
                        _context.EmailConfigurationEditCheckRole.Add(detail);
                        _context.Save();
                    }
                    else
                    {
                        var data1 = All.Where(x => x.EmailConfigurationEditCheckId == emailConfigurationEditCheckRoleDto.EmailConfigurationEditCheckId && x.DeletedDate != null && x.RoleId == item).FirstOrDefault();
                        if (data1 != null)
                        {
                            data1.DeletedBy = null;
                            data1.DeletedDate = null;
                            _context.EmailConfigurationEditCheckRole.Add(data1);
                            _context.Save();
                        }
                    }
                }
                if (obj != null)
                {
                    obj.Subject = emailConfigurationEditCheckRoleDto.Subject;
                    obj.EmailBody = emailConfigurationEditCheckRoleDto.EmailBody;
                    if (_jwtTokenAccesser.GetHeader("audit-reason-oth") != null && _jwtTokenAccesser.GetHeader("audit-reason-oth") != "")
                        obj.EditCheckRoleReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                    if (_jwtTokenAccesser.GetHeader("audit-reason-id") != null && _jwtTokenAccesser.GetHeader("audit-reason-id") != "")
                        obj.EditCheckRoleAuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
                    _emailConfigurationEditCheckRepository.Update(obj);
                    _context.Save();
                }
            }
        }

        public List<DropDownDto> GetProjectRightsRoleEmailTemplate(int projectId)
        {

            var projectrights = _context.ProjectRight.Include(x => x.User).Where(x => (x.ProjectId == projectId || x.project.ParentProjectId == projectId) && x.DeletedDate == null)
                              .Select(x => new DropDownDto { Id = x.RoleId, Value = x.role.RoleName }).Distinct().ToList();
            var randmomization = _context.Randomization.Include(s => s.Project).Where(s => s.Project.ParentProjectId == projectId).Select(s => s.UserId).ToList();
            if (randmomization.Count > 0)
            {
                var user = _context.Users.Where(s => randmomization.Contains(s.Id) && s.UserType == Shared.Generic.UserMasterUserType.Patient).FirstOrDefault();
                if (user != null)
                {
                    var role = _context.UserRole.Where(s => s.UserId == user.Id && s.DeletedDate == null).Select(s => s.SecurityRole).FirstOrDefault();
                    if (role != null)
                    {
                        DropDownDto obj = new DropDownDto();
                        obj.Id = role.Id;
                        obj.Value = role.RoleName;
                        projectrights.Add(obj);
                    }
                }
            }

            return projectrights; ;

        }


    }
}
