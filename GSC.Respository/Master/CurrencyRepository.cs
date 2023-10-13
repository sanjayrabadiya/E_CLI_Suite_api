using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class CurrencyRepository : GenericRespository<Currency>, ICurrencyRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public CurrencyRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }
       public List<CurrencyGridDto> GetCurrencyList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<CurrencyGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public string Duplicate(Currency objSave)
        {             
            if (All.Any(x => x.Id != objSave.Id && x.CurrencyName == objSave.CurrencyName && x.CountryId== objSave.CountryId && x.DeletedDate == null))
                return "Duplicate CurrencyName : " + objSave.CurrencyName;
            return "";
        }
        public List<DropDownDto> GetCountryDropDown()
        {
            return _context.Country.Where(x => x.DeletedBy == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.CountryName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}