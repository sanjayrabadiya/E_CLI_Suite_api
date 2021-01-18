using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.LanguageSetup;
using GSC.Data.Entities.LanguageSetup;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.LanguageSetup
{
    public class VariableCategoryLanguageRepository : GenericRespository<VariableCategoryLanguage>, IVariableCategoryLanguageRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public VariableCategoryLanguageRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<VariableCategoryLanguageGridDto> GetVariableCategoryLanguageList(int VariableCategoryId)
        {
            return All.Where(x => x.DeletedDate == null && x.VariableCategoryId == VariableCategoryId).
                   ProjectTo<VariableCategoryLanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public bool IsLanguageExist(int LanguageId)
        {
            var check = All.Where(x => x.DeletedDate == null && x.LanguageId == LanguageId).
                   ProjectTo<VariableCategoryLanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            if (check.Count != 0)
                return false;
            return true;
        }
    }
}
