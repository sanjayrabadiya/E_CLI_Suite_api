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
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public PassthroughMilestoneRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public IList<PassthroughMilestoneGridDto> GetPassthroughMilestoneList(int parentProjectId, bool isDeleted)
        {
            var PaymentMilestoneData = new List<PassthroughMilestoneGridDto>();

            PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == parentProjectId).
                        ProjectTo<PassthroughMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return PaymentMilestoneData;
        }
        public string DuplicatePaymentMilestone(PassthroughMilestone paymentMilestone)
        {
            if (All.Any(x =>
                x.Id != paymentMilestone.Id && x.PassThroughCostActivityId == paymentMilestone.PassThroughCostActivityId &&
                x.ProjectId == paymentMilestone.ProjectId && x.DeletedDate == null && x.PassThroughCostActivityId != null))
            {
                return "Duplicate Activity ";
            }
            return "";
        }
        public decimal GetPassthroughMilestoneAmount(PassthroughMilestoneDto paymentMilestoneDto)
        {        
                return _context.PassThroughCost.Where(s => s.PassThroughCostActivityId == paymentMilestoneDto.PassThroughCostActivityId && s.ProjectId == paymentMilestoneDto.ProjectId && s.DeletedBy == null).
                        Sum(d => d.Total).GetValueOrDefault();     
        }
        public List<DropDownDto> GetPassThroughCostActivity(int projectId)
        {
            var data = _context.PassThroughCost.Include(s => s.PassThroughCostActivity).Include(s => s.Country).Where(d => d.ProjectId == projectId && d.DeletedBy == null)
                  .Select(c => new DropDownDto
                  {
                      Id = c.PassThroughCostActivity.Id,
                      Value = c.PassThroughCostActivity.ActivityName,
                      ExtraData = c.Country.CountryName
                  }).ToList();
            return data;
        }
        public decimal GetFinalPassthroughTotal(int projectId)
        {
            //first time Total get from Budget Payment FinalCost
            decimal? paymentFinalCost = _context.PassthroughMilestone.Where(s => s.ProjectId == projectId && s.DeletedBy == null).OrderBy(s => s.Id).Select(r => r.PassThroughTotal).LastOrDefault();
            paymentFinalCost ??= _context.BudgetPaymentFinalCost.Where(x => x.ProjectId == projectId && x.MilestoneType == MilestoneType.PassThroughCost && x.DeletedDate == null).Select(s => s.FinalTotalAmount).FirstOrDefault();

            return paymentFinalCost ?? 0;
        }
        public string UpdatePaybalAmount(PassthroughMilestone passthroughMilestone)
        {
            var passthroughMilestoneData = _context.PassthroughMilestone.Where(s => s.ProjectId == passthroughMilestone.ProjectId && s.DeletedBy == null).OrderBy(s => s.Id).LastOrDefault();
            if (passthroughMilestoneData != null)
            {
                passthroughMilestoneData.PassThroughTotal += passthroughMilestoneData.PaybalAmount;
                _context.PassthroughMilestone.Update(passthroughMilestoneData);
                _context.Save();
            }
            return "";
        }
    }
}
