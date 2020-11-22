using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Master
{
    public class LanguageRepository : GenericRespository<Language, GscContext>, ILanguageRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public LanguageRepository(IUnitOfWork<GscContext> uow,
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
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.LanguageName, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Language objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.LanguageName == objSave.LanguageName && x.DeletedDate == null))
                return "Duplicate Language name : " + objSave.LanguageName;

            return "";
        }

        public List<LanguageGridDto> GetLanguageList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<LanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}