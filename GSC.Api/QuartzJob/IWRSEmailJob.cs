using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.Attendance;
using GSC.Respository.SupplyManagement;
using Quartz;
using System;
using System.Threading.Tasks;

namespace GSC.Api.QuartzJob
{
    public class IwrsEmailJob : IJob
    {
        private readonly IGSCContext _context;
        private readonly ISupplyManagementRequestRepository _supplyManagementRequestRepository;
        private readonly ISupplyManagementShipmentRepository _supplyManagementShipmentRepository;
        private readonly IVerificationApprovalTemplateRepository _verificationApprovalTemplateRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        public IwrsEmailJob(ISupplyManagementRequestRepository supplyManagementRequestRepository,
            ISupplyManagementShipmentRepository supplyManagementShipmentRepository,
            IGSCContext context,
            IVerificationApprovalTemplateRepository verificationApprovalTemplateRepository,
            IRandomizationRepository randomizationRepository)
        {
            _supplyManagementRequestRepository = supplyManagementRequestRepository;
            _supplyManagementShipmentRepository = supplyManagementShipmentRepository;
            _context = context;
            _verificationApprovalTemplateRepository = verificationApprovalTemplateRepository;
            _randomizationRepository = randomizationRepository;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await SendIWRSEmail();
        }
        public async Task SendIWRSEmail()
        {
            try
            {
                await _supplyManagementRequestRepository.ShipmentRequestEmailSchedule();
                await _supplyManagementShipmentRepository.ShipmentShipmentEmailSchedule();
                await _verificationApprovalTemplateRepository.SendForApprovalVerificationTemplateScheduleEmail();
                await _randomizationRepository.SendRandomizationThresholdEmailSchedule();
            }
            catch(Exception ex)
            {
                SupplyManagementEmailScheduleLog supplyManagementEmailScheduleLog = new SupplyManagementEmailScheduleLog();
                supplyManagementEmailScheduleLog.Message = ex.Message.ToString();
                supplyManagementEmailScheduleLog.TriggerType = "Error In IWRS Email Schedule Log";
                _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog);
                _context.Save();
            }
        }
    }
}
