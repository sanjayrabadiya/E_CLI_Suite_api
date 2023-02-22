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
            
            if (obj.KitCreationType == KitCreationType.KitWise)
            {
                if (_context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).Count(x => x.DeletedDate == null && x.SupplyManagementKIT.ProjectId == obj.ProjectId) > 0)
                {
                    return "Kits are already been preapared you can not modify or delete";
                }
            }
            if (obj.KitCreationType == KitCreationType.SequenceWise)
            {
                if (_context.SupplyManagementKITSeries.Count(x => x.DeletedDate == null && x.ProjectId == obj.ProjectId) > 0)
                {
                    return "Kits are already been preapared you can not modify or delete";
                }
            }

            return "";
        }
    }
}
