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
    public class ProductReceiptRepository : GenericRespository<ProductReceipt>, IProductReceiptRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public ProductReceiptRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetProductReceipteDropDown(int ProjectId)
        {
            return All.Where(c => c.ProjectId == ProjectId).Select(c => new DropDownDto { Id = c.Id, Value = c.Project.ProjectCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public List<ProductReceiptGridDto> GetProductReceiptList(int ProjectId,bool isDeleted)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == ProjectId).
                   ProjectTo<ProductReceiptGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        // If Study Product Type already use than not delete and Edit
        public string StudyProductTypeAlreadyUse(int PharmacyStudyProductTypeId)
        {
            if (All.Any(x => x.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId && x.DeletedDate == null))
                return "Study Product is in use. Cannot edit or delete!";
            return "";
        }

    }
}
