using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class PharmacyStudyProductTypeRepository : GenericRespository<PharmacyStudyProductType>, IPharmacyStudyProductTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public PharmacyStudyProductTypeRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetPharmacyStudyProductTypeDropDown(int ProjectId)
        {
            return All.Where(c=>c.ProjectId == ProjectId).Select(c => new DropDownDto { Id = c.Id, Value = c.ProductType.ProductTypeCode + "-" + c.ProductType.ProductTypeName, IsDeleted = c.DeletedDate != null,ExtraData=((int)c.ProductUnitType) })
                .OrderBy(o => o.Value).ToList();
        }

        public List<PharmacyStudyProductTypeGridDto> GetPharmacyStudyProductTypeList(int ProjectId,bool isDeleted)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == ProjectId).
                   ProjectTo<PharmacyStudyProductTypeGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(PharmacyStudyProductType objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId && x.ProductTypeId == objSave.ProductTypeId && x.DeletedDate == null))
                return "Duplicate record found.";
            return "";
        }
    }
}
