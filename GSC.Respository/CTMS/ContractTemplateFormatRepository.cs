using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;

namespace GSC.Respository.Master
{
    public class ContractTemplateFormatRepository : GenericRespository<ContractTemplateFormat>, IContractTemplateFormatRepository
    {
        private readonly IMapper _mapper;

        public ContractTemplateFormatRepository(IGSCContext context, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
        }
        List<ContractTemplateFormatGridDto> IContractTemplateFormatRepository.GetContractTemplateFormateList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<ContractTemplateFormatGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public string Duplicate(ContractTemplateFormat objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.TemplateName == objSave.TemplateName.Trim() && x.DeletedDate == null))
                return "Duplicate Contract Template Name : " + objSave.TemplateName;
            return "";
        }
        public List<DropDownDto> GetContractFormatTypeDropDown()
        {
            return All.Where(x =>
                    x.DeletedBy == null && x.CreatedBy != null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.TemplateName, Code = c.TemplateCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }
    }
}