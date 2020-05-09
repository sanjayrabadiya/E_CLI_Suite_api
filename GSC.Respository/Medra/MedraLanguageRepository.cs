using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Medra
{
    public class MedraLanguageRepository : GenericRespository<MedraLanguage, GscContext>, IMedraLanguageRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public MedraLanguageRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetLanguageDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.LanguageName }).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(MedraLanguage objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.LanguageName == objSave.LanguageName && x.DeletedDate == null))
                return "Duplicate Language name : " + objSave.LanguageName;

            return "";
        }
    }
}