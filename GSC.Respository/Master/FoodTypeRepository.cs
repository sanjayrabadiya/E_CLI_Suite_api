using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class FoodTypeRepository : GenericRespository<FoodType>, IFoodTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public FoodTypeRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetFoodTypeDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.TypeName,IsDeleted=c.DeletedDate !=null}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(FoodType objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.TypeName == objSave.TypeName.Trim() && x.DeletedDate == null))
                return "Duplicate FoodType name : " + objSave.TypeName;
            return "";
        }

        List<DropDownDto> IFoodTypeRepository.GetFoodTypeDropDown()
        {
            throw new System.NotImplementedException();
        }

        List<FoodTypeGridDto> IFoodTypeRepository.GetFoodTypeList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<FoodTypeGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}