﻿using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Etmf;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.UserMgt;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectArtificateDocumentApproverRepository : GenericRespository<ProjectArtificateDocumentApprover, GscContext>, IProjectArtificateDocumentApproverRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IProjectWorkplaceArtificateRepository _projectWorkplaceArtificateRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public ProjectArtificateDocumentApproverRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser, IMapper mapper,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IProjectWorkplaceArtificateRepository projectWorkplaceArtificateRepository,
            IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
            IEmailSenderRespository emailSenderRespository,
            IUserRepository userRepository,
            IUploadSettingRepository uploadSettingRepository)
           : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
            _mapper = mapper;
            _projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
            _projectWorkplaceArtificateRepository = projectWorkplaceArtificateRepository;
            _etmfArtificateMasterLbraryRepository = etmfArtificateMasterLbraryRepository;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public List<ProjectArtificateDocumentReviewDto> UserNameForApproval(int Id)
        {
            var users = Context.Users.Where(x => x.DeletedDate == null && x.Id != _jwtTokenAccesser.UserId).Select(c => new ProjectArtificateDocumentReviewDto
            {
                UserId = c.Id,
                Name = c.UserName,
                IsSelected = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.Id && b.DeletedDate == null 
                && (b.IsApproved == true || b.IsApproved == null)),
            }).Where(x => x.IsSelected == false).ToList();

            return users;
        }

        public void SendMailForApprover(ProjectArtificateDocumentApproverDto ProjectArtificateDocumentApproverDto)
        {
            var project = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                   .ThenInclude(x => x.ProjectWorkplaceArtificate)
                   .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                   .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace)
                   .Where(x => x.ProjectWorkplaceArtificatedDocumentId == ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId).FirstOrDefault();
            var ProjectName = project.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectName;

            var document = _projectWorkplaceArtificatedocumentRepository.Find(ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId);
            var artificate = _projectWorkplaceArtificateRepository.FindByInclude(x => x.Id == document.ProjectWorkplaceArtificateId, x => x.EtmfArtificateMasterLbrary).FirstOrDefault();
            var user = _userRepository.Find(ProjectArtificateDocumentApproverDto.UserId);

            _emailSenderRespository.SendApproverEmailOfArtificate(user.Email, user.UserName, document.DocumentName, artificate.EtmfArtificateMasterLbrary.ArtificateName, ProjectName);
        }

        public List<DashboardDto> GetEtmfMyTaskList(int ProjectId)
        {
            var result = All.Include(t => t.ProjectWorkplaceArtificatedDocument)
                .ThenInclude(x => x.ProjectWorkplaceArtificate)
                .ThenInclude(x => x.ProjectWorkplaceSection).ThenInclude(x => x.ProjectWorkPlaceZone)
                .ThenInclude(x => x.ProjectWorkplaceDetail).ThenInclude(x => x.ProjectWorkplace)
                .Where(x => x.UserId == _jwtTokenAccesser.UserId && x.IsApproved == null && x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection
                .ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == ProjectId)
                .Select(s => new DashboardDto
                {
                    Id = s.Id,
                    TaskInformation = s.ProjectWorkplaceArtificatedDocument.DocumentName + " for " +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.EtmfArtificateMasterLbrary.ArtificateName + "_" +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.EtmfSectionMasterLibrary.SectionName + "_" +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.EtmfZoneMasterLibrary.ZonName + "_" +
                    s.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkplaceSection.ProjectWorkPlaceZone.ProjectWorkplaceDetail.ProjectWorkplace.Project.ProjectName
                    + " Pending approve from your side",
                    //ExtraData = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), FolderType.ProjectWorksplace.GetDescription(), s.ProjectWorkplaceArtificatedDocument.DocPath, s.ProjectWorkplaceArtificatedDocument.DocumentName),
                    ExtraData = s.ProjectWorkplaceArtificatedDocumentId
                }).OrderByDescending(x => x.Id).ToList();

            return result;
        }
    }
}