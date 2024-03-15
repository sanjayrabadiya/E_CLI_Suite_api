using AutoMapper;
using AutoMapper.Internal;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabReportManagement;
using GSC.Data.Entities.LabReportManagement;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GSC.Respository.LabReportManagement
{
    public class LabReportRepository : GenericRespository<LabReport>, ILabReportRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public LabReportRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IUploadSettingRepository uploadSettingRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _uploadSettingRepository = uploadSettingRepository;
        }


        public List<LabReportGridDto> GetLabReports(bool isDeleted)
        {
            var dataList= All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.UserId == _jwtTokenAccesser.UserId).
                   ProjectTo<LabReportGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            dataList.ForEach(x =>
            {
                var path = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), x.DocumentPath).Replace('\\', '/');
                x.DocumentPath = path;
            });

            return dataList;
        }
        public int SaveLabReportDocument(LabReportDto reportDto)
        {
            reportDto.UserId = _jwtTokenAccesser.UserId;
            var documentPath = Path.Combine("LabReportDocuments", reportDto.UserId.ToString());
            var path = Path.Combine(_uploadSettingRepository.GetDocumentPath(), documentPath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var fileExtension = Path.GetExtension(reportDto.DocumentName);
            var fileName = Guid.NewGuid().ToString().ToUpper() + fileExtension;
            var filePath = Path.Combine(path, fileName);
            if (!File.Exists(filePath))
            {
                try
                {
                    byte[] fileBytes = Convert.FromBase64String(reportDto.DocumentBase64String);
                    File.WriteAllBytes(filePath, fileBytes);
                    reportDto.DocumentPath = Path.Combine(documentPath, fileName);
                    var labReport = _mapper.Map<LabReport>(reportDto);
                    _context.LabReport.Add(labReport);
                    _context.Save();
                    return labReport.Id;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}
