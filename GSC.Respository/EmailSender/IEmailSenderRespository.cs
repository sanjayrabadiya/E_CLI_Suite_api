using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.Configuration;
using System.Threading.Tasks;

namespace GSC.Respository.EmailSender
{
    public interface IEmailSenderRespository : IGenericRepository<EmailTemplate>
    {
        void SendRegisterEMail(string toMail, string password, string userName,string companyName);
        void SendChangePasswordEMail(string toMail, string password, string userName,string companyName);
        //void SendForgotPasswordEMail(string toMail, string password, string userName);
        Task SendForgotPasswordEMail(string toMail,string mobile, string password, string userName,string companyName);

        void SendPdfGeneratedEMail(string toMail, string userName, string projectName, string linkOfPdf);
        void SendApproverEmailOfArtificate(string toMail, string userName, string documentName, string ArtificateName, string ProjectName);
        void SendEmailOfReview(string toMail, string userName, string documentName, string ArtificateName, string ProjectName);
        void SendEmailOfSendBack(string toMail, string userName, string documentName, string ArtificateName, string ProjectName);
         void SendEmailOfStartEconsent(string toMail, string userName, string documentName, string ProjectName);
        void SendEmailOfPatientReviewedPDFtoPatient(string toMail, string userName, string documentName, string ProjectName, string filepath);
        void SendEmailOfPatientReviewedPDFtoInvestigator(string toMail, string userName, string documentName, string ProjectName, string patientName, string filepath);
        void SendEmailOfInvestigatorApprovedPDFtoPatient(string toMail, string userName, string documentName, string ProjectName, string filepath);
        void SendEmailOfEconsentDocumentuploaded(string toMail, string userName, string documentName, string ProjectName);
        void SendDBDSGeneratedEMail(string toMail, string userName, string projectName, string linkOfPdf);
        Task SendEmailOfScreenedPatient(string toMail, string patientName, string userName, string password, string ProjectName,string mobile,int sendtype,bool isSendEmail,bool isSendSMS);
        Task SendAdverseEventAlertEMailtoInvestigator(string toMail, string mobile, string userName, string projectName, string patientname,string reportdate);
        void SendOfflineChatNotification(string toMail, string userName);
        void SendWithDrawEmail(string toMail, string userName, string documentName, string ProjectName, string patientName, string filepath);
        public void SendEmailOfRejectedDocumenttoPatient(string toMail, string userName, string documentName, string ProjectName, string filepath);
        void SendEmailOfTemplateReview(string toMail, string userName, string activity, string template, string project);
        void SendEmailOfTemplateSendBack(string toMail, string userName, string activity, string template, string ProjectName);
        void SendLabManagementAbnormalEMail(string toMail, LabManagementEmail email);
        void SendEmailOfTemplateApprove(string toMail, string userName, string activity, string template, string project);
        void SendApproveVerificationEmail(string toMail);
        void ApproveByApproverVerificationEmail(string toMail);
        void RejectByApproverVerificationEmail(string toMail);
    }
}