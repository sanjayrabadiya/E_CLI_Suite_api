using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.IDVerificationSystem;
using GSC.Data.Entities.IDVerificationSystem;
using GSC.Data.Entities.LabReportManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
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
            var dataList = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.UserId == _jwtTokenAccesser.UserId).Include(x => x.IDVerificationFiles).
                  ProjectTo<IDVerificationDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            dataList.ForEach(x =>
            {
                x.IDVerificationFiles.ForEach(m =>
                {
                    var path = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), m.DocumentPath).Replace('\\', '/');
                    m.DocumentPath = path;
                });
            });

            return dataList;
        }

        public List<IDVerificationDto> GetIDVerificationByUser(int userId)
        {
            var dataList = All.Where(x => x.DeletedDate == null && x.UserId == userId).Include(x => x.IDVerificationFiles).
                  ProjectTo<IDVerificationDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            dataList.ForEach(x =>
            {
                x.IDVerificationFiles.ForEach(m =>
                {
                    if (Path.GetExtension(m.DocumentPath) == ".pdf")
                    {
                        var path = Path.Combine(_uploadSettingRepository.GetDocumentPath(), m.DocumentPath);
                        m.DocumentPath = path;
                    }
                    else
                    {
                        var path = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), m.DocumentPath).Replace('\\', '/');
                        m.DocumentPath = path;
                    }
                });
            });

            return dataList;
        }


        public int SaveIDVerificationDocument(IDVerificationDto reportDto)
        {
            reportDto.UserId = _jwtTokenAccesser.UserId;
            reportDto.IsUpload = true;

            var iDVerification = _mapper.Map<IDVerification>(reportDto);
            iDVerification.IDVerificationFiles = null;
            iDVerification.VerifyStatus = DocumentVerifyStatus.Pending;
            _context.IDVerification.Add(iDVerification);
            _context.Save();

            foreach (var IdFile in reportDto.IDVerificationFiles)
            {
                var documentPath = Path.Combine("IDVerificationDocuments", reportDto.UserId.ToString());
                var path = Path.Combine(_uploadSettingRepository.GetDocumentPath(), documentPath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var fileExtension = Path.GetExtension(IdFile.DocumentName);
                var fileName = Guid.NewGuid().ToString().ToUpper() + fileExtension;
                var filePath = Path.Combine(path, fileName);
                if (!File.Exists(filePath))
                {
                    try
                    {
                        byte[] fileBytes = Convert.FromBase64String(IdFile.DocumentBase64String);
                        File.WriteAllBytes(filePath, fileBytes);
                        IdFile.DocumentPath = Path.Combine(documentPath, fileName);
                        IdFile.IDVerificationId = iDVerification.Id;
                        var iDVerificationFile = _mapper.Map<IDVerificationFile>(IdFile);
                        _context.IDVerificationFile.Add(iDVerificationFile);
                    }
                    catch
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }

            var result = _context.Save();

            return result;
        }
    }
}
