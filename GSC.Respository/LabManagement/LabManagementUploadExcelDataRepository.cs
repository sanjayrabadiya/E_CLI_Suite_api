using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
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
    }
}
