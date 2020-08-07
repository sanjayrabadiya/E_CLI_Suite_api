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
using GSC.Respository.UserMgt;
using GSC.Respository.Volunteer;
using Microsoft.AspNetCore.Mvc;
using Telerik.Reporting;

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

        public GscReport(IReportBaseRepository reportBaseRepository, IJwtTokenAccesser jwtTokenAccesser,
            IProjectRepository projectRepository, IVolunteerRepository volunteerRepository,
             IProjectDesignPeriodRepository projectDesignPeriodRepository, IProjectDesignRepository projectDesignRepository,
              IUploadSettingRepository uploadSettingRepository, IUserRepository userRepository, IEmailSenderRespository emailSenderRespository)
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
        }

        public FileStreamResult GetProjectDesign(int id)
        {
            var parameter = new SqlDataSourceParameterCollection();
            parameter.Add(new SqlDataSourceParameter("@id", DbType.String, id));
            parameter.Add(new SqlDataSourceParameter("@CompanyId", DbType.String, _jwtTokenAccesser.CompanyId));
            var strSql =
                @"Select Project.Id AS ProjectId, Project.ProjectCode,Project.IsStatic, Project.ProjectName,ProjectDesignPeriod.Id AS ProjectDesignPeriodId,
                ProjectDesignPeriod.DisplayName AS PeriodName,ProjectDesignVisit.Id AS ProjectDesignVisitId,ProjectDesignVisit.DisplayName VisitName,
                ProjectDesignTemplate.Id AS ProjectDesignTemplateId, ProjectDesignTemplate.TemplateName,ProjectDesignTemplate.ActivityName,
                ProjectDesignTemplate.DesignOrder AS TemplateSeqNo, ProjectDesignTemplate.TemplateCode,
                ProjectDesignVariable.Id AS ProjectDesignVariableId, ProjectDesignVariable.VariableCode,ProjectDesignVariable.VariableName,
                ProjectDesignVariable.DesignOrder AS VariableSeq,ProjectDesignVariable.DataType,
                Unit.UnitName,
                CASE WHEN ISNULL(ProjectDesignVariable.Annotation,'')<>'' THEN +'['+ProjectDesignVariable.Annotation+']' END VariableAnnotation,
                CASE WHEN ISNULL(ProjectDesignVariable.UnitAnnotation,'')<>'' THEN +'['+ProjectDesignVariable.UnitAnnotation+']' END UnitAnnotation,
                ProjectDesignVariable.CollectionSource, 
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.DateFormat' AND CompanyId=@CompanyId) As DateFormat, 
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.TimeFormat' AND CompanyId=@CompanyId) HourFormat,ProjectDesignVariableValue.ValueName,ProjectDesignVariableValue.SeqNo AS ValueSeqNo,
                ProjectDesignVariableValue.ValueName,ProjectDesignVariableValue.SeqNo AS ValueSeqNo,
                REPLACE(ISNULL(STUFF((SELECT CHAR(10) + Note FROM VariableTemplateNote Where VariableTemplateNote.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId  FOR XML PATH ('')), 1, 1, ''
                ),''),'amp;','')  TemplateNote,VariableTemplateDetail.Note AS VariableNote,Domain.DomainName,
                CASE WHEN ISNULL(ProjectDesignVariable.CollectionAnnotation,'')<>'' THEN +'('+ProjectDesignVariable.CollectionAnnotation+')' END CollectionAnnotation,
                CASE WHEN ProjectDesignVariable.DataType=1 THEN '*Only numeric' 
                WHEN ProjectDesignVariable.DataType=4 THEN '*Numeric with 1 decimal'
                WHEN ProjectDesignVariable.DataType=5 THEN '*Numeric with 2 decimal'
                WHEN ProjectDesignVariable.DataType=6 THEN '*Numeric with 3 decimal'
                WHEN ProjectDesignVariable.DataType=7 THEN '*Numeric with 4 decimal'
                WHEN ProjectDesignVariable.DataType=8 THEN '*Numeric with 5 decimal' END  AS DataTypeName
                FROM ProjectDesignVariable
                INNER JOIN ProjectDesignTemplate ON ProjectDesignTemplate.Id = ProjectDesignVariable.ProjectDesignTemplateId AND ProjectDesignTemplate.DeletedDate IS NULL
                INNER JOIN ProjectDesignVisit ON ProjectDesignVisit.Id = ProjectDesignTemplate.ProjectDesignVisitId AND ProjectDesignVisit.DeletedDate IS NULL
                INNER JOIN ProjectDesignPeriod ON ProjectDesignPeriod.Id = ProjectDesignVisit.ProjectDesignPeriodId AND ProjectDesignPeriod.DeletedDate IS NULL
                INNER JOIN ProjectDesign ON ProjectDesign.Id = ProjectDesignPeriod.ProjectDesignId AND ProjectDesign.DeletedDate IS NULL
                INNER JOIN Project ON Project.Id = ProjectDesign.ProjectId AND Project.DeletedDate IS NULL
                LEFT JOIN Domain ON ProjectDesignTemplate.DomainId = Domain.Id 
                LEFT OUTER JOIN Unit ON Unit.Id = ProjectDesignVariable.UnitId AND Unit.DeletedDate IS NULL
                LEFT OUTER JOIN ProjectDesignVariableValue ON ProjectDesignVariableValue.ProjectDesignVariableId = ProjectDesignVariable.Id AND ProjectDesignVariable.DeletedDate IS NULL
                LEFT OUTER JOIN VariableTemplateDetail ON VariableTemplateDetail.VariableId = ProjectDesignVariable.VariableId AND VariableTemplateDetail.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId
                WHERE ProjectDesign.Id=@id AND ISNULL(ProjectDesignVariable.DeletedBy,0) = 0
                ORDER BY ProjectDesignPeriod.Id,ProjectDesignVisit.Id,ProjectDesignTemplate.DesignOrder,ProjectDesignVariable.DesignOrder,ProjectDesignVariableValue.SeqNo ";

            var sqlDataSource = _reportBaseRepository.DataSource(strSql, parameter);

            return _reportBaseRepository.ReportRun("ProjectDesign\\ProjectDesign", sqlDataSource);
        }

        public FileStreamResult GetProjectDesignWithFliter(ReportSettingNew reportSettingNew, CompanyDataDto companyData, JobMonitoring jobMonitoring)
        {
            string logStatus = string.Empty;
            string strSQL = string.Empty;
            var projectId = _projectDesignRepository.Find(reportSettingNew.ProjectId).ProjectId;
            var base_URL = _uploadSettingRepository.All.OrderByDescending(x => x.Id).FirstOrDefault().DocumentPath;
            reportSettingNew.TimezoneoffSet = reportSettingNew.TimezoneoffSet * (-1);
            FileSaveInfo fileInfo = new FileSaveInfo();
            fileInfo.Base_URL = base_URL;
            fileInfo.ModuleName = Enum.GetName(typeof(JobNameType), jobMonitoring.JobName);
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            string parentFolderName = string.Empty;
            FileStreamResult result = null;
            try
            {
                if (reportSettingNew.PdfStatus == 1)
                {
                    #region Blank PDF
                    logStatus = DateTime.Now + "===1. FileStart ===";
                    fileInfo.FolderType = Enum.GetName(typeof(DossierPdfStatus), jobMonitoring.JobDetails);
                    fileInfo.ParentFolderName = _projectRepository.Find(projectId).ProjectCode + "-" + _projectRepository.Find(projectId).ProjectName + "_" + DateTime.Now.Ticks;
                    fileInfo.FileName = fileInfo.ParentFolderName;
                    parentFolderName = fileInfo.ParentFolderName.Trim().Replace(" ", "");
                    fileInfo.ParentFolderName = parentFolderName;
                    SqlDataSourceParameterCollection parameter = new SqlDataSourceParameterCollection();
                    parameter.Add(new SqlDataSourceParameter("@id", DbType.String, reportSettingNew.ProjectId));
                    parameter.Add(new SqlDataSourceParameter("@CompanyId", DbType.String, _jwtTokenAccesser.CompanyId));
                    if (reportSettingNew.AnnotationType)
                        #region Empty + Annotation
                        strSQL = @"Select Project.Id AS ProjectId,Project.SiteName As SiteCode, Project.ProjectCode,Project.IsStatic, Project.ProjectNumber,Project.ProjectName,ProjectDesignPeriod.Id AS ProjectDesignPeriodId,
                CASE 
					WHEN  ProjectDesignVariable.IsNa = 1 THEN 'true'
					ELSE 'false'
				END AS ProjectDesignVariableISNA,
                CASE
                    WHEN Project.IsStatic = 'true'  OR Project.IsStatic = '1' THEN ''
		            ELSE  UPPER(ProjectDesignPeriod.DisplayName)
                END AS PeriodName,
                ProjectDesignVisit.Id AS ProjectDesignVisitId,ProjectDesignVisit.DisplayName VisitName,
                ProjectDesignTemplate.Id AS ProjectDesignTemplateId, ProjectDesignTemplate.TemplateName,ProjectDesignTemplate.ActivityName,
                ProjectDesignTemplate.DesignOrder AS TemplateSeqNo, ProjectDesignTemplate.TemplateCode,
                ProjectDesignVariable.Id AS ProjectDesignVariableId, ProjectDesignVariable.VariableCode,ProjectDesignVariable.VariableName,
                ProjectDesignVariable.DesignOrder AS VariableSeq,ProjectDesignVariable.DataType,
                Unit.UnitName,
                CASE WHEN ISNULL(ProjectDesignVariable.Annotation,'')<>'' THEN +'['+ProjectDesignVariable.Annotation+']' END VariableAnnotation,
                CASE WHEN ISNULL(ProjectDesignVariable.UnitAnnotation,'')<>'' THEN +'['+ProjectDesignVariable.UnitAnnotation+']' END UnitAnnotation,
                ProjectDesignVariable.CollectionSource, 
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.DateFormat' AND CompanyId=@CompanyId) As DateFormat, 
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.TimeFormat' AND CompanyId=@CompanyId) HourFormat,ProjectDesignVariableValue.ValueName,ProjectDesignVariableValue.SeqNo AS ValueSeqNo,
                ProjectDesignVariableValue.ValueName,ProjectDesignVariableValue.SeqNo AS ValueSeqNo,
                REPLACE(ISNULL(STUFF((SELECT CHAR(10) + Note FROM VariableTemplateNote Where VariableTemplateNote.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId  FOR XML PATH ('')), 1, 1, ''
                ),''),'amp;','')  TemplateNote,VariableTemplateDetail.Note AS VariableNote,Domain.DomainName,
                CASE WHEN ISNULL(ProjectDesignVariable.CollectionAnnotation,'')<>'' THEN +'('+ProjectDesignVariable.CollectionAnnotation+')' END CollectionAnnotation,
                CASE WHEN ProjectDesignVariable.DataType=1 THEN '*Only numeric' 
                WHEN ProjectDesignVariable.DataType=4 THEN '*Numeric with 1 decimal'
                WHEN ProjectDesignVariable.DataType=5 THEN '*Numeric with 2 decimal'
                WHEN ProjectDesignVariable.DataType=6 THEN '*Numeric with 3 decimal'
                WHEN ProjectDesignVariable.DataType=7 THEN '*Numeric with 4 decimal'
                WHEN ProjectDesignVariable.DataType=8 THEN '*Numeric with 5 decimal' END  AS DataTypeName
                FROM ProjectDesignVariable
                INNER JOIN ProjectDesignTemplate ON ProjectDesignTemplate.Id = ProjectDesignVariable.ProjectDesignTemplateId AND ProjectDesignTemplate.DeletedDate IS NULL
                INNER JOIN ProjectDesignVisit ON ProjectDesignVisit.Id = ProjectDesignTemplate.ProjectDesignVisitId AND ProjectDesignVisit.DeletedDate IS NULL
                INNER JOIN ProjectDesignPeriod ON ProjectDesignPeriod.Id = ProjectDesignVisit.ProjectDesignPeriodId AND ProjectDesignPeriod.DeletedDate IS NULL
                INNER JOIN ProjectDesign ON ProjectDesign.Id = ProjectDesignPeriod.ProjectDesignId AND ProjectDesign.DeletedDate IS NULL
                INNER JOIN Project ON Project.Id = ProjectDesign.ProjectId AND Project.DeletedDate IS NULL
                LEFT JOIN Domain ON ProjectDesignTemplate.DomainId = Domain.Id 
                LEFT OUTER JOIN Unit ON Unit.Id = ProjectDesignVariable.UnitId AND Unit.DeletedDate IS NULL
                LEFT OUTER JOIN ProjectDesignVariableValue ON ProjectDesignVariableValue.ProjectDesignVariableId = ProjectDesignVariable.Id AND ProjectDesignVariable.DeletedDate IS NULL
                LEFT OUTER JOIN VariableTemplateDetail ON VariableTemplateDetail.VariableId = ProjectDesignVariable.VariableId AND VariableTemplateDetail.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId
                WHERE ProjectDesign.Id=@id AND ISNULL(ProjectDesignVariable.DeletedBy,0) = 0";
                    #endregion
                    else
                        #region Empty + Without Annotation
                        strSQL = @"Select Project.Id AS ProjectId, Project.SiteName As SiteCode, Project.ProjectCode,Project.IsStatic,Project.ProjectNumber, Project.ProjectName,ProjectDesignPeriod.Id AS ProjectDesignPeriodId,
                CASE 
					WHEN  ProjectDesignVariable.IsNa = 1 THEN 'true'
					ELSE 'false'
				END AS ProjectDesignVariableISNA,               
                CASE
                    WHEN Project.IsStatic = 'true'  OR Project.IsStatic = '1' THEN ''
		            ELSE  UPPER(ProjectDesignPeriod.DisplayName)
                END AS PeriodName,
                ProjectDesignVisit.Id AS ProjectDesignVisitId,ProjectDesignVisit.DisplayName VisitName,
                ProjectDesignTemplate.Id AS ProjectDesignTemplateId, ProjectDesignTemplate.TemplateName,ProjectDesignTemplate.ActivityName,
                ProjectDesignTemplate.DesignOrder AS TemplateSeqNo, ProjectDesignTemplate.TemplateCode,
                ProjectDesignVariable.Id AS ProjectDesignVariableId, ProjectDesignVariable.VariableCode,ProjectDesignVariable.VariableName,
                ProjectDesignVariable.DesignOrder AS VariableSeq,ProjectDesignVariable.DataType,
                Unit.UnitName,
                '' AS VariableAnnotation,
                '' AS UnitAnnotation,
                ProjectDesignVariable.CollectionSource, 
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.DateFormat' AND CompanyId=@CompanyId) As DateFormat, 
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.TimeFormat' AND CompanyId=@CompanyId) HourFormat,ProjectDesignVariableValue.ValueName,ProjectDesignVariableValue.SeqNo AS ValueSeqNo,
                ProjectDesignVariableValue.ValueName,ProjectDesignVariableValue.SeqNo AS ValueSeqNo,
                REPLACE(ISNULL(STUFF((SELECT CHAR(10) + Note FROM VariableTemplateNote Where VariableTemplateNote.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId  FOR XML PATH ('')), 1, 1, ''
                ),''),'amp;','')  TemplateNote,VariableTemplateDetail.Note AS VariableNote,Domain.DomainName,
                '' AS CollectionAnnotation,
                CASE WHEN ProjectDesignVariable.DataType=1 THEN '*Only numeric' 
                WHEN ProjectDesignVariable.DataType=4 THEN '*Numeric with 1 decimal'
                WHEN ProjectDesignVariable.DataType=5 THEN '*Numeric with 2 decimal'
                WHEN ProjectDesignVariable.DataType=6 THEN '*Numeric with 3 decimal'
                WHEN ProjectDesignVariable.DataType=7 THEN '*Numeric with 4 decimal'
                WHEN ProjectDesignVariable.DataType=8 THEN '*Numeric with 5 decimal' END  AS DataTypeName
                FROM ProjectDesignVariable
                INNER JOIN ProjectDesignTemplate ON ProjectDesignTemplate.Id = ProjectDesignVariable.ProjectDesignTemplateId AND ProjectDesignTemplate.DeletedDate IS NULL
                INNER JOIN ProjectDesignVisit ON ProjectDesignVisit.Id = ProjectDesignTemplate.ProjectDesignVisitId AND ProjectDesignVisit.DeletedDate IS NULL
                INNER JOIN ProjectDesignPeriod ON ProjectDesignPeriod.Id = ProjectDesignVisit.ProjectDesignPeriodId AND ProjectDesignPeriod.DeletedDate IS NULL
                INNER JOIN ProjectDesign ON ProjectDesign.Id = ProjectDesignPeriod.ProjectDesignId AND ProjectDesign.DeletedDate IS NULL
                INNER JOIN Project ON Project.Id = ProjectDesign.ProjectId AND Project.DeletedDate IS NULL
                LEFT JOIN Domain ON ProjectDesignTemplate.DomainId = Domain.Id 
                LEFT OUTER JOIN Unit ON Unit.Id = ProjectDesignVariable.UnitId AND Unit.DeletedDate IS NULL
                LEFT OUTER JOIN ProjectDesignVariableValue ON ProjectDesignVariableValue.ProjectDesignVariableId = ProjectDesignVariable.Id AND ProjectDesignVariable.DeletedDate IS NULL
                LEFT OUTER JOIN VariableTemplateDetail ON VariableTemplateDetail.VariableId = ProjectDesignVariable.VariableId AND VariableTemplateDetail.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId
                WHERE ProjectDesign.Id=@id AND ISNULL(ProjectDesignVariable.DeletedBy,0) = 0";
                    #endregion

                    strSQL += " ORDER BY ProjectDesignPeriod.Id,ProjectDesignVisit.Id,ProjectDesignTemplate.DesignOrder,ProjectDesignVariable.DesignOrder,ProjectDesignVariableValue.SeqNo ";
                    SqlDataSource sqlDataSource = _reportBaseRepository.DataSource(strSQL, parameter);
                    result = _reportBaseRepository.ReportRunNew("ProjectDesign\\ProjectDesignWithFilter", sqlDataSource, reportSettingNew, companyData, fileInfo);
                    #endregion
                    logStatus += DateTime.Now + "===1. FileEnd ===" + Environment.NewLine;
                }
                else
                {
                    var timediff = Convert.ToInt32((DateTime.Now - DateTime.Now.UtcDateTime()).TotalMinutes);
                    logStatus = DateTime.Now + "===1. FileStart ===" + Environment.NewLine;
                    parentFolderName = _projectRepository.Find(projectId).ProjectCode + "-" + _projectRepository.Find(projectId).ProjectName + "_" + DateTime.Now.Ticks;
                    parentFolderName = parentFolderName.Trim().Replace(" ", "");
                    if (reportSettingNew.SubjectIds != null)
                    {
                        var ProjectDetail = _projectRepository.Find(reportSettingNew.SiteId[0]);
                        #region FileSave

                        fileInfo.FolderType = Enum.GetName(typeof(DossierPdfStatus), jobMonitoring.JobDetails);
                        fileInfo.ParentFolderName = parentFolderName;
                        fileInfo.ChildFolderName = ProjectDetail.ProjectCode + "-" + ProjectDetail.ProjectName;
                        #endregion
                        foreach (var SubjectId in reportSettingNew.SubjectIds)
                        {
                            fileInfo.FileName = SubjectId.Value;
                            SqlDataSourceParameterCollection parameter = new SqlDataSourceParameterCollection();
                            parameter.Add(new SqlDataSourceParameter("@id", DbType.String, reportSettingNew.SiteId[0]));
                            parameter.Add(new SqlDataSourceParameter("@CompanyId", DbType.String, _jwtTokenAccesser.CompanyId));
                            parameter.Add(new SqlDataSourceParameter("@timezonediff", DbType.Int32, reportSettingNew.TimezoneoffSet));
                            if (reportSettingNew.AnnotationType)

                                #region Subject + Annotation
                                strSQL = @"Select  ScreeningTemplate.Id AS ScreeningTemplateId,Project.Id AS ProjectId,Project.SiteName As SiteCode, Project.ProjectCode,Project.IsStatic, Project.ProjectName,
                tempP.ProjectCode AS ParentProjectCode,Project.ProjectNumber,ProjectDesignPeriod.Id AS ProjectDesignPeriodId,
                
                CASE
                    WHEN Project.IsStatic = 'true'  OR Project.IsStatic = '1' THEN ''
		            ELSE  UPPER(ProjectDesignPeriod.DisplayName)
                END AS PeriodName,
                ProjectDesignVisit.Id AS ProjectDesignVisitId,
                CAST(ProjectDesignVisit.DisplayName AS VARCHAR(MAX)) + ISNULL('_' + CAST(ScreeningTemplate.RepeatedVisit AS VARCHAR(MAX)),'') AS  VisitName,
               CAST(CAST(ProjectDesignVisit.Id AS VARCHAR(MAX)) + ISNULL('.' + CAST(ScreeningTemplate.RepeatedVisit AS VARCHAR(MAX)),'') AS DECIMAL(18,2)) AS  VisitSeqNo,
                ProjectDesignTemplate.Id AS ProjectDesignTemplateId, ProjectDesignTemplate.TemplateName,ProjectDesignTemplate.ActivityName,
                CAST(ProjectDesignTemplate.DesignOrder AS VARCHAR(16)) + ISNULL('.' + CAST(ScreeningTemplate.RepeatSeqNo AS VARCHAR(16)),'')  AS TemplateSeqNo,
                CAST((CAST(ProjectDesignTemplate.DesignOrder AS VARCHAR(16)) + ISNULL('.' + CAST(ScreeningTemplate.RepeatSeqNo AS VARCHAR(16)),'')) AS DECIMAL(18,2)) AS TemplateSeqOrder,
                ProjectDesignTemplate.TemplateCode,
                ProjectDesignVariable.Id AS ProjectDesignVariableId,ProjectDesignVariableValue.Id AS ProjectDesignVariableValue, ProjectDesignVariable.VariableCode,ProjectDesignVariable.VariableName,
				CASE 
					WHEN  ScreeningTemplateValue.IsNa = 1 THEN 'true'
					ELSE 'false'
				END AS ScreeningTemplateValueIsNa,
				CASE 
					WHEN  ProjectDesignVariable.IsNa = 1 THEN 'true'
					ELSE 'false'
				END AS ProjectDesignVariableISNA,
                CASE
                WHEN ProjectDesignVariable.CollectionSource IN (1,10,11)  THEN ScreeningTemplateValue.Value
                WHEN ProjectDesignVariable.CollectionSource IN (7) THEN 
	                CASE 
		                WHEN (CAST(projectdesignvariablevalue.id AS VARCHAR) = ScreeningTemplateValue.value) THEN 'true'
		                ELSE  'false'
	                END
                 WHEN ProjectDesignVariable.CollectionSource = 2 THEN  
					CASE 
		                WHEN ScreeningTemplateValue.Value = '' OR ScreeningTemplateValue.Value IS NULL THEN ''
		                ELSE  projectdesignvariablevalue.ValueName
	                END
                WHEN ProjectDesignVariable.CollectionSource = 8 THEN  
	                CASE 
		                WHEN ScreeningTemplateValue.Value = '' OR ScreeningTemplateValue.Value IS NULL THEN 'false'
		                ELSE  'true'
	                END
                WHEN ProjectDesignVariable.CollectionSource = 9 THEN  
	                CASE 
		                WHEN ScreeningTemplateValuechild.Value = '' OR ScreeningTemplateValuechild.Value IS NULL   THEN 'false'
		                ELSE  ScreeningTemplateValuechild.Value
	                END
                ELSE NULL END as ScreenValue,
                CASE 
	                WHEN ProjectDesignVariable.CollectionSource = 2 and (cast(projectdesignvariablevalue.id AS VARCHAR) = ScreeningTemplateValue.value)THEN 0
		                ELSE  1
	                END AS aaa,
                CASE
	                WHEN ProjectDesignVariable.CollectionSource IN (3,4,5) THEN
	                CASE 
		                WHEN ScreeningTemplateValue.Value = ''  OR ScreeningTemplateValue.Value IS NULL THEN null
                            ELSE SWITCHOFFSET(CAST(ScreeningTemplateValue.Value AS DateTime), @timezonediff)
	                END 
                END AS ScreenValueDate,
                ProjectDesignVariableValue.ValueName,ProjectDesignVariable.DesignOrder AS VariableSeq,ProjectDesignVariable.DataType,
                Unit.UnitName,
                CASE WHEN ISNULL(ProjectDesignVariable.Annotation,'')<>'' THEN +'['+ProjectDesignVariable.Annotation+']' END VariableAnnotation,
                CASE WHEN ISNULL(ProjectDesignVariable.UnitAnnotation,'')<>'' THEN +'['+ProjectDesignVariable.UnitAnnotation+']' END UnitAnnotation,
                ProjectDesignVariable.CollectionSource,
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.DateFormat' AND CompanyId=@CompanyId) As DateFormat,
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.TimeFormat' AND CompanyId=@CompanyId) HourFormat ,ProjectDesignVariableValue.ValueName
                ,ProjectDesignVariableValue.SeqNo AS ValueSeqNo,
              
                REPLACE(ISNULL(STUFF((SELECT CHAR(10) + Note FROM VariableTemplateNote Where VariableTemplateNote.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId FOR XML PATH ('')), 1, 1, ''
                ),''),'amp;','') TemplateNote,VariableTemplateDetail.Note AS VariableNote,Domain.DomainName,
                CASE WHEN ISNULL(ProjectDesignVariable.CollectionAnnotation,'')<>'' THEN +'('+ProjectDesignVariable.CollectionAnnotation+')' END CollectionAnnotation,
                CASE WHEN ProjectDesignVariable.DataType=1 THEN '*Only numeric'
                WHEN ProjectDesignVariable.DataType=4 THEN '*Numeric with 1 decimal'
                WHEN ProjectDesignVariable.DataType=5 THEN '*Numeric with 2 decimal'
                WHEN ProjectDesignVariable.DataType=6 THEN '*Numeric with 3 decimal'
                WHEN ProjectDesignVariable.DataType=7 THEN '*Numeric with 4 decimal'
                WHEN ProjectDesignVariable.DataType=8 THEN '*Numeric with 5 decimal' END AS DataTypeName,
 	 
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.Initial ELSE  Volunteer.AliasName END AS Initial,
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.ScreeningNumber ELSE  Volunteer.VolunteerNo END AS SubjectNo,
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.RandomizationNumber ELSE  ProjectSubject.Number END AS RandomizationNumber,
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.Initial + '-'+ NoneRegister.ScreeningNumber +'-'+NoneRegister.RandomizationNumber
						  ELSE  (Volunteer.AliasName + '-' + Volunteer.VolunteerNo  + '-' + Volunteer.FirstName+ ' ' + Volunteer.LastName + ISNULL('-' +ProjectSubject.Number,'')) END AS SubjectFullName
                FROM ScreeningEntry SE
                INNER JOIN ProjectDesignPeriod ON ProjectDesignPeriod.Id = SE.ProjectDesignPeriodId AND ProjectDesignPeriod.DeletedDate IS NULL
                INNER JOIN ProjectDesignVisit ON ProjectDesignVisit.ProjectDesignPeriodId = ProjectDesignPeriod.Id AND ProjectDesignVisit.DeletedDate IS NULL
                INNER JOIN ProjectDesignTemplate ON ProjectDesignTemplate.ProjectDesignVisitId = ProjectDesignVisit.Id
                INNER JOIN ProjectDesignVariable ON ProjectDesignVariable.ProjectDesignTemplateId = ProjectDesignTemplate.Id AND ProjectDesignVariable.DeletedDate IS NULL
                INNER JOIN ProjectDesign ON ProjectDesign.Id = ProjectDesignPeriod.ProjectDesignId AND ProjectDesign.DeletedDate IS NULL
                INNER JOIN Project ON Project.Id = SE.ProjectId AND Project.DeletedDate IS NULL
                INNER JOIN Project tempP ON tempP.Id = ProjectDesign.ProjectId AND tempP.DeletedDate IS NULL    
                LEFT OUTER JOIN Attendance ON Attendance.Id = SE.AttendanceId AND Attendance.DeletedDate IS NULL
                LEFT OUTER JOIN Volunteer ON Volunteer.Id = Attendance.VolunteerId AND Volunteer.DeletedDate IS NULL
                LEFT OUTER JOIN NoneRegister ON Attendance.Id = NoneRegister.AttendanceId AND NoneRegister.DeletedDate IS NULL
                LEFT OUTER JOIN ProjectSubject ON ProjectSubject.Id = Attendance.ProjectSubjectId AND ProjectSubject.DeletedDate IS NULL
                LEFT JOIN Domain ON ProjectDesignTemplate.DomainId = Domain.Id
                LEFT OUTER JOIN Unit ON Unit.Id = ProjectDesignVariable.UnitId AND Unit.DeletedDate IS NULL
                LEFT OUTER JOIN ProjectDesignVariableValue ON ProjectDesignVariableValue.ProjectDesignVariableId = ProjectDesignVariable.Id AND ISNULL(ProjectDesignVariable.DeletedDate ,0) = 0
                INNER JOIN ScreeningTemplate ON ScreeningTemplate.ProjectDesignTemplateId = ProjectDesignTemplate.Id
													AND ScreeningTemplate.ScreeningEntryId = SE.Id                
                LEFT OUTER join ScreeningTemplateValue ON ScreeningTemplateValue.ScreeningTemplateId= ScreeningTemplate.Id AND projectdesignvariable.Id = ScreeningTemplateValue.ProjectDesignVariableId
                LEFT OUTER join ScreeningTemplateValuechild ON ScreeningTemplateValue.id = ScreeningTemplateValuechild.ScreeningTemplateValueId and projectdesignvariablevalue.id = ScreeningTemplateValuechild.[ProjectDesignVariableValueId]   
                LEFT OUTER JOIN VariableTemplateDetail ON VariableTemplateDetail.VariableId = ProjectDesignVariable.VariableId AND VariableTemplateDetail.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId
                WHERE  SE.ProjectId=@id AND ISNULL(ProjectDesignVariable.DeletedBy,0) = 0 AND ScreeningTemplate.[Status] > 1 ";
                            #endregion
                            else
                                #region Subject + Without Annotation
                                strSQL = @"Select ScreeningTemplate.Id AS ScreeningTemplateId, Project.Id AS ProjectId,Project.SiteName As SiteCode, Project.ProjectCode,Project.IsStatic, Project.ProjectName,
               tempP.ProjectCode AS ParentProjectCode,Project.ProjectNumber,ProjectDesignPeriod.Id AS ProjectDesignPeriodId,
                CASE
                    WHEN Project.IsStatic = 'true'  OR Project.IsStatic = '1' THEN ''
		            ELSE  UPPER(ProjectDesignPeriod.DisplayName)
                END AS PeriodName,
                ProjectDesignVisit.Id AS ProjectDesignVisitId,
                CAST(ProjectDesignVisit.DisplayName AS VARCHAR(MAX)) + ISNULL('_' + CAST(ScreeningTemplate.RepeatedVisit AS VARCHAR(MAX)),'') AS  VisitName,
                CAST(CAST(ProjectDesignVisit.Id AS VARCHAR(MAX)) + ISNULL('.' + CAST(ScreeningTemplate.RepeatedVisit AS VARCHAR(MAX)),'') AS DECIMAL(18,2)) AS  VisitSeqNo,
                ProjectDesignTemplate.Id AS ProjectDesignTemplateId, ProjectDesignTemplate.TemplateName,ProjectDesignTemplate.ActivityName,
                 CAST(ProjectDesignTemplate.DesignOrder AS VARCHAR(16)) + ISNULL('.' + CAST(ScreeningTemplate.RepeatSeqNo AS VARCHAR(16)),'')  AS TemplateSeqNo, 
                CAST((CAST(ProjectDesignTemplate.DesignOrder AS VARCHAR(16)) + ISNULL('.' + CAST(ScreeningTemplate.RepeatSeqNo AS VARCHAR(16)),'')) AS DECIMAL(18,2)) AS TemplateSeqOrder,
                ProjectDesignTemplate.TemplateCode,
                ProjectDesignVariable.Id AS ProjectDesignVariableId,ProjectDesignVariableValue.Id AS ProjectDesignVariableValue, ProjectDesignVariable.VariableCode,ProjectDesignVariable.VariableName,
				CASE 
					WHEN  ScreeningTemplateValue.IsNa = 1 THEN 'true'
					ELSE 'false'
				END AS ScreeningTemplateValueIsNa,
				CASE 
					WHEN  ProjectDesignVariable.IsNa = 1 THEN 'true'
					ELSE 'false'
				END AS ProjectDesignVariableISNA,
                CASE
                WHEN ProjectDesignVariable.CollectionSource IN (1,10,11)  THEN ScreeningTemplateValue.Value
                WHEN ProjectDesignVariable.CollectionSource IN (7) THEN 
	                CASE 
		                WHEN (CAST(projectdesignvariablevalue.id AS VARCHAR) = ScreeningTemplateValue.value) THEN 'true'
		                ELSE  'false'
	                END
                 WHEN ProjectDesignVariable.CollectionSource = 2 THEN  
					CASE 
		                WHEN ScreeningTemplateValue.Value = '' OR ScreeningTemplateValue.Value IS NULL THEN ''
		                ELSE  projectdesignvariablevalue.ValueName
	                END
                WHEN ProjectDesignVariable.CollectionSource = 8 THEN  
	                CASE 
		                WHEN ScreeningTemplateValue.Value = '' OR ScreeningTemplateValue.Value IS NULL THEN 'false'
		                ELSE  'true'
	                END
                WHEN ProjectDesignVariable.CollectionSource = 9 THEN  
	                CASE 
		                WHEN ScreeningTemplateValuechild.Value = '' OR ScreeningTemplateValuechild.Value IS NULL   THEN 'false'
		                ELSE  ScreeningTemplateValuechild.Value
	                END
                ELSE NULL END as ScreenValue,
                CASE 
	                WHEN ProjectDesignVariable.CollectionSource = 2 and (cast(projectdesignvariablevalue.id AS VARCHAR) = ScreeningTemplateValue.value)THEN 0
		                ELSE  1
	                END AS aaa,
                CASE
	                WHEN ProjectDesignVariable.CollectionSource IN (3,4,5) THEN
	                CASE 
		                WHEN ScreeningTemplateValue.Value = ''  OR ScreeningTemplateValue.Value IS NULL THEN null
		                ELSE SWITCHOFFSET(CAST(ScreeningTemplateValue.Value AS DateTime), @timezonediff)
	                END 
                END AS ScreenValueDate,
                ProjectDesignVariableValue.ValueName,ProjectDesignVariable.DesignOrder AS VariableSeq,ProjectDesignVariable.DataType,
                Unit.UnitName,
                '' AS VariableAnnotation,
                ''  AS UnitAnnotation,
                ProjectDesignVariable.CollectionSource,
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.DateFormat' AND CompanyId=@CompanyId) As DateFormat,
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.TimeFormat' AND CompanyId=@CompanyId) HourFormat 
                ,ProjectDesignVariableValue.ValueName
                ,ProjectDesignVariableValue.SeqNo AS ValueSeqNo,
                REPLACE(ISNULL(STUFF((SELECT CHAR(10) + Note FROM VariableTemplateNote Where VariableTemplateNote.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId FOR XML PATH ('')), 1, 1, ''
                ),''),'amp;','') TemplateNote,VariableTemplateDetail.Note AS VariableNote,Domain.DomainName,
                ''  AS CollectionAnnotation,
                CASE WHEN ProjectDesignVariable.DataType=1 THEN '*Only numeric'
                WHEN ProjectDesignVariable.DataType=4 THEN '*Numeric with 1 decimal'
                WHEN ProjectDesignVariable.DataType=5 THEN '*Numeric with 2 decimal'
                WHEN ProjectDesignVariable.DataType=6 THEN '*Numeric with 3 decimal'
                WHEN ProjectDesignVariable.DataType=7 THEN '*Numeric with 4 decimal'
                WHEN ProjectDesignVariable.DataType=8 THEN '*Numeric with 5 decimal' END AS DataTypeName,
 	 
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.Initial ELSE  Volunteer.AliasName END AS Initial,
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.ScreeningNumber ELSE  Volunteer.VolunteerNo END AS SubjectNo,
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.RandomizationNumber ELSE  ProjectSubject.Number END AS RandomizationNumber,
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.Initial + '-'+ NoneRegister.ScreeningNumber +'-'+NoneRegister.RandomizationNumber
						  ELSE  (Volunteer.AliasName + '-' + Volunteer.VolunteerNo  + '-' + Volunteer.FirstName+ ' ' + Volunteer.LastName + ISNULL('-' +ProjectSubject.Number,'')) END AS SubjectFullName
                FROM ScreeningEntry SE
                INNER JOIN ProjectDesignPeriod ON ProjectDesignPeriod.Id = SE.ProjectDesignPeriodId AND ProjectDesignPeriod.DeletedDate IS NULL
                INNER JOIN ProjectDesignVisit ON ProjectDesignVisit.ProjectDesignPeriodId = ProjectDesignPeriod.Id AND ProjectDesignVisit.DeletedDate IS NULL
                INNER JOIN ProjectDesignTemplate ON ProjectDesignTemplate.ProjectDesignVisitId = ProjectDesignVisit.Id 
                INNER JOIN ProjectDesignVariable ON ProjectDesignVariable.ProjectDesignTemplateId = ProjectDesignTemplate.Id AND ProjectDesignVariable.DeletedDate IS NULL
                INNER JOIN ProjectDesign ON ProjectDesign.Id = ProjectDesignPeriod.ProjectDesignId AND ProjectDesign.DeletedDate IS NULL
                INNER JOIN Project ON Project.Id = SE.ProjectId AND Project.DeletedDate IS NULL
                INNER JOIN Project tempP ON tempP.Id = ProjectDesign.ProjectId AND tempP.DeletedDate IS NULL
                LEFT OUTER JOIN Attendance ON Attendance.Id = SE.AttendanceId AND Attendance.DeletedDate IS NULL
                LEFT OUTER JOIN Volunteer ON Volunteer.Id = Attendance.VolunteerId AND Volunteer.DeletedDate IS NULL
                LEFT OUTER JOIN NoneRegister ON Attendance.Id = NoneRegister.AttendanceId AND NoneRegister.DeletedDate IS NULL
                LEFT OUTER JOIN ProjectSubject ON ProjectSubject.Id = Attendance.ProjectSubjectId AND ProjectSubject.DeletedDate IS NULL
                LEFT JOIN Domain ON ProjectDesignTemplate.DomainId = Domain.Id
                LEFT OUTER JOIN Unit ON Unit.Id = ProjectDesignVariable.UnitId AND Unit.DeletedDate IS NULL
                LEFT OUTER JOIN ProjectDesignVariableValue ON ProjectDesignVariableValue.ProjectDesignVariableId = ProjectDesignVariable.Id AND ISNULL(ProjectDesignVariable.DeletedDate ,0) = 0
              -- LEFT OUTER join ScreeningTemplateValue ON projectdesignvariable.Id = ScreeningTemplateValue.ProjectDesignVariableId
                INNER JOIN ScreeningTemplate ON ScreeningTemplate.ProjectDesignTemplateId = ProjectDesignTemplate.Id
													AND ScreeningTemplate.ScreeningEntryId = SE.Id                
                LEFT OUTER join ScreeningTemplateValue ON ScreeningTemplateValue.ScreeningTemplateId= ScreeningTemplate.Id AND projectdesignvariable.Id = ScreeningTemplateValue.ProjectDesignVariableId
                LEFT OUTER join ScreeningTemplateValuechild ON ScreeningTemplateValue.id = ScreeningTemplateValuechild.ScreeningTemplateValueId and projectdesignvariablevalue.id = ScreeningTemplateValuechild.[ProjectDesignVariableValueId]   
                LEFT OUTER JOIN VariableTemplateDetail ON VariableTemplateDetail.VariableId = ProjectDesignVariable.VariableId AND VariableTemplateDetail.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId
                WHERE  SE.ProjectId=@id AND ISNULL(ProjectDesignVariable.DeletedBy,0) = 0 AND ScreeningTemplate.[Status] > 1";
                            #endregion

                            strSQL += "AND SE.Id IN (" + string.Join(",", SubjectId.Id) + ") ";

                            strSQL += "ORDER BY SubjectNo,ProjectDesignVariable.Id,aaa,ProjectDesignPeriod.Id,ProjectDesignVisit.Id,ProjectDesignTemplate.DesignOrder,ProjectDesignVariable.DesignOrder,ProjectDesignVariableValue.SeqNo ";
                            SqlDataSource sqlDataSource = _reportBaseRepository.DataSource(strSQL, parameter);
                            result = _reportBaseRepository.ReportRunNew("ProjectDesign\\ProjectDesignWithData", sqlDataSource, reportSettingNew, companyData, fileInfo);
                        }
                    }
                    else
                    {
                        #region If select only Parent Project
                        foreach (var ChildProjectId in reportSettingNew.SiteId)
                        {
                            var ProjectDetail = _projectRepository.Find(ChildProjectId);

                            #region FileSave
                            fileInfo.FolderType = Enum.GetName(typeof(DossierPdfStatus), jobMonitoring.JobDetails);
                            fileInfo.ParentFolderName = parentFolderName;
                            fileInfo.ChildFolderName = ProjectDetail.ProjectCode + "-" + ProjectDetail.ProjectName;
                            #endregion

                            #region Get All subject from ProjectId

                            var isStatic = _projectRepository.Find(projectId).IsStatic;
                            var volunteers = new List<DropDownDto>();
                            if (isStatic)
                            {
                                var projectDesignPeriod = _projectDesignPeriodRepository.FindByInclude(x => x.ProjectDesignId == reportSettingNew.ProjectId).FirstOrDefault();
                                var attendance = _volunteerRepository.GetVolunteersForDataEntryByPeriodId(projectDesignPeriod.Id, ChildProjectId);
                                volunteers.AddRange(attendance);
                            }
                            else
                            {
                                var projectDesignPeriod = _projectDesignPeriodRepository.FindByInclude(x => x.ProjectDesignId == reportSettingNew.ProjectId).ToList();
                                projectDesignPeriod.ForEach(t =>
                                {
                                    var subject = _volunteerRepository.GetVolunteersForDataEntryByPeriodId(t.Id, ChildProjectId);
                                    volunteers.AddRange(subject);
                                });
                            }
                            #endregion

                            foreach (var SubjectId in volunteers)
                            {
                                fileInfo.FileName = SubjectId.Value;
                                SqlDataSourceParameterCollection parameter = new SqlDataSourceParameterCollection();
                                parameter.Add(new SqlDataSourceParameter("@id", DbType.String, ChildProjectId));
                                parameter.Add(new SqlDataSourceParameter("@CompanyId", DbType.String, _jwtTokenAccesser.CompanyId));
                                parameter.Add(new SqlDataSourceParameter("@timezonediff", DbType.Int32, reportSettingNew.TimezoneoffSet));
                                if (reportSettingNew.AnnotationType)

                                    #region Subject + Annotation
                                    strSQL = @"Select ScreeningTemplate.Id AS ScreeningTemplateId,Project.Id AS ProjectId,Project.SiteName As SiteCode, Project.ProjectCode,Project.IsStatic, Project.ProjectName,
                tempP.ProjectCode AS ParentProjectCode,Project.ProjectNumber,ProjectDesignPeriod.Id AS ProjectDesignPeriodId,
                CASE
                    WHEN Project.IsStatic = 'true'  OR Project.IsStatic = '1' THEN ''
		            ELSE  UPPER(ProjectDesignPeriod.DisplayName)
                END AS PeriodName,
                ProjectDesignVisit.Id AS ProjectDesignVisitId,
                CAST(ProjectDesignVisit.DisplayName AS VARCHAR(MAX)) + ISNULL('_' + CAST(ScreeningTemplate.RepeatedVisit AS VARCHAR(MAX)),'') AS  VisitName,
                CAST(CAST(ProjectDesignVisit.Id AS VARCHAR(MAX)) + ISNULL('.' + CAST(ScreeningTemplate.RepeatedVisit AS VARCHAR(MAX)),'') AS DECIMAL(18,2)) AS  VisitSeqNo,
                ProjectDesignTemplate.Id AS ProjectDesignTemplateId, ProjectDesignTemplate.TemplateName,ProjectDesignTemplate.ActivityName,
                 CAST(ProjectDesignTemplate.DesignOrder AS VARCHAR(16)) + ISNULL('.' + CAST(ScreeningTemplate.RepeatSeqNo AS VARCHAR(16)),'')  AS TemplateSeqNo,
                CAST((CAST(ProjectDesignTemplate.DesignOrder AS VARCHAR(16)) + ISNULL('.' + CAST(ScreeningTemplate.RepeatSeqNo AS VARCHAR(16)),'')) AS DECIMAL(18,2)) AS TemplateSeqOrder,
                ProjectDesignTemplate.TemplateCode,
                ProjectDesignVariable.Id AS ProjectDesignVariableId,ProjectDesignVariableValue.Id AS ProjectDesignVariableValue, ProjectDesignVariable.VariableCode,ProjectDesignVariable.VariableName,
				CASE 
					WHEN  ScreeningTemplateValue.IsNa = 1 THEN 'true'
					ELSE 'false'
				END AS ScreeningTemplateValueIsNa,
				CASE 
					WHEN  ProjectDesignVariable.IsNa = 1 THEN 'true'
					ELSE 'false'
				END AS ProjectDesignVariableISNA,
                CASE
                WHEN ProjectDesignVariable.CollectionSource IN (1,10,11)  THEN ScreeningTemplateValue.Value
                WHEN ProjectDesignVariable.CollectionSource IN (7) THEN 
	                CASE 
		                WHEN (CAST(projectdesignvariablevalue.id AS VARCHAR) = ScreeningTemplateValue.value) THEN 'true'
		                ELSE  'false'
	                END
               WHEN ProjectDesignVariable.CollectionSource = 2 THEN  
					CASE 
		                WHEN ScreeningTemplateValue.Value = '' OR ScreeningTemplateValue.Value IS NULL THEN ''
		                ELSE  projectdesignvariablevalue.ValueName
	                END
                WHEN ProjectDesignVariable.CollectionSource = 8 THEN  
	                CASE 
		                WHEN ScreeningTemplateValue.Value = '' OR ScreeningTemplateValue.Value IS NULL THEN 'false'
		                ELSE  'true'
	                END
                WHEN ProjectDesignVariable.CollectionSource = 9 THEN  
	                CASE 
		                WHEN ScreeningTemplateValuechild.Value = '' OR ScreeningTemplateValuechild.Value IS NULL   THEN 'false'
		                ELSE  ScreeningTemplateValuechild.Value
	                END
                ELSE NULL END as ScreenValue,
                CASE 
	                WHEN ProjectDesignVariable.CollectionSource = 2 and (cast(projectdesignvariablevalue.id AS VARCHAR) = ScreeningTemplateValue.value)THEN 0
		                ELSE  1
	                END AS aaa,
                CASE
	                WHEN ProjectDesignVariable.CollectionSource IN (3,4,5) THEN
	                CASE 
		                WHEN ScreeningTemplateValue.Value = ''  OR ScreeningTemplateValue.Value IS NULL THEN null
		                    ELSE SWITCHOFFSET(CAST(ScreeningTemplateValue.Value AS DateTime), @timezonediff)
	                END 
                END AS ScreenValueDate,
                ProjectDesignVariableValue.ValueName,ProjectDesignVariable.DesignOrder AS VariableSeq,ProjectDesignVariable.DataType,
                Unit.UnitName,
                CASE WHEN ISNULL(ProjectDesignVariable.Annotation,'')<>'' THEN +'['+ProjectDesignVariable.Annotation+']' END VariableAnnotation,
                CASE WHEN ISNULL(ProjectDesignVariable.UnitAnnotation,'')<>'' THEN +'['+ProjectDesignVariable.UnitAnnotation+']' END UnitAnnotation,
                ProjectDesignVariable.CollectionSource,
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.DateFormat' AND CompanyId=@CompanyId) As DateFormat,
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.TimeFormat' AND CompanyId=@CompanyId) HourFormat ,ProjectDesignVariableValue.ValueName
                ,ProjectDesignVariableValue.SeqNo AS ValueSeqNo,
              
                REPLACE(ISNULL(STUFF((SELECT CHAR(10) + Note FROM VariableTemplateNote Where VariableTemplateNote.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId FOR XML PATH ('')), 1, 1, ''
                ),''),'amp;','') TemplateNote,VariableTemplateDetail.Note AS VariableNote,Domain.DomainName,
                CASE WHEN ISNULL(ProjectDesignVariable.CollectionAnnotation,'')<>'' THEN +'('+ProjectDesignVariable.CollectionAnnotation+')' END CollectionAnnotation,
                CASE WHEN ProjectDesignVariable.DataType=1 THEN '*Only numeric'
                WHEN ProjectDesignVariable.DataType=4 THEN '*Numeric with 1 decimal'
                WHEN ProjectDesignVariable.DataType=5 THEN '*Numeric with 2 decimal'
                WHEN ProjectDesignVariable.DataType=6 THEN '*Numeric with 3 decimal'
                WHEN ProjectDesignVariable.DataType=7 THEN '*Numeric with 4 decimal'
                WHEN ProjectDesignVariable.DataType=8 THEN '*Numeric with 5 decimal' END AS DataTypeName,
 	 
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.Initial ELSE  Volunteer.AliasName END AS Initial,
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.ScreeningNumber ELSE  Volunteer.VolunteerNo END AS SubjectNo,
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.RandomizationNumber ELSE  ProjectSubject.Number END AS RandomizationNumber,
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.Initial + '-'+ NoneRegister.ScreeningNumber +'-'+NoneRegister.RandomizationNumber
						  ELSE  (Volunteer.AliasName + '-' + Volunteer.VolunteerNo  + '-' + Volunteer.FirstName+ ' ' + Volunteer.LastName + ISNULL('-' +ProjectSubject.Number,'')) END AS SubjectFullName
                FROM ScreeningEntry SE
                INNER JOIN ProjectDesignPeriod ON ProjectDesignPeriod.Id = SE.ProjectDesignPeriodId AND ProjectDesignPeriod.DeletedDate IS NULL
                INNER JOIN ProjectDesignVisit ON ProjectDesignVisit.ProjectDesignPeriodId = ProjectDesignPeriod.Id AND ProjectDesignVisit.DeletedDate IS NULL
                INNER JOIN ProjectDesignTemplate ON ProjectDesignTemplate.ProjectDesignVisitId = ProjectDesignVisit.Id 
                INNER JOIN ProjectDesignVariable ON ProjectDesignVariable.ProjectDesignTemplateId = ProjectDesignTemplate.Id AND ProjectDesignVariable.DeletedDate IS NULL
                INNER JOIN ProjectDesign ON ProjectDesign.Id = ProjectDesignPeriod.ProjectDesignId AND ProjectDesign.DeletedDate IS NULL
                INNER JOIN Project ON Project.Id = SE.ProjectId AND Project.DeletedDate IS NULL
                INNER JOIN Project tempP ON tempP.Id = ProjectDesign.ProjectId AND tempP.DeletedDate IS NULL
                LEFT OUTER JOIN Attendance ON Attendance.Id = SE.AttendanceId AND Attendance.DeletedDate IS NULL
                LEFT OUTER JOIN Volunteer ON Volunteer.Id = Attendance.VolunteerId AND Volunteer.DeletedDate IS NULL
                LEFT OUTER JOIN NoneRegister ON Attendance.Id = NoneRegister.AttendanceId AND NoneRegister.DeletedDate IS NULL
                LEFT OUTER JOIN ProjectSubject ON ProjectSubject.Id = Attendance.ProjectSubjectId AND ProjectSubject.DeletedDate IS NULL
                LEFT JOIN Domain ON ProjectDesignTemplate.DomainId = Domain.Id
                LEFT OUTER JOIN Unit ON Unit.Id = ProjectDesignVariable.UnitId AND Unit.DeletedDate IS NULL
                LEFT OUTER JOIN ProjectDesignVariableValue ON ProjectDesignVariableValue.ProjectDesignVariableId = ProjectDesignVariable.Id AND ISNULL(ProjectDesignVariable.DeletedDate ,0) = 0
                INNER JOIN ScreeningTemplate ON ScreeningTemplate.ProjectDesignTemplateId = ProjectDesignTemplate.Id
													AND ScreeningTemplate.ScreeningEntryId = SE.Id                
                LEFT OUTER join ScreeningTemplateValue ON ScreeningTemplateValue.ScreeningTemplateId= ScreeningTemplate.Id AND projectdesignvariable.Id = ScreeningTemplateValue.ProjectDesignVariableId
                LEFT OUTER join ScreeningTemplateValuechild ON ScreeningTemplateValue.id = ScreeningTemplateValuechild.ScreeningTemplateValueId and projectdesignvariablevalue.id = ScreeningTemplateValuechild.[ProjectDesignVariableValueId]   
                LEFT OUTER JOIN VariableTemplateDetail ON VariableTemplateDetail.VariableId = ProjectDesignVariable.VariableId AND VariableTemplateDetail.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId
                WHERE  SE.ProjectId=@id AND ISNULL(ProjectDesignVariable.DeletedBy,0) = 0 AND ScreeningTemplate.[Status] > 1 ";
                                #endregion
                                else
                                    #region Subject + Without Annotation
                                    strSQL = @"Select ScreeningTemplate.Id AS ScreeningTemplateId, Project.Id AS ProjectId,Project.SiteName As SiteCode, Project.ProjectCode,Project.IsStatic, Project.ProjectName,
                tempP.ProjectCode AS ParentProjectCode,Project.ProjectNumber,ProjectDesignPeriod.Id AS ProjectDesignPeriodId,
                CASE
                    WHEN Project.IsStatic = 'true'  OR Project.IsStatic = '1' THEN ''
		            ELSE  UPPER(ProjectDesignPeriod.DisplayName)
                END AS PeriodName,
                ProjectDesignVisit.Id AS ProjectDesignVisitId,
                CAST(ProjectDesignVisit.DisplayName AS VARCHAR(MAX)) + ISNULL('_' + CAST(ScreeningTemplate.RepeatedVisit AS VARCHAR(MAX)),'') AS  VisitName,
                CAST(CAST(ProjectDesignVisit.Id AS VARCHAR(MAX)) + ISNULL('.' + CAST(ScreeningTemplate.RepeatedVisit AS VARCHAR(MAX)),'') AS DECIMAL(18,2)) AS  VisitSeqNo,
                ProjectDesignTemplate.Id AS ProjectDesignTemplateId, ProjectDesignTemplate.TemplateName,ProjectDesignTemplate.ActivityName,
                CAST(ProjectDesignTemplate.DesignOrder AS VARCHAR(16)) + ISNULL('.' + CAST(ScreeningTemplate.RepeatSeqNo AS VARCHAR(16)),'')  AS TemplateSeqNo,
                CAST((CAST(ProjectDesignTemplate.DesignOrder AS VARCHAR(16)) + ISNULL('.' + CAST(ScreeningTemplate.RepeatSeqNo AS VARCHAR(16)),'')) AS DECIMAL(18,2)) AS TemplateSeqOrder,
                ProjectDesignTemplate.TemplateCode,
                ProjectDesignVariable.Id AS ProjectDesignVariableId,ProjectDesignVariableValue.Id AS ProjectDesignVariableValue, ProjectDesignVariable.VariableCode,ProjectDesignVariable.VariableName,
				CASE 
					WHEN  ScreeningTemplateValue.IsNa = 1 THEN 'true'
					ELSE 'false'
				END AS ScreeningTemplateValueIsNa,
				CASE 
					WHEN  ProjectDesignVariable.IsNa = 1 THEN 'true'
					ELSE 'false'
				END AS ProjectDesignVariableISNA,
                CASE
                WHEN ProjectDesignVariable.CollectionSource IN (1,10,11)  THEN ScreeningTemplateValue.Value
                WHEN ProjectDesignVariable.CollectionSource IN (7) THEN 
	                CASE 
		                WHEN (CAST(projectdesignvariablevalue.id AS VARCHAR) = ScreeningTemplateValue.value) THEN 'true'
		                ELSE  'false'
	                END
                WHEN ProjectDesignVariable.CollectionSource = 2 THEN  
					CASE 
		                WHEN ScreeningTemplateValue.Value = '' OR ScreeningTemplateValue.Value IS NULL THEN ''
		                ELSE  projectdesignvariablevalue.ValueName
	                END
                WHEN ProjectDesignVariable.CollectionSource = 8 THEN  
	                CASE 
		                WHEN ScreeningTemplateValue.Value = '' OR ScreeningTemplateValue.Value IS NULL THEN 'false'
		                ELSE  'true'
	                END
                WHEN ProjectDesignVariable.CollectionSource = 9 THEN  
	                CASE 
		                WHEN ScreeningTemplateValuechild.Value = '' OR ScreeningTemplateValuechild.Value IS NULL   THEN 'false'
		                ELSE  ScreeningTemplateValuechild.Value
	                END
                ELSE NULL END as ScreenValue,
                CASE 
	                WHEN ProjectDesignVariable.CollectionSource = 2 and (cast(projectdesignvariablevalue.id AS VARCHAR) = ScreeningTemplateValue.value)THEN 0
		                ELSE  1
	                END AS aaa,
                CASE
	                WHEN ProjectDesignVariable.CollectionSource IN (3,4,5) THEN
	                CASE 
		                WHEN ScreeningTemplateValue.Value = ''  OR ScreeningTemplateValue.Value IS NULL THEN null
		                    ELSE SWITCHOFFSET(CAST(ScreeningTemplateValue.Value AS DateTime), @timezonediff)
	                END 
                END AS ScreenValueDate,
                ProjectDesignVariableValue.ValueName,ProjectDesignVariable.DesignOrder AS VariableSeq,ProjectDesignVariable.DataType,
                Unit.UnitName,
                '' AS VariableAnnotation,
                ''  AS UnitAnnotation,
                ProjectDesignVariable.CollectionSource,
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.DateFormat' AND CompanyId=@CompanyId) As DateFormat,
                (Select Top 1 KeyValue from AppSetting Where KeyName='GeneralSettingsDto.TimeFormat' AND CompanyId=@CompanyId) HourFormat 
                ,ProjectDesignVariableValue.ValueName
                ,ProjectDesignVariableValue.SeqNo AS ValueSeqNo,
                REPLACE(ISNULL(STUFF((SELECT CHAR(10) + Note FROM VariableTemplateNote Where VariableTemplateNote.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId FOR XML PATH ('')), 1, 1, ''
                ),''),'amp;','') TemplateNote,VariableTemplateDetail.Note AS VariableNote,Domain.DomainName,
                ''  AS CollectionAnnotation,
                CASE WHEN ProjectDesignVariable.DataType=1 THEN '*Only numeric'
                WHEN ProjectDesignVariable.DataType=4 THEN '*Numeric with 1 decimal'
                WHEN ProjectDesignVariable.DataType=5 THEN '*Numeric with 2 decimal'
                WHEN ProjectDesignVariable.DataType=6 THEN '*Numeric with 3 decimal'
                WHEN ProjectDesignVariable.DataType=7 THEN '*Numeric with 4 decimal'
                WHEN ProjectDesignVariable.DataType=8 THEN '*Numeric with 5 decimal' END AS DataTypeName,
 	 
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.Initial ELSE  Volunteer.AliasName END AS Initial,
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.ScreeningNumber ELSE  Volunteer.VolunteerNo END AS SubjectNo,
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.RandomizationNumber ELSE  ProjectSubject.Number END AS RandomizationNumber,
                CASE WHEN Project.IsStatic = 1 THEN NoneRegister.Initial + '-'+ NoneRegister.ScreeningNumber +'-'+NoneRegister.RandomizationNumber
						  ELSE  (Volunteer.AliasName + '-' + Volunteer.VolunteerNo  + '-' + Volunteer.FirstName+ ' ' + Volunteer.LastName + ISNULL('-' +ProjectSubject.Number,'')) END AS SubjectFullName
                FROM ScreeningEntry SE
                INNER JOIN ProjectDesignPeriod ON ProjectDesignPeriod.Id = SE.ProjectDesignPeriodId AND ProjectDesignPeriod.DeletedDate IS NULL
                INNER JOIN ProjectDesignVisit ON ProjectDesignVisit.ProjectDesignPeriodId = ProjectDesignPeriod.Id AND ProjectDesignVisit.DeletedDate IS NULL
                INNER JOIN ProjectDesignTemplate ON ProjectDesignTemplate.ProjectDesignVisitId = ProjectDesignVisit.Id
                INNER JOIN ProjectDesignVariable ON ProjectDesignVariable.ProjectDesignTemplateId = ProjectDesignTemplate.Id AND ProjectDesignVariable.DeletedDate IS NULL
                INNER JOIN ProjectDesign ON ProjectDesign.Id = ProjectDesignPeriod.ProjectDesignId AND ProjectDesign.DeletedDate IS NULL
                INNER JOIN Project ON Project.Id = SE.ProjectId AND Project.DeletedDate IS NULL
                INNER JOIN Project tempP ON tempP.Id = ProjectDesign.ProjectId AND tempP.DeletedDate IS NULL
                LEFT OUTER JOIN Attendance ON Attendance.Id = SE.AttendanceId AND Attendance.DeletedDate IS NULL
                LEFT OUTER JOIN Volunteer ON Volunteer.Id = Attendance.VolunteerId AND Volunteer.DeletedDate IS NULL
                LEFT OUTER JOIN NoneRegister ON Attendance.Id = NoneRegister.AttendanceId AND NoneRegister.DeletedDate IS NULL
                LEFT OUTER JOIN ProjectSubject ON ProjectSubject.Id = Attendance.ProjectSubjectId AND ProjectSubject.DeletedDate IS NULL
                LEFT JOIN Domain ON ProjectDesignTemplate.DomainId = Domain.Id
                LEFT OUTER JOIN Unit ON Unit.Id = ProjectDesignVariable.UnitId AND Unit.DeletedDate IS NULL
                LEFT OUTER JOIN ProjectDesignVariableValue ON ProjectDesignVariableValue.ProjectDesignVariableId = ProjectDesignVariable.Id AND ISNULL(ProjectDesignVariable.DeletedDate ,0) = 0
              -- LEFT OUTER join ScreeningTemplateValue ON projectdesignvariable.Id = ScreeningTemplateValue.ProjectDesignVariableId
                INNER JOIN ScreeningTemplate ON ScreeningTemplate.ProjectDesignTemplateId = ProjectDesignTemplate.Id
													AND ScreeningTemplate.ScreeningEntryId = SE.Id                
                LEFT OUTER join ScreeningTemplateValue ON ScreeningTemplateValue.ScreeningTemplateId= ScreeningTemplate.Id AND projectdesignvariable.Id = ScreeningTemplateValue.ProjectDesignVariableId
                LEFT OUTER join ScreeningTemplateValuechild ON ScreeningTemplateValue.id = ScreeningTemplateValuechild.ScreeningTemplateValueId and projectdesignvariablevalue.id = ScreeningTemplateValuechild.[ProjectDesignVariableValueId]   
                LEFT OUTER JOIN VariableTemplateDetail ON VariableTemplateDetail.VariableId = ProjectDesignVariable.VariableId AND VariableTemplateDetail.VariableTemplateId=ProjectDesignTemplate.VariableTemplateId
                WHERE  SE.ProjectId=@id AND ISNULL(ProjectDesignVariable.DeletedBy,0) = 0 AND ScreeningTemplate.[Status] > 1";
                                #endregion
                                strSQL += "AND SE.Id IN (" + string.Join(",", SubjectId.Id) + ") ";

                                strSQL += "ORDER BY SubjectNo,ProjectDesignVariable.Id,aaa,ProjectDesignPeriod.Id,ProjectDesignVisit.Id,ProjectDesignTemplate.DesignOrder,ProjectDesignVariable.DesignOrder,ProjectDesignVariableValue.SeqNo ";
                                SqlDataSource sqlDataSource = _reportBaseRepository.DataSource(strSQL, parameter);
                                result = _reportBaseRepository.ReportRunNew("ProjectDesign\\ProjectDesignWithData", sqlDataSource, reportSettingNew, companyData, fileInfo);
                            }
                        }
                        #endregion

                    }
                    logStatus += DateTime.Now + "1. === FileEnd ===" + Environment.NewLine;
                }
                #region Update Job Status
                logStatus += DateTime.Now + "===2. Job Update ===" + Environment.NewLine;
                jobMonitoring.CompletedTime = DateTime.Now.UtcDateTime();
                jobMonitoring.JobStatus = JobStatusType.Completed;
                jobMonitoring.FolderPath = System.IO.Path.Combine(documentUrl, fileInfo.ModuleName, fileInfo.FolderType);
                jobMonitoring.FolderName = parentFolderName + ".zip";
                var completeJobMonitoring = _reportBaseRepository.CompleteJobMonitoring(jobMonitoring);
                #endregion
                logStatus += DateTime.Now + "===2. Job Update Finished ===" + Environment.NewLine;

                #region Create Zip File and Remove Folder
                logStatus += DateTime.Now + " ===3. Create Zip and Remove Direcotry===" + Environment.NewLine;
                string Zipfilename = Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, fileInfo.FolderType, parentFolderName);
                ZipFile.CreateFromDirectory(Zipfilename, Zipfilename + ".zip");
                Directory.Delete(Zipfilename, true);
                logStatus += DateTime.Now + " ===3. Create Zip and Remove Direcotry Finished===" + Environment.NewLine;
                #endregion

                #region EmailSend
                logStatus += DateTime.Now + " ===4. Send Email===";
                var user = _userRepository.Find(_jwtTokenAccesser.UserId);
                var ProjectName = _projectRepository.Find(projectId).ProjectCode + "-" + _projectRepository.Find(projectId).ProjectName;
                string asa = Path.Combine(documentUrl, fileInfo.ModuleName, fileInfo.FolderType, jobMonitoring.FolderName);
                var linkOfPdf = "<a href='" + asa + "'>Click Here</a>";
                _emailSenderRespository.SendPdfGeneratedEMail(user.Email, _jwtTokenAccesser.UserName, ProjectName, linkOfPdf);
                logStatus += DateTime.Now + " ===4. Send Email Complete===" + Environment.NewLine;
                #endregion
                return result;
            }
            catch (Exception ex)
            {
                string path = System.IO.Path.Combine(fileInfo.Base_URL, fileInfo.ModuleName, Enum.GetName(typeof(JobStatusType), JobStatusType.Log), fileInfo.ParentFolderName, fileInfo.ParentFolderName + ".txt");
                jobMonitoring.JobStatus = JobStatusType.Log;
                jobMonitoring.CompletedTime = DateTime.Now.UtcDateTime();
                jobMonitoring.FolderPath = System.IO.Path.Combine(documentUrl, fileInfo.ModuleName, Enum.GetName(typeof(JobStatusType), JobStatusType.Log));
                jobMonitoring.FolderName = parentFolderName + ".txt";
                _reportBaseRepository.WriteLog(logStatus + Environment.NewLine + ex.Message, path, jobMonitoring);
                throw;
            }
            #region Please dont delete, this are period , visit and template filter
            //if (reportSettingNew.PeriodIds != null && reportSettingNew.PeriodIds.Length > 0)
            //{
            //    strSQL += "AND ProjectDesignPeriod.Id IN (" + string.Join(",", reportSettingNew.PeriodIds) + ") ";
            //}
            //if (reportSettingNew.VisitIds != null && reportSettingNew.VisitIds.Length > 0)
            //{
            //    strSQL += "AND ProjectDesignVisit.Id IN (" + string.Join(",", reportSettingNew.VisitIds) + ") ";
            //}
            //if (reportSettingNew.TemplateIds != null && reportSettingNew.TemplateIds.Length > 0)
            //{
            //    strSQL += "AND ProjectDesignTemplate.Id IN (" + string.Join(",", reportSettingNew.TemplateIds) + ") ";
            //}
            #endregion

        }
    }
}