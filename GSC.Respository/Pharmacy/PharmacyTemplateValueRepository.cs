using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Pharmacy;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Pharmacy
{
    public class PharmacyTemplateValueRepository : GenericRespository<PharmacyTemplateValue>,
        IPharmacyTemplateValueRepository
    {
        private readonly IGSCContext _context;
        public PharmacyTemplateValueRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser
        )
            : base(context)
        {
            _context = context;
        }

        public List<PharmacyTemplateValueDto> GetPharmacyTemplateTree(int pharmacyEntryId)
        {
            return All.Where(s => s.PharmacyEntryId == pharmacyEntryId && s.DeletedDate == null).Select(s =>
                new PharmacyTemplateValueDto
                {
                    Id = s.Id,
                    PharmacyEntryId = s.PharmacyEntryId,
                    VariableId = s.VariableId,
                    Status = s.Status,
                    StatusName = Enum.GetName(typeof(IsFormType), s.Status),
                    Value = s.Value                 
                }).OrderBy(o => o.VariableName).ToList();
        }

        public PharmacyTemplateValue SaveValue(PharmacyTemplateValue pharmacyTemplateValue)
        {
            if (pharmacyTemplateValue == null)
                return null;

            var result = All.FirstOrDefault(x => x.PharmacyEntryId == pharmacyTemplateValue.PharmacyEntryId
                                                 && x.VariableId == pharmacyTemplateValue.VariableId
                                                 && x.Value == pharmacyTemplateValue.Value
                                                 && x.Status == pharmacyTemplateValue.Status
            );
            if (result != null)
                return result;

            return pharmacyTemplateValue;
        }

        public VariableDto GetPharmacyVariable(VariableDto designVariableDto, int pharmacyEntryId)
        {           
            var values = _context.PharmacyTemplateValue.Where(t => t.PharmacyEntryId == pharmacyEntryId).ToList();
            values.ForEach(t =>
            {
                var variable = designVariableDto;
                if (variable != null)
                    variable.Id = t.Id;
            });

            return designVariableDto;
        }
    }
}