using AutoMapper;
using AutoMapper.QueryableExtensions;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.LabManagement
{
    public class LabManagementUploadDataRepository : GenericRespository<LabManagementUploadData>, ILabManagementUploadDataRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public LabManagementUploadDataRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IUploadSettingRepository uploadSettingRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public List<LabManagementUploadDataGridDto> GetUploadDataList(bool isDeleted)
        {
            var result= All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<LabManagementUploadDataGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            var documentUrl = _uploadSettingRepository.GetDocumentPath();
            result.ForEach(t => t.FullPath = documentUrl + t.PathName);
            return result;
        }

        //Upload data insert into database
        public List<LabManagementUploadExcelData> InsertExcelDataIntoDatabaseTable(LabManagementUploadDataDto labManagementUploadDataDto) 
        {
            var documentUrl = _uploadSettingRepository.GetDocumentPath();
            string pathname = documentUrl + labManagementUploadDataDto.FileName;
            FileStream streamer = new FileStream(pathname, FileMode.Open);
            IExcelDataReader reader = null;
            if (Path.GetExtension(pathname) == ".xls")
                reader = ExcelReaderFactory.CreateBinaryReader(streamer);
            else
                reader = ExcelReaderFactory.CreateOpenXmlReader(streamer);
            DataSet results = reader.AsDataSet();
            results.Tables[0].Rows[0].Delete();
            results.Tables[0].AcceptChanges();

            List<LabManagementUploadExcelData> objLst = new List<LabManagementUploadExcelData>();

            foreach (var item in results.Tables[0].Rows)
            {
                LabManagementUploadExcelData obj = new LabManagementUploadExcelData();
                obj.ScreeningNo = ((DataRow)item).ItemArray[0].ToString();
                obj.RandomizationNo = ((DataRow)item).ItemArray[1].ToString();
                obj.Visit = ((DataRow)item).ItemArray[2].ToString();
                obj.RepeatSampleCollection = ((DataRow)item).ItemArray[3].ToString();
                obj.LaboratryName = ((DataRow)item).ItemArray[4].ToString();
                obj.DateOfSampleCollection = (DateTime)((DataRow)item).ItemArray[5];
                obj.DateOfReport = (DateTime)((DataRow)item).ItemArray[6];
                obj.Panel = ((DataRow)item).ItemArray[7].ToString();
                obj.TestName = ((DataRow)item).ItemArray[8].ToString();
                obj.Result = ((DataRow)item).ItemArray[9].ToString();
                obj.Unit = ((DataRow)item).ItemArray[10].ToString();
                obj.AbnoramalFlag = ((DataRow)item).ItemArray[11].ToString();
                obj.ReferenceRangeLow = ((DataRow)item).ItemArray[12].ToString();
                obj.ReferenceRangeHigh = ((DataRow)item).ItemArray[13].ToString();
                obj.ClinicallySignificant = ((DataRow)item).ItemArray[14].ToString();
                obj.CreatedBy = _jwtTokenAccesser.UserId;
                obj.CreatedDate = _jwtTokenAccesser.GetClientDate();
                objLst.Add(obj);
            }
           // _context.LabManagementUploadExcelData.AddRange(objLst);
            streamer.Dispose();
            return objLst;
        }
    }
}
