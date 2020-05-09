using System.Data;
using GSC.Data.Dto.Configuration;
using GSC.Helper;
using GSC.Report.Common;
using Microsoft.AspNetCore.Mvc;
using Telerik.Reporting;

namespace GSC.Report
{
    public class GscReport : IGscReport
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IReportBaseRepository _reportBaseRepository;

        public GscReport(IReportBaseRepository reportBaseRepository, IJwtTokenAccesser jwtTokenAccesser)
        {
            _reportBaseRepository = reportBaseRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
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

        public FileStreamResult GetProjectDesignWithFliter(ReportSettingNew reportSettingNew, CompanyData companyData)
        {
            SqlDataSourceParameterCollection parameter = new SqlDataSourceParameterCollection();
            parameter.Add(new SqlDataSourceParameter("@id", DbType.String, reportSettingNew.ProjectId));
            parameter.Add(new SqlDataSourceParameter("@CompanyId", DbType.String, _jwtTokenAccesser.CompanyId));
            string strSQL = string.Empty;
            if (reportSettingNew.PdfStatus == 1)
            {
                if (reportSettingNew.AnnotationType)
                    #region Empty + Annotation
                    strSQL = @"Select Project.Id AS ProjectId, Project.ProjectCode,Project.IsStatic, Project.ProjectName,ProjectDesignPeriod.Id AS ProjectDesignPeriodId,
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
                WHERE ProjectDesign.Id=@id AND ISNULL(ProjectDesignVariable.DeletedBy,0) = 0";
                #endregion
                else
                    #region Empty + Without Annotation
                    strSQL = @"Select Project.Id AS ProjectId, Project.ProjectCode,Project.IsStatic, Project.ProjectName,ProjectDesignPeriod.Id AS ProjectDesignPeriodId,
                ProjectDesignPeriod.DisplayName AS PeriodName,ProjectDesignVisit.Id AS ProjectDesignVisitId,ProjectDesignVisit.DisplayName VisitName,
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
            }
            else
            {
                if (reportSettingNew.AnnotationType)

                    #region Subject + Annotation
                    strSQL = @"Select Project.Id AS ProjectId, Project.ProjectCode,Project.IsStatic, Project.ProjectName,Project.ProjectNumber,ProjectDesignPeriod.Id AS ProjectDesignPeriodId,
                UPPER(ProjectDesignPeriod.DisplayName) AS PeriodName,ProjectDesignVisit.Id AS ProjectDesignVisitId,ProjectDesignVisit.DisplayName VisitName,
                ProjectDesignTemplate.Id AS ProjectDesignTemplateId, ProjectDesignTemplate.TemplateName,ProjectDesignTemplate.ActivityName,
                ProjectDesignTemplate.DesignOrder AS TemplateSeqNo, ProjectDesignTemplate.TemplateCode,
                ProjectDesignVariable.Id AS ProjectDesignVariableId,ProjectDesignVariableValue.Id AS ProjectDesignVariableValue, ProjectDesignVariable.VariableCode,ProjectDesignVariable.VariableName,
				
                    CASE
                WHEN ProjectDesignVariable.CollectionSource IN (1,10)  THEN ScreeningTemplateValue.Value
                WHEN ProjectDesignVariable.CollectionSource IN (7) THEN 
	                CASE 
		                WHEN (CAST(projectdesignvariablevalue.id AS VARCHAR) = ScreeningTemplateValue.value) THEN 'true'
		                ELSE  'false'
	                END
                WHEN ProjectDesignVariable.CollectionSource = 2 THEN  projectdesignvariablevalue.ValueName
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
	                WHEN ProjectDesignVariable.CollectionSource IN (3,4,5,11) THEN
	                CASE 
		                WHEN ScreeningTemplateValue.Value = ''  OR ScreeningTemplateValue.Value IS NULL THEN null
		                ELSE CAST(ScreeningTemplateValue.Value AS DATETIME)
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
                INNER JOIN ProjectDesignTemplate ON ProjectDesignTemplate.ProjectDesignVisitId = ProjectDesignVisit.Id AND ProjectDesignTemplate.DeletedDate IS NULL
                INNER JOIN ProjectDesignVariable ON ProjectDesignVariable.ProjectDesignTemplateId = ProjectDesignTemplate.Id AND ProjectDesignVariable.DeletedDate IS NULL
                INNER JOIN ProjectDesign ON ProjectDesign.Id = ProjectDesignPeriod.ProjectDesignId AND ProjectDesign.DeletedDate IS NULL
                INNER JOIN Project ON Project.Id = ProjectDesign.ProjectId AND Project.DeletedDate IS NULL
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
                WHERE  ProjectDesign.Id=@id AND ISNULL(ProjectDesignVariable.DeletedBy,0) = 0 AND ScreeningTemplate.[Status] > 1 ";
                #endregion
                else
                    #region Subject + Without Annotation
                    strSQL = @"Select Project.Id AS ProjectId, Project.ProjectCode,Project.IsStatic, Project.ProjectName,Project.ProjectNumber,ProjectDesignPeriod.Id AS ProjectDesignPeriodId,
                UPPER(ProjectDesignPeriod.DisplayName) AS PeriodName,ProjectDesignVisit.Id AS ProjectDesignVisitId,ProjectDesignVisit.DisplayName VisitName,
                ProjectDesignTemplate.Id AS ProjectDesignTemplateId, ProjectDesignTemplate.TemplateName,ProjectDesignTemplate.ActivityName,
                ProjectDesignTemplate.DesignOrder AS TemplateSeqNo, ProjectDesignTemplate.TemplateCode,
                ProjectDesignVariable.Id AS ProjectDesignVariableId,ProjectDesignVariableValue.Id AS ProjectDesignVariableValue, ProjectDesignVariable.VariableCode,ProjectDesignVariable.VariableName,
				
                    CASE
                WHEN ProjectDesignVariable.CollectionSource IN (1,10)  THEN ScreeningTemplateValue.Value
                WHEN ProjectDesignVariable.CollectionSource IN (7) THEN 
	                CASE 
		                WHEN (CAST(projectdesignvariablevalue.id AS VARCHAR) = ScreeningTemplateValue.value) THEN 'true'
		                ELSE  'false'
	                END
                WHEN ProjectDesignVariable.CollectionSource = 2 THEN  projectdesignvariablevalue.ValueName
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
	                WHEN ProjectDesignVariable.CollectionSource IN (3,4,5,11) THEN
	                CASE 
		                WHEN ScreeningTemplateValue.Value = ''  OR ScreeningTemplateValue.Value IS NULL THEN null
		                ELSE CAST(ScreeningTemplateValue.Value AS DATETIME)
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
                INNER JOIN ProjectDesignTemplate ON ProjectDesignTemplate.ProjectDesignVisitId = ProjectDesignVisit.Id AND ProjectDesignTemplate.DeletedDate IS NULL
                INNER JOIN ProjectDesignVariable ON ProjectDesignVariable.ProjectDesignTemplateId = ProjectDesignTemplate.Id AND ProjectDesignVariable.DeletedDate IS NULL
                INNER JOIN ProjectDesign ON ProjectDesign.Id = ProjectDesignPeriod.ProjectDesignId AND ProjectDesign.DeletedDate IS NULL
                INNER JOIN Project ON Project.Id = ProjectDesign.ProjectId AND Project.DeletedDate IS NULL
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
                WHERE  ProjectDesign.Id=@id AND ISNULL(ProjectDesignVariable.DeletedBy,0) = 0 AND ScreeningTemplate.[Status] > 1";
                #endregion
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

            if (reportSettingNew.SubjectIds != null && reportSettingNew.SubjectIds.Length > 0 && reportSettingNew.PdfStatus != 1)
            {
                strSQL += "AND SE.AttendanceId IN (" + string.Join(",", reportSettingNew.SubjectIds) + ") ";
            }

            if (reportSettingNew.PdfStatus == 1)
            {
                strSQL += " ORDER BY ProjectDesignPeriod.Id,ProjectDesignVisit.Id,ProjectDesignTemplate.DesignOrder,ProjectDesignVariable.DesignOrder,ProjectDesignVariableValue.SeqNo ";
            }
            else
            {
                strSQL += "ORDER BY SubjectNo,ProjectDesignVariable.Id,aaa,ProjectDesignPeriod.Id,ProjectDesignVisit.Id,ProjectDesignTemplate.DesignOrder,ProjectDesignVariable.DesignOrder,ProjectDesignVariableValue.SeqNo ";
            }
            SqlDataSource sqlDataSource = _reportBaseRepository.DataSource(strSQL, parameter);
            if (reportSettingNew.PdfStatus == 1)
            {
                return _reportBaseRepository.ReportRunNew("ProjectDesign\\ProjectDesignWithFilter", sqlDataSource, reportSettingNew, companyData);
            }
            else
            {
                return _reportBaseRepository.ReportRunNew("ProjectDesign\\ProjectDesignWithData", sqlDataSource, reportSettingNew, companyData);
            }
        }
    }
}