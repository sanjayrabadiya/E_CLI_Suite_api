using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.UserMgt
{
    public class RoleRepository : GenericRespository<SecurityRole, GscContext>, IRoleRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        public RoleRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public void UpdateSecurityRole(int id)
        {
            var userRole = Find(id);
            if (userRole == null) return;
            if (userRole.DeletedDate != null)
            {
                userRole.DeletedBy = null;
                userRole.DeletedDate = null;
                Update(userRole);
            }
            else
            {
                Delete(userRole);
            }
        }

        public string ValidateRole(SecurityRole objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.RoleShortName == objSave.RoleShortName && x.DeletedDate == null))
                return "Duplicate Short Role Name : " + objSave.RoleShortName;

            if (All.Any(x => x.Id != objSave.Id && x.RoleName == objSave.RoleName && x.DeletedDate == null))
                return "Duplicate Role Name : " + objSave.RoleName;

            return "";
        }

        public List<DropDownDto> GetSecurityRoleDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.RoleName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public List<SecurityRoleGridDto> GetSecurityRolesList(bool isDeleted)
        {

            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<SecurityRoleGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }
    }
}