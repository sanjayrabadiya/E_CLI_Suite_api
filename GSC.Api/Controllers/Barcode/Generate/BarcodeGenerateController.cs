using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode.Generate;
using GSC.Data.Entities.Barcode.Generate;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Barcode;
using GSC.Respository.Barcode.Generate;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Barcode.Generate
{
    [Route("api/[controller]")]
    public class BarcodeGenerateController : BaseController
    {
        private readonly IBarcodeGenerateRepository _barcodeGenerateRepository;
        private readonly IMapper _mapper;
        private readonly IProjectDesignPeriodRepository _projectDesignPeriodRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IProjectSubjectRepository _projectSubjectRepository;
        private readonly IUnitOfWork _uow;

        public BarcodeGenerateController(IBarcodeGenerateRepository barcodeGenerateRepository,
            IProjectSubjectRepository projectSubjectRepository,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _barcodeGenerateRepository = barcodeGenerateRepository;
            _projectSubjectRepository = projectSubjectRepository;
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            return Ok(_barcodeGenerateRepository.GetBarcodeGenerate(isDeleted));
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var barcodeGenerate = _barcodeGenerateRepository.Find(id);
            var barcodeGenerateDto = _mapper.Map<BarcodeGenerateDto>(barcodeGenerate);
            return Ok(barcodeGenerateDto);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] BarcodeGenerateDto barcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            barcodeGenerateDto.Id = 0;
            //For Multiple Template
            foreach (var id in barcodeGenerateDto.Ids)
            {
                barcodeGenerateDto.ProejctDesignTemplateId = id;

                if (barcodeGenerateDto.ProejctDesignTemplateId <= 0) return BadRequest();

                var barcodeGenerate = _mapper.Map<BarcodeGenerate>(barcodeGenerateDto);

                var subs = new BarcodeSubjectDetailDto();
                subs.ProjectNo = barcodeGenerateDto.ProjectCode;
                var period = _projectDesignPeriodRepository.GetPeriod(barcodeGenerateDto.ProjectDesignPeriodId);
                subs.PeriodNo = period.DisplayName;
                var template =
                    _projectDesignTemplateRepository.GetTemplate(barcodeGenerateDto.ProejctDesignTemplateId);
                subs.TemmplateNo = template.TemplateCode;
                subs.RandomizationNo = "R001";
                barcodeGenerate.BarcodeSubjects = new List<BarcodeSubjectDetail>();
                if (barcodeGenerateDto.NoOfSubjectGenerate != null)
                    for (var i = 0; i < barcodeGenerateDto.NoOfSubjectGenerate; i++)
                    {
                        var barcodeDetail = new BarcodeSubjectDetail();

                        barcodeDetail.ProjectSubject =
                            _projectSubjectRepository.SaveSubjectForProject(barcodeGenerateDto.ProjectId,
                                SubjectNumberType.Normal);

                        barcodeDetail.BarcodeLabelString += subs.TemmplateNo;
                        barcodeGenerate.BarcodeSubjects.Add(barcodeDetail);
                    }

                _barcodeGenerateRepository.Add(barcodeGenerate);
                _uow.Save();
            }
            return Ok();
        }


        [HttpPut]
        public IActionResult Put([FromBody] BarcodeGenerateDto barcodeGenerateDto)
        {
            if (barcodeGenerateDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var barcodeGenerate = _mapper.Map<BarcodeGenerate>(barcodeGenerateDto);

            _barcodeGenerateRepository.Update(barcodeGenerate);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating barcode generate failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(barcodeGenerate.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _barcodeGenerateRepository.Find(id);

            if (record == null)
                return NotFound();

            _barcodeGenerateRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _barcodeGenerateRepository.Find(id);

            if (record == null)
                return NotFound();

            _barcodeGenerateRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetTemplateBarcode/{templateId}")]
        public async Task<IActionResult> GetTemplateBarcode(int[] templateId)
        {
            var barcodeGenerate = await _barcodeGenerateRepository.GetGenerateBarcodeDetail(templateId);
            return Ok(barcodeGenerate);
        }
    }
}