using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Custom;
using GSC.Data.Entities.Client;
using GSC.Data.Entities.Custom;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.Workflow;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.PropertyMapping;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignReportSettingRepository : GenericRespository<ProjectDesignReportSetting>, IProjectDesignReportSettingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public ProjectDesignReportSettingRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IMapper mapper,
               IUploadSettingRepository uploadSettingRepository
            ) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public List<CompanyDataDto> GetProjectDesignWithFliter(ReportSettingNew reportSetting)
        {

            var WorkFlowQuery = @"SELECT TOP 1 PWL.* FROM [ProjectWorkflow] PW 
                                    INNER JOIN [ProjectWorkflowLevel] PWL ON PW.Id = PWL.[ProjectWorkflowId] 
                                    WHERE PW.[ProjectDesignId] =" + reportSetting.ProjectId + " AND PWL.[SecurityRoleId] =" + _jwtTokenAccesser.RoleId + " ORDER BY PWL.Id DESC";

            var workflowData = _context.FromSql<ProjectWorkflowLevel>(WorkFlowQuery).ToList();
            bool isSig = workflowData.FirstOrDefault()?.IsElectricSignature ?? false;

            var clientlogo = (from projectdesign in _context.ProjectDesign.Where(t => t.Id == reportSetting.ProjectId)
                              join project in _context.Project on projectdesign.ProjectId equals project.Id
                              join client in _context.Client on project.ClientId equals client.Id
                              select new
                              {
                                  client.Logo
                              }).FirstOrDefault();

            string Clinetlogo = string.IsNullOrEmpty(clientlogo?.Logo) ? "" : _uploadSettingRepository.getWebImageUrl() + clientlogo.Logo;
            var query = @" SELECT company.Id AS Id, company.CompanyName AS CompanyName, company.Phone1 AS Phone1,
		                    company.Phone2 AS Phone2, location.Address AS Address,
                            '" + isSig + "'  AS IsSignature," +
                        " '" + Clinetlogo + "' AS ClientLogo," +
                        " state.StateName AS StateName, city.CityName AS CityName," +
                        " '" + _jwtTokenAccesser.UserName + "' AS Username," +
                        " country.CountryName AS CountryName, ISNULL((uploadSetting.ImageUrl + company.Logo),'') AS Logo," +
                        "'" + reportSetting.IsCompanyLogo.ToString().ToLower() + "' AS IsComLogo," +
                        "'" + reportSetting.IsClientLogo.ToString().ToLower() + "' AS IsClientLogo, " +
                        "'" + reportSetting.IsScreenNumber.ToString().ToLower() + "' AS IsScreenNumber, " +
                        "'" + reportSetting.IsSponsorNumber.ToString().ToLower() + "' AS IsSponsorNumber, " +
                        "'" + reportSetting.IsSubjectNumber.ToString().ToLower() + "' AS IsSubjectNumber, " +
                        "'" + reportSetting.IsSiteCode.ToString().ToLower() + "' AS IsSiteCode " +

                        " FROM Company company " +
                        " LEFT JOIN Location location ON location.Id = company.LocationId" +
                        " LEFT Join State state ON state.id = location.stateID" +
                        " LEFT JOIN City city ON city.Id = location.CityId" +
                        " LEFT JOIN country country ON country.Id = location.CountryId" +
                        " LEFT JOIN UploadSetting uploadsetting ON uploadsetting.CompanyId = company.Id" +
                        " LEFT JOIN Client client ON client.CompanyId = company.Id WHERE company.Id = " + _jwtTokenAccesser.CompanyId;
            var cData = _context.FromSql<CompanyData>(query).ToList();
            var cDataDto = _mapper.Map<List<CompanyDataDto>>(cData);      
            return cDataDto;

        }
    }
}
