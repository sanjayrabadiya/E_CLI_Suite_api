using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;

namespace GSC.Respository.Master
{
    public class PatientSiteContractRepository : GenericRespository<PatientSiteContract>, IPatientSiteContractRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public PatientSiteContractRepository(IGSCContext context,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }
        public IList<PatientSiteContractGridDto> GetPatientSiteContractList(bool isDeleted, int siteContractId)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.SiteContractId == siteContractId).
                 ProjectTo<PatientSiteContractGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public string Duplicate(PatientSiteContractDto patientSiteContractDto)
        {
            if (All.Any(x => x.Id != patientSiteContractDto.Id && x.SiteContractId == patientSiteContractDto.SiteContractId && x.ProjectDesignVisitId == patientSiteContractDto.ProjectDesignVisitId && x.DeletedDate == null))
            {
                return "Duplicate Vitis";
            }
            return "";

        }
    }
}
