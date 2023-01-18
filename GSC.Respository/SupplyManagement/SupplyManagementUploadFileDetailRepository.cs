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
    public class SupplyManagementUploadFileDetailRepository : GenericRespository<SupplyManagementUploadFileDetail>, ISupplyManagementUploadFileDetailRepository
    {

        private readonly IMapper _mapper;
        public SupplyManagementUploadFileDetailRepository(IGSCContext context,

            IMapper mapper)
            : base(context)
        {

            _mapper = mapper;
        }

        public List<SupplyManagementUploadFileDetailDto> GetSupplyManagementUploadFileDetailList(int SupplyManagementUploadFileId)
        {
            var details = All.Where(x => x.SupplyManagementUploadFileId == SupplyManagementUploadFileId).
                   ProjectTo<SupplyManagementUploadFileDetailDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.RandomizationNo).ToList();
            foreach (var item in details)
            {
                item.Visits = item.Visits.OrderBy(x => x.Id).ToList();
            }

            return details;
        }
    }
}
