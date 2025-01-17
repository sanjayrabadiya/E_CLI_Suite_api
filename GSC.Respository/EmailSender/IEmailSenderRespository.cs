﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Configuration;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Data.Entities.SupplyManagement;
using GSC.Shared.Email;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GSC.Respository.EmailSender
{
    public interface IEmailSenderRespository : IGenericRepository<EmailTemplate>
    {
        void SendRegisterEMail(string toMail, string password, string userName, string companyName);
        void SendChangePasswordEMail(string toMail, string password, string userName, string companyName);
       
        Task SendForgotPasswordEMail(string toMail, string mobile, string password, string userName, string companyName);

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
        Task SendEmailOfScreenedPatient(string toMail, string patientName, string userName, string password, string ProjectName, string mobile, int sendtype, bool isSendEmail, bool isSendSMS);
        Task SendAdverseEventAlertEMailtoInvestigator(string toMail, string mobile, string userName, string projectName, string patientname, string reportdate);
        void SendOfflineChatNotification(string toMail, string userName);
        void SendWithDrawEmail(string toMail, string userName, string documentName, string ProjectName, string patientName, string filepath);
        public void SendEmailOfRejectedDocumenttoPatient(string toMail, string userName, string documentName, string ProjectName, string filepath);
        void SendEmailOfTemplateReview(string toMail, string userName, string activity, string template, string project);
        void SendEmailOfTemplateSendBack(string toMail, string userName, string activity, string template, string ProjectName);
        void SendLabManagementAbnormalEMail(string toMail, LabManagementEmail email);
        void SendEmailOfTemplateApprove(string toMail, string userName, string activity, string template, string project);

        void SendVariableValueEmail(ScreeningTemplateValueDto screeningTemplateValueDto, string toMail, string Template);
      
        void SendEmailOfLARReviewedPDFtoInvestigator(string toMail, string userName, string documentName, string ProjectName, string patientName, string filepath);
        void SendWithDrawEmailLAR(string toMail, string userName, string documentName, string ProjectName, string patientName, string filepath);
        public void SendEmailOfReviewed(string toMail, string userName, string documentName, string ArtificateName, string ProjectName);
        public void SendApprovedEmailOfArtificate(string toMail, string userName, string documentName, string ArtificateName, string ProjectName);
        public void SendRejectedEmailOfArtificate(string toMail, string userName, string documentName, string ArtificateName, string ProjectName);

        void SendforApprovalEmailIWRS(IwrsEmailModel iWRSEmailModel, IList<string> toMails, SupplyManagementEmailConfiguration supplyManagementEmailConfiguration);

        void SendforShipmentApprovalEmailIWRS(IwrsEmailModel iWRSEmailModel, IList<string> toMails, SupplyManagementApproval supplyManagementEmailConfiguration);

        void SendEmailonEmailvariableConfiguration(EmailConfigurationEditCheckSendEmail email, int userId, string toMails, string tophone);

        void SendEmailonVisitStatus(VisitEmailConfigurationGridDto email, Data.Entities.ProjectRight.ProjectRight data, Randomization randomization);

        void SendALettersMailtoInvestigator(string fullPath, string email, string body, string CtmsActivity, string ScheduleStartDate);

        Task SendEmailonEmailvariableConfigurationSMS(EmailConfigurationEditCheckSendEmail email, EmailMessage EmailMessage, int userId, string toMails, string tophone);

        void SendDesignAuditGeneratedEMail(string toMail, string userName, string projectName, string linkOfPdf);
        EmailMessage ConfigureEmail(string keyName, string userName);
        void SendEmailOfReviewDue(string toMail, string userName, string documentName, string ArtificateName, string ProjectName, DateTime? dueDate);
        void SendEmailOfApproveDue(string toMail, string userName, string documentName, string ArtificateName, string ProjectName, DateTime? dueDate);
        void SendMailCtmsApproval(CtmsApprovalUsers ctmsApprovalWorkFlowDetail, bool ifPlanApproval);
        void SendDueResourceMilestoneEmail(ResourceMilestone resourceMilestone);
        void SendCtmsApprovalEmail(List<CtmsWorkflowApproval> ctmsWorkflowApprovals);
        void SendCtmsReciverEmail(List<CtmsWorkflowApproval> ctmsWorkflowApprovals);
        void SendCtmsApprovedEmail(List<CtmsWorkflowApproval> ctmsWorkflowApprovals);

        void SendCtmsSiteContractApprovalEmail(List<CtmsSiteContractWorkflowApproval> ctmsWorkflowApprovals);
        void SendCtmsSiteContractReciverEmail(List<CtmsSiteContractWorkflowApproval> ctmsWorkflowApprovals);
        void SendCtmsSiteContractApprovedEmail(List<CtmsSiteContractWorkflowApproval> ctmsWorkflowApprovals);
    }
}