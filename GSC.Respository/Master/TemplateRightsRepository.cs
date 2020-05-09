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
    public class TemplateRightsRepository : GenericRespository<TemplateRights, GscContext>, ITemplateRightsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public TemplateRightsRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
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