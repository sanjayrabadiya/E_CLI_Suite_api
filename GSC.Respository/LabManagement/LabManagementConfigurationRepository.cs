using AutoMapper;
using AutoMapper.QueryableExtensions;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace GSC.Respository.LabManagement
{
    public class LabManagementConfigurationRepository : GenericRespository<Data.Entities.LabManagement.LabManagementConfiguration>, ILabManagementConfigurationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public LabManagementConfigurationRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<LabManagementConfigurationGridDto> GetConfigurationList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<LabManagementConfigurationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(Data.Entities.LabManagement.LabManagementConfiguration objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ProjectDesignTemplateId == objSave.ProjectDesignTemplateId && x.MimeType == objSave.MimeType && x.DeletedDate == null))
                return "Duplicate File format for: " + objSave.ProjectDesignTemplate.TemplateCode;
            return "";
        }

        public object[] GetMappingData(int LabManagementConfigurationId)
        {
            string pathname = @"C:\Users\User\Desktop\MappingSheet.xlsx";
            FileStream streamer = new FileStream(pathname, FileMode.Open);
            IExcelDataReader reader = null;
            if (Path.GetExtension(pathname) == ".xls")
                reader = ExcelReaderFactory.CreateBinaryReader(streamer);
            else
                reader = ExcelReaderFactory.CreateOpenXmlReader(streamer);
            DataSet results = reader.AsDataSet();
            results.Tables[0].Rows[0].ToString();
            results.AcceptChanges();
            var MappingData = results.Tables[0].Rows[0].Table.Rows[0].ItemArray;
            streamer.Dispose();
            return MappingData;
        }
    }
}
