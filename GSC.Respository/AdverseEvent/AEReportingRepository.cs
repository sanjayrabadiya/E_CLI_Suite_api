using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Entities.AdverseEvent;
using GSC.Domain.Context;
using GSC.Respository.Attendance;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.AdverseEvent
{
    public class AEReportingRepository : GenericRespository<AEReporting>, IAEReportingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IMapper _mapper;
        public AEReportingRepository(IGSCContext context, 
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper,
            IRandomizationRepository randomizationRepository) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _randomizationRepository = randomizationRepository;
            _mapper = mapper;
        }

        public List<AEReportingGridDto> GetAEReportingGridData(int projectId)
        {
            var aEData = All.Include(x => x.Randomization).Where(x => x.Randomization.ProjectId == projectId && x.DeletedDate == null).ToList();//FindByInclude(x => x.Randomization.ProjectId == projectId && x.DeletedDate == null).ToList();
            var aEGridData = aEData.Select(c => new AEReportingGridDto
            {
                Id = c.Id,
                SubjectName = c.Randomization.FirstName + " " + c.Randomization.LastName,
                CreatedDate = c.CreatedDate,
                EventDescription = c.EventDescription,
                EventEffectName = c.EventEffect.GetDescription(),
                StartDate = c.StartDate,
                IsReviewedDone = c.IsReviewedDone
            }).ToList();
            return aEGridData;
        }

        public List<AEReportingDto> GetAEReportingList()
        {
            var randomization = _randomizationRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            if (randomization == null) return new List<AEReportingDto>();
            var data = FindBy(x => x.RandomizationId == randomization.Id).ToList();
            var datadtos = _mapper.Map<List<AEReportingDto>>(data);
            datadtos.ForEach(x =>
            {
                x.EventEffectName = x.EventEffect.GetDescription();
            });
            return datadtos;
        }
    }
}
