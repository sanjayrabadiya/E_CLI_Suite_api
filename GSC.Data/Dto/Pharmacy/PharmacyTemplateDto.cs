using System.Collections.Generic;
using GSC.Data.Entities.Master;

namespace GSC.Data.Dto.Pharmacy
{
    public class PharmacyTemplateDto
    {
        public VariableTemplate VariableTemplate { get; set; }


        public List<PharmacyTemplateValueDto> PharmacyTemplateValue { get; set; }
    }
}