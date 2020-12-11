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
    public class TemplateLanguageRepository : GenericRespository<TemplateLanguage>, ITemplateLanguageRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public TemplateLanguageRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<TemplateLanguageGridDto> GetTemplateLanguageList(int TemplateId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == TemplateId).
                   ProjectTo<TemplateLanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public bool IsLanguageExist(int LanguageId)
        {
            var check = All.Where(x => x.DeletedDate == null && x.LanguageId == LanguageId).
                   ProjectTo<TemplateLanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            if (check != null)
                return false;
            return true;
        }
    }
}
