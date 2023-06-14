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
    public class VariabeLabelLanguageRepository : GenericRespository<VariableLabelLanguage>, IVariabeLabelLanguageRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public VariabeLabelLanguageRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<VariableLabelLanguageGridDto> GetVariableLabelLanguageList(int VariableId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignVariableId == VariableId).
                   ProjectTo<VariableLabelLanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public bool IsLanguageExist(int LanguageId)
        {
            var check = All.Where(x => x.DeletedDate == null && x.LanguageId == LanguageId).
                   ProjectTo<VariableLabelLanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            if (check.Count != 0)
                return false;
            return true;
        }

    }
}
