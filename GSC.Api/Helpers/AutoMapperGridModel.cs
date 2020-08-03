using AutoMapper;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Helpers
{
    public class AutoMapperGridModel : Profile
    {
        public AutoMapperGridModel()
        {
            CreateMap<BaseEntity, BaseAuditDto>()
            .ForMember(x => x.CreatedByUser, y => y.MapFrom(a => a.CreatedByUser.UserName))
            .ForMember(x => x.IsDeleted, y => y.MapFrom(a => a.DeletedDate != null))
            .ForMember(x => x.DeletedByUser, y => y.MapFrom(a => a.DeletedByUser.UserName))
            .ForMember(x => x.ModifiedByUser, y => y.MapFrom(a => a.ModifiedByUser.UserName)).IncludeAllDerived();


            CreateMap<Data.Entities.Master.Domain, DomainGridDto>()
                 .ForMember(x => x.DomainClassName, x => x.MapFrom(a => a.DomainClass.DomainClassName)).ReverseMap();

            CreateMap<DomainClass, DomainClassGridDto>().ReverseMap();
        }
    }
}
