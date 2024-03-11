using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using GSC.Respository.Barcode;
using GSC.Respository.UserMgt;
using GSC.Respository.Volunteer;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using GSC.Respository.Attendance;

namespace GSC.Api.Controllers.Barcode
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleBarcodeGenerateController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ISampleBarcodeGenerateRepository _sampleBarcodeGenerateRepository;
        private readonly IUnitOfWork _uow;
        private readonly ISampleBarcodeRepository _sampleBarcodeRepository;
        private readonly IAppScreenRepository _appScreenRepository;
        private readonly IBarcodeConfigRepository _barcodeConfigRepository;
        private readonly IBarcodeAuditRepository _barcodeAuditRepository;

        public SampleBarcodeGenerateController(
            IMapper mapper
            , ISampleBarcodeGenerateRepository sampleBarcodeGenerateRepository
            , ISampleBarcodeRepository sampleBarcodeRepository
            , IAppScreenRepository appScreenRepository
            , IBarcodeConfigRepository barcodeConfigRepository
            , IBarcodeAuditRepository barcodeAuditRepository
            , IUnitOfWork uow)
        {
            _mapper = mapper;
            _sampleBarcodeGenerateRepository = sampleBarcodeGenerateRepository;
            _sampleBarcodeRepository = sampleBarcodeRepository;
            _appScreenRepository = appScreenRepository;
            _barcodeConfigRepository = barcodeConfigRepository;
            _barcodeAuditRepository = barcodeAuditRepository;
            _uow = uow;
        }

        [HttpPost]
        public IActionResult Post([FromBody] List<SampleBarcodeGenerateDto> SampleBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var Page = _appScreenRepository.FindBy(x => x.TableName == "SampleBarcode" && x.DeletedBy == null).FirstOrDefault();
            if (Page == null)
            {
                ModelState.AddModelError("Message", "Barcode configuration not found.");
                return BadRequest(ModelState);
            }
            var barcodeConfig = _barcodeConfigRepository.FindBy(x => x.PageId == Page.Id && x.DeletedBy == null).FirstOrDefault();
            if (barcodeConfig == null)
            {
                ModelState.AddModelError("Message", "Barcode configuration not found.");
                return BadRequest(ModelState);
            }

            List<int> Ids = new List<int>();

            foreach (var item in SampleBarcodeGenerateDto)
            {
                var SampleBarcodeData = _sampleBarcodeRepository.Find(item.SampleBarcodeId);
                if ((int)SampleBarcodeData.PKBarcodeOption > 1)
                {
                    for (int i = 1; i <= (int)SampleBarcodeData.PKBarcodeOption; i++)
                    {
                        item.Id = 0;
                        item.BarcodeConfigId = barcodeConfig.Id;
                        item.BarcodeString = _sampleBarcodeGenerateRepository.GetBarcodeString(SampleBarcodeData, i);
                        var SampleBarcode = _mapper.Map<SampleBarcodeGenerate>(item);
                        _sampleBarcodeGenerateRepository.Add(SampleBarcode);
                        _uow.Save();
                        Ids.Add(SampleBarcode.Id);
                        _barcodeAuditRepository.Save("SampleBarcodeGenerate", AuditAction.Inserted, SampleBarcode.Id);
                    }
                }
                else
                {
                    item.Id = 0;
                    item.BarcodeConfigId = barcodeConfig.Id;
                    item.BarcodeString = _sampleBarcodeGenerateRepository.GetBarcodeString(SampleBarcodeData, 0);
                    var SampleBarcode = _mapper.Map<SampleBarcodeGenerate>(item);
                    _sampleBarcodeGenerateRepository.Add(SampleBarcode);
                    _uow.Save();
                    Ids.Add(SampleBarcode.Id);
                    _barcodeAuditRepository.Save("SampleBarcodeGenerate", AuditAction.Inserted, SampleBarcode.Id);
                }
            }

            var generatedData = _sampleBarcodeGenerateRepository.GetReprintBarcodeGenerateData(Ids.ToArray());

            return Ok(generatedData);
        }

        [HttpGet]
        [Route("ViewBarcode/{SampleBarcodeId}")]
        public IActionResult ViewBarcode(int SampleBarcodeId)
        {
            var Page = _appScreenRepository.FindBy(x => x.TableName == "SampleBarcode" && x.DeletedBy == null).FirstOrDefault();
            if (Page == null)
            {
                ModelState.AddModelError("Message", "Barcode configuration not found.");
                return BadRequest(ModelState);
            }
            var barcodeConfig = _barcodeConfigRepository.FindBy(x => x.PageId == Page.Id && x.DeletedBy == null).FirstOrDefault();
            if (barcodeConfig == null)
            {
                ModelState.AddModelError("Message", "Barcode configuration not found.");
                return BadRequest(ModelState);
            }
            return Ok(_sampleBarcodeGenerateRepository.GetBarcodeDetail(SampleBarcodeId));
        }

        [HttpPost]
        [Route("RePrintBarcode")]
        public IActionResult RePrintBarcode([FromBody] List<SampleBarcodeGenerateDto> SampleBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            List<int> Ids = new List<int>();
            foreach (var item in SampleBarcodeGenerateDto)
            {
                var SampleBarcodeGenerate = _sampleBarcodeGenerateRepository.All.Where(x => x.DeletedDate == null && x.SampleBarcodeId == item.SampleBarcodeId).ToList();
                foreach (var barcodeData in SampleBarcodeGenerate)
                {
                    barcodeData.IsRePrint = item.IsRePrint;
                    Ids.Add(barcodeData.Id);
                    _sampleBarcodeGenerateRepository.Update(barcodeData);
                    _uow.Save();
                    _barcodeAuditRepository.Save("SampleBarcodeGenerate", AuditAction.RePrint, barcodeData.Id);
                }
            }
            return Ok(_sampleBarcodeGenerateRepository.GetReprintBarcodeGenerateData(Ids.ToArray()));
        }

        [HttpPost]
        [Route("DeleteBarcode")]
        public IActionResult DeleteBarcode([FromBody] List<SampleBarcodeGenerateDto> SampleBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in SampleBarcodeGenerateDto)
            {
                var record = _sampleBarcodeGenerateRepository.FindByInclude(x => x.SampleBarcodeId == item.SampleBarcodeId && x.DeletedDate == null).OrderByDescending(d => d.Id);

                if (!record.Any())
                    return NotFound();

                foreach (var barcodeGenerate in record)
                {
                    _sampleBarcodeGenerateRepository.Delete(barcodeGenerate);
                    _uow.Save();
                    _barcodeAuditRepository.Save("SampleBarcodeGenerate", AuditAction.Deleted, item.Id);
                }
            }

            return Ok();
        }
    }
}
