using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementKitNumberSettingsRepository : GenericRespository<SupplyManagementKitNumberSettings>, ISupplyManagementKitNumberSettingsRepository
    {

        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SupplyManagementKitNumberSettingsRepository(IGSCContext context,
        IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }

        public List<SupplyManagementKitNumberSettingsGridDto> GetKITNumberList(bool isDeleted, int ProjectId)
        {
            return _context.SupplyManagementKitNumberSettings.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementKitNumberSettingsGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public string CheckKitCreateion(SupplyManagementKitNumberSettings obj)
        {

            if (obj.KitCreationType == KitCreationType.KitWise && _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).Any(x => x.DeletedDate == null && x.SupplyManagementKIT.ProjectId == obj.ProjectId))
            {
                return "Kits are already been preapared you can not modify or delete";
            }
            if (obj.KitCreationType == KitCreationType.SequenceWise && _context.SupplyManagementKITSeries.Any(x => x.DeletedDate == null && x.ProjectId == obj.ProjectId))
            {
                return "Kits are already been preapared you can not modify or delete";
            }

            return "";
        }

        public void SaveRoleNumberSetting(SupplyManagementKitNumberSettingsDto supplyManagementKitNumberSettingsDto)
        {
            if (supplyManagementKitNumberSettingsDto.IsBlindedStudy == true && supplyManagementKitNumberSettingsDto.RoleId != null && supplyManagementKitNumberSettingsDto.RoleId.Count > 0)
            {
                foreach (var item in supplyManagementKitNumberSettingsDto.RoleId)
                {
                    SupplyManagementKitNumberSettingsRole supplyManagementKitNumberSettingsRole = new SupplyManagementKitNumberSettingsRole();
                    supplyManagementKitNumberSettingsRole.SupplyManagementKitNumberSettingsId = supplyManagementKitNumberSettingsDto.Id;
                    supplyManagementKitNumberSettingsRole.RoleId = item;
                    _context.SupplyManagementKitNumberSettingsRole.Add(supplyManagementKitNumberSettingsRole);

                }
                _context.Save();
            }
        }

        public void DeleteRoleNumberSetting(int id)
        {
            var data = _context.SupplyManagementKitNumberSettingsRole.Where(s => s.SupplyManagementKitNumberSettingsId == id).ToList();
            _context.SupplyManagementKitNumberSettingsRole.RemoveRange(data);
            _context.Save();

        }
    }
}
