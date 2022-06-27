using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class VerificationApprovalTemplateValueRepository : GenericRespository<VerificationApprovalTemplateValue>, IVerificationApprovalTemplateValueRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public VerificationApprovalTemplateValueRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public string GetValueForAudit(VerificationApprovalTemplateValueDto verificationApprovalTemplateValueDto)
        {
            if (verificationApprovalTemplateValueDto.IsDeleted) return null;

            if (verificationApprovalTemplateValueDto.Children?.Count > 0)
            {
                var child = verificationApprovalTemplateValueDto.Children.First();

                var variableValue = _context.StudyLevelFormVariableValue.Find(child.StudyLevelFormVariableValueId);
                if (variableValue != null)
                {
                    var valueChild = _context.VerificationApprovalTemplateValueChild.AsNoTracking()
                        .FirstOrDefault(t => t.Id == child.Id);
                    if (valueChild != null && child.Value == "false")
                    {
                        verificationApprovalTemplateValueDto.OldValue = variableValue.ValueName;
                        return "";
                    }

                    verificationApprovalTemplateValueDto.OldValue = "";
                    return variableValue.ValueName;
                }

                return child.Value;
            }

            if (verificationApprovalTemplateValueDto.IsNa)
                return "N/A";

            return string.IsNullOrWhiteSpace(verificationApprovalTemplateValueDto.ValueName)
                ? verificationApprovalTemplateValueDto.Value
                : verificationApprovalTemplateValueDto.ValueName;
        }

        public void DeleteChild(int verificationApprovalTemplateValueId)
        {
            var childs = _context.VerificationApprovalTemplateValueChild
                .Where(t => t.VerificationApprovalTemplateValueId == verificationApprovalTemplateValueId).ToList();
            _context.VerificationApprovalTemplateValueChild.RemoveRange(childs);
        }

    }
}
