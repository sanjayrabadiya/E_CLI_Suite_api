using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.Workflow;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Report;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class ReportController : BaseController
    {
        private readonly IGscReport _gscReport;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IProjectDesignReportSettingRepository _projectDesignReportSettingRepository;
        public ReportController(IProjectDesignReportSettingRepository projectDesignReportSettingRepository, IGscReport gscReport
            , IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser)
        {
            _uow = uow;
            _gscReport = gscReport;
            _projectDesignReportSettingRepository = projectDesignReportSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet]
        [Route("GetProjectDesign/{projectDesignId}/{periodId}/{visitId}/{templateId}/{annotation}")]
        [AllowAnonymous]
        public IActionResult GetProjectDesign(int projectDesignId, int periodId, int visitId, int templateId, bool annotation)
        {
            var abc = _gscReport.GetProjectDesign(projectDesignId);
            return abc;
        }

        //[HttpGet]
        //[Route("GetProjectDesignWithFliter/{projectDesignId}/{periodId}/{visitId}/{templateId}/{annotation}/{pdfType}/{pdfStatus}/{reportSetting}")]
        //[AllowAnonymous]
        //public IActionResult GetProjectDesignWithFliter(int projectDesignId, int[] periodId, int[] visitId, int[] templateId, bool annotation, int pdfType, int pdfStatus, [FromRoute]ReportSettingDto reportSetting)
        //{
        //    var abc = _gscReport.GetProjectDesignWithFliter(projectDesignId, periodId, visitId, templateId, null, annotation, pdfType, pdfStatus);
        //    return abc;
        //}

        [HttpPost]
        [Route("GetProjectDesignWithFliter")]

        public IActionResult GetProjectDesignWithFliter([FromBody]ReportSettingNew reportSetting)
        {
            var WorkFlowQuery = @"SELECT TOP 1 PWL.* FROM [ProjectWorkflow] PW 
                                    INNER JOIN [ProjectWorkflowLevel] PWL ON PW.Id = PWL.[ProjectWorkflowId] 
                                    WHERE PW.[ProjectDesignId] =" + reportSetting.ProjectId + " AND PWL.[SecurityRoleId] =" + _jwtTokenAccesser.RoleId;

            var WorkFlowData = _uow.FromSql<ProjectWorkflowLevel>(WorkFlowQuery).ToList();
            bool issig = WorkFlowData != null && WorkFlowData.Count > 0 ? WorkFlowData.FirstOrDefault().IsElectricSignature : false;
            var sqlquery = @"SELECT Company.Id AS Id, '" + issig + "' AS IsSignature, " +
                            "'" + _jwtTokenAccesser.UserName + "' AS Username ," +
                            "'" + reportSetting.IsCompanyLogo.ToString().ToLower() + "' AS IsComLogo," +
                            "'" + reportSetting.IsClientLogo.ToString().ToLower() + "' AS IsClientLogo," +
                            "Company.CompanyName,Company.Phone1,Company.Phone2,Location.Address, " +
                            "State.StateName,City.CityName,Country.CountryName," +
                            "CASE WHEN ISNULL(Company.Logo,'')<>'' THEN + UploadSetting.ImageUrl + Company.Logo END Logo," +
                            "CASE WHEN ISNULL(Client.Logo,'')<>'' THEN + UploadSetting.ImageUrl + Client.Logo  END ClientLogo " +
                            "FROM Company " +
                            "LEFT OUTER JOIN Location ON Location.Id = Company.LocationId " +
                            "LEFT OUTER JOIN State ON State.Id = Location.StateId " +
                            "LEFT OUTER JOIN City ON City.Id = Location.CityId " +
                            "LEFT OUTER JOIN Country ON Country.Id = Location.CountryId " +
                            "LEFT OUTER JOIN UploadSetting ON UploadSetting.CompanyId = Company.Id " +
                            "LEFT OUTER JOIN Client ON Client.CompanyId = Company.Id " +
                              "WHERE Company.Id=" + Convert.ToString(_jwtTokenAccesser.CompanyId == 0 ? 1 : _jwtTokenAccesser.CompanyId);

            var companyData = _uow.FromSql<CompanyData>(sqlquery).ToList();
            var result = _gscReport.GetProjectDesignWithFliter(reportSetting, companyData.FirstOrDefault());
            var reportSettingForm = _projectDesignReportSettingRepository.All.Where(x => x.ProjectDesignId == reportSetting.ProjectId && x.CompanyId == reportSetting.CompanyId && x.DeletedBy == null).FirstOrDefault();
            if (reportSettingForm == null)
            {
                ProjectDesignReportSetting objNew = new ProjectDesignReportSetting();

                objNew.ProjectDesignId = reportSetting.ProjectId;
                objNew.IsClientLogo = reportSetting.IsClientLogo;
                objNew.IsCompanyLogo = reportSetting.IsCompanyLogo;
                objNew.IsInitial = reportSetting.IsInitial;
                objNew.IsScreenNumber = reportSetting.IsScreenNumber;
                objNew.IsSponsorNumber = reportSetting.IsSponsorNumber;
                objNew.IsSubjectNumber = reportSetting.IsSubjectNumber;
                objNew.LeftMargin = reportSetting.LeftMargin;
                objNew.RightMargin = reportSetting.RightMargin;
                objNew.TopMargin = reportSetting.TopMargin;
                objNew.BottomMargin = reportSetting.BottomMargin;
                _projectDesignReportSettingRepository.Add(objNew);
            }
            else
            {
                reportSettingForm.ProjectDesignId = reportSetting.ProjectId;
                reportSettingForm.IsClientLogo = reportSetting.IsClientLogo;
                reportSettingForm.IsCompanyLogo = reportSetting.IsCompanyLogo;
                reportSettingForm.IsInitial = reportSetting.IsInitial;
                reportSettingForm.IsScreenNumber = reportSetting.IsScreenNumber;
                reportSettingForm.IsSponsorNumber = reportSetting.IsSponsorNumber;
                reportSettingForm.IsSubjectNumber = reportSetting.IsSubjectNumber;
                reportSettingForm.LeftMargin = reportSetting.LeftMargin;
                reportSettingForm.RightMargin = reportSetting.RightMargin;
                reportSettingForm.TopMargin = reportSetting.TopMargin;
                reportSettingForm.BottomMargin = reportSetting.BottomMargin;

                _projectDesignReportSettingRepository.Update(reportSettingForm);
            }
            if (_uow.Save() <= 0)
            {
                throw new Exception($"Creating Report Setting failed on save.");
            }

            return result;
        }
    }
}