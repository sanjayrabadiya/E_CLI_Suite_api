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
        private readonly IProjectRepository _projectRepository;
        private readonly IGSCContext _context;
        public AppScreenPatientRightsRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IProjectRepository projectRepository) : base(context)
        {
            _projectRepository = projectRepository;
            _context = context;
        }

        public List<AppScreenPatientRightsGridDto> GetAppScreenPatientList(int projectid)
        {
            var patientrights = FindByInclude(x => x.ProjectId == projectid && x.DeletedDate == null, x => x.AppScreenPatient).ToList();
            if (patientrights.Count > 0)
            {
                List<AppScreenPatientRightsGridDto> appScreenPatientRightsGridDtos = new List<AppScreenPatientRightsGridDto>();
                AppScreenPatientRightsGridDto appScreenPatientRightsGridDto = new AppScreenPatientRightsGridDto();
                appScreenPatientRightsGridDto.ProjectId = projectid;
                appScreenPatientRightsGridDto.StudyName = _projectRepository.Find(projectid).ProjectName;
                StringBuilder patientModules = new StringBuilder();
                for (int i = 0; i < patientrights.Count; i++)
                {
                    patientModules.Append(patientrights[i].AppScreenPatient.ScreenName);
                    patientModules.Append(" , ");
                }
                appScreenPatientRightsGridDto.PatientModules = patientModules.ToString().Substring(0, patientModules.Length - 3);
                appScreenPatientRightsGridDtos.Add(appScreenPatientRightsGridDto);
                return appScreenPatientRightsGridDtos;
            }
            else
            {
                return new List<AppScreenPatientRightsGridDto>();
            }
        }

        public List<AppScreenPatientRightsDto> GetAppScreenPatientModules(int projectid)
        {
            var data = (from a in _context.AppScreenPatient
                        join b in _context.AppScreenPatientRights.Where(x => x.ProjectId == projectid) on a.Id equals b.AppScreenPatientId into ps
                        from p in ps.DefaultIfEmpty()
                        select new AppScreenPatientRightsDto
                        {
                            ProjectId = projectid,
                            AppScreenPatientId = a.Id,
                            IsChecked = (p == null) ? false : true,
                            AppScreenPatientScreenName = a.ScreenName,
                        }).ToList();

            return data;
        }
    }
}
