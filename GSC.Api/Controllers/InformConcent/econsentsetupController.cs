using System.Linq;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Respository.InformConcent;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class econsentsetupController : ControllerBase
    {
        private readonly IEconsentSetupRepository _econsentSetupRepository;
        private readonly IUnitOfWork _uow;
        public econsentsetupController(
            IEconsentSetupRepository econsentSetupRepository,
            IUnitOfWork uow)
        {
            _econsentSetupRepository = econsentSetupRepository;
            _uow = uow;
        }


        [HttpGet]
        [HttpGet("{projectid}/{isDeleted:bool?}")]
        public IActionResult Get(int projectid,bool isDeleted)
        {
            //Get Econsent document list for selected project
            var econsentSetups = _econsentSetupRepository.GetEconsentSetupList(projectid,isDeleted);
            return Ok(econsentSetups);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            //calls when edit particular document
            if (id <= 0)
                return BadRequest();
            return Ok(_econsentSetupRepository.GetEconsent(id));
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            //calls when deactivate econsent document
            var record = _econsentSetupRepository.Find(id);
            if (record == null)
                return NotFound();
            _econsentSetupRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            //calls when activate econsent document
            var record = _econsentSetupRepository.Find(id);
            if (record == null)
                return NotFound();
            EconsentSetupDto econsentSetupDto = new EconsentSetupDto();
            econsentSetupDto.Id = record.Id;
            econsentSetupDto.LanguageId = record.LanguageId;
            econsentSetupDto.Version = record.Version;

            var validate = _econsentSetupRepository.Duplicate(econsentSetupDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _econsentSetupRepository.Active(record);
            _uow.Save();
            return Ok();
        }

        [HttpPost]
        public IActionResult Post([FromBody] EconsentSetupDto econsentSetupDto)
        {
            // add econsent document
            if (!ModelState.IsValid)
               return new UnprocessableEntityObjectResult(ModelState);
            var validate = _econsentSetupRepository.validatebeforeadd(econsentSetupDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            return Ok(_econsentSetupRepository.AddEconsentSetup(econsentSetupDto));
        }


        [HttpPut]
        public IActionResult Put([FromBody] EconsentSetupDto econsentSetupDto)
        {
            //update econsent document
            if (econsentSetupDto.Id <= 0)
                return BadRequest();
            if (!ModelState.IsValid)
               return new UnprocessableEntityObjectResult(ModelState);
            if (_econsentSetupRepository.All.Where(x => x.DocumentName == econsentSetupDto.DocumentName && x.LanguageId == econsentSetupDto.LanguageId && x.ProjectId == econsentSetupDto.ProjectId && x.Id != econsentSetupDto.Id).ToList().Count > 0)
            {
                ModelState.AddModelError("Message", "Please add different document name");
                return BadRequest(ModelState);
            }
            return Ok(_econsentSetupRepository.UpdateEconsentSetup(econsentSetupDto));
        }

        [HttpGet]
        [Route("GetEconsentDocumentDropDown/{projectId}")]
        public IActionResult GetEconsentDocumentDropDown(int projectId)
        {
            // use in econsent section reference page for document drop down data
            return Ok(_econsentSetupRepository.GetEconsentDocumentDropDown(projectId));
        }

        [HttpGet]
        [Route("GetPatientStatusDropDown")]
        public IActionResult GetPatientStatusDropDown()
        {
            // patient status dropdown use in econsent setup add/edit popup
            return Ok(_econsentSetupRepository.GetPatientStatusDropDown());
        }
    }
}
