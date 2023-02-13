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
using GSC.Respository.EmailSender;
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
    public class SupplyManagementKITSeriesRepository : GenericRespository<SupplyManagementKITSeries>, ISupplyManagementKITSeriesRepository
    {

        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IEmailSenderRespository _emailSenderRespository;
        public SupplyManagementKITSeriesRepository(IGSCContext context,
        IMapper mapper, IJwtTokenAccesser jwtTokenAccesser, IEmailSenderRespository emailSenderRespository)
            : base(context)
        {


            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _emailSenderRespository = emailSenderRespository;
        }

        public void AddKitSeriesVisitDetail(SupplyManagementKITSeriesDto data)
        {
            if (data.SupplyManagementKITSeriesDetail != null && data.SupplyManagementKITSeriesDetail.Count() > 0)
            {
                foreach (var item in data.SupplyManagementKITSeriesDetail)
                {
                    SupplyManagementKITSeriesDetail obj = new SupplyManagementKITSeriesDetail();
                    obj.SupplyManagementKITSeriesId = data.Id;
                    obj.NoOfImp = item.NoOfImp;
                    obj.NoofPatient = data.NoofPatient;
                    obj.ProjectDesignVisitId = item.ProjectDesignVisitId;
                    obj.PharmacyStudyProductTypeId = item.PharmacyStudyProductTypeId;
                    obj.TotalUnits = (item.NoOfImp * data.NoofPatient);
                    _context.SupplyManagementKITSeriesDetail.Add(obj);
                    
                }
                SupplyManagementKITSeriesDetailHistory history = new SupplyManagementKITSeriesDetailHistory();
                history.SupplyManagementKITSeriesId = data.Id;
                history.Status = KitStatus.AllocationPending;
                history.RoleId = _jwtTokenAccesser.RoleId;
                _context.SupplyManagementKITSeriesDetailHistory.Add(history);
                _context.Save();
            }
        }
    }
}
