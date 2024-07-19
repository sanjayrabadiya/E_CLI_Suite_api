using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using Microsoft.EntityFrameworkCore;

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

        public List<decimal> GetVisitAmount(int parentProjectId, int siteId, int visitId)
        {
            List<decimal> obj = new List<decimal>();
            var siteCountryId = _context.Project.Include(m => m.ManageSite).
                Where(w => w.Id == siteId && w.ParentProjectId == parentProjectId && w.DeletedBy == null).Select(x => x.ManageSite.City.State.CountryId).FirstOrDefault();

            var EstimatedTotal = _context.PatientCost.
                                Where(s => s.ProjectDesignVisitId == visitId && s.ProcedureId != null && s.ProjectId == parentProjectId && s.DeletedBy == null && s.Procedure.Currency.CountryId == siteCountryId).
                                Sum(d => d.Rate * d.Cost).GetValueOrDefault();

            obj.Add(EstimatedTotal);
            obj.Add(siteCountryId);

            return obj;
        }
    }
}
