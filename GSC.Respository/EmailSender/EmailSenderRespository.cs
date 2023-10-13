using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Configuration;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Data.Entities.SupplyManagement;
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
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public EmailSenderRespository(IGSCContext context,
             IJwtTokenAccesser jwtTokenAccesser,
        HttpClient httpClient,
            IEmailService emailService,
            ISMSSettingRepository iSMSSettingRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
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

        public void SendApprovedEmailOfArtificate(string toMail, string userName, string documentName, string ArtificateName, string ProjectName)
        {
            var emailMessage = ConfigureEmail("ArtificateApproved", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForArtificate(emailMessage.MessageBody, userName, documentName, ArtificateName, ProjectName);
            _emailService.SendMail(emailMessage);
        }

        public void SendRejectedEmailOfArtificate(string toMail, string userName, string documentName, string ArtificateName, string ProjectName)
        {
            var emailMessage = ConfigureEmail("ArtificateRejected", userName);
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

        public void SendEmailOfReviewed(string toMail, string userName, string documentName, string ArtificateName, string ProjectName)
        {
            var emailMessage = ConfigureEmail("ArtificateReviewed", userName);
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

        public void SendEmailOfLARReviewedPDFtoInvestigator(string toMail, string userName, string documentName, string ProjectName, string patientName, string filepath)
        {
            var emailMessage = ConfigureEmail("LARSignedDocumentToInvestigator", userName);
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

        public void SendWithDrawEmailLAR(string toMail, string userName, string documentName, string ProjectName, string patientName, string filepath)
        {
            var emailMessage = ConfigureEmail("WithdrawDocumentpatientLAR", userName);
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
        public async Task SendSMSParticaularStudyWise(string mobile, string messagebody, string? DLTTemplateId)
        {
            var smstemplate = messagebody;//emailMessage.MessageBody;
            smstemplate = smstemplate.Replace("<p>", "");
            smstemplate = smstemplate.Replace("</p>", "\r\n");
            smstemplate = smstemplate.Replace("<strong>", "");
            smstemplate = smstemplate.Replace("</strong>", "");
            smstemplate = Regex.Replace(smstemplate, "<.*?>", String.Empty);
            
            var url = "https://api.msg91.com/api/sendhttp.php?authkey=##AuthKey##&mobiles=##Mobile##&country=91&message=##message##&sender=##senderid##&route=##route##&DLT_TE_ID=##DLTTemplateId##&dev_mode=1";
            url = url.Replace("##AuthKey##", "349610As7MvryDDp5fdb3d6bP1");
            url = url.Replace("##Mobile##", "91" + mobile);
            url = url.Replace("##senderid##", "425096");
            url = url.Replace("##route##", "1");
            url = url.Replace("##message##", Uri.EscapeDataString(smstemplate));//emailMessage.MessageBody
            if (DLTTemplateId != null && DLTTemplateId != "")
                url = url.Replace("##DLTTemplateId##", DLTTemplateId);
            else
                url = url.Replace("&DLT_TE_ID=##DLTTemplateId##", "");
            await HttpService.Get(_httpClient, url, null);
            
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


        public EmailMessage ConfigureEmail(string keyName, string userName)
        {
            //        var user = _context.Users.Where(x => x.UserName == userName && x.DeletedDate == null).FirstOrDefault();
            var result = All.AsNoTracking().Include(x => x.EmailSetting).FirstOrDefault(x =>
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
                emailMessage.Attachments =null;
                emailMessage.EmailFrom = result.EmailSetting.EmailFrom;
                emailMessage.PortName = result.EmailSetting.PortName;
                emailMessage.DomainName = result.EmailSetting.DomainName;
                emailMessage.EmailPassword = result.EmailSetting.EmailPassword;
                emailMessage.MailSsl = result.EmailSetting.MailSsl;
                emailMessage.DLTTemplateId = result.DLTTemplateId;
            }

            return emailMessage;
        }
        private EmailMessage ConfigureEmailLetters(string keyName, string userName,string fullPath)
        {
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
                Attachment file = new Attachment(fullPath);
                emailMessage.Attachments.Add(file);
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
            emailMessage.Subject = "Verification template received.";//ReplaceSubjectForLabManagementEmail(emailMessage.Subject, email.ScreeningNumber);
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
        //for variable email .prakash chauhan 14-05-2022
        public void SendVariableValueEmail(ScreeningTemplateValueDto screeningTemplateValueDto, string toMail, string Template)
        {
            var emailMessage = ConfigureEmailForVariable();
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBodyForVariableEmail(Template, screeningTemplateValueDto);
            emailMessage.Subject = "SAE Notification";
            _emailService.SendMail(emailMessage);
        }

        private string ReplaceBodyForVariableEmail(string body, ScreeningTemplateValueDto email)
        {
            int screeningentryid = 0;

            if (email.ScreeningEntryId == 0)
            {
                var data = _context.ScreeningTemplateValue.Include(x => x.ScreeningTemplate).ThenInclude(x => x.ScreeningVisit).Where(x => x.Id == email.Id).FirstOrDefault();
                if (data != null)
                {
                    if (data.ScreeningTemplate != null && data.ScreeningTemplate.ScreeningVisit != null)
                        screeningentryid = data.ScreeningTemplate.ScreeningVisit.ScreeningEntryId;
                }
            }
            else
            {
                screeningentryid = email.ScreeningEntryId;
            }
            var str = "<p>Study code: ##StudyCode##<br></p><p>Site code : ##SiteName##<br></p><p>Visit : ##VisitName##<br></p><p>Template : ##TemplateName##<br></p><p>Screening No : ##ScreeningNo##<br></p><p>SAE Term : ##VariableName##<br></p>";
            var screeningdata = _context.ScreeningEntry.Include(x => x.Randomization).ThenInclude(x => x.Project).ThenInclude(x => x.ManageSite).Include(x => x.Project).Where(x => x.Id == screeningentryid).FirstOrDefault();
            if (screeningdata != null)
            {
                str = Regex.Replace(str, "##ScreeningNo##", screeningdata.Randomization.ScreeningNumber, RegexOptions.IgnoreCase);

                if (screeningdata.Project != null)
                {
                    var projectdata = _context.Project.Where(x => x.Id == screeningdata.Project.ParentProjectId).FirstOrDefault();
                    if (projectdata != null)
                    {
                        str = Regex.Replace(str, "##StudyCode##", projectdata.ProjectCode, RegexOptions.IgnoreCase);

                        if (screeningdata.Project != null)
                        {
                            var managesite = _context.ManageSite.Where(x => x.DeletedDate == null && x.Id == screeningdata.Project.ManageSiteId).FirstOrDefault();
                            if (managesite != null)
                                str = Regex.Replace(str, "##SiteName##", screeningdata.Project.ProjectCode + " - " + managesite.SiteName, RegexOptions.IgnoreCase);
                        }
                    }
                }
            }
            var variable = _context.ProjectDesignVariable.Include(x => x.ProjectDesignTemplate).ThenInclude(x => x.ProjectDesignVisit).Where(x => x.Id == email.ProjectDesignVariableId).FirstOrDefault();

            if (variable != null)
            {
                str = Regex.Replace(str, "##VariableName##", variable.VariableName, RegexOptions.IgnoreCase);
                if (variable.ProjectDesignTemplate != null)
                {
                    str = Regex.Replace(str, "##TemplateName##", variable.ProjectDesignTemplate.TemplateName, RegexOptions.IgnoreCase);
                    if (variable.ProjectDesignTemplate.ProjectDesignVisit != null)
                    {
                        str = Regex.Replace(str, "##VisitName##", variable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName, RegexOptions.IgnoreCase);
                    }

                }

            }

            return str + body;
        }
        //for variable email .prakash chauhan 14-05-2022
        private EmailMessage ConfigureEmailForVariable()
        {

            var result = _context.EmailSetting.FirstOrDefault(x =>
               x.DeletedDate == null);
            var emailMessage = new EmailMessage();

            if (result != null)
            {
                emailMessage.SendFrom = result.EmailFrom;
                emailMessage.Subject = "";
                emailMessage.MessageBody = "";
                emailMessage.Cc = "";
                emailMessage.Bcc = "";
                emailMessage.IsBodyHtml = true;
                emailMessage.Attachments = null;
                emailMessage.EmailFrom = result.EmailFrom;
                emailMessage.PortName = result.PortName;
                emailMessage.DomainName = result.DomainName;
                emailMessage.EmailPassword = result.EmailPassword;
                emailMessage.MailSsl = result.MailSsl;
                emailMessage.DLTTemplateId = "";
            }

            return emailMessage;
        }
        public void SendforApprovalEmailIWRS(IWRSEmailModel iWRSEmailModel, IList<string> toMails, SupplyManagementEmailConfiguration supplyManagementEmailConfiguration)
        {
            var emailMessage = ConfigureEmailForVariable();
            emailMessage.Subject = GetSubjectIWRSEmail(supplyManagementEmailConfiguration, iWRSEmailModel);
            emailMessage.MessageBody = ReplaceBodyForIWRSEmail(supplyManagementEmailConfiguration.EmailBody, iWRSEmailModel);

            if (toMails != null && toMails.Count > 0)
            {
                foreach (var item in toMails)
                {
                    emailMessage.SendTo = item;
                    _emailService.SendMail(emailMessage);
                }
            }
        }

        public void SendforShipmentApprovalEmailIWRS(IWRSEmailModel iWRSEmailModel, IList<string> toMails, SupplyManagementApproval supplyManagementEmailConfiguration)
        {
            var emailMessage = ConfigureEmailForVariable();
            if (supplyManagementEmailConfiguration.ApprovalType == Helper.SupplyManagementApprovalType.ShipmentApproval)
                emailMessage.Subject = "Shipment Request Approval " + supplyManagementEmailConfiguration.Project.ProjectCode;
            if (supplyManagementEmailConfiguration.ApprovalType == Helper.SupplyManagementApprovalType.WorkflowApproval)
                emailMessage.Subject = "Shipment Workflow Approval " + supplyManagementEmailConfiguration.Project.ProjectCode;
            emailMessage.MessageBody = ReplaceBodyForIWRSEmail(supplyManagementEmailConfiguration.EmailTemplate, iWRSEmailModel);

            if (toMails != null && toMails.Count > 0)
            {
                foreach (var item in toMails)
                {
                    emailMessage.SendTo = item;
                    _emailService.SendMail(emailMessage);
                }
            }
        }
        private string ReplaceBodyForIWRSEmail(string body, IWRSEmailModel email)
        {

            var str = body;
            if (!string.IsNullOrEmpty(email.StudyCode))
            {
                str = Regex.Replace(str, "##StudyCode##", email.StudyCode, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.SiteCode))
            {
                str = Regex.Replace(str, "##SiteCode##", email.SiteCode, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.SiteName))
            {
                str = Regex.Replace(str, "##SiteName##", email.SiteName, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.RequestFromSiteCode))
            {
                str = Regex.Replace(str, "##RequestFromSiteCode##", email.RequestFromSiteCode, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.RequestFromSiteName))
            {
                str = Regex.Replace(str, "##RequestFromSiteName##", email.RequestFromSiteName, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.SiteName))
            {
                str = Regex.Replace(str, "##SiteName##", email.SiteName, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.ProductType))
            {
                str = Regex.Replace(str, "##ProductType##", email.ProductType, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.Visit))
            {
                str = Regex.Replace(str, "##Visit##", email.Visit, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.Reason))
            {
                str = Regex.Replace(str, "##Reason##", email.Reason, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.RequestType))
            {
                str = Regex.Replace(str, "##RequestType##", email.RequestType, RegexOptions.IgnoreCase);
            }
            if (email.RequestedQty > 0)
            {
                str = Regex.Replace(str, "##RequestedQty##", email.RequestedQty.ToString(), RegexOptions.IgnoreCase);
            }
            if (email.ApprovedQty > 0)
            {
                str = Regex.Replace(str, "##ApprovedQty##", email.ApprovedQty.ToString(), RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.Status))
            {
                str = Regex.Replace(str, "##Status##", email.Status, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.ScreeningNo))
            {
                str = Regex.Replace(str, "##ScreeningNo##", email.ScreeningNo, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.RandomizationNo))
            {
                str = Regex.Replace(str, "##RandomizationNo##", email.RandomizationNo, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.KitNo))
            {
                str = Regex.Replace(str, "##KitNo##", email.KitNo, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.ActionBy))
            {
                str = Regex.Replace(str, "##DoneBy##", email.ActionBy, RegexOptions.IgnoreCase);
                str = Regex.Replace(str, "##SendBy##", email.ActionBy, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.RequestedBy))
            {
                str = Regex.Replace(str, "##RequestedBy##", email.RequestedBy, RegexOptions.IgnoreCase);

            }
            if (!string.IsNullOrEmpty(email.RequestToSiteName))
            {
                str = Regex.Replace(str, "##RequestToSiteName##", email.RequestToSiteName, RegexOptions.IgnoreCase);

            }
            if (!string.IsNullOrEmpty(email.RequestToSiteCode))
            {
                str = Regex.Replace(str, "##RequestToSiteCode##", email.RequestToSiteCode, RegexOptions.IgnoreCase);

            }
            if (email.ThresholdValue > 0)
            {
                str = Regex.Replace(str, "##ThresholdValue##", email.ThresholdValue.ToString(), RegexOptions.IgnoreCase);

            }
            if (email.RemainingKit > 0 || email.RemainingKit == 0)
            {
                str = Regex.Replace(str, "##RemainingKit##", email.RemainingKit.ToString(), RegexOptions.IgnoreCase);

            }
            if (!string.IsNullOrEmpty(email.ReasonForUnblind))
            {
                str = Regex.Replace(str, "##ReasonForUnblind##", email.ReasonForUnblind, RegexOptions.IgnoreCase);

            }
            if (!string.IsNullOrEmpty(email.UnblindBy))
            {
                str = Regex.Replace(str, "##UnblindBy##", email.UnblindBy, RegexOptions.IgnoreCase);

            }
            if (email.UnblindDatetime != null)
            {
                str = Regex.Replace(str, "##UnblindDatetime##", email.UnblindDatetime.ToString("dddd, dd MMMM yyyy HH:mm"), RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.Treatment))
            {
                str = Regex.Replace(str, "##Treatment##", email.Treatment, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.TypeOfKitReturn))
            {
                str = Regex.Replace(str, "##TypeOfKitReturn##", email.TypeOfKitReturn, RegexOptions.IgnoreCase);
            }
            if (email.NoOfKitReturn > 0)
            {
                str = Regex.Replace(str, "##NoOfKitReturn##", email.NoOfKitReturn.ToString(), RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.ApprovedBy))
            {
                str = Regex.Replace(str, "##ApprovedBy##", email.ApprovedBy.ToString(), RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.ApprovedOn))
            {
                str = Regex.Replace(str, "##ApprovedOn##", email.ApprovedOn.ToString(), RegexOptions.IgnoreCase);
            }
            return str;
        }

        private string GetSubjectIWRSEmail(SupplyManagementEmailConfiguration supplyManagementEmailConfiguration, IWRSEmailModel iWRSEmailModel)
        {
            string str = string.Empty;
            if (supplyManagementEmailConfiguration.Triggers == Helper.SupplyManagementEmailTriggers.SendforApprovalVerificationTemplate)
            {
                str = " Verification Approval : " + iWRSEmailModel.StudyCode;
            }
            if (supplyManagementEmailConfiguration.Triggers == Helper.SupplyManagementEmailTriggers.VerificationTemplateApproveReject)
            {
                str = " Verification Approval : " + iWRSEmailModel.StudyCode;
            }
            if (supplyManagementEmailConfiguration.Triggers == Helper.SupplyManagementEmailTriggers.RandomizationSheetApprovedRejected)
            {
                str = " Randomization Sheet : " + iWRSEmailModel.Status + " , " + iWRSEmailModel.StudyCode;
            }
            if (supplyManagementEmailConfiguration.Triggers == Helper.SupplyManagementEmailTriggers.ShipmentRequest)
            {
                str = " Shipment Request : " + iWRSEmailModel.StudyCode;
            }
            if (supplyManagementEmailConfiguration.Triggers == Helper.SupplyManagementEmailTriggers.ShipmentApproveReject)
            {
                str = " Shipment : " + iWRSEmailModel.Status + " , " + iWRSEmailModel.StudyCode;
            }
            if (supplyManagementEmailConfiguration.Triggers == Helper.SupplyManagementEmailTriggers.KitReturn)
            {
                str = " Kit Return : " + iWRSEmailModel.KitNo;
            }
            if (supplyManagementEmailConfiguration.Triggers == Helper.SupplyManagementEmailTriggers.SubjectRandomization)
            {
                str = "Subject Randomization : " + iWRSEmailModel.StudyCode;
            }
            if (supplyManagementEmailConfiguration.Triggers == Helper.SupplyManagementEmailTriggers.Threshold)
            {
                str = "IMP Threshold Intimation : " + iWRSEmailModel.StudyCode;
            }
            if (supplyManagementEmailConfiguration.Triggers == Helper.SupplyManagementEmailTriggers.Unblind)
            {
                str = "Unblind : " + iWRSEmailModel.StudyCode;
            }

            return str;
        }

        public void SendEmailonEmailvariableConfiguration(EmailConfigurationEditCheckSendEmail email, int userId, string toMails, string tophone)
        {
            var emailMessage = ConfigureEmailForVariable();
            emailMessage.Subject = email.Subject;
            emailMessage.SendTo = toMails;
            emailMessage.MessageBody = ReplaceBodyForEmailvariableConfiguration(email.EmailBody, email, userId);
            _emailService.SendMail(emailMessage);


        }

        public async Task SendEmailonEmailvariableConfigurationSMS(EmailConfigurationEditCheckSendEmail email, EmailMessage EmailMessage, int userId, string toMails, string tophone)
        {
            if (email.IsSMS)
            {
                var emailMessagesms = EmailMessage;
                if (!string.IsNullOrEmpty(tophone))
                {
                    var body = ReplaceBodyForEmailvariableConfigurationSMS(emailMessagesms.MessageBody, email, userId);
                    await SendSMSParticaularStudyWise(tophone, body, emailMessagesms.DLTTemplateId);
                }
            }
        }

        private string ReplaceBodyForEmailvariableConfiguration(string body, EmailConfigurationEditCheckSendEmail email, int? userId)
        {

            var str = body;
            if (userId > 0)
            {
                var user = _context.Users.Where(s => s.Id == userId).FirstOrDefault();
                if (user != null)
                    str = Regex.Replace(str, "##UserName##", user.UserName, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.StudyCode))
            {
                str = Regex.Replace(str, "##StudyCode##", email.StudyCode, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.SiteCode))
            {
                str = Regex.Replace(str, "##SiteCode##", email.SiteCode, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.SiteName))
            {
                str = Regex.Replace(str, "##SiteName##", email.SiteName, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.VisitName))
            {
                str = Regex.Replace(str, "##VisitName##", email.VisitName, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.TemplateName))
            {
                str = Regex.Replace(str, "##TemplateName##", email.TemplateName, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.VariableName))
            {
                str = Regex.Replace(str, "##VariableName##", email.VariableName, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.ScreeningNo))
            {
                str = Regex.Replace(str, "##ScreeningNo##", email.ScreeningNo, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.RandomizationNo))
            {
                str = Regex.Replace(str, "##RandomizationNo##", email.RandomizationNo, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.CompanyName))
            {
                str = Regex.Replace(str, "##CompanyName##", email.CompanyName, RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(email.CurrentDate))
            {
                str = Regex.Replace(str, "##CurrentDate##", email.CurrentDate, RegexOptions.IgnoreCase);
            }

            return str;
        }
        private string ReplaceBodyForEmailvariableConfigurationSMS(string body, EmailConfigurationEditCheckSendEmail email, int? userId)
        {

            var str = body;
           
            if (!string.IsNullOrEmpty(email.TemplateName))
            {
                str = Regex.Replace(str, "#TemplateName#", email.TemplateName, RegexOptions.IgnoreCase);
            }
            
            if (!string.IsNullOrEmpty(email.ScreeningNo))
            {
                str = Regex.Replace(str, "#ScreeningNo#", email.ScreeningNo, RegexOptions.IgnoreCase);
            }
          

            return str;
        }
        // for visit email
        public void SendEmailonVisitStatus(VisitEmailConfigurationGridDto email, Data.Entities.ProjectRight.ProjectRight data, Randomization randomization)
        {
            var emailMessage = ConfigureEmailForVariable();
            emailMessage.Subject = email.Subject;
            emailMessage.SendTo = data.User.Email;
            emailMessage.MessageBody = ReplaceBodyForVisitStatusEmail(email.EmailBody, email, data, randomization);
            _emailService.SendMail(emailMessage);
        }

        private string ReplaceBodyForVisitStatusEmail(string body, VisitEmailConfigurationGridDto email, Data.Entities.ProjectRight.ProjectRight data, Randomization randomization)
        {
            var str = body;

            if (data.User != null)
                str = Regex.Replace(str, "##UserName##", data.User.UserName, RegexOptions.IgnoreCase);

            str = Regex.Replace(str, "##StudyCode##", _context.Project.Find(_context.Project.Find(randomization.ProjectId).ParentProjectId).ProjectCode, RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "##SiteCode##", _context.Project.Find(randomization.ProjectId).ProjectCode, RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "##VisitName##", email.VisitName, RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "##VisitStatus##", email.VisitStatus, RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "##ScreeningNo##", randomization.ScreeningNumber, RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "##CompanyName##", _context.Company.Find(_jwtTokenAccesser.CompanyId).CompanyName, RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "##CurrentDate##", DateTime.Now.Date.ToString("dddd, dd MMMM yyyy"), RegexOptions.IgnoreCase);

            if (!string.IsNullOrEmpty(randomization.RandomizationNumber))
                str = Regex.Replace(str, "##RandomizationNo##", randomization.RandomizationNumber, RegexOptions.IgnoreCase);

            return str;
        }
        public void SendALettersMailtoInvestigator(string fullPath, string email,string CtmsActivity,string ScheduleStartDate)
        {
            var userName = _jwtTokenAccesser.UserName;
            var emailMessage = ConfigureEmailLetters("Letters", userName, fullPath) ;
            emailMessage.SendTo = email;
            emailMessage.MessageBody = ReplaceBodyForLetters(emailMessage.MessageBody, userName);
            emailMessage.Subject = ReplaceSubjectForLetters(emailMessage.Subject, CtmsActivity, ScheduleStartDate);
            _emailService.SendMail(emailMessage);
        }
        private string ReplaceBodyForLetters(string body, string userName)
        {
            body = Regex.Replace(body, "##UserName##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>UserName</strong>##", "<strong>" + userName + "</strong>",RegexOptions.IgnoreCase);
            return body;
        }
        private string ReplaceSubjectForLetters(string body, string CtmsActivity, string ScheduleStartDate)
        {
            body = Regex.Replace(body, "##CTMSACTVITY##", CtmsActivity, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>CTMSACTVITY</strong>##", "<strong>" + CtmsActivity + "</strong>",RegexOptions.IgnoreCase);

            body = Regex.Replace(body, "##DATE##", ScheduleStartDate.ToString(), RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>DATE</strong>##", "<strong>" + ScheduleStartDate + "</strong>", RegexOptions.IgnoreCase);

            return body;
        }
    }
}