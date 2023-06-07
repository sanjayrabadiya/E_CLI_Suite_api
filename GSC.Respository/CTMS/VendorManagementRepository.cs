using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class VendorManagementRepository : GenericRespository<VendorManagement>, IVendorManagementRepository
    {
        private readonly IMapper _mapper;

        public VendorManagementRepository(IGSCContext context,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
        }

        public List<DropDownDto> GetVendorDropDown()
        {
            return All.Where(x =>
                    x.VendorManagementAuditId == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.CompanyName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(VendorManagement objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ServiceType == objSave.ServiceType.Trim() && x.DeletedDate == null))
                return "Duplicate Vendor type name : " + objSave.CompanyName;
            return "";
        }
        public List<VendorManagementGridDto> GetVendorManagementList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
            ProjectTo<VendorManagementGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}