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
using DocumentFormat.OpenXml.Office2010.Excel;

namespace GSC.Api.Controllers.Barcode
{
    [Route("api/[controller]")]
    [ApiController]
    public class PkBarcodeGenerateController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IPkBarcodeGenerateRepository _pkBarcodeGenerateRepository;
        private readonly IUnitOfWork _uow;
        private readonly IPKBarcodeRepository _pKBarcodeRepository;
        private readonly IAppScreenRepository _appScreenRepository;
        private readonly IBarcodeConfigRepository _barcodeConfigRepository;
        private readonly IBarcodeAuditRepository _barcodeAuditRepository;

        public PkBarcodeGenerateController(
            IMapper mapper
            , IPkBarcodeGenerateRepository pkBarcodeGenerateRepository
            , IPKBarcodeRepository pKBarcodeRepository
            , IAppScreenRepository appScreenRepository
            , IBarcodeConfigRepository barcodeConfigRepository
            , IBarcodeAuditRepository barcodeAuditRepository
            , IUnitOfWork uow)
        {
            _mapper = mapper;
            _pkBarcodeGenerateRepository = pkBarcodeGenerateRepository;
            _pKBarcodeRepository = pKBarcodeRepository;
            _appScreenRepository = appScreenRepository;
            _barcodeConfigRepository = barcodeConfigRepository;
            _barcodeAuditRepository = barcodeAuditRepository;
            _uow = uow;
        }

        [HttpPost]
        public IActionResult Post([FromBody] List<PkBarcodeGenerateDto> PkBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var Page = _appScreenRepository.FindBy(x => x.TableName == "PKBarcode" && x.DeletedBy == null).FirstOrDefault();
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

            foreach (var item in PkBarcodeGenerateDto)
            {
                var PKBarcodeData = _pKBarcodeRepository.Find(item.PKBarcodeId);
                if ((int)PKBarcodeData.PKBarcodeOption > 1)
                {
                    for (int i = 1; i <= (int)PKBarcodeData.PKBarcodeOption; i++)
                    {
                        item.Id = 0;
                        item.BarcodeConfigId = barcodeConfig.Id;
                        item.BarcodeString = _pkBarcodeGenerateRepository.GetBarcodeString(PKBarcodeData, i);
                        var pkBarcode = _mapper.Map<PkBarcodeGenerate>(item);
                        _pkBarcodeGenerateRepository.Add(pkBarcode);
                        _uow.Save();
                        Ids.Add(pkBarcode.Id);
                        _barcodeAuditRepository.Save("PkBarcodeGenerate", AuditAction.Inserted, pkBarcode.Id);
                    }
                }
                else
                {
                    item.Id = 0;
                    item.BarcodeConfigId = barcodeConfig.Id;
                    item.BarcodeString = _pkBarcodeGenerateRepository.GetBarcodeString(PKBarcodeData, 0);
                    var pkBarcode = _mapper.Map<PkBarcodeGenerate>(item);
                    _pkBarcodeGenerateRepository.Add(pkBarcode);
                    _uow.Save();
                    Ids.Add(pkBarcode.Id);
                    _barcodeAuditRepository.Save("PkBarcodeGenerate", AuditAction.Inserted, pkBarcode.Id);
                }
            }

            var generatedData = _pkBarcodeGenerateRepository.GetReprintBarcodeGenerateData(Ids.ToArray());

            return Ok(generatedData);
        }

        [HttpGet]
        [Route("ViewBarcode/{PKBarcodeId}")]
        public IActionResult ViewBarcode(int PKBarcodeId)
        {
            var Page = _appScreenRepository.FindBy(x => x.TableName == "PKBarcode" && x.DeletedBy == null).FirstOrDefault();
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
            return Ok(_pkBarcodeGenerateRepository.GetBarcodeDetail(PKBarcodeId));
        }

        [HttpPost]
        [Route("RePrintBarcode")]
        public IActionResult RePrintBarcode([FromBody] List<PkBarcodeGenerateDto> PkBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            List<int> Ids = new List<int>();
            foreach (var item in PkBarcodeGenerateDto)
            {
                var pkBarcodeGenerate = _pkBarcodeGenerateRepository.All.Where(x => x.DeletedDate == null && x.PKBarcodeId == item.PKBarcodeId).ToList();
                foreach (var barcodeData in pkBarcodeGenerate)
                {
                    barcodeData.IsRePrint = item.IsRePrint;
                    Ids.Add(barcodeData.Id);
                    _pkBarcodeGenerateRepository.Update(barcodeData);
                    _uow.Save();
                    _barcodeAuditRepository.Save("PkBarcodeGenerate", AuditAction.RePrint, barcodeData.Id);
                }
            }
            return Ok(_pkBarcodeGenerateRepository.GetReprintBarcodeGenerateData(Ids.ToArray()));
        }

        [HttpPost]
        [Route("DeleteBarcode")]
        public IActionResult DeleteBarcode([FromBody] List<PkBarcodeGenerateDto> PkBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in PkBarcodeGenerateDto)
            {
                var record = _pkBarcodeGenerateRepository.FindByInclude(x => x.PKBarcodeId == item.PKBarcodeId && x.DeletedDate == null).OrderByDescending(d => d.Id);

                if (!record.Any())
                    return NotFound();


                foreach (var barcodeGenerate in record)
                {
                    _pkBarcodeGenerateRepository.Delete(barcodeGenerate);
                    _uow.Save();
                    _barcodeAuditRepository.Save("PkBarcodeGenerate", AuditAction.Deleted, item.Id);
                }
            }

            return Ok();
        }
    }
}
