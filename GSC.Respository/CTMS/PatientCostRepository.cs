using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace GSC.Respository.CTMS
{
    public class PatientCostRepository : GenericRespository<StudyPlan>, IPatientCostRepository
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public PatientCostRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }
        public List<ProcedureVisitdadaDto> getBudgetPlaner(bool isDeleted, int studyId)
        {
            var Proceduredata =_context.Procedure.Where(x=>x.DeletedBy==null).
                Select(t => new ProcedureVisitdadaDto
            {
                Id = t.Id,
                Name = t.Name,
                CostPerUnit = t.CostPerUnit
            }).ToList();

            if (Proceduredata != null)
                foreach (var item in Proceduredata)
                {
                    var VisitData = _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriod.ProjectDesign.ProjectId == studyId && x.DeletedBy==null).Select(t => new VisitdadaDto
                    {
                        Id = t.Id,
                        VisitName = t.DisplayName,
                        Cost = null,
                        Total= null
                    }).ToList();

                    item.VisitdadaDto = VisitData;
                } 
            return Proceduredata;
        }
    }
}
