using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Master
{
    public class FileSizeConfigurationRepository : GenericRespository<FileSizeConfiguration>, IFileSizeConfigurationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        public FileSizeConfigurationRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }
        public string Duplicate(FileSizeConfiguration objSave)
        {
            var screen = FindByInclude(s => s.ScreenId == objSave.ScreenId, x => x.AppScreens).FirstOrDefault();
            if (All.Any(x => x.Id != objSave.Id && x.ScreenId == objSave.ScreenId && x.DeletedDate == null))
                return "Duplicate File Size Configuration : " + screen.AppScreens.ScreenName;
            return "";
        }

        public List<DropDownDto> GetFileSizeConfigurationDropDown()
        {
            return All.Where(x =>
                   (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
               .Select(c => new DropDownDto { Id = c.Id, Value = c.AppScreens.ScreenName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public List<FileSizeConfigurationGridDto> GetFileSizeConfigurationList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                  ProjectTo<FileSizeConfigurationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}
