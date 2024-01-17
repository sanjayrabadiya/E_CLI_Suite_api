using GSC.Data.Entities.Master;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Etmf;
using GSC.Respository.SupplyManagement;
using Quartz;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GSC.Api.QuartzJob
{
    public class IWRSEmailJob : IJob
    {
        private readonly IGSCContext _context;
        ISupplyManagementRequestRepository _supplyManagementRequestRepository;
        ISupplyManagementShipmentRepository _supplyManagementShipmentRepository;
        IVerificationApprovalTemplateRepository _verificationApprovalTemplateRepository;
        IRandomizationRepository _randomizationRepository;
        public IWRSEmailJob(ISupplyManagementRequestRepository supplyManagementRequestRepository,
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
