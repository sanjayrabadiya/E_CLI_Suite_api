using GSC.Common.GenericRespository;
using GSC.Data.Entities.Configuration;

namespace GSC.Respository.EmailSender
{
    public interface IEmailSenderRespository : IGenericRepository<EmailTemplate>
    {
        void SendRegisterEMail(string toMail, string password, string userName);
        void SendChangePasswordEMail(string toMail, string password, string userName);
        void SendForgotPasswordEMail(string toMail, string password, string userName);

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
        void SendEmailOfScreenedPatient(string toMail, string patientName, string userName, string password, string ProjectName,string mobile);
    }
}