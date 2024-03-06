using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using System.Collections.Generic;
using System.Linq;


namespace GSC.Respository.SupplyManagement
{
    public class SupplyLocationRepository : GenericRespository<SupplyLocation>, ISupplyLocationRepository
    {
        private readonly IMapper _mapper;

        public SupplyLocationRepository(IGSCContext context,IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
        }

        public List<DropDownDto> GetSupplyLocationDropDown()
        {
            return All.Select(c => new DropDownDto { Id = c.Id, Value = c.LocationCode + "-" + c.LocationName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(SupplyLocation objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.LocationCode == objSave.LocationCode && x.DeletedDate == null))
                return "Duplicate Location code : " + objSave.LocationCode;

            if (All.Any(x => x.Id != objSave.Id && x.LocationName == objSave.LocationName && x.DeletedDate == null))
                return "Duplicate Location name : " + objSave.LocationName;
            return "";
        }

        public List<SupplyLocationGridDto> GetSupplyLocationList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<SupplyLocationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}
