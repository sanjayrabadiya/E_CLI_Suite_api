using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementUploadFileDetailRepository : GenericRespository<SupplyManagementUploadFileDetail>, ISupplyManagementUploadFileDetailRepository
    {

        private readonly IMapper _mapper;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public SupplyManagementUploadFileDetailRepository(IGSCContext context, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<SupplyManagementUploadFileDetailDto> GetSupplyManagementUploadFileDetailList(int SupplyManagementUploadFileId)
        {
            var data = _context.SupplyManagementUploadFile.Where(s => s.Id == SupplyManagementUploadFileId).FirstOrDefault();
            var isShow = _context.SupplyManagementKitNumberSettingsRole.
                         Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == data.ProjectId
                         && s.RoleId == _jwtTokenAccesser.RoleId);


            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == data.ProjectId).FirstOrDefault();

            var details = All.Where(x => x.SupplyManagementUploadFileId == SupplyManagementUploadFileId).
                   ProjectTo<SupplyManagementUploadFileDetailDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.RandomizationNo).ToList();
            foreach (var item in details)
            {
                item.Visits = item.Visits.OrderBy(x => x.Id).ToList();
                item.TreatmentType = setting != null && setting.IsBlindedStudy == true && isShow ? "" : item.TreatmentType;
            }

            return details;
        }
    }
}
