using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class CentralDepotRepository : GenericRespository<CentralDepot>, ICentralDepotRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public CentralDepotRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetStorageAreaByDepoDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.ProjectId == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.StorageArea, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public List<CentralDepotGridDto> GetCentralDepotList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<CentralDepotGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<DropDownDto> GetStorageAreaByProjectDropDown(int ProjectId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.ProjectId == ProjectId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.StorageArea, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }
    }
}
