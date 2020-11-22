using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Medra;
using GSC.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Medra
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeddraCodingCommentController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMeddraCodingRepository _meddraCodingRepository;
        //private readonly IMeddraSocTermRepository _meddraSocTermRepository;
        private readonly IMeddraCodingAuditRepository _meddraCodingAuditRepository;
      //  private readonly IStudyScopingRepository _studyScopingRepository;
        private readonly IMeddraCodingCommentRepository _meddraCodingCommentRepository;
        private readonly IMeddraMdHierarchyRepository _meddraMdHierarchyRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public MeddraCodingCommentController(IMeddraCodingRepository meddraCodingRepository,
            IMeddraCodingCommentRepository meddraCodingCommentRepository,
            IMeddraCodingAuditRepository meddraCodingAuditRepository,
          //  IStudyScopingRepository studyScopingRepository,
          //  IMeddraSocTermRepository meddraSocTermRepository,
            IMeddraMdHierarchyRepository meddraMdHierarchyRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _meddraCodingRepository = meddraCodingRepository;
            _meddraCodingCommentRepository = meddraCodingCommentRepository;
            _meddraMdHierarchyRepository = meddraMdHierarchyRepository;
            _meddraCodingAuditRepository = meddraCodingAuditRepository;
        //    _studyScopingRepository = studyScopingRepository;
           // _meddraSocTermRepository = meddraSocTermRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpPost("AddComment")]
        public IActionResult AddComment([FromBody] MeddraCodingCommentDto meddraCodingCommentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var recodeData = _meddraCodingRepository.GetRecordForComment(meddraCodingCommentDto.ScreeningTemplateValueId);
            var oldHierarchy = new MeddraMdHierarchy();
            if (recodeData.MeddraSocTermId != null && recodeData.MeddraLowLevelTermId != null)
                oldHierarchy = _meddraMdHierarchyRepository.GetHierarchyData((int)recodeData.MeddraSocTermId, (int)recodeData.MeddraLowLevelTermId);
            var newHierarchy = _meddraMdHierarchyRepository.GetHierarchyData((int)meddraCodingCommentDto.MeddraSocTermId, (int)meddraCodingCommentDto.MeddraLowLevelTermId);
            if (recodeData != null)
            {
                recodeData.ModifiedDate = DateTime.Now;
                recodeData.ModifiedBy = _jwtTokenAccesser.UserId;
                recodeData.CreatedRole = _jwtTokenAccesser.RoleId;
                recodeData.MeddraLowLevelTermId = meddraCodingCommentDto.MeddraLowLevelTermId;
                recodeData.MeddraSocTermId = meddraCodingCommentDto.MeddraSocTermId;
                var medra = _mapper.Map<MeddraCoding>(recodeData);
                _meddraCodingRepository.Update(medra);
            }

            if (recodeData.MeddraSocTermId != null && recodeData.MeddraLowLevelTermId != null)
            {

                var meddraCodingValue = _meddraCodingCommentRepository.GetLatest(recodeData.Id);
                meddraCodingCommentDto.Value = newHierarchy.soc_name;
                meddraCodingCommentDto.OldValue = oldHierarchy.soc_name;
                meddraCodingCommentDto.OldPTCode = oldHierarchy.pt_code;
                meddraCodingCommentDto.NewPTCode = newHierarchy.pt_code;
                meddraCodingCommentDto.MeddraCodingId = recodeData.Id;
                if (meddraCodingValue == null)
                    meddraCodingCommentDto.CommentStatus = CommentStatus.SelfCorrection;
                else
                {
                    if (meddraCodingValue.CommentStatus == CommentStatus.Open && meddraCodingValue.UserRole == _jwtTokenAccesser.RoleId)
                        meddraCodingCommentDto.CommentStatus = CommentStatus.Closed;
                    else if (meddraCodingValue.CommentStatus == CommentStatus.Closed)
                        meddraCodingCommentDto.CommentStatus = CommentStatus.SelfCorrection;
                    else
                        meddraCodingCommentDto.CommentStatus = CommentStatus.Resolved;
                }
                var meddraCodingComment = _mapper.Map<MeddraCodingComment>(meddraCodingCommentDto);
                meddraCodingComment.UserRole = _jwtTokenAccesser.RoleId;
                _meddraCodingCommentRepository.Add(meddraCodingComment);
                _meddraCodingAuditRepository.SaveAudit(meddraCodingComment.ReasonOth, meddraCodingComment.MeddraCodingId, meddraCodingCommentDto.MeddraLowLevelTermId, meddraCodingCommentDto.MeddraSocTermId, "Comment Status " + meddraCodingCommentDto.CommentStatus.GetDescription() + " Added", meddraCodingCommentDto.ReasonId, meddraCodingCommentDto.ReasonOth);
            }
            else {
                _meddraCodingAuditRepository.SaveAudit(meddraCodingCommentDto.ReasonOth, recodeData.Id, meddraCodingCommentDto.MeddraLowLevelTermId, meddraCodingCommentDto.MeddraSocTermId, "Manual Coding for record data.", meddraCodingCommentDto.ReasonId, meddraCodingCommentDto.ReasonOth);
            }
            
            if (_uow.Save() <= 0)
                throw new Exception("Creating Value Query failed on save.");

            return Ok();
        }

        [HttpPost("AddNote")]
        public IActionResult AddNote([FromBody] MeddraCodingCommentDto meddraCodingCommentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var meddraCodingValue = _meddraCodingCommentRepository.GetLatest(meddraCodingCommentDto.MeddraCodingId);

            if (meddraCodingValue == null)
                meddraCodingCommentDto.CommentStatus = CommentStatus.Open;
            else
            {
                if (meddraCodingValue.CommentStatus == CommentStatus.Open && meddraCodingValue.UserRole == _jwtTokenAccesser.RoleId)
                    meddraCodingCommentDto.CommentStatus = CommentStatus.Closed;
                else if (meddraCodingValue.CommentStatus == CommentStatus.Answered || meddraCodingValue.CommentStatus == CommentStatus.Resolved)
                    meddraCodingCommentDto.CommentStatus = CommentStatus.Closed;
                else if (meddraCodingValue.CommentStatus == CommentStatus.Closed)
                    meddraCodingCommentDto.CommentStatus = CommentStatus.Open;
                else
                    meddraCodingCommentDto.CommentStatus = CommentStatus.Answered;
            }
            var meddraCodingComment = _mapper.Map<MeddraCodingComment>(meddraCodingCommentDto);
            meddraCodingComment.UserRole = _jwtTokenAccesser.RoleId;
            _meddraCodingCommentRepository.Add(meddraCodingComment);
            if (meddraCodingCommentDto.CommentStatus == CommentStatus.Open)
                _meddraCodingAuditRepository.SaveAudit(meddraCodingComment.Note, meddraCodingComment.MeddraCodingId, null, null, "Comment Status " + meddraCodingCommentDto.CommentStatus.GetDescription() + " Added", meddraCodingComment.ReasonId, meddraCodingComment.ReasonOth);
            else
                _meddraCodingAuditRepository.SaveAudit(meddraCodingComment.ReasonOth, meddraCodingComment.MeddraCodingId, null, null, "Comment Status " + meddraCodingCommentDto.CommentStatus.GetDescription() + " Added", meddraCodingComment.ReasonId, meddraCodingComment.ReasonOth);
            if (_uow.Save() <= 0)
                throw new Exception("Creating Value Query failed on save.");

            return Ok(meddraCodingComment.Id);
        }

        [HttpGet]
        [Route("GetDetails/{MeddraCodingId}")]
        public IActionResult GetDetails(int MeddraCodingId)
        {
            var comments = _meddraCodingCommentRepository.GetData(MeddraCodingId);
            return Ok(comments);
        }
    }
}