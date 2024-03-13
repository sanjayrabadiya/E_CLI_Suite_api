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
    public class DossingBarcodeGenerateController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IDossingBarcodeGenerateRepository _dossingBarcodeGenerateRepository;
        private readonly IUnitOfWork _uow;
        private readonly IDossingBarcodeRepository _dossingBarcodeRepository;
        private readonly IAppScreenRepository _appScreenRepository;
        private readonly IBarcodeConfigRepository _barcodeConfigRepository;
        private readonly IBarcodeAuditRepository _barcodeAuditRepository;

        public DossingBarcodeGenerateController(
            IMapper mapper
            , IDossingBarcodeGenerateRepository dossingBarcodeGenerateRepository
            , IDossingBarcodeRepository sampleBarcodeRepository
            , IAppScreenRepository appScreenRepository
            , IBarcodeConfigRepository barcodeConfigRepository
            , IBarcodeAuditRepository barcodeAuditRepository
            , IUnitOfWork uow)
        {
            _mapper = mapper;
            _dossingBarcodeGenerateRepository = dossingBarcodeGenerateRepository;
            _dossingBarcodeRepository = sampleBarcodeRepository;
            _appScreenRepository = appScreenRepository;
            _barcodeConfigRepository = barcodeConfigRepository;
            _barcodeAuditRepository = barcodeAuditRepository;
            _uow = uow;
        }

        [HttpPost]
        public IActionResult Post([FromBody] List<DossingBarcodeGenerateDto> DossingBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var Page = _appScreenRepository.FindBy(x => x.TableName == "DossingBarcode" && x.DeletedBy == null).FirstOrDefault();
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

            foreach (var item in DossingBarcodeGenerateDto)
            {
                var DossingBarcodeData = _dossingBarcodeRepository.Find(item.DossingBarcodeId);
                item.Id = 0;
                item.BarcodeConfigId = barcodeConfig.Id;
                item.BarcodeString = _dossingBarcodeGenerateRepository.GetBarcodeString(DossingBarcodeData, 0);
                if (string.IsNullOrEmpty(item.BarcodeString))
                {
                    return BadRequest("Faild to generate barcode");
                }
                var DossingBarcode = _mapper.Map<DossingBarcodeGenerate>(item);
                _dossingBarcodeGenerateRepository.Add(DossingBarcode);
                _uow.Save();
                Ids.Add(DossingBarcode.Id);
                _barcodeAuditRepository.Save("DossingBarcodeGenerate", AuditAction.Inserted, DossingBarcode.Id);
            }

            var generatedData = _dossingBarcodeGenerateRepository.GetReprintBarcodeGenerateData(Ids.ToArray());

            return Ok(generatedData);
        }

        [HttpGet]
        [Route("ViewBarcode/{DossingBarcodeId}")]
        public IActionResult ViewBarcode(int DossingBarcodeId)
        {
            var Page = _appScreenRepository.FindBy(x => x.TableName == "DossingBarcode" && x.DeletedBy == null).FirstOrDefault();
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
            return Ok(_dossingBarcodeGenerateRepository.GetBarcodeDetail(DossingBarcodeId));
        }

        [HttpPost]
        [Route("RePrintBarcode")]
        public IActionResult RePrintBarcode([FromBody] List<DossingBarcodeGenerateDto> DossingBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            List<int> Ids = new List<int>();
            foreach (var item in DossingBarcodeGenerateDto)
            {
                var DossingBarcodeGenerate = _dossingBarcodeGenerateRepository.All.Where(x => x.DeletedDate == null && x.DossingBarcodeId == item.DossingBarcodeId).ToList();
                foreach (var barcodeData in DossingBarcodeGenerate)
                {
                    barcodeData.IsRePrint = item.IsRePrint;
                    Ids.Add(barcodeData.Id);
                    _dossingBarcodeGenerateRepository.Update(barcodeData);
                    _uow.Save();
                    _barcodeAuditRepository.Save("DossingBarcodeGenerate", AuditAction.RePrint, barcodeData.Id);
                }
            }
            return Ok(_dossingBarcodeGenerateRepository.GetReprintBarcodeGenerateData(Ids.ToArray()));
        }

        [HttpPost]
        [Route("DeleteBarcode")]
        public IActionResult DeleteBarcode([FromBody] List<DossingBarcodeGenerateDto> DossingBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in DossingBarcodeGenerateDto)
            {
                var record = _dossingBarcodeGenerateRepository.FindByInclude(x => x.DossingBarcodeId == item.DossingBarcodeId && x.DeletedDate == null).OrderByDescending(d => d.Id);

                if (!record.Any())
                    return NotFound();

                foreach (var barcodeGenerate in record)
                {
                    _dossingBarcodeGenerateRepository.Delete(barcodeGenerate);
                    _uow.Save();
                    _barcodeAuditRepository.Save("DossingBarcodeGenerate", AuditAction.Deleted, item.Id);
                }
            }

            return Ok();
        }
    }
}
