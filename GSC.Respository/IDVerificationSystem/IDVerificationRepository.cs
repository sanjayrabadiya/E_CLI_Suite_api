using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.IDVerificationSystem;
using GSC.Data.Entities.IDVerificationSystem;
using GSC.Data.Entities.LabReportManagement;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.IDVerificationSystem
{
    public class IDVerificationRepository : GenericRespository<IDVerification>, IIDVerificationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public IDVerificationRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IUploadSettingRepository uploadSettingRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public List<IDVerificationDto> GetIDVerificationList(bool isDeleted)
        {
            var dataList = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.UserId == _jwtTokenAccesser.UserId).
                  ProjectTo<IDVerificationDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            dataList.ForEach(x =>
            {
                var path = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), x.DocumentPath).Replace('\\', '/');
                x.DocumentPath = path;
            });

            return dataList;
        }

        public int SaveIDVerificationDocument(IDVerificationDto reportDto)
        {
            reportDto.UserId = _jwtTokenAccesser.UserId;
            var documentPath = Path.Combine("IDVerificationDocuments", reportDto.UserId.ToString());
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
                    //reportDto.DocumentName = fileName;
                    reportDto.DocumentPath = Path.Combine(documentPath, fileName);
                    var iDVerification = _mapper.Map<IDVerification>(reportDto);
                    _context.IDVerification.Add(iDVerification);
                    _context.Save();
                    return iDVerification.Id;
                }
                catch (Exception ex)
                {
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
