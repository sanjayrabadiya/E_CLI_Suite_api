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
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
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
    public class SupplyManagementKITDetailRepository : GenericRespository<SupplyManagementKITDetail>, ISupplyManagementKITDetailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IGSCContext _context;

        public SupplyManagementKITDetailRepository(IGSCContext context,
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
    }
}
