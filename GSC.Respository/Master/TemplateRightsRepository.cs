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
    public class TemplateRightsRepository : GenericRespository<TemplateRights>, ITemplateRightsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public TemplateRightsRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetDrugDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.TemplateCode}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(TemplateRights objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.TemplateCode == objSave.TemplateCode && x.DeletedDate == null))
                return "Duplicate Template Rights name : " + objSave.TemplateCode;
            return "";
        }
    }
}