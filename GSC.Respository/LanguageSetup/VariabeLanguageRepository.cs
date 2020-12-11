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
    public class VariabeLanguageRepository : GenericRespository<VariableLanguage>, IVariabeLanguageRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public VariabeLanguageRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<VariableLanguageGridDto> GetVariableLanguageList(int VariableId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignVariableId == VariableId).
                   ProjectTo<VariableLanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public bool IsLanguageExist(int LanguageId)
        {
            var check = All.Where(x => x.DeletedDate == null && x.LanguageId == LanguageId).
                   ProjectTo<VariableLanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            if (check != null)
                return false;
            return true;
        }
    }
}
