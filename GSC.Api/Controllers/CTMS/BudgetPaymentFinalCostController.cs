using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class BudgetPaymentFinalCostController : BaseController
    {
        private readonly IBudgetPaymentFinalCostRepository _budgetPaymentFinalCostRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public BudgetPaymentFinalCostController(IBudgetPaymentFinalCostRepository budgetPaymentFinalCostRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _budgetPaymentFinalCostRepository = budgetPaymentFinalCostRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var ctmsActionPoint = _budgetPaymentFinalCostRepository.Find(id);
            var ctmsActionPointDto = _mapper.Map<BudgetPaymentFinalCostDto>(ctmsActionPoint);
            return Ok(ctmsActionPointDto);
        }

        [HttpGet]
        [Route("GetBudgetPaymentFinalCostList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetBudgetPaymentFinalCostList(int projectId,bool isDeleted)
        {
            if (projectId <= 0) return BadRequest();
            var ctmsActionPoint = _budgetPaymentFinalCostRepository.GetBudgetPaymentFinalCostList(projectId, isDeleted);
            return Ok(ctmsActionPoint);
        }

        [HttpPost]
        public IActionResult Post([FromBody] BudgetPaymentFinalCostDto ctmsActionPointDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ctmsActionPointDto.Id = 0;
            ctmsActionPointDto.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            ctmsActionPointDto.IpAddress = _jwtTokenAccesser.IpAddress;
            var ctmsActionPoint = _mapper.Map<BudgetPaymentFinalCost>(ctmsActionPointDto);
            _budgetPaymentFinalCostRepository.Add(ctmsActionPoint);
            if (_uow.Save() <= 0) throw new Exception("Creating budget payment final cost failed on save.");

            return Ok(ctmsActionPoint.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] BudgetPaymentFinalCostDto ctmsActionPointDto)
        {
            if (ctmsActionPointDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            ctmsActionPointDto.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            ctmsActionPointDto.IpAddress = _jwtTokenAccesser.IpAddress;
         
            var ctmsActionPoint = _mapper.Map<BudgetPaymentFinalCost>(ctmsActionPointDto);

            _budgetPaymentFinalCostRepository.Update(ctmsActionPoint);
            if (_uow.Save() <= 0) throw new Exception("Updating action point failed on save.");
            return Ok(ctmsActionPoint.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _budgetPaymentFinalCostRepository.Find(id);

            if (record == null)
                return NotFound();

            _budgetPaymentFinalCostRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

       
        [HttpGet]
        [Route("GetFinalBudgetCost/{ProjectId}")]
        public IActionResult GetFinalBudgetCost(int ProjectId)
        {
            if (ProjectId <= 0) return BadRequest();
            var ctmsActionPoint = _budgetPaymentFinalCostRepository.GetFinalBudgetCost(ProjectId);
            return Ok(ctmsActionPoint);
        }
    }
}