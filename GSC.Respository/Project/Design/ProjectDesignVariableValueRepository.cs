using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Report;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignVariableValueRepository : GenericRespository<ProjectDesignVariableValue>,
        IProjectDesignVariableValueRepository
    {
        private readonly IGSCContext _context;
        public ProjectDesignVariableValueRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) :
            base(context)
        {
            _context = context;
        }

        public IList<DropDownDto> GetProjectDesignVariableValueDropDown(int projectDesignVariableId)
        {
            return All.Where(x => x.DeletedDate == null &&
                                  x.ProjectDesignVariableId == projectDesignVariableId).OrderBy(o => o.SeqNo)
                .Select(c => new DropDownDto
                {
                    Id = c.Id,
                    Value = c.ValueName,
                    Code = c.ValueCode,
                    ExtraData = c.SeqNo
                }).OrderBy(o => o.ExtraData).ToList();
        }

        // Design Report
        public FileStreamResult GetDesignReport(ProjectDatabaseSearchDto search)
        {
            var query = _context.ProjectDesignVariable.AsQueryable();
            query = query.Where(x => x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == search.ParentProjectId
            && x.ProjectDesignTemplate.DeletedDate == null && x.ProjectDesignTemplate.ProjectDesignVisit.DeletedDate == null
            && x.DeletedDate == null);

            if (search.VisitIds != null && search.VisitIds.Length > 0)
                query = query.Where(x => search.VisitIds.Contains(x.ProjectDesignTemplate.ProjectDesignVisit.Id));

            if (search.TemplateIds != null && search.TemplateIds.Length > 0)
                query = query.Where(x => search.VisitIds.Contains(x.ProjectDesignTemplate.Id));

            if (search.VariableIds != null && search.VariableIds.Length > 0)
                query = query.Where(x => search.VisitIds.Contains(x.Id));

            return GetItems(query, search);
        }

        public FileStreamResult GetItems(IQueryable<ProjectDesignVariable> query, ProjectDatabaseSearchDto filters)
        {
            var MainData = query.Select(r => new ProjectDesignReportDto
            {
                StudyCode = r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ProjectCode,
                VisitOrderId = r.ProjectDesignTemplate.ProjectDesignVisit.DesignOrder,
                Visit = r.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                Template = r.ProjectDesignTemplate.TemplateName,
                TemplateOrderId = r.ProjectDesignTemplate.DesignOrder,
                DomainName = r.Domain.DomainName,
                IsRepeated = r.ProjectDesignTemplate.IsRepeated,
                IsParticipantView = r.ProjectDesignTemplate.IsParticipantView,
                VariableOrderId = r.DesignOrder,
                VariableName = r.VariableName,
                VariableCode = r.VariableCode,
                VariableAlias = r.VariableAlias,
                VariableAnnotation = r.Annotation,
                VariableCategoryName = r.VariableCategoryName,
                Role = r.RoleVariableType.GetDescription(),
                CoreType = r.CoreVariableType.GetDescription(),
                CollectionSource = r.CollectionSource.GetDescription(),
                DataType = r.DataType.GetDescription(),
                IsNa = r.IsNa,
                DateValidate = r.DateValidate.GetDescription(),
                UnitName = r.Unit.UnitName,
                UnitAnnotation = r.UnitAnnotation,
                CollectionAnnotation = r.CollectionAnnotation,
                ValidationType = r.ValidationType.GetDescription(),
                Length = r.Length,
                LowRangeValue = r.LowRangeValue,
                HighRangeValue = r.HighRangeValue,
                DefaultValue = r.DefaultValue,
                IsDocument = r.IsDocument,
                Note = r.Note,
                IsEncrypt = r.IsEncrypt,
                EncryptRole = string.Join(", ", r.Roles.Where(x => x.DeletedDate == null).Select(s => s.SecurityRole.RoleShortName).ToList()),
                CollectionValue = string.Join(", ", r.Values.Where(x => x.DeletedDate == null).Select(s => s.ValueName + (s.Label == null ? "" : "-") + s.Label).ToList()),
                //    ProjectDesignLanguages = GetVisitLanguageData(r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.Id)
            }).ToList().OrderBy(x => x.VisitOrderId).ToList();

            var VisitLanguageData = GetVisitLanguageData(query.Select(r => r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.Id).FirstOrDefault());



            #region Excel Report Design
            var repeatdata = new List<RepeatTemplateDto>();
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet;
                worksheet = workbook.Worksheets.Add("Design");

                worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, 1).Value = "STUDY CODE";
                worksheet.Cell(1, 2).Value = "Visit";
                worksheet.Cell(1, 3).Value = "Template";
                worksheet.Cell(1, 4).Value = "Domain";
                worksheet.Cell(1, 5).Value = "Repeate Template";
                worksheet.Cell(1, 6).Value = "Participant view";
                worksheet.Cell(1, 7).Value = "Variable Name";
                worksheet.Cell(1, 8).Value = "Variable Code";
                worksheet.Cell(1, 9).Value = "Variable Alias";
                worksheet.Cell(1, 10).Value = "Variable Annotation";
                worksheet.Cell(1, 11).Value = "Variable Category";
                worksheet.Cell(1, 12).Value = "Role";
                worksheet.Cell(1, 13).Value = "Core Type";
                worksheet.Cell(1, 14).Value = "Collection Source";
                worksheet.Cell(1, 15).Value = "DataType";
                worksheet.Cell(1, 16).Value = "Na";
                worksheet.Cell(1, 17).Value = "Date Validate";
                worksheet.Cell(1, 18).Value = "Unit";
                worksheet.Cell(1, 19).Value = "Unit Annotation";
                worksheet.Cell(1, 20).Value = "Collection Annotation";
                worksheet.Cell(1, 21).Value = "Validation Type";
                worksheet.Cell(1, 22).Value = "Length";
                worksheet.Cell(1, 23).Value = "Low Range";
                worksheet.Cell(1, 24).Value = "High Range";
                worksheet.Cell(1, 25).Value = "Default Value";
                worksheet.Cell(1, 26).Value = "Variable Note";
                worksheet.Cell(1, 27).Value = "Document";
                worksheet.Cell(1, 28).Value = "Encrypt";
                worksheet.Cell(1, 29).Value = "Encrypted Role";
                worksheet.Cell(1, 30).Value = "Collection Value";

                var j = 2;

                MainData.ForEach(d =>
                            {
                                worksheet.Row(j).Cell(1).SetValue(d.StudyCode);
                                worksheet.Row(j).Cell(2).SetValue(d.Visit);
                                worksheet.Row(j).Cell(3).SetValue(d.Template);
                                worksheet.Row(j).Cell(4).SetValue(d.DomainName);
                                worksheet.Row(j).Cell(5).SetValue(d.IsRepeated);
                                worksheet.Row(j).Cell(6).SetValue(d.IsParticipantView);
                                worksheet.Row(j).Cell(7).SetValue(d.VariableName);
                                worksheet.Row(j).Cell(8).SetValue(d.VariableCode);
                                worksheet.Row(j).Cell(9).SetValue(d.VariableAlias);
                                worksheet.Row(j).Cell(10).SetValue(d.VariableAnnotation);
                                worksheet.Row(j).Cell(11).SetValue(d.VariableCategoryName);
                                worksheet.Row(j).Cell(12).SetValue(d.Role);
                                worksheet.Row(j).Cell(13).SetValue(d.CoreType);
                                worksheet.Row(j).Cell(14).SetValue(d.CollectionSource);
                                worksheet.Row(j).Cell(15).SetValue(d.DataType);
                                worksheet.Row(j).Cell(16).SetValue(d.IsNa);
                                worksheet.Row(j).Cell(17).SetValue(d.DateValidate);
                                worksheet.Row(j).Cell(18).SetValue(d.UnitName);
                                worksheet.Row(j).Cell(19).SetValue(d.UnitAnnotation);
                                worksheet.Row(j).Cell(20).SetValue(d.CollectionAnnotation);
                                worksheet.Row(j).Cell(21).SetValue(d.ValidationType);
                                worksheet.Row(j).Cell(22).SetValue(d.Length);
                                worksheet.Row(j).Cell(23).SetValue(d.LowRangeValue);
                                worksheet.Row(j).Cell(24).SetValue(d.HighRangeValue);
                                worksheet.Row(j).Cell(25).SetValue(d.DefaultValue);
                                worksheet.Row(j).Cell(26).SetValue(d.Note);
                                worksheet.Row(j).Cell(27).SetValue(d.IsDocument);
                                worksheet.Row(j).Cell(28).SetValue(d.IsEncrypt);
                                worksheet.Row(j).Cell(29).SetValue(d.EncryptRole);
                                worksheet.Row(j).Cell(30).SetValue(d.CollectionValue);
                                j++;
                            });

                worksheet = workbook.Worksheets.Add("Visit Language");

                worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, 1).Value = "Visit";
                worksheet.Cell(1, 2).Value = "Language";
                worksheet.Cell(1, 3).Value = "Conversion";

                var v = 2;
                VisitLanguageData.ToList().ForEach(d =>
                {
                    worksheet.Row(v).Cell(1).SetValue(d.Name);
                    worksheet.Row(v).Cell(2).SetValue(d.Language);
                    worksheet.Row(v).Cell(3).SetValue(d.Value);
                    v++;
                });

                MemoryStream memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;
                FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/vnd.ms-excel");
                fileStreamResult.FileDownloadName = "Blank.xls";
                return fileStreamResult;
            }
            #endregion
        }

        public IList<ProjectDesignLanguageReportDto> GetVisitLanguageData(int ProjectDesignPeriodId)
        {
            var visits = new List<int>();
            visits = _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriodId == ProjectDesignPeriodId && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();

            return _context.VisitLanguage.Where(t => visits.Contains(t.ProjectDesignVisitId) && t.DeletedDate == null).Select(r => new ProjectDesignLanguageReportDto
            {
                Name = r.ProjectDesignVisit.DisplayName,
                Language = r.Language.LanguageName,
                Value = r.Display
            }).ToList();
        }
    }
}