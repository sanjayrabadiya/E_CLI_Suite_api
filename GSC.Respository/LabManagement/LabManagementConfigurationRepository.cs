using AutoMapper;
using AutoMapper.QueryableExtensions;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.LabManagement;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace GSC.Respository.LabManagement
{
    public class LabManagementConfigurationRepository : GenericRespository<LabManagementConfiguration>, ILabManagementConfigurationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectRightRepository _projectRightRepository;

        public LabManagementConfigurationRepository(IGSCContext context,
             IUploadSettingRepository uploadSettingRepository,
             IProjectRightRepository projectRightRepository,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uploadSettingRepository = uploadSettingRepository;
            _projectRightRepository = projectRightRepository;
            _mapper = mapper;
            _context = context;
        }

        public List<LabManagementConfigurationGridDto> GetConfigurationList(bool isDeleted)
        {
            var projectList = _projectRightRepository.GetParentProjectRightIdList();

            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && projectList.Any(c => c == x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId)).
                   ProjectTo<LabManagementConfigurationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(LabManagementConfiguration objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ProjectDesignTemplateId == objSave.ProjectDesignTemplateId && x.MimeType == objSave.MimeType && x.DeletedDate == null))
                return "You already configure this template, Please configure unique template mapping.";
            return "";
        }

        public T[] GetMappingData<T>(int LabManagementConfigurationId)
        {
            var Exists = _context.LabManagementVariableMapping.Where(x => x.LabManagementConfigurationId == LabManagementConfigurationId && x.DeletedDate == null).Count();
            if (Exists == 0)
            {
                var documentUrl = _uploadSettingRepository.GetDocumentPath();
                var projectDocuments = All.Where(x => x.Id == LabManagementConfigurationId).FirstOrDefault().PathName;

                string pathname = documentUrl + projectDocuments;
                FileStream streamer = new FileStream(pathname, FileMode.Open);
                IExcelDataReader reader = null;
                if (Path.GetExtension(pathname) == ".xls")
                    reader = ExcelReaderFactory.CreateBinaryReader(streamer);
                else
                    reader = ExcelReaderFactory.CreateOpenXmlReader(streamer);
                DataSet results = reader.AsDataSet();
                results.Tables[0].Rows[0].Delete();
                results.Tables[0].AcceptChanges();
                var data = results.Tables[0].AsEnumerable().Select(r => r.Field<string>("Column10").Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
                streamer.Dispose();
                return (T[])(object)data.ToArray();
            }
            else
            {
                var result = _context.LabManagementVariableMapping.Where(x => x.LabManagementConfigurationId == LabManagementConfigurationId && x.DeletedDate == null).
                    Select(x => new { ProjectDesignVariable = x.ProjectDesignVariable.VariableName, TargetVariable = x.TargetVariable }).ToList();
                return (T[])(object)result.ToArray();
            }
        }

        // Add by vipul for only bind that project which map in lab management configuration
        public List<ProjectDropDown> GetParentProjectDropDownForUploadLabData()
        {
            var projectList = _projectRightRepository.GetParentProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var result = All.Where(x => x.DeletedDate == null
                    && projectList.Any(c => c == x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId))
                .Select(c => new ProjectDropDown
                {
                    Id = c.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId,
                    Value = c.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ProjectCode,
                    Code = c.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ProjectCode,
                    IsStatic = c.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.IsStatic,
                    ParentProjectId = c.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ParentProjectId ?? c.Id,
                    IsDeleted = c.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.DeletedDate != null
                }).Distinct().OrderBy(o => o.Value).ToList();

            result = result.GroupBy(x => x.Id).Select(c => new ProjectDropDown
            {
                Id = c.Key,
                Value = c.FirstOrDefault().Value,
                Code = c.FirstOrDefault().Code,
                IsStatic = c.FirstOrDefault().IsStatic,
                ParentProjectId = c.FirstOrDefault().ParentProjectId,
                IsDeleted = c.FirstOrDefault().IsDeleted
            }).OrderBy(o => o.Value).ToList();

            return result;
        }

        // Add by vipul for only bind that visit which map in lab management configuration
        public IList<DropDownDto> GetVisitDropDownForUploadLabData(int projectDesignPeriodId)
        {
            var visits = All.Where(x => x.DeletedDate == null
                                        && x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId == projectDesignPeriodId).OrderBy(t => t.ProjectDesignTemplate.ProjectDesignVisit.DesignOrder).Select(
                t => new DropDownDto
                {
                    Id = t.ProjectDesignTemplate.ProjectDesignVisit.Id,
                    Value = t.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                    Code = t.ProjectDesignTemplate.ProjectDesignVisit.StudyVersion != null || t.ProjectDesignTemplate.ProjectDesignVisit.InActiveVersion != null ?
                    "( V : " + t.ProjectDesignTemplate.ProjectDesignVisit.StudyVersion + (t.ProjectDesignTemplate.ProjectDesignVisit.StudyVersion != null && t.ProjectDesignTemplate.ProjectDesignVisit.InActiveVersion != null ? " - " : "" + t.ProjectDesignTemplate.ProjectDesignVisit.InActiveVersion) + ")" : "",
                    ExtraData = t.ProjectDesignTemplate.ProjectDesignVisit.IsNonCRF,
                }).ToList();

            visits = visits.GroupBy(x => x.Id).Select(c => new DropDownDto
            {
                Id = c.Key,
                Value = c.FirstOrDefault().Value,
                Code = c.FirstOrDefault().Code,
                ExtraData = c.FirstOrDefault().ExtraData
            }).ToList();

            return visits;
        }

        // Add by vipul for only bind that template which map in lab management configuration
        public IList<DropDownDto> GetTemplateDropDownForUploadLabData(int projectDesignVisitId)
        {
            var templates = All.Where(x => x.DeletedDate == null
                                           && x.ProjectDesignTemplate.ProjectDesignVisitId == projectDesignVisitId).OrderBy(t => t.ProjectDesignTemplate.DesignOrder).Select(
                t => new DropDownDto
                {
                    Id = t.ProjectDesignTemplate.Id,
                    Value = t.ProjectDesignTemplate.TemplateName
                }).ToList();

            return templates;
        }

        // Get Project design variable id by lab management configuration Id
        public int getProjectDesignVariableId(int LabManagementConfigurationId, string VariableName)
        {
            var ProjectDesignTemplateId = All.Where(x => x.Id == LabManagementConfigurationId && x.DeletedDate == null).FirstOrDefault().ProjectDesignTemplateId;
            var ProjectDesignVariable = _context.ProjectDesignVariable.Where(x => x.ProjectDesignTemplateId == ProjectDesignTemplateId
            && x.VariableName.Trim().ToLower() == VariableName.Trim().ToLower() && x.DeletedDate == null).ToList();

            if (ProjectDesignVariable.Count() == 0)
                return 0;
            else
                return ProjectDesignVariable.FirstOrDefault().Id;
        }

    }
}
