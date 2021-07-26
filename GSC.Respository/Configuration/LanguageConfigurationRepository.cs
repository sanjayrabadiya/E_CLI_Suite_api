using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Configuration
{
    public class LanguageConfigurationRepository : GenericRespository<LanguageConfiguration>, ILanguageConfigurationRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public LanguageConfigurationRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
           : base(context)
        {
            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<LanguageConfigurationGridDto> GetlanguageConfiguration(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
             ProjectTo<LanguageConfigurationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<LanguageConfigurationDetailsGridDto> GetLanguageDetails(int LanguageConfigurationDetailsId)
        {
            return _context.LanguageConfigurationDetails.Where(x => x.LanguageConfigurationId == LanguageConfigurationDetailsId && x.DeletedDate == null).
             ProjectTo<LanguageConfigurationDetailsGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(LanguageConfiguration objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.KeyCode == objSave.KeyCode && x.DeletedDate == null))
                return "Duplicate Key code : " + objSave.KeyCode;

            if (All.Any(x => x.Id != objSave.Id && x.KeyName == objSave.KeyName && x.DeletedDate == null))
                return "Duplicate Key Name : " + objSave.KeyName;

            return "";
        }

        public string DuplicateLanguage(LanguageConfigurationDetails objSave)
        {
            if (_context.LanguageConfigurationDetails.Any(x => x.Id != objSave.Id && x.LanguageId == objSave.LanguageId && x.LanguageConfigurationId == objSave.LanguageConfigurationId && x.DeletedDate == null))
                return "Duplicate Language : ";

            return "";
        }

        public List<LanguageMessageDto> GetMultiLanguage()
        {
            var detail = All.Select(x => new LanguageMessageDto
            {
                KeyCode = x.KeyCode,
                KeyName = x.KeyName,
                Message = x.LanguageConfigurationDetailslist.Any(a => a.LanguageId == _jwtTokenAccesser.Language && a.LanguageConfigurationId == x.Id) ? x.LanguageConfigurationDetailslist.Where(a => a.LanguageId == _jwtTokenAccesser.Language && a.LanguageConfigurationId == x.Id).FirstOrDefault().Message : x.DefaultMessage              
            }).ToList();
            return detail;



        }
    }
}
