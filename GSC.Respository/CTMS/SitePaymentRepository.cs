using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.Extension;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class SitePaymentRepository : GenericRespository<SitePayment>, ISitePaymentRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SitePaymentRepository(IGSCContext context, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }

        public IList<SitePaymentGridDto> GetSitePaymentList(bool isDeleted, int studyId, int siteId)
        {
            var query = _context.SitePayment.AsQueryable();

            if (studyId != 0)
                query = query.Where(x => x.ProjectId == studyId);

            if (siteId != 0)
                query = query.Where(x => x.SiteId == siteId);

            query = query.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null);

            var sitePaymentData = query
                .Select(sp => new SitePaymentGridDto
                {
                    ProjectId = sp.ProjectId,
                    ProjectName = sp.Project.ProjectCode,
                    SiteId = sp.SiteId,
                    CountryId = sp.CountryId,
                    CountryName = sp.Country.CountryName,
                    BudgetPaymentTypeID = sp.BudgetPaymentType,
                    BudgetPaymentType = sp.BudgetPaymentType.GetDescription()
                })
                .Distinct()
                .ToList();

            foreach (var item in sitePaymentData)
            {
                var sitePaymentChild = All
                    .Where(s => (isDeleted ? s.DeletedDate != null : s.DeletedDate == null)
                                && s.ProjectId == item.ProjectId
                                && s.SiteId == item.SiteId
                                && s.CountryId == item.CountryId
                                && s.BudgetPaymentType == item.BudgetPaymentTypeID)
                    .ProjectTo<SitePaymentGridDto>(_mapper.ConfigurationProvider)
                    .OrderByDescending(x => x.Id)
                    .ToList();

                item.SiteName = _context.Project
                    .Include(s => s.ManageSite)
                    .Where(w => w.Id == item.SiteId)
                    .Select(d => d.ProjectCode ?? d.ManageSite.SiteName)
                    .FirstOrDefault();

                item.SitePaymentChildGridDto = sitePaymentChild;
            }

            return sitePaymentData;
        }

        public List<DropDownDto> GetVisitDropDown(int parentProjectId, int siteId)
        {
            var siteCountryId = _context.Project
                .Include(m => m.ManageSite)
                .Where(w => w.Id == siteId && w.ParentProjectId == parentProjectId && w.DeletedBy == null)
                .Select(x => x.CountryId)
                .FirstOrDefault();

            return _context.PatientCost
                .Include(s => s.ProjectDesignVisit)
                .Where(d => d.ProjectId == parentProjectId && d.ProcedureId != null && d.DeletedBy == null && d.Procedure.Currency.CountryId == siteCountryId)
                .Select(c => new DropDownDto
                {
                    Id = c.Id,
                    Value = c.ProjectDesignVisit.DisplayName,
                    ExtraData = c.ProjectDesignVisitId
                })
                .Distinct()
                .ToList();
        }

        public List<decimal> GetVisitAmount(int parentProjectId, int siteId, int visitId)
        {
            var siteCountryId = _context.Project
                .Include(m => m.ManageSite)
                .Where(w => w.Id == siteId && w.ParentProjectId == parentProjectId && w.DeletedBy == null)
                .Select(x => x.CountryId)
                .FirstOrDefault();

            var estimatedTotal = _context.PatientCost
                .Where(s => s.ProjectDesignVisitId == visitId && s.ProcedureId != null && s.ProjectId == parentProjectId && s.DeletedBy == null && s.Procedure.Currency.CountryId == siteCountryId)
                .Sum(d => d.Rate * d.Cost)
                .GetValueOrDefault();

            return new List<decimal> { estimatedTotal, siteCountryId };
        }

        public List<DropDownDto> GetPassThroughCostActivity(int projectId, int siteId)
        {
            var siteCountryId = _context.Project
                .Include(m => m.ManageSite)
                .Where(w => w.Id == siteId && w.ParentProjectId == projectId && w.DeletedBy == null)
                .Select(x => x.CountryId)
                .FirstOrDefault();

            return _context.PassThroughCost
                .Include(s => s.PassThroughCostActivity)
                .Include(s => s.Country)
                .Where(d => d.ProjectId == projectId && d.CountryId == siteCountryId && d.DeletedBy == null)
                .Select(c => new DropDownDto
                {
                    Id = c.PassThroughCostActivity.Id,
                    Value = c.PassThroughCostActivity.ActivityName,
                    ExtraData = c.Country.CountryName
                })
                .ToList();
        }

        public List<decimal> GetPassthroughTotalAmount(int parentProjectId, int siteId, int passThroughCostActivityId)
        {
            var siteCountryId = _context.Project
                .Include(m => m.ManageSite)
                .Where(w => w.Id == siteId && w.ParentProjectId == parentProjectId && w.DeletedBy == null)
                .Select(x => x.CountryId)
                .FirstOrDefault();

            var passThroughCost = _context.PassThroughCost
                .Where(s => s.PassThroughCostActivityId == passThroughCostActivityId && s.ProjectId == parentProjectId && s.CountryId == siteCountryId && s.DeletedBy == null)
                .Sum(d => d.CurrencyRate.LocalCurrencyRate * d.Rate)
                .GetValueOrDefault();

            return new List<decimal> { passThroughCost, siteCountryId };
        }
    }
}
