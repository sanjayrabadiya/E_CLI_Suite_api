using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class PassthroughMilestoneRepository : GenericRespository<PassthroughMilestone>, IPassthroughMilestoneRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public PassthroughMilestoneRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, IProjectRepository projectRepository, IProjectRightRepository projectRightRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _projectRepository = projectRepository;
            _context = context;
            _projectRightRepository = projectRightRepository;
        }

        public IList<PassthroughMilestoneGridDto> GetPassthroughMilestoneList(int parentProjectId, bool isDeleted)
        {
            var PaymentMilestoneData = new List<PassthroughMilestoneGridDto>();


            return PaymentMilestoneData;
        }
        public string DuplicatePaymentMilestone(PassthroughMilestone paymentMilestone)
        {
            return "";
        }
        public decimal GetPassthroughMilestoneAmount(PassthroughMilestoneDto paymentMilestoneDto)
        {
            decimal EstimatedTotal = 0;
                foreach (var PassThro in paymentMilestoneDto.PassThroughCostIds)
                {
                    EstimatedTotal += _context.PassThroughCost.Where(s => s.Id == PassThro && s.DeletedBy == null).Sum(d => d.Total).GetValueOrDefault();
                }
            return EstimatedTotal;
        }
        public void AddPaymentMilestonePassThroughCostDetail(PassthroughMilestoneDto paymentMilestoneDto)
        {
            foreach (var item in paymentMilestoneDto.PassThroughCostIds)
            {
                var paymentMilestonePassThroughtDetail = new PaymentMilestonePassThroughDetail();
                paymentMilestonePassThroughtDetail.Id = 0;
                paymentMilestonePassThroughtDetail.PassthroughMilestoneId = paymentMilestoneDto.Id;
                paymentMilestonePassThroughtDetail.PassThroughCostId = item;
                _context.PaymentMilestonePassThroughDetail.Add(paymentMilestonePassThroughtDetail);
                _context.Save();
            }
        }
        public void DeletePaymentMilestonePassThroughCostDetail(int Id)
        {
            var paymentMilestonePassThroughDetail = _context.PaymentMilestonePassThroughDetail.Where(s => s.PassthroughMilestoneId == Id && s.DeletedBy == null).ToList();
            paymentMilestonePassThroughDetail.ForEach(s =>
            {
                s.DeletedDate = DateTime.UtcNow;
                s.DeletedBy = _jwtTokenAccesser.UserId;
                _context.PaymentMilestonePassThroughDetail.Update(s);
                _context.Save();
            });
        }
        public void ActivePaymentMilestonePassThroughCostDetail(int Id)
        {
            var paymentMilestonePassThroughDetail = _context.PaymentMilestonePassThroughDetail.Where(s => s.PassthroughMilestoneId == Id && s.DeletedBy != null).ToList();
            paymentMilestonePassThroughDetail.ForEach(s =>
            {
                s.DeletedDate = null;
                s.DeletedBy = null;
                _context.PaymentMilestonePassThroughDetail.Update(s);
                _context.Save();
            });
        }
        public List<DropDownDto> GetPassThroughCostActivity(int projectId)
        {
            var data = _context.PassThroughCost.Include(s => s.PassThroughCostActivity).Include(s => s.Country).Where(d => d.ProjectId == projectId && d.DeletedBy == null)
                  .Select(c => new DropDownDto
                  {
                      Id = c.Id,
                      Value = c.PassThroughCostActivity.ActivityName,
                      ExtraData = c.Country.CountryName
                  }).ToList();
            return data;
        }
        public BudgetPaymentFinalCostDto GetFinalPassthroughTotal(int projectId)
        {
            BudgetPaymentFinalCostDto data = new BudgetPaymentFinalCostDto();
            //one time Add Paybal Amount id diduct in main total
            var resourcePaybalAmount = _context.BudgetPaymentFinalCost.FirstOrDefault(x => x.ProjectId == projectId && x.DeletedDate == null && x.MilestoneType == MilestoneType.PassThroughCost);
            data.PassThroughCost = resourcePaybalAmount?.FinalTotalAmount ?? 0;
            return data;
        }
    }
}
