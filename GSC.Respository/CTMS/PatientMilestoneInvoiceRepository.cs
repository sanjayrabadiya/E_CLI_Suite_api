using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.CTMS
{
    public class PatientMilestoneInvoiceRepository : GenericRespository<PatientMilestoneInvoice>, IPatientMilestoneInvoiceRepository
    {
        private readonly IMapper _mapper;
        public PatientMilestoneInvoiceRepository(IGSCContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public string Duplicate(PatientMilestoneInvoice patientMilestoneInvoice)
        {
            if (All.Any(x =>
                x.Id != patientMilestoneInvoice.Id && x.PatientMilestoneId == patientMilestoneInvoice.PatientMilestoneId && x.DeletedDate == null))
                return "Duplicate Patient Milestone Invoice";
            return "";
        }

        public PatientMilestoneInvoiceDto GetPatientMilestoneInvoiceById(int milestoneId)
        {
            var value = All.FirstOrDefault(x => x.DeletedDate == null && x.PatientMilestoneId == milestoneId);
            return _mapper.Map<PatientMilestoneInvoiceDto>(value);
        }

        public List<PatientMilestoneInvoiceGridDto> GetPatientMilestoneInvoiceList(bool isDeleted)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                   ProjectTo<PatientMilestoneInvoiceGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}
