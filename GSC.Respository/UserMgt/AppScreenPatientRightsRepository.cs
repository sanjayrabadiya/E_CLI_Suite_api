using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Respository.Master;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.UserMgt
{
    public class AppScreenPatientRightsRepository : GenericRespository<AppScreenPatientRights>, IAppScreenPatientRightsRepository
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProjectRepository _projectRepository;
        public AppScreenPatientRightsRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IProjectRepository projectRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _projectRepository = projectRepository;
        }

        public List<AppScreenPatientRightsGridDto> GetAppScreenPatientList(int projectid)
        {
            var patientrights = FindByInclude(x => x.ProjectId == projectid && x.DeletedDate == null, x=> x.AppScreenPatient).ToList();
            List<AppScreenPatientRightsGridDto> appScreenPatientRightsGridDtos = new List<AppScreenPatientRightsGridDto>();
            AppScreenPatientRightsGridDto appScreenPatientRightsGridDto = new AppScreenPatientRightsGridDto();
            appScreenPatientRightsGridDto.ProjectId = projectid;
            appScreenPatientRightsGridDto.StudyName = _projectRepository.Find(projectid).ProjectName;
            string patientModules = "";
            for (int i = 0; i < patientrights.Count; i++)
            {
                patientModules = patientModules + patientrights[i].AppScreenPatient.ScreenName + " , ";
            }
            appScreenPatientRightsGridDto.PatientModules = patientModules;
            appScreenPatientRightsGridDtos.Add(appScreenPatientRightsGridDto);
            return appScreenPatientRightsGridDtos;
        }

        //public List<AppScreenPatientRightsDto> GetAppScreenPatientModules(int projectid)
        //{
            
        //}
    }
}
