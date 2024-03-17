using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Pharmacy;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Pharmacy
{
    public class PharmacyVerificationTemplateValueRepository :
        GenericRespository<PharmacyVerificationTemplateValue>, IPharmacyVerificationTemplateValueRepository
    {
        public PharmacyVerificationTemplateValueRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser
        )
            : base(context)
        {
        }

        public List<PharmacyVerificationTemplateValueDto> GetPharmacyVerificationTemplateTree(
            int pharmacyVerificationEntryId)
        {
            return All.Where(s => s.PharmacyVerificationEntryId == pharmacyVerificationEntryId && s.DeletedDate == null)
                .Select(s => new PharmacyVerificationTemplateValueDto
                {
                    Id = s.Id,
                    PharmacyVerificationEntryId = s.PharmacyVerificationEntryId,
                    VariableId = s.VariableId,
                    Status = s.Status,
                    StatusName = Enum.GetName(typeof(IsFormType), s.Status),
                    Value = s.Value
                }).OrderBy(o => o.VariableName).ToList();
        }
    }
}