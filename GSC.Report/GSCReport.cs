using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using GSC.Data.Dto.Common;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Custom;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Custom;
using GSC.Data.Entities.Report;
using GSC.Helper;
using GSC.Report.Common;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using GSC.Respository.Volunteer;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;


namespace GSC.Report
{
    public class GscReport : IGscReport
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IReportBaseRepository _reportBaseRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IProjectDesignPeriodRepository _projectDesignPeriodRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        private readonly IScreeningEntryRepository _screeningEntryRepository;
        public GscReport(IReportBaseRepository reportBaseRepository, IJwtTokenAccesser jwtTokenAccesser,
            IProjectRepository projectRepository, IVolunteerRepository volunteerRepository,
             IProjectDesignPeriodRepository projectDesignPeriodRepository, IProjectDesignRepository projectDesignRepository,
              IUploadSettingRepository uploadSettingRepository, IUserRepository userRepository, IEmailSenderRespository emailSenderRespository,
              IScreeningEntryRepository screeningEntryRepository)
        {
            _reportBaseRepository = reportBaseRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRepository = projectRepository;
            _volunteerRepository = volunteerRepository;
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _projectDesignRepository = projectDesignRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _screeningEntryRepository = screeningEntryRepository;
        }

        

        
    }
}