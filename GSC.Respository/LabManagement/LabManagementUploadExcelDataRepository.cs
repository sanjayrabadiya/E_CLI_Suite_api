using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GSC.Respository.LabManagement
{
    public class LabManagementUploadExcelDataRepository : GenericRespository<LabManagementUploadExcelData>, ILabManagementUploadExcelDataRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public LabManagementUploadExcelDataRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IUploadSettingRepository uploadSettingRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public List<LabManagementUploadExcelDataDto> GetExcelDataList(int labManagementUploadDataId)
        {
            var result = All.Where(x => x.DeletedDate == null && x.LabManagementUploadDataId == labManagementUploadDataId).
                   ProjectTo<LabManagementUploadExcelDataDto>(_mapper.ConfigurationProvider).OrderBy(x => x.Id).ToList();
            return result;
        }

        public FileStreamResult GetDataNotUseInDataEntry(LabManagementUploadDataDto search)
        {
            var query = All.AsQueryable();
            query = query.Where(x => x.LabManagementUploadDataId == search.Id && !_context.ScreeningTemplateValue
                                                                                       .AsNoTracking().Any(a =>
                                                                                           a.LabManagementUploadExcelDataId == x.Id
                                                                                          ));

            return GetItems(query, search);
        }

        public FileStreamResult GetItems(IQueryable<LabManagementUploadExcelData> query, LabManagementUploadDataDto search)
        {
            var StudyCode = _context.Project.Where(x => x.Id == search.ParentProjectId).FirstOrDefault().ProjectCode;
            var MainData = query.Select(r => new LabManagementUploadExcelDataDto
            {
                StudyCode = StudyCode,
                SiteCode = r.LabManagementUploadData.Project.ProjectCode,
                ScreeningNo = r.ScreeningNo,
                RandomizationNo = r.RandomizationNo,
                Visit = r.Visit,
                RepeatSampleCollection = r.RepeatSampleCollection,
                LaboratryName = r.LaboratryName,
                DateOfSampleCollection = r.DateOfSampleCollection,
                DateOfReport = r.DateOfReport,
                Panel = r.Panel,
                TestName = r.TestName,
                Result = r.Result,
                Unit = r.Unit,
                AbnoramalFlag = r.AbnoramalFlag,
                ReferenceRangeLow = r.ReferenceRangeLow,
                ReferenceRangeHigh = r.ReferenceRangeHigh
            }).ToList().OrderBy(x => x.Id).ToList();

            #region Excel Report Design
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("Sheet1");

                worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, 1).Value = "Study Code";
                worksheet.Cell(1, 2).Value = "Site Code";
                worksheet.Cell(1, 3).Value = "Screening ID";
                worksheet.Cell(1, 4).Value = "Randomization Number";
                worksheet.Cell(1, 5).Value = "Visit";
                worksheet.Cell(1, 6).Value = "Repeat sample collection (Yes, No)";
                worksheet.Cell(1, 7).Value = "Laboratory Name";
                worksheet.Cell(1, 8).Value = "Date of sample collection (dd-mmm-yyyy)";
                worksheet.Cell(1, 9).Value = "Date of Report (dd-mmm-yyyy)";
                worksheet.Cell(1, 10).Value = "Panel (Hematology, Biochemistry, serology, Urinalysis)";
                worksheet.Cell(1, 11).Value = "Test Name";
                worksheet.Cell(1, 12).Value = "Result";
                worksheet.Cell(1, 13).Value = "Unit";
                worksheet.Cell(1, 14).Value = "Abnormal Flag (L=Low, H=High, N=Normal)";
                worksheet.Cell(1, 15).Value = "Reference Range Low";
                worksheet.Cell(1, 16).Value = "Reference Range High";
                var j = 2;

                MainData.ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(2).SetValue(d.SiteCode);
                    worksheet.Row(j).Cell(3).SetValue(d.ScreeningNo);
                    worksheet.Row(j).Cell(4).SetValue(d.RandomizationNo);
                    worksheet.Row(j).Cell(5).SetValue(d.Visit);
                    worksheet.Row(j).Cell(6).SetValue(d.RepeatSampleCollection);
                    worksheet.Row(j).Cell(7).SetValue(d.LaboratryName);
                    worksheet.Row(j).Cell(8).SetValue(d.DateOfSampleCollection);
                    worksheet.Row(j).Cell(9).SetValue(d.DateOfReport);
                    worksheet.Row(j).Cell(10).SetValue(d.Panel);
                    worksheet.Row(j).Cell(11).SetValue(d.TestName);
                    worksheet.Row(j).Cell(12).SetValue(d.Result);
                    worksheet.Row(j).Cell(13).SetValue(d.Unit);
                    worksheet.Row(j).Cell(14).SetValue(d.AbnoramalFlag);
                    worksheet.Row(j).Cell(15).SetValue(d.ReferenceRangeLow);
                    worksheet.Row(j).Cell(16).SetValue(d.ReferenceRangeHigh);
                    j++;
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
    }
}
