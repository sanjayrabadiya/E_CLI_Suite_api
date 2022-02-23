using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Shared;
using GSC.Shared.Email;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.EmailSender
{
    public class EmailSenderRespository : GenericRespository<EmailTemplate>, IEmailSenderRespository
    {
        private readonly IGSCContext _context;
        private readonly IEmailService _emailService;
        private readonly ISMSSettingRepository _iSMSSettingRepository;
        private readonly HttpClient _httpClient;

        public EmailSenderRespository(IGSCContext context,
            HttpClient httpClient,
            IJwtTokenAccesser jwtTokenAccesser,
            IEmailService emailService,
            ISMSSettingRepository iSMSSettingRepository)
            : base(context)
        {
            _emailService = emailService;
            _context = context;
            _httpClient = httpClient;
            _iSMSSettingRepository = iSMSSettingRepository;
        }

        public void SendRegisterEMail(string toMail, string password, string userName, string companyName)
        {
            var emailMessage = ConfigureEmail("empreg", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBody(emailMessage.MessageBody, userName, password, companyName);
            _emailService.SendMail(emailMessage);
        }


        public void SendChangePasswordEMail(string toMail, string password, string userName, string companyName)
        {
            var emailMessage = ConfigureEmail("resetpass", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBody(emailMessage.MessageBody, userName, password, companyName);
            _emailService.SendMail(emailMessage);
        }

        public async Task SendForgotPasswordEMail(string toMail, string mobile, string password, string userName, string companyName)
        //void SendForgotPasswordEMail(string toMail, string password, string userName)
        {
            var emailMessage = ConfigureEmail("forgotpass", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBody(emailMessage.MessageBody, userName, password, companyName);
            if (toMail != null && toMail != "")
            {
                _emailService.SendMail(emailMessage);
            }
            if (mobile != null && mobile != "")
            {
                await SendSMS(mobile, emailMessage.MessageBody, emailMessage.DLTTemplateId);
            }

        }

        public void SendPdfGeneratedEMail(string toMail, string userName, string projectName, string linkOfPdf)
        {
            var emailMessage = ConfigureEmail("pdfcompleted", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForPDF(emailMessage.MessageBody, userName, projectName, linkOfPdf);
            _emailService.SendMail(emailMessage);
        }

        public void SendApproverEmailOfArtificate(string toMail, string userName, string documentName, string ArtificateName, string ProjectName)
        {
            var emailMessage = ConfigureEmail("ArtificateApprover", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForArtificate(emailMessage.MessageBody, userName, documentName, ArtificateName, ProjectName);
            _emailService.SendMail(emailMessage);
        }

        public void SendEmailOfReview(string toMail, string userName, string documentName, string ArtificateName, string ProjectName)
        {
            var emailMessage = ConfigureEmail("ArtificateReview", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForArtificate(emailMessage.MessageBody, userName, documentName, ArtificateName, ProjectName);
            _emailService.SendMail(emailMessage);
        }

        public void SendEmailOfTemplateReview(string toMail, string userName, string activity, string template, string project)
        {
            var emailMessage = ConfigureEmail("TemplateReview", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForTemplate(emailMessage.MessageBody, userName, activity, template, project);
            _emailService.SendMail(emailMessage);
        }

        public void SendEmailOfTemplateApprove(string toMail, string userName, string activity, string template, string project)
        {
            var emailMessage = ConfigureEmail("TemplateApprove", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForTemplate(emailMessage.MessageBody, userName, activity, template, project);
            _emailService.SendMail(emailMessage);
        }

        public void SendEmailOfSendBack(string toMail, string userName, string documentName, string ArtificateName, string ProjectName)
        {
            var emailMessage = ConfigureEmail("ArtificateSendBack", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForArtificate(emailMessage.MessageBody, userName, documentName, ArtificateName, ProjectName);
            _emailService.SendMail(emailMessage);
        }

        public void SendEmailOfTemplateSendBack(string toMail, string userName, string activity, string template, string ProjectName)
        {
            var emailMessage = ConfigureEmail("TemplateSendBack", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForArtificate(emailMessage.MessageBody, userName, activity, template, ProjectName);
            _emailService.SendMail(emailMessage);
        }

        public void SendEmailOfStartEconsent(string toMail, string userName, string documentName, string ProjectName)
        {
            var emailMessage = ConfigureEmail("StartEconsent", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForStartEconsent(emailMessage.MessageBody, userName, documentName, ProjectName);
            _emailService.SendMail(emailMessage);
        }

        public void SendEmailOfPatientReviewedPDFtoPatient(string toMail, string userName, string documentName, string ProjectName, string filepath)
        {
            var emailMessage = ConfigureEmail("PatientSignedDocumentToPatient", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForPatientReviewedPDFtoPatient(emailMessage.MessageBody, userName, documentName, ProjectName);
            emailMessage.Subject = ReplaceSubjectForPatientReviewedPDFtoPatient(emailMessage.Subject, documentName);
            emailMessage.Attachments.Add(new Attachment(filepath));
            _emailService.SendMail(emailMessage);
        }

        public void SendEmailOfPatientReviewedPDFtoInvestigator(string toMail, string userName, string documentName, string ProjectName, string patientName, string filepath)
        {
            var emailMessage = ConfigureEmail("PatientSignedDocumentToInvestigator", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForPatientReviewedPDFtoInvestigator(emailMessage.MessageBody, userName, documentName, ProjectName, patientName);
            emailMessage.Subject = ReplaceSubjectForPatientReviewedPDFtoInvestigator(emailMessage.Subject, documentName, patientName);
            emailMessage.Attachments.Add(new Attachment(filepath));
            _emailService.SendMail(emailMessage);
        }

        public void SendEmailOfRejectedDocumenttoPatient(string toMail, string userName, string documentName, string ProjectName, string filepath)
        {
            var emailMessage = ConfigureEmail("RejectedSignedDocumentToPatient", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForPatientReviewedPDFtoPatient(emailMessage.MessageBody, userName, documentName, ProjectName);
            emailMessage.Subject = ReplaceSubjectForPatientReviewedPDFtoPatient(emailMessage.Subject, documentName);
            emailMessage.Attachments.Add(new Attachment(filepath));
            _emailService.SendMail(emailMessage);
        }

        public void SendEmailOfInvestigatorApprovedPDFtoPatient(string toMail, string userName, string documentName, string ProjectName, string filepath)
        {
            var emailMessage = ConfigureEmail("InvestigatorSignedDocumentToPatient", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForPatientReviewedPDFtoPatient(emailMessage.MessageBody, userName, documentName, ProjectName);
            emailMessage.Subject = ReplaceSubjectForPatientReviewedPDFtoPatient(emailMessage.Subject, documentName);
            emailMessage.Attachments.Add(new Attachment(filepath));
            _emailService.SendMail(emailMessage);
        }

        public void SendEmailOfEconsentDocumentuploaded(string toMail, string userName, string documentName, string ProjectName)
        {
            var emailMessage = ConfigureEmail("EConsentDocumentUploaded", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForPatientReviewedPDFtoPatient(emailMessage.MessageBody, userName, documentName, ProjectName);
            emailMessage.Subject = ReplaceSubjectForPatientReviewedPDFtoPatient(emailMessage.Subject, documentName);
            _emailService.SendMail(emailMessage);
        }

        public async Task SendEmailOfScreenedPatient(string toMail, string patientName, string userName, string password, string ProjectName, string mobile, int sendtype, bool isSendEmail, bool isSendSMS)
        {
            if (isSendEmail == true || isSendSMS == true)
            {
                var emailMessage = ConfigureEmail("PatientScreened", userName);
                emailMessage.SendTo = toMail;
                emailMessage.MessageBody = ReplaceBodyForPatientScreened(emailMessage.MessageBody, userName, patientName, ProjectName, password);
                emailMessage.Subject = ReplaceSubjectForPatientScreened(emailMessage.Subject, ProjectName);
                if (toMail != null && toMail != "" && (sendtype == 1 || sendtype == 2))
                {
                    if (isSendEmail == true)
                        _emailService.SendMail(emailMessage);
                }
                if (mobile != "" && (sendtype == 0 || sendtype == 2))
                {
                    if (isSendSMS == true)
                        await SendSMS(mobile, emailMessage.MessageBody, emailMessage.DLTTemplateId);
                }
            }
        }

        public async Task SendAdverseEventAlertEMailtoInvestigator(string toMail, string mobile, string userName, string projectName, string patientname, string reportdate)
        {
            var emailMessage = ConfigureEmail("AdverseEventAlerttoInvestigator", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForAdverseEventAlerttoInvestigator(emailMessage.MessageBody, userName, patientname, projectName, reportdate);
            emailMessage.Subject = ReplaceSubjectForAdverseEventAlerttoInvestigator(emailMessage.Subject, patientname);
            _emailService.SendMail(emailMessage);
            await SendSMS(mobile, emailMessage.MessageBody, emailMessage.DLTTemplateId);
        }

        public void SendOfflineChatNotification(string toMail, string userName)
        {
            var emailMessage = ConfigureEmail("empreg", userName);
            emailMessage.SendTo = toMail;
            //emailMessage.MessageBody = ReplaceBody(emailMessage.MessageBody, userName, password);
            emailMessage.MessageBody = "Message offline alert";
            _emailService.SendMail(emailMessage);
        }
        public void SendWithDrawEmail(string toMail, string userName, string documentName, string ProjectName, string patientName, string filepath)
        {
            var emailMessage = ConfigureEmail("WithdrawDocumentpatient", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForPatientWithDrawDocument(emailMessage.MessageBody, userName, documentName, ProjectName, patientName);
            emailMessage.Subject = ReplaceSubjectForPatientWithDraw(emailMessage.Subject, documentName, patientName);
            emailMessage.Attachments.Add(new Attachment(filepath));
            _emailService.SendMail(emailMessage);
        }

        private string ReplaceBodyForPatientWithDrawDocument(string body, string userName, string documentName, string projectName, string patientName)
        {
            body = Regex.Replace(body, "##name##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##document##", documentName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>documentName</strong>##", "<strong>" + documentName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##projectName##", projectName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>projectName</strong>##", "<strong>" + projectName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##patientname##", patientName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>patientname</strong>##", "<strong>" + patientName + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceSubjectForPatientWithDraw(string body, string documentName, string patientName)
        {
            body = Regex.Replace(body, "##document##", documentName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>documentName</strong>##", "<strong>" + documentName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##patientname##", patientName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>patientname</strong>##", "<strong>" + patientName + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        public async Task SendSMS(string mobile, string messagebody, string? DLTTemplateId)
        {
            var smstemplate = messagebody;//emailMessage.MessageBody;
            smstemplate = smstemplate.Replace("<p>", "");
            smstemplate = smstemplate.Replace("</p>", "\r\n");
            smstemplate = smstemplate.Replace("<strong>", "");
            smstemplate = smstemplate.Replace("</strong>", "");
            smstemplate = Regex.Replace(smstemplate, "<.*?>", String.Empty);
            var smssetting = _iSMSSettingRepository.FindBy(x => x.KeyName == "msg91").ToList().FirstOrDefault();
            var url = smssetting.SMSurl;
            url = url.Replace("##AuthKey##", smssetting.AuthKey);
            url = url.Replace("##Mobile##", "91" + mobile);
            url = url.Replace("##senderid##", smssetting.SenderId);
            url = url.Replace("##route##", "4");
            url = url.Replace("##message##", Uri.EscapeDataString(smstemplate));//emailMessage.MessageBody
            if (DLTTemplateId != null && DLTTemplateId != "")
                url = url.Replace("##DLTTemplateId##", DLTTemplateId);
            else
                url = url.Replace("&DLT_TE_ID=##DLTTemplateId##", "");
            await HttpService.Get(_httpClient, url, null);
            //var responseresult = _aPICall.Get(url);
        }

        private string ReplaceBodyForPDF(string body, string userName, string project, string linkOfPdf)
        {
            body = Regex.Replace(body, "##name##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##projectname##", project, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>project</strong>##", "<strong>" + project + "</strong>",
                RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##link##", linkOfPdf, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>linkOfPdf</strong>##", "<strong>" + linkOfPdf + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }


        private EmailMessage ConfigureEmail(string keyName, string userName)
        {
            //        var user = _context.Users.Where(x => x.UserName == userName && x.DeletedDate == null).FirstOrDefault();
            var result = All.Include(x => x.EmailSetting).FirstOrDefault(x =>
               x.DeletedDate == null && x.KeyName == keyName);
            var emailMessage = new EmailMessage();

            if (result != null)
            {
                emailMessage.SendFrom = result.EmailSetting.EmailFrom;
                emailMessage.Subject = result.SubjectName;
                emailMessage.MessageBody = result.Body;
                emailMessage.Cc = result.Bcc;
                emailMessage.Bcc = result.Bcc;
                emailMessage.IsBodyHtml = true;
                emailMessage.Attachments = null;
                emailMessage.EmailFrom = result.EmailSetting.EmailFrom;
                emailMessage.PortName = result.EmailSetting.PortName;
                emailMessage.DomainName = result.EmailSetting.DomainName;
                emailMessage.EmailPassword = result.EmailSetting.EmailPassword;
                emailMessage.MailSsl = result.EmailSetting.MailSsl;
                emailMessage.DLTTemplateId = result.DLTTemplateId;
            }

            return emailMessage;
        }

        public void SendDBDSGeneratedEMail(string toMail, string userName, string projectName, string linkOfPdf)
        {
            var emailMessage = ConfigureEmail("DBDS", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForPDF(emailMessage.MessageBody, userName, projectName, linkOfPdf);
            _emailService.SendMail(emailMessage);
        }

        private string ReplaceBody(string body, string userName, string password, string companyName)
        {
            body = Regex.Replace(body, "##name##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##password##", password, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>password</strong>##", "<strong>" + password + "</strong>",
                RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>company</strong>##", "<strong>" + companyName + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceBodyForArtificate(string body, string userName, string documentName, string artificateName, string projectName)
        {
            body = Regex.Replace(body, "##name##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##document##", documentName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>documentName</strong>##", "<strong>" + documentName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##artificateName##", artificateName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>artificateName</strong>##", "<strong>" + artificateName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##projectName##", projectName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>projectName</strong>##", "<strong>" + projectName + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceBodyForTemplate(string body, string userName, string activityName, string formName, string projectName)
        {
            body = Regex.Replace(body, "##name##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##activityName##", activityName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>activityName</strong>##", "<strong>" + activityName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##formName##", formName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>formName</strong>##", "<strong>" + formName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##projectName##", projectName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>projectName</strong>##", "<strong>" + projectName + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceBodyForStartEconsent(string body, string userName, string documentName, string projectName)
        {
            body = Regex.Replace(body, "##name##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##document##", documentName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>documentName</strong>##", "<strong>" + documentName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##projectName##", projectName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>projectName</strong>##", "<strong>" + projectName + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceBodyForPatientReviewedPDFtoPatient(string body, string userName, string documentName, string projectName)
        {
            body = Regex.Replace(body, "##name##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##document##", documentName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>documentName</strong>##", "<strong>" + documentName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##projectName##", projectName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>projectName</strong>##", "<strong>" + projectName + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceSubjectForPatientReviewedPDFtoPatient(string body, string documentName)
        {
            body = Regex.Replace(body, "##document##", documentName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>documentName</strong>##", "<strong>" + documentName + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceBodyForPatientReviewedPDFtoInvestigator(string body, string userName, string documentName, string projectName, string patientName)
        {
            body = Regex.Replace(body, "##name##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##document##", documentName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>documentName</strong>##", "<strong>" + documentName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##projectName##", projectName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>projectName</strong>##", "<strong>" + projectName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##patientname##", patientName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>patientname</strong>##", "<strong>" + patientName + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceSubjectForPatientReviewedPDFtoInvestigator(string body, string documentName, string patientName)
        {
            body = Regex.Replace(body, "##document##", documentName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>documentName</strong>##", "<strong>" + documentName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##patientname##", patientName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>patientname</strong>##", "<strong>" + patientName + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceSubjectForPatientScreened(string body, string studyName)
        {
            body = Regex.Replace(body, "##studyName##", studyName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>studyName</strong>##", "<strong>" + studyName + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceBodyForPatientScreened(string body, string userName, string patientname, string projectName, string password)
        {
            body = Regex.Replace(body, "##username##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##patientname##", patientname, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>patientname</strong>##", "<strong>" + patientname + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##studyName##", projectName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>studyName</strong>##", "<strong>" + projectName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##username##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##password##", password, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>password</strong>##", "<strong>" + password + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceBodyForAdverseEventAlerttoInvestigator(string body, string userName, string patientname, string projectName, string reportdate)
        {
            body = Regex.Replace(body, "##name##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##patientname##", patientname, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>patientname</strong>##", "<strong>" + patientname + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##studyname##", projectName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>studyName</strong>##", "<strong>" + projectName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##username##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##reportdate##", reportdate, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>reportdate</strong>##", "<strong>" + reportdate + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceSubjectForAdverseEventAlerttoInvestigator(string body, string patientname)
        {
            body = Regex.Replace(body, "##patientname##", patientname, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>patientname</strong>##", "<strong>" + patientname + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }

        public void SendLabManagementAbnormalEMail(string toMail, LabManagementEmail email)
        {
            var emailMessage = ConfigureEmail("LabManagementAbnormalEmail", "");
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForLabManagementEmail(emailMessage.MessageBody, email);
            emailMessage.Subject = ReplaceSubjectForLabManagementEmail(emailMessage.Subject, email.ScreeningNumber);
            _emailService.SendMail(emailMessage);
        }

        private string ReplaceBodyForLabManagementEmail(string body, LabManagementEmail email)
        {
            body = Regex.Replace(body, "##studyCode##", email.StudyCode, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##siteCode##", email.SiteCode, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##visit##", email.Visit, RegexOptions.IgnoreCase);

            string temp = "<table style='width: 100 %;'><tr><th style='border: 1px solid black;'>Test Name</th><th style='border: 1px solid black;'>Result</th><th style='border: 1px solid black;'>Low Range</th><th style='border: 1px solid black;'>High Range</th><th style='border: 1px solid black;'>Flag</th></tr>";

            foreach (var item in email.LabManagementEmailDetail)
            {
                temp += "<tr><td style='border: 1px solid black;'>" + item.TestName + "</td><td style='border: 1px solid black;'>" + item.Result + "</td><td style='border: 1px solid black;'> " + item.ReferenceRangeLow + "</td><td style='border: 1px solid black;'>" + item.ReferenceRangeHigh + "</td><td style='border: 1px solid black;'>" + item.AbnoramalFlag + "</td></tr>";
            }
            temp += "</table>";
            body = Regex.Replace(body, "##appendBody##", temp, RegexOptions.IgnoreCase);
            return body;
        }

        private string ReplaceSubjectForLabManagementEmail(string subject, string screeningId)
        {
            subject = Regex.Replace(subject, "##screeningId##", screeningId, RegexOptions.IgnoreCase);
            return subject;
        }


        public void SendApproveVerificationEmail(string toMail)
        {
            var emailMessage = ConfigureEmail("SendForVerification", "");
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = "Check Verification you have got an verification template for verified.";//ReplaceBodyForLabManagementEmail(emailMessage.MessageBody, email);
            emailMessage.Subject ="Verification template received." ;//ReplaceSubjectForLabManagementEmail(emailMessage.Subject, email.ScreeningNumber);
            _emailService.SendMail(emailMessage);
        }

        public void RejectByApproverVerificationEmail(string toMail)
        {
            var emailMessage = ConfigureEmail("SendBackByApprover", "");
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = "Check Verification you have got an verification template for verified.";//ReplaceBodyForLabManagementEmail(emailMessage.MessageBody, email);
            emailMessage.Subject = "Rejected Verification By Approver.";//ReplaceSubjectForLabManagementEmail(emailMessage.Subject, email.ScreeningNumber);
            _emailService.SendMail(emailMessage);
        }

        public void ApproveByApproverVerificationEmail(string toMail)
        {
            var emailMessage = ConfigureEmail("SendBackByApprover", "");
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = "Check Verification you have got an verification template for verified.";//ReplaceBodyForLabManagementEmail(emailMessage.MessageBody, email);
            emailMessage.Subject = "Approved Verification By Approver.";//ReplaceSubjectForLabManagementEmail(emailMessage.Subject, email.ScreeningNumber);
            _emailService.SendMail(emailMessage);
        }

    }
}