using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Medra
{
    public class MedraLanguageRepository : GenericRespository<MedraLanguage, GscContext>, IMedraLanguageRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public MedraLanguageRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
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

        public List<MedraLanguageGridDto> GetMedraLanguageList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<MedraLanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}