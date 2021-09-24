using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.CTMS
{
   public  class ResourceTypeRepository: GenericRespository<ResourceType>, IResourceTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public ResourceTypeRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetResourceTypeDropDown()
        {
            return All.Select(c => new DropDownDto { Id = c.Id, Value = c.ResourceName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(ResourceType objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ResourceCode == objSave.ResourceCode.Trim() && x.DeletedDate == null))
                return "Duplicate Resource Code : " + objSave.ResourceCode;

            if (All.Any(x => x.Id != objSave.Id && x.ResourceName == objSave.ResourceName.Trim() && x.DeletedDate == null))
                return "Duplicate Resource Name : " + objSave.ResourceName;

            return "";
        }
        public List<ResourceTypeGridDto> GetResourceTypeList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<ResourceTypeGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }
    }
}
