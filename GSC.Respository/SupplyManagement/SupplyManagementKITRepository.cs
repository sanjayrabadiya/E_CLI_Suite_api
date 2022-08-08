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
    public class SupplyManagementKITRepository : GenericRespository<SupplyManagementKIT>, ISupplyManagementKITRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IGSCContext _context;

        public SupplyManagementKITRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
        IProjectDesignVisitRepository projectDesignVisitRepository,
             IProjectRepository projectRepository,
         ICountryRepository countryRepository,
        IMapper mapper)
            : base(context)
        {

            _projectDesignVisitRepository = projectDesignVisitRepository;
            _projectRepository = projectRepository;
            _countryRepository = countryRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<SupplyManagementKITGridDto> GetKITList(bool isDeleted, int ProjectId)
        {
            return _context.SupplyManagementKITDetail.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.SupplyManagementKIT.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementKITGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public IList<DropDownDto> GetVisitDropDownByAllocation(int projectId)
        {
            var visits = _context.SupplyManagementAllocation.Where(x => x.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
             && x.ProjectDesignVisit.ProjectDesignPeriod.DeletedDate == null && x.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.DeletedDate == null
             && x.DeletedDate == null)
                .Select(x => new DropDownDto
                {
                    Id = x.ProjectDesignVisit.Id,
                    Value = x.ProjectDesignVisit.DisplayName,
                }).Distinct().ToList();
            return visits;
        }

        public List<KitListApproved> getApprovedKit(int id)
        {
            var obj = _context.SupplyManagementShipment.Where(x => x.Id == id).FirstOrDefault();
            if (obj == null)
                return new List<KitListApproved>();
            var data = new List<KitListApproved>();

            data = _context.SupplyManagementKITDetail.Where(x =>
                    x.SupplyManagementShipmentId == id
                    && x.Status == Helper.KitStatus.Allocated
                    && x.DeletedDate == null).Select(x => new KitListApproved
                    {
                        Id = x.Id,
                        KitNo = x.KitNo,
                        VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                        SiteCode = x.SupplyManagementKIT.Site.ProjectCode

                    }).OrderByDescending(x => x.KitNo).ToList();
            foreach (var item in data)
            {

                var refrencetype = Enum.GetValues(typeof(KitStatus))
                                    .Cast<KitStatus>().Select(e => new DropDownEnum
                                    {
                                        Id = Convert.ToInt16(e),
                                        Value = e.GetDescription()
                                    }).Where(x => x.Id == 4 || x.Id == 5 || x.Id == 6 || x.Id == 7).ToList();
                item.StatusList = refrencetype;
            }

            return data;
        }
    }
}
