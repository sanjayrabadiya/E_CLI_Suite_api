using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.ProjectRight;
using GSC.Respository.Configuration;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.ProjectRight
{
    [Route("api/[controller]")]
    public class ProjectDocumentReviewController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IProjectDocumentReviewRepository _projectDocumentReviewRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public ProjectDocumentReviewController(IProjectDocumentReviewRepository projectDocumentReviewRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository,
            IUploadSettingRepository uploadSettingRepository)
        {
            _projectDocumentReviewRepository = projectDocumentReviewRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _projectRightRepository = projectRightRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_projectDocumentReviewRepository.GetProjectDashboard());
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var dataObj = _projectDocumentReviewRepository.GetProjectDashboardbyId(id);
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            dataObj.ProjectList.ForEach(t => t.DocumentPath = documentUrl + t.DocumentPath);
            return Ok(dataObj);
        }

        [HttpPut]
        public IActionResult DocumentReview([FromBody] ProjectDocumentReviewDto projectDocumentReviewDto)
        {
            var projectid = 0;

            foreach (var id in projectDocumentReviewDto.Ids)
            {
                projectDocumentReviewDto.Id = id;

                if (projectDocumentReviewDto.Id <= 0) return BadRequest();

                if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

                var result = _projectDocumentReviewRepository.Find(projectDocumentReviewDto.Id);

                var resultList = _projectDocumentReviewRepository.FindBy(t =>
                        t.UserId == result.UserId && t.ProjectDocumentId == result.ProjectDocumentId &&
                        t.Id != result.Id)
                    .ToList();

                if (result != null)
                {
                    projectid = result.ProjectId;
                    result.IsReview = true;
                    result.ReviewNote = projectDocumentReviewDto.ReviewNote;
                    result.ReviewDate = _jwtTokenAccesser.GetClientDate();
                    result.TrainingType = projectDocumentReviewDto.TrainingType;
                    result.TrainerId = projectDocumentReviewDto.TrainerId;
                    result.TrainingDuration = projectDocumentReviewDto.TrainingDuration;
                    _projectDocumentReviewRepository.Update(result);
                    _uow.Save();
                }

                foreach (var review in resultList)
                {
                    review.IsReview = true;
                    review.ReviewNote = projectDocumentReviewDto.ReviewNote;
                    review.ReviewDate = _jwtTokenAccesser.GetClientDate();
                    review.TrainingType = projectDocumentReviewDto.TrainingType;
                    review.TrainerId = projectDocumentReviewDto.TrainerId;
                    review.TrainingDuration = projectDocumentReviewDto.TrainingDuration;

                    var projectdocumentreview = _mapper.Map<ProjectDocumentReview>(review);

                    _projectDocumentReviewRepository.Update(projectdocumentreview);
                    _uow.Save();
                }
            }

            _projectRightRepository.UpdateIsReviewDone(projectid);
            return Ok(projectDocumentReviewDto.Id);
        }

        [HttpGet]
        [Route("GetProjectDropDownProjectRight")]
        public IActionResult GetProjectDropDownProjectRight()
        {
            return Ok(_projectDocumentReviewRepository.GetProjectDropDownProjectRight());
        }

        [HttpGet]
        [Route("GetParentProjectDropDownProjectRight")]
        public IActionResult GetParentProjectDropDownProjectRight()
        {
            return Ok(_projectDocumentReviewRepository.GetParentProjectDropDownProjectRight());
        }

        [HttpGet]
        [Route("GetChildProjectDropDownProjectRight/{ParentProjectId}")]
        public IActionResult GetChildProjectDropDownProjectRight(int ParentProjectId)
        {
            return Ok(_projectDocumentReviewRepository.GetChildProjectDropDownProjectRight(ParentProjectId));
        }

        [HttpGet]
        [Route("GetCompleteTrainingDashboard/{id}")]
        public IActionResult GetCompleteTrainingDashboard(int id)
        {
            var dataObj = _projectDocumentReviewRepository.GetCompleteTrainingDashboard(id);
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            dataObj.ProjectList.ForEach(t => t.DocumentPath = documentUrl + t.DocumentPath);
            return Ok(dataObj);
        }

        [HttpGet]
        [Route("GetPendingProjectTrainingCount/{id}")]
        public IActionResult GetPendingProjectTrainingCount(int id)
        {
            return Ok(_projectDocumentReviewRepository.GetPendingProjectTrainingCount(id));
        }


        [HttpGet("GetDashboardList")]
        public IActionResult GetDashboardList()
        {
            return Ok(_projectDocumentReviewRepository.GetDashboardProjectList());
        }      
    }
}