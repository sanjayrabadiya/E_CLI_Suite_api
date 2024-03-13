using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementKitAllocationSettingsRepository : GenericRespository<SupplyManagementKitAllocationSettings>, ISupplyManagementKitAllocationSettingsRepository
    {

        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public SupplyManagementKitAllocationSettingsRepository(IGSCContext context,
        IMapper mapper, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<SupplyManagementKitAllocationSettingsGridDto> GetKITAllocationList(bool isDeleted, int ProjectId)
        {
            var isShow = _context.SupplyManagementKitNumberSettingsRole.
                        Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == ProjectId
                        && s.RoleId == _jwtTokenAccesser.RoleId);

            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == ProjectId).FirstOrDefault();

            var data = _context.SupplyManagementKitAllocationSettings.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementKitAllocationSettingsGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(x =>
            {
                var pharmacyProductType = _context.PharmacyStudyProductType.Include(x => x.ProductType).Where(z => z.Id == x.PharmacyStudyProductTypeId).FirstOrDefault();
                if (pharmacyProductType != null && setting != null)
                {
                    x.ProductName = setting.IsBlindedStudy == true && isShow ? "" : pharmacyProductType.ProductType.ProductTypeCode;
                }
            });
            return data;
        }

        public IList<DropDownDto> GetVisitDropDownByProjectId(int projectId)
        {
            var othervisits = _context.SupplyManagementUploadFileVisit.Include(x => x.ProjectDesignVisit).ThenInclude(x => x.ProjectDesignPeriod).ThenInclude(x => x.ProjectDesign).ThenInclude(x => x.Project).Where(x =>
                                                               x.DeletedDate == null
                                                               && x.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId).Select(s => s.ProjectDesignVisitId).ToList();

            if (!othervisits.Any())
                return new List<DropDownDto>();

            var visits = _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriod.ProjectDesign.Project.Id == projectId
                         && x.DeletedDate == null && x.InActiveVersion == null && othervisits.Contains(x.Id))
                    .Select(x => new DropDownDto
                    {
                        Id = x.Id,
                        Value = x.DisplayName,
                    }).Distinct().ToList();
            return visits;

        }
    }
}
