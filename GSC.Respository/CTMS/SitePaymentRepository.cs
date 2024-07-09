using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Wordprocessing;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
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
        public SitePaymentRepository(IGSCContext context,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }
        public IList<SitePaymentGridDto> GetSitePaymentList(bool isDeleted, int studyId, int siteId)
        {
            var SitePaymentData = new List<SitePaymentGridDto>();

            if (studyId != 0 && siteId != 0)
            {
               var sitePayment = _context.SitePayment
                    .Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && (x.ProjectId == studyId) && x.SiteId == siteId)
                    .Select(sp => new SitePaymentGridDto
                     {
                         ProjectId = sp.ProjectId,
                         ProjectName = sp.Project.ProjectCode,
                         SiteId = sp.SiteId,
                         CountryId = sp.CountryId,
                         CountryName = sp.Country.CountryName,
                         BudgetPaymentTypeID = sp.BudgetPaymentType,
                         BudgetPaymentType = sp.BudgetPaymentType.GetDescription()
                     }).Distinct().ToList();

                sitePayment.ForEach(x =>
                {
                    SitePaymentData.Add(x);
                });
            }
            else
            {
                var sitePayment = _context.SitePayment.
                     Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && (x.ProjectId == studyId))
                    .Select(sp => new SitePaymentGridDto
                    {
                        ProjectId = sp.ProjectId,
                        ProjectName = sp.Project.ProjectCode,
                        SiteId = sp.SiteId,
                        CountryId = sp.CountryId,
                        CountryName = sp.Country.CountryName,
                        BudgetPaymentTypeID = sp.BudgetPaymentType,
                        BudgetPaymentType = sp.BudgetPaymentType.GetDescription()
                    }).Distinct().ToList();

                sitePayment.ForEach(x =>
                {
                    SitePaymentData.Add(x);
                });
            }

            foreach (var item in SitePaymentData)
            { 
                var sitePaymentChild = All.Where(s => (isDeleted ? s.DeletedDate != null : s.DeletedDate == null) && s.ProjectId == item.ProjectId && s.SiteId == item.SiteId && s.CountryId == item.CountryId && s.BudgetPaymentType == item.BudgetPaymentTypeID).
                             ProjectTo<SitePaymentGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

                item.SiteName = _context.Project.Include(s => s.ManageSite).Where(w => w.Id == item.SiteId).Select(d => d.ProjectCode == null ? d.ManageSite.SiteName : d.ProjectCode).FirstOrDefault();

                item.SitePaymentChildGridDto = sitePaymentChild;
            }
            return SitePaymentData;
        }
        public List<DropDownDto> GetVisitDropDown(int parentProjectId, int siteId)
        {
            //Site->Country To get Visit #Add by mitul on ID: 1621
            var siteCountryId = _context.Project.Include(m => m.ManageSite).
                Where(w => w.Id == siteId && w.ParentProjectId == parentProjectId && w.DeletedBy == null).Select(x => x.CountryId).FirstOrDefault();

            var data = _context.PatientCost.Include(s => s.ProjectDesignVisit).
                Where(d => d.ProjectId == parentProjectId && d.ProcedureId != null && d.DeletedBy == null && d.Procedure.Currency.CountryId == siteCountryId)
                  .Select(c => new DropDownDto
                  {
                      Id = c.Id,
                      Value = c.ProjectDesignVisit.DisplayName,
                      ExtraData = c.ProjectDesignVisitId

                  }).Distinct().ToList();
            return data;
        }
        public List<decimal> GetVisitAmount(int parentProjectId, int siteId,int visitId)
        {
            //List<decimal> obj = new List<decimal>();
            //var siteCountryId = _context.Project.Include(m => m.ManageSite).
            //    Where(w => w.Id == siteId && w.ParentProjectId == parentProjectId && w.DeletedBy == null).Select(x => x.CountryId).FirstOrDefault();

            //var EstimatedTotal = _context.PatientCost.
            //                    Where(s => s.ProjectDesignVisitId == visitId && s.ProcedureId != null && s.ProjectId == parentProjectId && s.DeletedBy == null && s.Procedure.Currency.CountryId == siteCountryId).
            //                    Sum(d => d.Rate * d.Cost).GetValueOrDefault();

            //obj.Add(EstimatedTotal);
            //obj.Add(siteCountryId);

            List<decimal> obj = new List<decimal>();
            var siteCountryId = _context.Project.Include(m => m.ManageSite).
                Where(w => w.Id == siteId && w.ParentProjectId == parentProjectId && w.DeletedBy == null).Select(x => x.CountryId).FirstOrDefault();

            var EstimatedTotal = _context.PatientSiteContract.Include(i=>i.SiteContract).
                Where(w=>w.ProjectDesignVisitId== visitId && w.DeletedBy==null && w.SiteContract.ProjectId == parentProjectId && w.SiteContract.SiteId == siteId).Select(s=>s.PayableTotal).FirstOrDefault();

            obj.Add(EstimatedTotal);
            obj.Add(siteCountryId);


            return obj;
        }

        public List<DropDownDto> GetPassThroughCostActivity(int projectId, int siteId)
        {
            //Site->Country To get Activite #Add by mitul on ID: 1621

            var siteCountryId = _context.Project.Include(m => m.ManageSite).
                Where(w => w.Id == siteId && w.ParentProjectId == projectId && w.DeletedBy == null).Select(x => x.CountryId).FirstOrDefault();

            var data = _context.PassThroughCost.Include(s => s.PassThroughCostActivity).Include(s => s.Country).Where(d => d.ProjectId == projectId && d.CountryId == siteCountryId && d.DeletedBy == null)
                  .Select(c => new DropDownDto
                  {
                      Id = c.PassThroughCostActivity.Id,
                      Value = c.PassThroughCostActivity.ActivityName,
                      ExtraData = c.Country.CountryName
                  }).ToList();
            return data;
        }
        public List<decimal> GetPassthroughTotalAmount(int parentProjectId, int siteId, int passThroughCostActivityId)
        {
            List<decimal> obj = new List<decimal>();
            var siteCountryId = _context.Project.Include(m => m.ManageSite).
              Where(w => w.Id == siteId && w.ParentProjectId == parentProjectId && w.DeletedBy == null).Select(x => x.CountryId).FirstOrDefault();

            var PassThroughCost = _context.PassthroughSiteContract.Include(i => i.SiteContract).
                Where(w => w.PassThroughCostActivityId == passThroughCostActivityId && w.DeletedBy == null && w.SiteContract.ProjectId == parentProjectId && w.SiteContract.SiteId == siteId).Select(s => s.PayableTotal).FirstOrDefault();


            obj.Add(PassThroughCost);
            obj.Add(siteCountryId);

            return obj;
        }

        public string Duplicate(SitePaymentDto sitePaymentDto)
        {
            if (All.Any(x => x.Id != sitePaymentDto.Id && x.ProjectId == sitePaymentDto.ProjectId && x.SiteId == sitePaymentDto.SiteId && x.ProjectDesignVisitId== sitePaymentDto.ProjectDesignVisitId && x.DeletedDate == null))
            {
                return "Duplicate this Site payment";
            }
            return "";
        }
    }
}
