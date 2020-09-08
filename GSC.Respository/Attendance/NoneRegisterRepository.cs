using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Entities.Attendance;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Attendance
{
    public class NoneRegisterRepository : GenericRespository<NoneRegister, GscContext>, INoneRegisterRepository
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;
        public NoneRegisterRepository(IUnitOfWork<GscContext> uow,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IAttendanceRepository attendanceRepository,
            IProjectDesignRepository projectDesignRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IStateRepository stateRepository,
            ICountryRepository countryRepository,
            ICityRepository cityRepository,
             IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _attendanceRepository = attendanceRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectDesignRepository = projectDesignRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _stateRepository = stateRepository;
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
            _mapper = mapper;
        }

        public void SaveNonRegister(NoneRegister noneRegister, NoneRegisterDto noneRegisterDto)
        {
            noneRegister.Id = 0;
            noneRegister.Initial = noneRegister.Initial.PadRight(3, '-');

            Add(noneRegister);
            var attendance = new Data.Entities.Attendance.Attendance();
            attendance.ProjectId = noneRegisterDto.ProjectId;
            attendance.ProjectDesignPeriodId = noneRegisterDto.ProjectDesignPeriodId;
            attendance.NoneRegister = noneRegister;
            attendance.AttendanceType = AttendanceType.NoneRegister;
            _attendanceRepository.SaveAttendance(attendance);
        }

        public List<NoneRegisterGridDto> GetNonRegisterList(int projectId, bool isDeleted)
        {
            var result = All.Where(x => x.ProjectId == projectId && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                   ProjectTo<NoneRegisterGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            return result;

        }

        public string Duplicate(NoneRegister objSave, int projectId)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.RandomizationNumber == objSave.RandomizationNumber &&
                x.Attendance.ProjectId == projectId && !string.IsNullOrEmpty(x.RandomizationNumber) &&
                x.DeletedDate == null)) return "Duplicate Randomization Number : " + objSave.RandomizationNumber;

            return "";
        }
    }
}