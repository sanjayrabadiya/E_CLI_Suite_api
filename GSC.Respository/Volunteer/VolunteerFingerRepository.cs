using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Screening;
using GSC.Data.Dto.Volunteer;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using Microsoft.EntityFrameworkCore;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Data.Entities.Volunteer;

namespace GSC.Respository.Volunteer
{
    public class VolunteerFingerRepository : GenericRespository<Data.Entities.Volunteer.VolunteerFinger>,
        IVolunteerFingerRepository
    {
        private readonly ICityRepository _cityRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly INumberFormatRepository _numberFormatRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectDesignPeriodRepository _projectDesignPeriodRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;

        public VolunteerFingerRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            INumberFormatRepository numberFormatRepository,
            IUploadSettingRepository uploadSettingRepository,
            ICityRepository cityRepository,
            ICompanyRepository companyRepository,
            IRolePermissionRepository rolePermissionRepository,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IMapper mapper
        )
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uploadSettingRepository = uploadSettingRepository;
            _numberFormatRepository = numberFormatRepository;
            _cityRepository = cityRepository;
            _companyRepository = companyRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _context = context;
            _mapper = mapper;
        }

        public List<DbRecords> GetFingers()
        {
            var finger = FindByInclude(t => t.DeletedDate == null, t => t.Volunteer).Select(x => new DbRecords
            {
                m_Id = x.Volunteer.Id,
                m_UserName = x.Volunteer.VolunteerNo + " " + x.Volunteer.FirstName + " " + x.Volunteer.MiddleName + " " + x.Volunteer.LastName,
                m_Template = x.FingerImage,
                UserBlock = x.Volunteer.IsBlocked == true ? true : false,
                UserInActive = x.Volunteer.DeletedDate != null,
            }).ToList();

            return finger;
        }
    }
}