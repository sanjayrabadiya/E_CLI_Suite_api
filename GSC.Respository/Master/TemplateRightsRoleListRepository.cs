using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class TemplateRightsRoleListRepository : GenericRespository<TemplateRightsRoleList>,
        ITemplateRightsRoleListRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public TemplateRightsRoleListRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetTemplateRightsRoleDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.TemplateRightsId.ToString()}).OrderBy(o => o.Value)
                .ToList();
        }

        public string Duplicate(TemplateRightsRoleList objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.TemplateRightsId == objSave.TemplateRightsId && x.DeletedDate == null))
                return "Duplicate Template Rights name : " + objSave.TemplateRightsId;
            return "";
        }
    }
}