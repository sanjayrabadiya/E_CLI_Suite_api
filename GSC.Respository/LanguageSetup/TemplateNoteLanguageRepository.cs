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

namespace GSC.Respository.LanguageSetup
{
    public class TemplateNoteLanguageRepository : GenericRespository<TemplateNoteLanguage>, ITemplateNoteLanguageRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public TemplateNoteLanguageRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<TemplateNoteLanguageGridDto> GetTemplateNoteLanguageList(int TemplateNoteId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateNoteId == TemplateNoteId).
                   ProjectTo<TemplateNoteLanguageGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}
