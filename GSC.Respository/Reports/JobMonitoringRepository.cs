using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Report;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Reports
{
    public class JobMonitoringRepository : GenericRespository<JobMonitoring, GscContext>, IJobMonitoringRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public JobMonitoringRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IUploadSettingRepository uploadSettingRepository)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public List<JobMonitoringDto> JobMonitoringList()
        {
            //var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var result = All.Select(x => new JobMonitoringDto
            {
                Id = x.Id,
                JobName = x.JobName,
                JobDescription = x.JobDescription,
                JobType = x.JobType,
                JobStatus = x.JobStatus,
                SubmittedBy = x.SubmittedBy,
                SubmittedTime = x.SubmittedTime.UtcDateTime(),
                CompletedTime = x.CompletedTime.UtcDateTime(),
                JobDetails = x.JobDetails,
                FolderPath = x.FolderPath,
                FolderName = x.FolderName,
                JobNamestr = x.JobName.GetDescription(),                
                JobDescriptionstr = Context.Project.Where(z => z.Id == x.JobDescription).FirstOrDefault().ProjectCode + " - " +
                                        Context.Project.Where(z => z.Id == x.JobDescription).FirstOrDefault().ProjectName,
                JobTypestr = x.JobType.GetDescription(),
                JobStatusstr = x.JobStatus.GetDescription(),
                SubmittedBystr = Context.Users.Where(y => y.Id == x.SubmittedBy).FirstOrDefault().UserName,
                JobDetailsstr = x.JobDetails.GetDescription(),
                FullPath = !(string.IsNullOrEmpty(x.FolderName) && (string.IsNullOrEmpty(x.FolderName))) ? System.IO.Path.Combine(x.FolderPath, x.FolderName) : "",
            }).Where(t=>t.SubmittedBy == _jwtTokenAccesser.UserId).OrderByDescending(t=>t.Id).ToList();

            return result;
        }

    }
}