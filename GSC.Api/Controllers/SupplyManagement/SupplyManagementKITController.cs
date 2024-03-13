using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyManagementKitController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementKitRepository _supplyManagementKITRepository;
        private readonly ISupplyManagementKitDetailRepository _supplyManagementKITDetailRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
       
        public SupplyManagementKitController(ISupplyManagementKitRepository supplyManagementKITRepository,
            IUnitOfWork uow, IMapper mapper,
            IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, ISupplyManagementKitDetailRepository supplyManagementKITDetailRepository)
        {
            _supplyManagementKITRepository = supplyManagementKITRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _supplyManagementKITDetailRepository = supplyManagementKITDetailRepository;
            
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _supplyManagementKITRepository.Find(id);
            var centralDepoDto = _mapper.Map<SupplyManagementKITDto>(centralDepo);
            return Ok(centralDepoDto);
        }

        [HttpGet("GetKITList/{projectId}/{siteId}/{isDeleted:bool?}")]
        public IActionResult Get(int projectId, int siteId, bool isDeleted)
        {
            var kits = _supplyManagementKITRepository.GetKITList(isDeleted, projectId, siteId);
            return Ok(kits);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] SupplyManagementKITDto supplyManagementUploadFileDto)
        {
           
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            supplyManagementUploadFileDto.Id = 0;
            var kitsettings = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == supplyManagementUploadFileDto.ProjectId).FirstOrDefault();
            if (kitsettings == null)
            {
                ModelState.AddModelError("Message", "please set kit number formate!");
                return BadRequest(ModelState);
            }
            var expire = _supplyManagementKITRepository.CheckExpiryDate(supplyManagementUploadFileDto);
            if (!string.IsNullOrEmpty(expire))
            {
                ModelState.AddModelError("Message", expire);
                return BadRequest(ModelState);
            }
            var supplyManagementUploadFile = _mapper.Map<SupplyManagementKIT>(supplyManagementUploadFileDto);
            supplyManagementUploadFile.TotalUnits = (supplyManagementUploadFileDto.NoOfImp * supplyManagementUploadFileDto.NoofPatient);

            var availableqty = _supplyManagementKITRepository.GetAvailableRemainingkitCount(supplyManagementUploadFileDto.ProjectId, supplyManagementUploadFileDto.PharmacyStudyProductTypeId, supplyManagementUploadFileDto.ProductReceiptId);
            if (availableqty < supplyManagementUploadFile.TotalUnits)
            {
                ModelState.AddModelError("Message", "Quantity is not available");
                return BadRequest(ModelState);
            }
            supplyManagementUploadFile.IpAddress = _jwtTokenAccesser.IpAddress;
            supplyManagementUploadFile.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _supplyManagementKITRepository.Add(supplyManagementUploadFile);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating Kit Creation failed on save."));


            for (int i = 0; i < supplyManagementUploadFile.NoofPatient; i++)
            {
                bool isexist = false;
                while (!isexist)
                {

                    SupplyManagementKITDetail obj = new SupplyManagementKITDetail();
                    obj.KitNo = _supplyManagementKITRepository.GenerateKitNo(kitsettings, 1);
                    obj.SupplyManagementKITId = supplyManagementUploadFile.Id;
                    obj.Status = KitStatus.AllocationPending;
                    obj.NoOfImp = supplyManagementUploadFileDto.NoOfImp;
                    if (kitsettings.IsBarcodeScan)
                        obj.Barcode = _supplyManagementKITRepository.GenerateKitBarcode(obj);
                    _supplyManagementKITDetailRepository.Add(obj);
                    if (!_supplyManagementKITDetailRepository.All.Any(x => x.KitNo == obj.KitNo))
                    {
                        if (_uow.Save() <= 0) return Ok(new Exception("Creating Kit Creation failed on save."));

                        SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                        history.SupplyManagementKITDetailId = obj.Id;
                        history.Status = KitStatus.AllocationPending;
                        history.RoleId = _jwtTokenAccesser.RoleId;
                        _supplyManagementKITRepository.InsertKitHistory(history);
                        _uow.Save();
                        isexist = true;
                    }
                    else
                    {
                        isexist = false;
                    }

                }

            }

            return Ok(supplyManagementUploadFile.Id);

        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementKITDetailRepository.Find(id);

            if (record == null)
                return NotFound();

            if (record.Status != KitStatus.AllocationPending)
            {
                ModelState.AddModelError("Message", "Kit should not be deleted once the shipment/receipt has been generated!");
                return BadRequest(ModelState);
            }

            _supplyManagementKITDetailRepository.Delete(record);
            _uow.Save();

            record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));

            _supplyManagementKITDetailRepository.Update(record);
            _uow.Save();
            return Ok();
        }
        [HttpPost("DeleteKits")]
        [TransactionRequired]
        public IActionResult DeleteKits([FromBody] DeleteKitDto deleteKitDto)
        {
            if (deleteKitDto.list.Count == 0)
            {
                ModelState.AddModelError("Message", "please select atleast one kit!");
                return BadRequest(ModelState);
            }
            foreach (var item in deleteKitDto.list)
            {
                var record = _supplyManagementKITDetailRepository.Find(item);
                if (record == null)
                    return NotFound();

                if (record.Status != KitStatus.AllocationPending)
                {
                    ModelState.AddModelError("Message", "Kit should not be deleted once the shipment/receipt has been generated!");
                    return BadRequest(ModelState);
                }
                _supplyManagementKITDetailRepository.Delete(record);
                _uow.Save();

                record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));

                _supplyManagementKITDetailRepository.Update(record);
                _uow.Save();
            }

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyManagementKITRepository.Find(id);

            if (record == null)
                return NotFound();

            _supplyManagementKITRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetVisitDropDownByAllocation/{projectId}")]
        public IActionResult GetVisitDropDownByAllocation(int projectId)
        {
            return Ok(_supplyManagementKITRepository.GetVisitDropDownByAllocation(projectId));
        }

        [HttpGet]
        [Route("getApprovedKit/{id}")]
        public IActionResult getApprovedKit(int id)
        {
            return Ok(_supplyManagementKITRepository.getApprovedKit(id));
        }

        [HttpGet]
        [Route("getIMPPerKitByKitAllocation/{visitId}/{pharmacyStudyProductTypeId}")]
        public IActionResult getIMPPerKitByKitAllocation(int visitId, int pharmacyStudyProductTypeId)
        {
            var data = _context.SupplyManagementKitAllocationSettings.Where(x => x.DeletedDate == null && x.ProjectDesignVisitId == visitId && x.PharmacyStudyProductTypeId == pharmacyStudyProductTypeId).FirstOrDefault();
            return Ok(data);
        }

        [HttpGet]
        [Route("GetAvailableRemainingkitCount/{projectId}/{projecttypeId}/{productReceiptId}")]
        public IActionResult GetAvailableRemainingkitCount(int projectId, int projecttypeId, int productReceiptId)
        {
            return Ok(_supplyManagementKITRepository.GetAvailableRemainingkitCount(projectId, projecttypeId, productReceiptId));
        }

        [HttpGet("GetRandomizationKitNumberAssignList/{projectId}/{siteId}/{id}")]
        public IActionResult GetRandomizationKitNumberAssignList(int projectId, int siteId, int id)
        {
            var productTypes = _supplyManagementKITRepository.GetRandomizationKitNumberAssignList(projectId, siteId, id);
            return Ok(productTypes);
        }
        [HttpGet("GetRandomizationDropdownKit/{projectId}")]
        public IActionResult GetRandomizationDropdownKit(int projectId)
        {
            var productTypes = _supplyManagementKITRepository.GetRandomizationDropdownKit(projectId);
            return Ok(productTypes);
        }

        [HttpPost]
        [Route("AssignKitNumber")]
        public IActionResult AssignKitNumber([FromBody] SupplyManagementVisitKITDetailDto supplyManagementVisitKITDetailDto)
        {

            supplyManagementVisitKITDetailDto = _supplyManagementKITRepository.SetKitNumber(supplyManagementVisitKITDetailDto);
            if (!string.IsNullOrEmpty(supplyManagementVisitKITDetailDto.ExpiryMesage))
            {
                ModelState.AddModelError("Message", supplyManagementVisitKITDetailDto.ExpiryMesage);
                return BadRequest(ModelState);
            }
            if (string.IsNullOrEmpty(supplyManagementVisitKITDetailDto.KitNo))
            {
                ModelState.AddModelError("Message", "Kit is not available");
                return BadRequest(ModelState);
            }
            return Ok();
        }

        [HttpGet("GetKitHistory/{id}")]
        public IActionResult GetKitHistory(int id)
        {
            var history = _supplyManagementKITRepository.KitHistoryList(id);
            return Ok(history);
        }

        [HttpGet]
        [Route("GetKitReturnList/{projectId}/{kitType}/{siteId?}/{visitId?}/{randomizationId?}")]
        public IActionResult GetKitReturnList(int projectId, KitStatusRandomization kitType, int? siteId, int? visitId, int? randomizationId)
        {
            return Ok(_supplyManagementKITRepository.GetKitReturnList(projectId, kitType, siteId, visitId, randomizationId));
        }

        [HttpPost]
        [Route("ReturnSave")]
        public IActionResult ReturnSave([FromBody] SupplyManagementKITReturnGridDto supplyManagementKITReturnGridDto)
        {
            var returnkit = _supplyManagementKITRepository.ReturnSave(supplyManagementKITReturnGridDto);
            return Ok(returnkit);
        }
        [HttpPost]
        [Route("ReturnSaveAll")]
        public IActionResult ReturnSaveAll([FromBody] SupplyManagementKITReturnDtofinal supplyManagementKITReturnGridDto)
        {
            _supplyManagementKITRepository.ReturnSaveAll(supplyManagementKITReturnGridDto);
            if (supplyManagementKITReturnGridDto != null && supplyManagementKITReturnGridDto.ProjectId > 0)
                _supplyManagementKITRepository.SendKitReturnEmail(supplyManagementKITReturnGridDto);
            return Ok();
        }

        [HttpGet]
        [Route("GetKitDiscardList/{projectId}/{kitType}/{siteId?}/{visitId?}/{randomizationId?}")]
        public IActionResult GetKitDiscardList(int projectId, KitStatusRandomization kitType, int? siteId, int? visitId, int? randomizationId)
        {
            return Ok(_supplyManagementKITRepository.GetKitDiscardList(projectId, kitType, siteId, visitId, randomizationId));
        }

        [HttpPost]
        [Route("KitDiscard")]
        public IActionResult KitDiscard([FromBody] SupplyManagementKITDiscardDtofinal supplyManagementKITReturnGridDto)
        {
            _supplyManagementKITRepository.KitDiscard(supplyManagementKITReturnGridDto);
            return Ok();
        }
        [HttpPost]
        [Route("SendToSponser")]
        public IActionResult KitSendtoSponser([FromBody] SupplyManagementKITDiscardDtofinal supplyManagementKITReturnGridDto)
        {
            _supplyManagementKITRepository.KitSendtoSponser(supplyManagementKITReturnGridDto);
            return Ok();
        }
        [HttpPost]
        [Route("returnVerificationStatus")]
        public IActionResult returnVerificationStatus([FromBody] SupplyManagementKITReturnVerificationDto supplyManagementKITReturnVerificationDto)
        {
            _supplyManagementKITRepository.returnVerificationStatus(supplyManagementKITReturnVerificationDto);
            return Ok();
        }
        [HttpPost]
        [Route("returnVerificationStatusSequence")]
        public IActionResult returnVerificationStatusSequence([FromBody] SupplyManagementKITReturnVerificationSequenceDto supplyManagementKITReturnVerificationDto)
        {
            _supplyManagementKITRepository.returnVerificationStatusSequence(supplyManagementKITReturnVerificationDto);
            return Ok();
        }
        [HttpPost]
        [Route("UnblindTreatment")]
        public IActionResult UnblindTreatment([FromBody] SupplyManagementUnblindTreatmentDto supplyManagementUnblindTreatmentDto)
        {
            _supplyManagementKITRepository.UnblindTreatment(supplyManagementUnblindTreatmentDto);
            return Ok();
        }
        [HttpGet]
        [Route("GetUnblindList/{projectId}/{siteId?}/{randomizationId?}")]
        public IActionResult GetUnblindList(int projectId, int? siteId, int? randomizationId)
        {
            return Ok(_supplyManagementKITRepository.GetUnblindList(projectId, siteId, randomizationId));
        }
        [HttpGet]
        [Route("GetAvailableRemainingkitSequenceCount/{projectId}/{pharmacyStudyProductTypeId}/{productReceiptId}")]
        public IActionResult GetAvailableRemainingkitSequenceCount(int projectId, int pharmacyStudyProductTypeId, int productReceiptId)
        {
            return Ok(_supplyManagementKITRepository.GetAvailableRemainingkitSequenceCount(projectId, pharmacyStudyProductTypeId, productReceiptId));
        }

        [HttpGet]
        [Route("GetDoseListByProductRecieptId/{projectId}/{projecttypeId}/{productReceiptId}")]
        public IActionResult GetDoseListByProductRecieptId(int projectId, int projecttypeId, int productReceiptId)
        {
            return Ok(_supplyManagementKITRepository.GetDoseListByProductRecieptId(projectId, projecttypeId, productReceiptId));
        }

        [HttpGet]
        [Route("KitViewBarcode/{id}/{type}")]
        public IActionResult KitViewBarcode(int id, string type)
        {
            if (type == "Kit")
            {
                var kit = _context.SupplyManagementKITDetail.Include(s => s.SupplyManagementKIT).Where(x => x.Id == id).FirstOrDefault();

                var barcodeConfig = _context.PharmacyBarcodeConfig.Include(s => s.BarcodeDisplayInfo).Where(x => x.ProjectId == kit.SupplyManagementKIT.ProjectId && x.BarcodeModuleType == BarcodeModuleType.kit && x.DeletedBy == null).FirstOrDefault();
                if (barcodeConfig == null)
                {
                    ModelState.AddModelError("Message", "Barcode configuration not found.");
                    return BadRequest(ModelState);
                }
            }
            if (type == "KitPack")
            {
                var kit = _context.SupplyManagementKITSeries.Where(x => x.Id == id).FirstOrDefault();

                var barcodeConfig = _context.PharmacyBarcodeConfig.Include(s => s.BarcodeDisplayInfo).Where(x => x.ProjectId == kit.ProjectId && x.BarcodeModuleType == BarcodeModuleType.kit && x.DeletedBy == null).FirstOrDefault();
                if (barcodeConfig == null)
                {
                    ModelState.AddModelError("Message", "Barcode configuration not found.");
                    return BadRequest(ModelState);
                }
            }
            return Ok(_supplyManagementKITRepository.GetkitBarcodeDetail(id, type));
        }
    }
}
