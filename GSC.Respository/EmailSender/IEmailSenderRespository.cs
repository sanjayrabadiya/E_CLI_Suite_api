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
    }
}