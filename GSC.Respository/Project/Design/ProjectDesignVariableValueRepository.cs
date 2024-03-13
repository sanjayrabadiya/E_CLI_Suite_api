using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using ClosedXML.Excel;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Report;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared.Extension;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignVariableValueRepository : GenericRespository<ProjectDesignVariableValue>,
        IProjectDesignVariableValueRepository
    {
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        public ProjectDesignVariableValueRepository(IGSCContext context, IMapper mapper) :
            base(context)
        {
            _context = context;
            _mapper = mapper;
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
            && x.DeletedDate == null).OrderBy(x=>x.ProjectDesignTemplate.ProjectDesignVisit.DesignOrder).ThenBy(x => x.ProjectDesignTemplate.DesignOrder).ThenBy(x=>x.DesignOrder);

            if (search.VisitIds != null && search.VisitIds.Length > 0)
                query = query.Where(x => search.VisitIds.Contains(x.ProjectDesignTemplate.ProjectDesignVisit.Id));

            if (search.TemplateIds != null && search.TemplateIds.Length > 0)
                query = query.Where(x => search.TemplateIds.Contains(x.ProjectDesignTemplate.Id));

            if (search.VariableIds != null && search.VariableIds.Length > 0)
                query = query.Where(x => search.VariableIds.Contains(x.Id));

            return GetItems(query, search);
        }

        public FileStreamResult GetItems(IQueryable<ProjectDesignVariable> query, ProjectDatabaseSearchDto filters)
        {
            var MainData = query.Select(r => new ProjectDesignReportDto
            {
                StudyCode = r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ProjectCode,
                Period = r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                VisitOrderId = r.ProjectDesignTemplate.ProjectDesignVisit.DesignOrder,
                Visit = r.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                IsVisitRepeated = r.ProjectDesignTemplate.ProjectDesignVisit.IsRepeated,
                IsNonCRF = r.ProjectDesignTemplate.ProjectDesignVisit.IsNonCRF,
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
                VariableCategoryName = r.VariableCategory.CategoryName,
                Role = r.RoleVariableType.GetDescription(),
                CoreType = r.CoreVariableType.GetDescription(),
                CollectionSource = r.CollectionSource.GetDescription(),
                DataType = r.DataType.GetDescription(),
                IsNa = r.IsNa,
                DateValidate = r.DateValidate.GetDescription(),
                AnnotationType = r.AnnotationType.AnnotationeName,
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
                CollectionValue = string.Join(", ", r.Values.Where(x => x.DeletedDate == null).Select(s => s.ValueCode + (s.ValueCode == null ? "" : "-") + s.ValueName + (s.Label == null ? "" : "-") + s.Label).ToList()),
                DisplayValue = r.LargeStep,
                DisplayValueToPatient = r.DisplayValue
            }).ToList().OrderBy(x => x.VisitOrderId).ToList();


            var VisitStatusData = GetVisitStatusData(query.Select(r => r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.Id).FirstOrDefault(), filters);
            var TemplateNoteData = GetTemplateNoteData(query.Select(r => r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.Id).FirstOrDefault(), filters);
            var VisitLanguageData = GetVisitLanguageData(query.Select(r => r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.Id).FirstOrDefault(), filters);
            var TemplateLanguageData = GetTemplateLanguageData(query.Select(r => r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.Id).FirstOrDefault(), filters);
            var TemplateNoteLanguageData = GetTemplateNoteLanguageData(query.Select(r => r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.Id).FirstOrDefault(), filters);

            var VariableLanguageData = GetVariableLanguageData(query.Select(r => r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.Id).FirstOrDefault(), filters);
            var VariableNoteLanguageData = GetVariableNoteLanguageData(query.Select(r => r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.Id).FirstOrDefault(), filters);
            var VariableValueLanguageData = GetVariableValueLanguageData(query.Select(r => r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.Id).FirstOrDefault(), filters);

            #region Excel Report Design
            var repeatdata = new List<RepeatTemplateDto>();
            using (var workbook = new XLWorkbook())
            {

                #region ProjectDesign sheet
                IXLWorksheet worksheet = workbook.Worksheets.Add("Design");

                worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, 1).Value = "STUDY CODE";
                worksheet.Cell(1, 2).Value = "Period";
                worksheet.Cell(1, 3).Value = "Visit";
                worksheet.Cell(1, 4).Value = "Repeate Visit";
                worksheet.Cell(1, 5).Value = "Non-CRF Visit";
                worksheet.Cell(1, 6).Value = "Template";
                worksheet.Cell(1, 7).Value = "Domain";
                worksheet.Cell(1, 8).Value = "Repeate Template";
                worksheet.Cell(1, 9).Value = "Participant view";
                worksheet.Cell(1, 10).Value = "Variable Name";
                worksheet.Cell(1, 11).Value = "Variable Code";
                worksheet.Cell(1, 12).Value = "Variable Alias";
                worksheet.Cell(1, 13).Value = "Variable Annotation";
                worksheet.Cell(1, 14).Value = "Annotation Type";
                worksheet.Cell(1, 15).Value = "Variable Category";
                worksheet.Cell(1, 16).Value = "Role";
                worksheet.Cell(1, 17).Value = "Core Type";
                worksheet.Cell(1, 18).Value = "Collection Source";
                worksheet.Cell(1, 19).Value = "DataType";
                worksheet.Cell(1, 20).Value = "Na";
                worksheet.Cell(1, 21).Value = "Date Validate";
                worksheet.Cell(1, 22).Value = "Unit";
                worksheet.Cell(1, 23).Value = "Unit Annotation";
                worksheet.Cell(1, 24).Value = "Collection Annotation";
                worksheet.Cell(1, 25).Value = "Validation Type";
                worksheet.Cell(1, 26).Value = "Length";
                worksheet.Cell(1, 27).Value = "Low Range";
                worksheet.Cell(1, 28).Value = "High Range";
                worksheet.Cell(1, 29).Value = "Default Value";
                worksheet.Cell(1, 30).Value = "Variable Note";
                worksheet.Cell(1, 31).Value = "Document";
                worksheet.Cell(1, 32).Value = "Encrypt";
                worksheet.Cell(1, 33).Value = "Encrypted Role";
                worksheet.Cell(1, 34).Value = "Collection Value";
                worksheet.Cell(1, 35).Value = "Display Value";
                worksheet.Cell(1, 36).Value = "Display Value To Patient";
                var j = 2;

                MainData.ForEach(d =>
                            {
                                worksheet.Row(j).Cell(1).SetValue(d.StudyCode);
                                worksheet.Row(j).Cell(2).SetValue(d.Period);
                                worksheet.Row(j).Cell(3).SetValue(d.Visit);
                                worksheet.Row(j).Cell(4).SetValue(d.IsVisitRepeated);
                                worksheet.Row(j).Cell(5).SetValue(d.IsNonCRF);
                                worksheet.Row(j).Cell(6).SetValue(d.Template);
                                worksheet.Row(j).Cell(7).SetValue(d.DomainName);
                                worksheet.Row(j).Cell(8).SetValue(d.IsRepeated);
                                worksheet.Row(j).Cell(9).SetValue(d.IsParticipantView);
                                worksheet.Row(j).Cell(10).SetValue(d.VariableName);
                                worksheet.Row(j).Cell(11).SetValue(d.VariableCode);
                                worksheet.Row(j).Cell(12).SetValue(d.VariableAlias);
                                worksheet.Row(j).Cell(13).SetValue(d.VariableAnnotation);
                                worksheet.Row(j).Cell(14).SetValue(d.AnnotationType);
                                worksheet.Row(j).Cell(15).SetValue(d.VariableCategoryName);
                                worksheet.Row(j).Cell(16).SetValue(d.Role);
                                worksheet.Row(j).Cell(17).SetValue(d.CoreType);
                                worksheet.Row(j).Cell(18).SetValue(d.CollectionSource);
                                worksheet.Row(j).Cell(19).SetValue(d.DataType);
                                worksheet.Row(j).Cell(20).SetValue(d.IsNa);
                                worksheet.Row(j).Cell(21).SetValue(d.DateValidate);
                                worksheet.Row(j).Cell(22).SetValue(d.UnitName);
                                worksheet.Row(j).Cell(23).SetValue(d.UnitAnnotation);
                                worksheet.Row(j).Cell(24).SetValue(d.CollectionAnnotation);
                                worksheet.Row(j).Cell(25).SetValue(d.ValidationType);
                                worksheet.Row(j).Cell(26).SetValue(d.Length);
                                worksheet.Row(j).Cell(27).SetValue(d.LowRangeValue);
                                worksheet.Row(j).Cell(28).SetValue(d.HighRangeValue);
                                worksheet.Row(j).Cell(29).SetValue(d.DefaultValue);
                                worksheet.Row(j).Cell(30).SetValue(d.Note);
                                worksheet.Row(j).Cell(31).SetValue(d.IsDocument == true ? "Yes" : "No");
                                worksheet.Row(j).Cell(32).SetValue(d.IsEncrypt == true ? "Yes" : "No");
                                worksheet.Row(j).Cell(33).SetValue(d.EncryptRole);
                                worksheet.Row(j).Cell(34).SetValue(d.CollectionValue);
                                worksheet.Row(j).Cell(35).SetValue(d.DisplayValue);
                                worksheet.Row(j).Cell(36).SetValue(d.DisplayValueToPatient == true ? "Yes" : "No");
                                j++;
                            });

                #endregion ProjectDesign sheet

                #region TempSheet
                IXLWorksheet Temp = workbook.Worksheets.Add("Temp");
                Temp.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                Temp.Cell(1, 1).Value = "Period";
                Temp.Cell(1, 2).Value = "Visit";
                Temp.Cell(1, 3).Value = "Template";
                #endregion

                #region Add Visit status sheet
                // IXLWorksheet worksheetVisit = workbook.Worksheets.Add("Visit Status");
                IXLWorksheet worksheetVisit = Temp.CopyTo("Visit Status");
                // worksheetVisit.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                //worksheetVisit.Cell(1, 1).Value = "Period";
                //worksheetVisit.Cell(1, 2).Value = "Visit";
                //worksheetVisit.Cell(1, 3).Value = "Template";
                worksheetVisit.Cell(1, 4).Value = "Variable";
                worksheetVisit.Cell(1, 5).Value = "Status";

                var vs = 2;
                VisitStatusData.ToList().ForEach(d =>
                {
                    worksheetVisit.Row(vs).Cell(1).SetValue(d.PeriodName);
                    worksheetVisit.Row(vs).Cell(2).SetValue(d.VisitName);
                    worksheetVisit.Row(vs).Cell(3).SetValue(d.TemplateName);
                    worksheetVisit.Row(vs).Cell(4).SetValue(d.VariableName);
                    worksheetVisit.Row(vs).Cell(5).SetValue(d.Status);
                    vs++;
                });
                #endregion Add Visit status sheet

                #region Add template note sheet
                IXLWorksheet WorkSheetTNote = Temp.CopyTo("Template Note");
                // IXLWorksheet WorkSheetTNote = workbook.Worksheets.Add("Template Note");
                // worksheetVisit.Copy(type workbook.Worksheets[1]);


                //WorkSheetTNote.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                //WorkSheetTNote.Cell(1, 1).Value = "Period";
                //WorkSheetTNote.Cell(1, 2).Value = "Visit";
                //WorkSheetTNote.Cell(1, 3).Value = "Template";
                WorkSheetTNote.Cell(1, 4).Value = "Note";
                var tnote = 2;
                TemplateNoteData.ToList().ForEach(d =>
                {
                    WorkSheetTNote.Row(tnote).Cell(1).SetValue(d.PeriodName);
                    WorkSheetTNote.Row(tnote).Cell(2).SetValue(d.VisitName);
                    WorkSheetTNote.Row(tnote).Cell(3).SetValue(d.TemplateName);
                    WorkSheetTNote.Row(tnote).Cell(4).SetValue(d.Note);
                    tnote++;
                });
                #endregion Add template note sheet

                #region Add Visit language sheet
                //IXLWorksheet WorkSheetTLan = Temp.CopyTo("Visit Language");
                IXLWorksheet worksheetVisitLan = workbook.Worksheets.Add("Visit Language");

                worksheetVisitLan.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheetVisitLan.Cell(1, 1).Value = "Period";
                worksheetVisitLan.Cell(1, 2).Value = "Visit";
                worksheetVisitLan.Cell(1, 3).Value = "Language";
                worksheetVisitLan.Cell(1, 4).Value = "Conversion";

                var v = 2;
                VisitLanguageData.ToList().ForEach(d =>
                {
                    worksheetVisitLan.Row(v).Cell(1).SetValue(d.PeriodName);
                    worksheetVisitLan.Row(v).Cell(2).SetValue(d.VisitName);
                    worksheetVisitLan.Row(v).Cell(3).SetValue(d.Language);
                    worksheetVisitLan.Row(v).Cell(4).SetValue(d.Value);
                    v++;
                });
                #endregion Add Visit language sheet

                #region Add template language sheet
                IXLWorksheet WorkSheetTLan = Temp.CopyTo("Template Language");
                //worksheet = workbook.Worksheets.Add("Template Language");

                //worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                //worksheet.Cell(1, 1).Value = "Period";
                //worksheet.Cell(1, 2).Value = "Visit";
                //worksheet.Cell(1, 3).Value = "Template";
                WorkSheetTLan.Cell(1, 4).Value = "Language";
                WorkSheetTLan.Cell(1, 5).Value = "Conversion";

                var t = 2;
                TemplateLanguageData.ToList().ForEach(d =>
                {
                    WorkSheetTLan.Row(t).Cell(1).SetValue(d.PeriodName);
                    WorkSheetTLan.Row(t).Cell(2).SetValue(d.VisitName);
                    WorkSheetTLan.Row(t).Cell(3).SetValue(d.TemplateName);
                    WorkSheetTLan.Row(t).Cell(4).SetValue(d.Language);
                    WorkSheetTLan.Row(t).Cell(5).SetValue(d.Value);
                    t++;
                });
                #endregion Add template language sheet

                #region Add template note language sheet
                IXLWorksheet WorkSheetTNoteLan = Temp.CopyTo("Template Note Lang");
                //worksheet = workbook.Worksheets.Add("Template Note Lang");

                //worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                //worksheet.Cell(1, 1).Value = "Period";
                //worksheet.Cell(1, 2).Value = "Visit";
                //worksheet.Cell(1, 3).Value = "Template";
                WorkSheetTNoteLan.Cell(1, 4).Value = "Note";
                WorkSheetTNoteLan.Cell(1, 5).Value = "Language";
                WorkSheetTNoteLan.Cell(1, 6).Value = "Conversion";

                var tn = 2;
                TemplateNoteLanguageData.ToList().ForEach(d =>
                {
                    WorkSheetTNoteLan.Row(tn).Cell(1).SetValue(d.PeriodName);
                    WorkSheetTNoteLan.Row(tn).Cell(2).SetValue(d.VisitName);
                    WorkSheetTNoteLan.Row(tn).Cell(3).SetValue(d.TemplateName);
                    WorkSheetTNoteLan.Row(tn).Cell(4).SetValue(d.Note);
                    WorkSheetTNoteLan.Row(tn).Cell(5).SetValue(d.Language);
                    WorkSheetTNoteLan.Row(tn).Cell(6).SetValue(d.Value);
                    tn++;
                });
                #endregion Add template note language sheet

                #region Add variable language sheet
                IXLWorksheet WorkSheetVLan = Temp.CopyTo("Variable Language");
                //worksheet = workbook.Worksheets.Add("Variable Language");

                //worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                //worksheet.Cell(1, 1).Value = "Period";
                //worksheet.Cell(1, 2).Value = "Visit";
                //worksheet.Cell(1, 3).Value = "Template";
                WorkSheetVLan.Cell(1, 4).Value = "Variable";
                WorkSheetVLan.Cell(1, 5).Value = "Language";
                WorkSheetVLan.Cell(1, 6).Value = "Conversion";

                var vl = 2;
                VariableLanguageData.ToList().ForEach(d =>
                {
                    WorkSheetVLan.Cell(vl, 1).SetValue(d.PeriodName);
                    WorkSheetVLan.Cell(vl, 2).SetValue(d.VisitName);
                    WorkSheetVLan.Cell(vl, 3).SetValue(d.TemplateName);
                    WorkSheetVLan.Cell(vl, 4).SetValue(d.VariableName);
                    WorkSheetVLan.Cell(vl, 5).SetValue(d.Language);
                    WorkSheetVLan.Cell(vl, 6).SetValue(d.Value);

                    //WorkSheetVLan.Row(vl).Cell(1).SetValue(d.PeriodName);
                    //WorkSheetVLan.Row(vl).Cell(2).SetValue(d.VisitName);
                    //WorkSheetVLan.Row(vl).Cell(3).SetValue(d.TemplateName);
                    //WorkSheetVLan.Row(vl).Cell(4).SetValue(d.VariableName);
                    //WorkSheetVLan.Row(vl).Cell(5).SetValue(d.Language);
                    //WorkSheetVLan.Row(vl).Cell(6).SetValue(d.Value);
                    vl++;
                });
                #endregion Add variable language sheet

                #region Add variable note language sheet
                IXLWorksheet WorkSheetVNoteLan = Temp.CopyTo("Variable Note Lang");
                //worksheet = workbook.Worksheets.Add("Variable Note Lang");

                //worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                //worksheet.Cell(1, 1).Value = "Period";
                //worksheet.Cell(1, 2).Value = "Visit";
                //worksheet.Cell(1, 3).Value = "Template";
                WorkSheetVNoteLan.Cell(1, 4).Value = "Variable";
                WorkSheetVNoteLan.Cell(1, 5).Value = "Note";
                WorkSheetVNoteLan.Cell(1, 6).Value = "Language";
                WorkSheetVNoteLan.Cell(1, 7).Value = "Conversion";

                var vn = 2;
                VariableNoteLanguageData.ToList().ForEach(d =>
                {
                    WorkSheetVNoteLan.Row(vn).Cell(1).SetValue(d.PeriodName);
                    WorkSheetVNoteLan.Row(vn).Cell(2).SetValue(d.VisitName);
                    WorkSheetVNoteLan.Row(vn).Cell(3).SetValue(d.TemplateName);
                    WorkSheetVNoteLan.Row(vn).Cell(4).SetValue(d.VariableName);
                    WorkSheetVNoteLan.Row(vn).Cell(5).SetValue(d.Note);
                    WorkSheetVNoteLan.Row(vn).Cell(6).SetValue(d.Language);
                    WorkSheetVNoteLan.Row(vn).Cell(7).SetValue(d.Value);
                    vn++;
                });
                #endregion Add variable note language sheet

                #region Add variable value language sheet
                IXLWorksheet WorkSheetVvNoteLan = Temp.CopyTo("Variable value Lang");

                //   worksheet = workbook.Worksheets.Add("Variable value Lang");

                //worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                //worksheet.Cell(1, 1).Value = "Period";
                //worksheet.Cell(1, 2).Value = "Visit";
                //worksheet.Cell(1, 3).Value = "Template";
                WorkSheetVvNoteLan.Cell(1, 4).Value = "Variable";
                WorkSheetVvNoteLan.Cell(1, 5).Value = "Value";
                WorkSheetVvNoteLan.Cell(1, 6).Value = "Language";
                WorkSheetVvNoteLan.Cell(1, 7).Value = "Conversion";

                var vv = 2;
                VariableValueLanguageData.ToList().ForEach(d =>
                {
                    WorkSheetVvNoteLan.Row(vv).Cell(1).SetValue(d.PeriodName);
                    WorkSheetVvNoteLan.Row(vv).Cell(2).SetValue(d.VisitName);
                    WorkSheetVvNoteLan.Row(vv).Cell(3).SetValue(d.TemplateName);
                    WorkSheetVvNoteLan.Row(vv).Cell(4).SetValue(d.VariableName);
                    WorkSheetVvNoteLan.Row(vv).Cell(5).SetValue(d.VariableValue);
                    WorkSheetVvNoteLan.Row(vv).Cell(6).SetValue(d.Language);
                    WorkSheetVvNoteLan.Row(vv).Cell(7).SetValue(d.Value);
                    vv++;
                });
                #endregion Add variable value language sheet

                workbook.Worksheets.Delete("Temp");

                MemoryStream memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;
                FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/vnd.ms-excel");
                fileStreamResult.FileDownloadName = "Blank.xls";
                return fileStreamResult;
            }
            #endregion
        }

        public IList<ProjectDesignLanguageReportDto> GetTemplateNoteData(int ProjectDesignPeriodId, ProjectDatabaseSearchDto filters)
        {
            var templates = _context.ProjectDesignTemplateNote.Where(x => x.DeletedDate == null && x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId);

            if (filters.VisitIds != null && filters.VisitIds.Count() > 0)
                templates = templates.Where(x => filters.VisitIds.Contains(x.ProjectDesignTemplate.ProjectDesignVisitId) && x.DeletedDate == null);


            if (filters.TemplateIds != null && filters.TemplateIds.Count() > 0)
                templates = templates.Where(x => filters.TemplateIds.Contains(x.ProjectDesignTemplate.Id) && x.DeletedDate == null);


            return templates.Select(r => new ProjectDesignLanguageReportDto
            {
                PeriodName = r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                VisitName = r.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                TemplateName = r.ProjectDesignTemplate.TemplateName,
                Note = r.Note
            }).ToList();
        }

        public IList<ProjectDesignLanguageReportDto> GetVisitLanguageData(int ProjectDesignPeriodId, ProjectDatabaseSearchDto filters)
        {
            var visits = _context.VisitLanguage.Where(x => x.DeletedDate == null && x.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId);

            if (filters.VisitIds != null && filters.VisitIds.Count() > 0)
                visits = visits.Where(x => filters.VisitIds.Contains(x.ProjectDesignVisitId) && x.DeletedDate == null);


            //if (templateIds != null && templateIds.Count() > 0)
            //    templates = templates.Where(x => templateIds.Contains(x.ProjectDesignTemplate.Id));


            //var visits = new List<int>();
            //if (visitIds != null && visitIds.Length > 0)
            //    visits = visitIds.OfType<int>().ToList();
            //else
            //    visits = _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriodId == ProjectDesignPeriodId && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();

            return visits.Select(r => new ProjectDesignLanguageReportDto
            {
                PeriodName = r.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                VisitName = r.ProjectDesignVisit.DisplayName,
                Language = r.Language.LanguageName,
                Value = r.Display
            }).ToList();
        }

        public IList<ProjectDesignLanguageReportDto> GetVisitStatusData(int ProjectDesignPeriodId, ProjectDatabaseSearchDto filters)
        {
            var visits = _context.ProjectDesignVisitStatus.Where(x => x.DeletedDate == null && x.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId);

            if (filters.VisitIds != null && filters.VisitIds.Count() > 0)
                visits = visits.Where(x => filters.VisitIds.Contains(x.ProjectDesignVisitId) && x.DeletedDate == null);

            //var visits = new List<int>();
            //if (visitIds != null && visitIds.Length > 0)
            //    visits= visitIds.OfType<int>().ToList();
            //else
            //visits = _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriodId == ProjectDesignPeriodId && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();

            return visits.Select(r => new ProjectDesignLanguageReportDto
            {
                PeriodName = r.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                VisitName = r.ProjectDesignVisit.DisplayName,
                TemplateName = r.ProjectDesignVariable.ProjectDesignTemplate.TemplateName,
                VariableName = r.ProjectDesignVariable.VariableName,
                Status = r.VisitStatusId.GetDescription()
            }).ToList();
        }

        public IList<ProjectDesignLanguageReportDto> GetTemplateLanguageData(int ProjectDesignPeriodId, ProjectDatabaseSearchDto filters)
        {
            var templates = _context.TemplateLanguage.Where(x => x.DeletedDate == null && x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId);

            if (filters.VisitIds != null && filters.VisitIds.Count() > 0)
                templates = templates.Where(x => filters.VisitIds.Contains(x.ProjectDesignTemplate.ProjectDesignVisitId) && x.DeletedDate == null);


            if (filters.TemplateIds != null && filters.TemplateIds.Count() > 0)
                templates = templates.Where(x => filters.TemplateIds.Contains(x.ProjectDesignTemplate.Id) && x.DeletedDate == null);

            //var templates = new List<int>();
            //if (templateIds != null && templateIds.Length > 0)
            //    templates = templateIds.OfType<int>().ToList();
            //else
            //    templates = _context.ProjectDesignTemplate.Where(x => x.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();

            return templates.Select(r => new ProjectDesignLanguageReportDto
            {
                PeriodName = r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                VisitName = r.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                TemplateName = r.ProjectDesignTemplate.TemplateName,
                Language = r.Language.LanguageName,
                Value = r.Display
            }).ToList();
        }

        public IList<ProjectDesignLanguageReportDto> GetTemplateNoteLanguageData(int ProjectDesignPeriodId, ProjectDatabaseSearchDto filters)
        {
            var templates = _context.TemplateNoteLanguage.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateNote.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId);

            if (filters.VisitIds != null && filters.VisitIds.Count() > 0)
                templates = templates.Where(x => filters.VisitIds.Contains(x.ProjectDesignTemplateNote.ProjectDesignTemplate.ProjectDesignVisitId) && x.DeletedDate == null);


            if (filters.TemplateIds != null && filters.TemplateIds.Count() > 0)
                templates = templates.Where(x => filters.TemplateIds.Contains(x.ProjectDesignTemplateNote.ProjectDesignTemplate.Id) && x.DeletedDate == null);

            //var templates = new List<int>();
            //if (templateIds != null && templateIds.Length > 0)
            //    templates = templateIds.OfType<int>().ToList();
            //else
            //    templates = _context.ProjectDesignTemplate.Where(x => x.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();

            return templates.Select(r => new ProjectDesignLanguageReportDto
            {
                PeriodName = r.ProjectDesignTemplateNote.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                VisitName = r.ProjectDesignTemplateNote.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                TemplateName = r.ProjectDesignTemplateNote.ProjectDesignTemplate.TemplateName,
                Note = r.ProjectDesignTemplateNote.Note,
                Language = r.Language.LanguageName,
                Value = r.Display
            }).ToList();
        }

        public IList<ProjectDesignLanguageReportDto> GetVariableLanguageData(int ProjectDesignPeriodId, ProjectDatabaseSearchDto filters)
        {
            var variables = _context.VariableLanguage.Where(x => x.DeletedDate == null && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId);

            if (filters.VisitIds != null && filters.VisitIds.Count() > 0)
                variables = variables.Where(x => filters.VisitIds.Contains(x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisitId) && x.DeletedDate == null);


            if (filters.TemplateIds != null && filters.TemplateIds.Count() > 0)
                variables = variables.Where(x => filters.TemplateIds.Contains(x.ProjectDesignVariable.ProjectDesignTemplate.Id) && x.DeletedDate == null);

            if (filters.VariableIds != null && filters.VariableIds.Count() > 0)
                variables = variables.Where(x => filters.VariableIds.Contains(x.ProjectDesignVariable.Id) && x.DeletedDate == null);

            //var variables = new List<int>();
            //if (variableIds != null && variableIds.Length > 0)
            //    variables = variableIds.OfType<int>().ToList();
            //else
            //    variables = _context.ProjectDesignVariable.Where(x => x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();

            return variables.Select(r => new ProjectDesignLanguageReportDto
            {
                PeriodName = r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                VisitName = r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                TemplateName = r.ProjectDesignVariable.ProjectDesignTemplate.TemplateName,
                VariableName = r.ProjectDesignVariable.VariableName,
                Language = r.Language.LanguageName,
                Value = r.Display
            }).ToList();
        }

        public IList<ProjectDesignLanguageReportDto> GetVariableNoteLanguageData(int ProjectDesignPeriodId, ProjectDatabaseSearchDto filters)
        {
            var variables = _context.VariableNoteLanguage.Where(x => x.DeletedDate == null && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId);

            if (filters.VisitIds != null && filters.VisitIds.Count() > 0)
                variables = variables.Where(x => filters.VisitIds.Contains(x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisitId) && x.DeletedDate == null);


            if (filters.TemplateIds != null && filters.TemplateIds.Count() > 0)
                variables = variables.Where(x => filters.TemplateIds.Contains(x.ProjectDesignVariable.ProjectDesignTemplate.Id) && x.DeletedDate == null);

            if (filters.VariableIds != null && filters.VariableIds.Count() > 0)
                variables = variables.Where(x => filters.VariableIds.Contains(x.ProjectDesignVariable.Id) && x.DeletedDate == null);
            //var variables = new List<int>();
            //if (variableIds != null && variableIds.Length > 0)
            //    variables = variableIds.OfType<int>().ToList();
            //else
            //    variables = _context.ProjectDesignVariable.Where(x => x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();

            return variables.Select(r => new ProjectDesignLanguageReportDto
            {
                PeriodName = r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                VisitName = r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                TemplateName = r.ProjectDesignVariable.ProjectDesignTemplate.TemplateName,
                VariableName = r.ProjectDesignVariable.VariableName,
                Note = r.ProjectDesignVariable.Note,
                Language = r.Language.LanguageName,
                Value = r.Display
            }).ToList();
        }

        public void UpdateVariableValues(ProjectDesignVariableDto variableDto, bool CollectionValueDisable, CheckVersionDto checkVersion)
        {

            if (CollectionValueDisable == true)
            {
                var deletedisableValues = All.Where(x => x.ProjectDesignVariableId == variableDto.Id).ToList();
                foreach (var item in deletedisableValues)
                {
                   Delete(item);
                }
            }
            else
            {
                if (variableDto.Values == null || variableDto.Values.Count() == 0) return;
                int seqNo = 0;
                variableDto.Values.ToList().ForEach(x =>
                {
                    var variableValue = _mapper.Map<ProjectDesignVariableValue>(x);
                    if (x.Id > 0 && x.IsDeleted && checkVersion.AnyLive)
                    {
                        variableValue.InActiveVersion = checkVersion.VersionNumber;
                        Update(variableValue);
                    }

                    // change by vipul
                    else if (x.Id > 0 && x.IsDeleted)
                        Delete(variableValue);
                    else if (x.Id > 0)
                    {
                        seqNo += 1;
                        variableValue.SeqNo = seqNo;
                        Update(variableValue);
                    }
                       
                    else if (x.Id == 0 && !x.IsDeleted)
                    {
                        seqNo += 1;
                        variableValue.ProjectDesignVariableId = variableDto.Id;
                        variableValue.SeqNo = seqNo;
                        variableValue.StudyVersion = checkVersion.VersionNumber;
                        Add(variableValue);
                    }
                });
            }
        }

        public IList<ProjectDesignLanguageReportDto> GetVariableValueLanguageData(int ProjectDesignPeriodId, ProjectDatabaseSearchDto filters)
        {
            var variables = _context.VariableValueLanguage.Where(x => x.DeletedDate == null && x.ProjectDesignVariableValue.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId);

            if (filters.VisitIds != null && filters.VisitIds.Count() > 0)
                variables = variables.Where(x => filters.VisitIds.Contains(x.ProjectDesignVariableValue.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisitId) && x.DeletedDate == null);


            if (filters.TemplateIds != null && filters.TemplateIds.Count() > 0)
                variables = variables.Where(x => filters.TemplateIds.Contains(x.ProjectDesignVariableValue.ProjectDesignVariable.ProjectDesignTemplate.Id) && x.DeletedDate == null);

            if (filters.VariableIds != null && filters.VariableIds.Count() > 0)
                variables = variables.Where(x => filters.VariableIds.Contains(x.ProjectDesignVariableValue.ProjectDesignVariable.Id) && x.DeletedDate == null);
            //var variables = new List<int>();
            //if (variableIds != null && variableIds.Length > 0)
            //    variables = variableIds.OfType<int>().ToList();
            //else
            //    variables = _context.ProjectDesignVariable.Where(x => x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId == ProjectDesignPeriodId && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();

            return variables.Select(r => new ProjectDesignLanguageReportDto
            {
                PeriodName = r.ProjectDesignVariableValue.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                VisitName = r.ProjectDesignVariableValue.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                TemplateName = r.ProjectDesignVariableValue.ProjectDesignVariable.ProjectDesignTemplate.TemplateName,
                VariableName = r.ProjectDesignVariableValue.ProjectDesignVariable.VariableName,
                VariableValue = r.ProjectDesignVariableValue.ValueName,
                Language = r.Language.LanguageName,
                Value = r.Display
            }).ToList();
        }
    }
}