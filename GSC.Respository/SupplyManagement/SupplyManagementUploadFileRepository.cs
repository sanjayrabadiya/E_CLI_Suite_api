using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementUploadFileRepository : GenericRespository<SupplyManagementUploadFile>, ISupplyManagementUploadFileRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly ISupplyManagementUploadFileVisitRepository _supplyManagementUploadFileVisitRepository;
        private readonly ISupplyManagementUploadFileDetailRepository _supplyManagementUploadFileDetailRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IPharmacyStudyProductTypeRepository _pharmacyStudyProductTypeRepository;

        public SupplyManagementUploadFileRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IUploadSettingRepository uploadSettingRepository,
            ISupplyManagementUploadFileVisitRepository supplyManagementUploadFileVisitRepository,
            ISupplyManagementUploadFileDetailRepository supplyManagementUploadFileDetailRepository,
            IPharmacyStudyProductTypeRepository pharmacyStudyProductTypeRepository,
        IProjectDesignVisitRepository projectDesignVisitRepository,
             IProjectRepository projectRepository,
         ICountryRepository countryRepository,
        IMapper mapper)
            : base(context)
        {
            _uploadSettingRepository = uploadSettingRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _supplyManagementUploadFileVisitRepository = supplyManagementUploadFileVisitRepository;
            _supplyManagementUploadFileDetailRepository = supplyManagementUploadFileDetailRepository;
            _pharmacyStudyProductTypeRepository = pharmacyStudyProductTypeRepository;
            _projectRepository = projectRepository;
            _countryRepository = countryRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<SupplyManagementUploadFileGridDto> GetSupplyManagementUploadFileList(bool isDeleted, int ProjectId)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementUploadFileGridDto>(_mapper.ConfigurationProvider).OrderBy(x => x.Id).ToList();
        }


        // Upload excel data insert into database
        public string InsertExcelDataIntoDatabaseTable(SupplyManagementUploadFile supplyManagementUploadFile)
        {
            var validate = All.Where(x => x.ProjectId == supplyManagementUploadFile.ProjectId && x.DeletedDate == null).FirstOrDefault();
            if (validate != null)
            {
                if (validate.SupplyManagementUploadFileLevel != supplyManagementUploadFile.SupplyManagementUploadFileLevel)
                {
                    return "can not upload data, it's already upload different level.";
                }
            }

            var documentUrl = _uploadSettingRepository.GetDocumentPath();
            string pathname = documentUrl + supplyManagementUploadFile.PathName;
            FileStream streamer = new FileStream(pathname, FileMode.Open);
            IExcelDataReader reader = null;
            if (Path.GetExtension(pathname) == ".xls")
                reader = ExcelReaderFactory.CreateBinaryReader(streamer);
            else
                reader = ExcelReaderFactory.CreateOpenXmlReader(streamer);

            DataSet results = reader.AsDataSet();
            DataTable dt = results.Tables[0].AsEnumerable().Where((row, index) => index > 4).CopyToDataTable();
            if (validate != null)
            {
                var isValidRandomizationNo = RandomizationNumberValidation(dt, supplyManagementUploadFile.ProjectId);
                if (isValidRandomizationNo != "")
                    return isValidRandomizationNo;
            }
            else
            {
                var isValidRandomizationNoInSerial = RandomizationNumberCheckSeries(dt, 0);
                if (isValidRandomizationNoInSerial != "")
                    return isValidRandomizationNoInSerial;
            }

            var productType = validateProductType(dt, supplyManagementUploadFile.ProjectId);
            if (productType != "")
                return productType;

            var isValid = validateExcel(results, supplyManagementUploadFile);

            if (isValid == "")
                return "";
            else
                return isValid;
        }

        public FileStreamResult DownloadFormat(SupplyManagementUploadFileDto supplyManagementUploadFile)
        {
            var studyCode = GetProjectCode(supplyManagementUploadFile.ProjectId); //_projectRepository.Find(supplyManagementUploadFile.ProjectId).ProjectCode;
            string site = "";
            string country = "";
            if (supplyManagementUploadFile.SiteId != null)
                site = (string)GetProjectCode((int)supplyManagementUploadFile.SiteId); //_projectRepository.Find((int)supplyManagementUploadFile.SiteId).ProjectCode;
            if (supplyManagementUploadFile.CountryId != null)
                country = (string)GetCountry((int)supplyManagementUploadFile.CountryId); //_countryRepository.Find((int)supplyManagementUploadFile.CountryId).CountryName;

            var projectDesignVisits = GetProjectDesignVisit(supplyManagementUploadFile.ProjectId); //_projectDesignVisitRepository.All
            //                            .Where(x => x.ProjectDesignPeriod.ProjectDesign.Project.Id == supplyManagementUploadFile.ProjectId
            //                            && x.DeletedDate == null).ToList();
            #region Excel Report Design
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("Sheet1");
                worksheet.Range("A1:B3").Style.Fill.BackgroundColor = XLColor.LightGreen;
                worksheet.Range("A1:B3").Style.Font.SetBold();
                worksheet.Range("A1:B3").Style.Border.SetDiagonalBorder(XLBorderStyleValues.Thin);
                worksheet.Cell(1, 1).Value = "Study Code";
                worksheet.Cell(2, 1).Value = "Country";
                worksheet.Cell(3, 1).Value = "Site Code";
                worksheet.Range("A5:B5").Style.Fill.BackgroundColor = XLColor.LightGreen;
                worksheet.Range("A5:B5").Style.Font.SetBold();
                worksheet.Cell(5, 1).Value = "Randomization No";
                worksheet.Cell(5, 2).Value = "Treatment Type";

                var j = 3;
                projectDesignVisits.ForEach(d =>
                 {
                     worksheet.Row(5).Cell(j).Style.Fill.BackgroundColor = XLColor.LightGreen;
                     worksheet.Row(5).Cell(j).Style.Font.SetBold();
                     worksheet.Row(5).Cell(j).SetValue(d.DisplayName);
                     j++;
                 });

                worksheet.Row(1).Cell(2).SetValue(studyCode);
                worksheet.Row(2).Cell(2).SetValue(country);
                worksheet.Row(3).Cell(2).SetValue(site);

                MemoryStream memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;
                FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/vnd.ms-excel");
                fileStreamResult.FileDownloadName = "RandomizationUploadExcel.xls";
                return fileStreamResult;
            }
            #endregion
        }

        public object GetProjectCode(int projectId)
        {
            return _projectRepository.Find(projectId).ProjectCode;
        }

        public object GetCountry(int countryId)
        {
            return _countryRepository.Find(countryId).CountryName;
        }

        public List<ProjectDesignVisit> GetProjectDesignVisit(int projectId)
        {
            return _projectDesignVisitRepository.All
                                        .Where(x => x.ProjectDesignPeriod.ProjectDesign.Project.Id == projectId
                                        && x.DeletedDate == null).ToList();
        }

        public string validateExcel(DataSet results, SupplyManagementUploadFile supplyManagementUploadFile)
        {
            var study = results.Tables[0].Rows[0].ItemArray[1].ToString().Trim();
            var country = results.Tables[0].Rows[1].ItemArray[1].ToString().Trim();
            var site = results.Tables[0].Rows[2].ItemArray[1].ToString().Trim();

            if (study.Trim().ToLower() != Convert.ToString(GetProjectCode(supplyManagementUploadFile.ProjectId)).ToLower())
                return "Please check study code";

            if (supplyManagementUploadFile.SupplyManagementUploadFileLevel == Helper.SupplyManagementUploadFileLevel.Study)
            {
                if (country.Trim() != "" || site.Trim() != "")
                    return "Don't enter country or site when it's upload as study level.";
            }
            else if (supplyManagementUploadFile.SupplyManagementUploadFileLevel == Helper.SupplyManagementUploadFileLevel.Site)
            {
                if (country.Trim() != "")
                    return "Don't enter country when it's upload as site level.";
                if (site.Trim() != "")
                {
                    if (site.Trim().ToLower() != Convert.ToString(GetProjectCode((int)supplyManagementUploadFile.SiteId)).ToLower())
                        return "Please check site code";
                }
                else return "Please enter site code";
            }
            else
            {
                if (site.Trim() != "")
                    return "Don't enter site code when it's upload as country level.";
                if (country.Trim() != "")
                {
                    if (country.Trim().ToLower() != Convert.ToString(GetCountry((int)supplyManagementUploadFile.CountryId)).ToLower())
                        return "Please check country name";
                }
                else
                    return "Please enter country name";
            }

            string selectQuery = "";
            var projectDesignVisits = GetProjectDesignVisit(supplyManagementUploadFile.ProjectId);
            var j = 0;
            foreach (var item in results.Tables[0].Rows[4].ItemArray.Where(x => x.ToString() != ""))
            {
                selectQuery += "Column" + j + " is null or ";
                if (j >= 2)
                {
                    var r = projectDesignVisits.Any(x => x.DisplayName.ToLower().Trim() == item.ToString().ToLower().Trim());
                    if (!r)
                        return "visit not match please check the sheet.";
                }
                else
                {
                    if (j == 0)
                        if (item.ToString().Trim().ToLower() != "randomization no")
                            return "Format not match.Please check the format.";
                    if (j == 1)
                        if (item.ToString().Trim().ToLower() != "treatment type")
                            return "Format not match.Please check the format.";
                }
                j++;
            }

            DataRow[] dr = results.Tables[0].AsEnumerable().Where((row, index) => index > 4).CopyToDataTable().Select(selectQuery.Substring(0, selectQuery.Length - 3));
            if (dr.Length != 0)
                return "Please fill required cell value.";
            else
                return InserFileData(results, supplyManagementUploadFile);
        }

        public string InserFileData(DataSet results, SupplyManagementUploadFile supplyManagementUploadFile)
        {
            DataTable dt = results.Tables[0].AsEnumerable().Where((row, index) => index > 4).CopyToDataTable();
            var visitIds = GetVisitId(results.Tables[0].Rows[4].ItemArray.Where(x => x.ToString() != "").ToList(), supplyManagementUploadFile);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var supplyManagementUploadFileDetail = new SupplyManagementUploadFileDetail();
                supplyManagementUploadFileDetail.Id = 0;
                supplyManagementUploadFileDetail.RandomizationNo = Convert.ToInt32(dt.Rows[i][0]);
                supplyManagementUploadFileDetail.TreatmentType = dt.Rows[i][1].ToString();

                _supplyManagementUploadFileDetailRepository.Add(supplyManagementUploadFileDetail);
                supplyManagementUploadFile.Details.Add(supplyManagementUploadFileDetail);

                supplyManagementUploadFileDetail.Visits = new List<SupplyManagementUploadFileVisit>();
                for (int j = 0; j < visitIds.Length; j++)
                {
                    var supplyManagementUploadFileVisit = new SupplyManagementUploadFileVisit();
                    supplyManagementUploadFileVisit.Id = 0;
                    supplyManagementUploadFileVisit.ProjectDesignVisitId = visitIds[j];
                    supplyManagementUploadFileVisit.Value = dt.Rows[i][j + 2].ToString();
                    _supplyManagementUploadFileVisitRepository.Add(supplyManagementUploadFileVisit);
                    supplyManagementUploadFileDetail.Visits.Add(supplyManagementUploadFileVisit);
                }
            }

            //Add supply Management Upload file Data
            Add(supplyManagementUploadFile);
            return "";
        }

        public int[] GetVisitId(List<object> dr, SupplyManagementUploadFile supplyManagementUploadFile)
        {
            var visits = GetProjectDesignVisit(supplyManagementUploadFile.ProjectId);
            List<int> visitIds = new List<int>();
            foreach (var item in dr)
            {
                var visit = visits.Find(x => x.DisplayName.ToString().Trim().ToLower() == item.ToString().Trim().ToLower());
                if (visit != null)
                    visitIds.Add(visit.Id);
            }
            return visitIds.ToArray();
        }

        public string RandomizationNumberValidation(DataTable dt, int projectId)
        {
            var supplyManagementUploadFile = All.Where(x => x.ProjectId == projectId && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();
            var maxRandomizationNo = _supplyManagementUploadFileDetailRepository.
                All.Where(x => x.SupplyManagementUploadFile.Id == supplyManagementUploadFile.Id).OrderByDescending(x => x.Id).FirstOrDefault();

            var checkRandomizationNumber = RandomizationNumberCheckSeries(dt, maxRandomizationNo.RandomizationNo);
            if (checkRandomizationNumber != "")
                return "randomization no not properformat";
            else
                return "";
        }

        public string RandomizationNumberCheckSeries(DataTable dt, int lastNo)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                lastNo += 1;
                if (Convert.ToInt32(dt.Rows[i][0]) != lastNo)
                    return "randomization no not properformat";
            }
            return "";
        }

        public string validateProductType(DataTable dt, int ProjectId)
        {
            var productTypes = _pharmacyStudyProductTypeRepository.All.Where(x => x.ProjectId == ProjectId).
                Select(a => new { a.ProductType.ProductTypeCode }).Distinct().ToList();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var re = true;
                foreach (var r in productTypes)
                {
                    var types = dt.Rows[i][1].ToString().Contains(r.ProductTypeCode.ToString());
                    if (types)
                    {
                        re = types;
                        break;
                    }
                    else
                        re = types;
                }
                if (!re)
                    return "product type not match.";
            }

            return "";
        }
    }
}
