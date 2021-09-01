using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementConfigurationRepository : GenericRespository<SupplyManagementConfiguration>, ISupplyManagementConfigurationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SupplyManagementConfigurationRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<SupplyManagementConfigurationGridDto> GetSupplyManagementTemplateList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<SupplyManagementConfigurationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(SupplyManagementConfiguration objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.AppScreenId == objSave.AppScreenId && x.VariableTemplateId == objSave.VariableTemplateId && x.DeletedDate == null))
                return "Duplicate form name : " + objSave.VariableTemplate.TemplateName;
            return "";
        }

        public SupplyManagementConfiguration GetTemplateByScreenCode(string screenCode)
        {
            return All.Where(x => x.AppScreen.ScreenCode == screenCode && x.DeletedDate == null).FirstOrDefault();
        }
    }
}
