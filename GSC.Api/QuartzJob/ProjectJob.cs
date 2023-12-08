using GSC.Respository.Etmf;
using Quartz;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GSC.Api.QuartzJob
{
    public class ProjectJob : IJob
    {
        IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        IProjectSubSecArtificateDocumentReviewRepository _projectSubSecArtificateDocumentReviewRepository;
        IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;
        IProjectSubSecArtificateDocumentApproverRepository _projectSubSecArtificateDocumentApproverRepository;
        public ProjectJob(IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
            IProjectSubSecArtificateDocumentReviewRepository projectSubSecArtificateDocumentReviewRepository,
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IProjectSubSecArtificateDocumentApproverRepository projectSubSecArtificateDocumentApproverRepository)
        {
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _projectSubSecArtificateDocumentReviewRepository = projectSubSecArtificateDocumentReviewRepository;
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _projectSubSecArtificateDocumentApproverRepository = projectSubSecArtificateDocumentApproverRepository;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await SendEtmfEmail();
        }

        //public void logfile(DateTime time)
        //{
        //    string path = "D:\\log\\sample.txt";
        //    using (StreamWriter writer = new StreamWriter(path, true))
        //    {
        //        writer.WriteLine(time);
        //        writer.Close();
        //    }
        //}

        public async Task SendEtmfEmail()
        {
            try
            {
                await _projectWorkplaceArtificateDocumentReviewRepository.SendDueReviewEmail();
                await _projectSubSecArtificateDocumentReviewRepository.SendDueReviewEmail();
                await _projectArtificateDocumentApproverRepository.SendDueApproveEmail();
                await _projectSubSecArtificateDocumentApproverRepository.SendDueApproveEmail();
            }
            catch(Exception ex)
            {

            }
        }
    }
}
