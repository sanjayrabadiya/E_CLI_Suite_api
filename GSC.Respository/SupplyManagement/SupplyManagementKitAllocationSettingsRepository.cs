using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementKitAllocationSettingsRepository : GenericRespository<SupplyManagementKitAllocationSettings>, ISupplyManagementKitAllocationSettingsRepository
    {
        
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SupplyManagementKitAllocationSettingsRepository(IGSCContext context,
        IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }

        public List<SupplyManagementKitAllocationSettingsGridDto> GetKITAllocationList(bool isDeleted, int ProjectId)
        {
            return _context.SupplyManagementKitAllocationSettings.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementKitAllocationSettingsGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public IList<DropDownDto> GetVisitDropDownByProjectId(int projectId)
        {
            var visits = _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriod.ProjectDesign.Project.Id == projectId
                         && x.DeletedDate == null)
                    .Select(x => new DropDownDto
                    {
                        Id = x.Id,
                        Value = x.DisplayName,
                    }).Distinct().ToList();
            return visits;

        }
    }
}
