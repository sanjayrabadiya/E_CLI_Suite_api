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
    public class LanguageRepository : GenericRespository<Language, GscContext>, ILanguageRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public LanguageRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetLanguageDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.LanguageName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Language objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.LanguageName == objSave.LanguageName && x.DeletedDate == null))
                return "Duplicate Language name : " + objSave.LanguageName;

            return "";
        }
    }
}