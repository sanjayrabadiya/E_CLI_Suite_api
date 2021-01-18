using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master.LanguageSetup;
using GSC.Data.Entities.LanguageSetup;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.LanguageSetup
{
    public class VariabeValueLanguageRepository : GenericRespository<VariableValueLanguage>, IVariabeValueLanguageRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public VariabeValueLanguageRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<VariableValueLanguageGridDto> GetVariableValueLanguageList(int VariableValueId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignVariableValueId == VariableValueId).
                   ProjectTo<VariableValueLanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public bool IsLanguageExist(int LanguageId)
        {
            var check = All.Where(x => x.DeletedDate == null && x.LanguageId == LanguageId).
                   ProjectTo<VariableValueLanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            if (check.Count != 0)
                return false;
            return true;
        }
    }
}
