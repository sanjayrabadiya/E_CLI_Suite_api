using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.LabManagement
{
    public class LabManagementVariableMappingRepository : GenericRespository<LabManagementVariableMapping>, ILabManagementVariableMappingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public LabManagementVariableMappingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public void DeleteMapping(LabManagementVariableMappingDto mappingDto)
        {
            var Result = All.Where(x => x.LabManagementConfigurationId == mappingDto.LabManagementConfigurationId && x.DeletedDate == null).ToList();
            foreach (var item in Result)
            {
                item.DeletedBy = _jwtTokenAccesser.UserId;
                item.DeletedDate = _jwtTokenAccesser.GetClientDate();
                item.AuditReasonId = mappingDto.AuditReasonId;
                item.ReasonOth = mappingDto.ReasonOth;
                Update(item);
            }
            _context.Save();
        }

    }
}
