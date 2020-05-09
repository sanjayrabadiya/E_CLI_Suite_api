using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class TemplateRightsRoleListRepository : GenericRespository<TemplateRightsRoleList, GscContext>,
        ITemplateRightsRoleListRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public TemplateRightsRoleListRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
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