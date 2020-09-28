using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public class EconsentReviewDetailsRepository : GenericRespository<EconsentReviewDetails, GscContext>, IEconsentReviewDetailsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IEconsentSetupRepository _econsentSetupRepository;
        private readonly IMapper _mapper;
        public EconsentReviewDetailsRepository(IUnitOfWork<GscContext> uow, 
                                                IJwtTokenAccesser jwtTokenAccesser,
                                                IEconsentSetupRepository econsentSetupRepository,
                                                IMapper mapper) : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _econsentSetupRepository = econsentSetupRepository;
            _mapper = mapper;
        }

        public string Duplicate(EconsentReviewDetailsDto objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.EconsentDocumentId == objSave.EconsentDocumentId && x.AttendanceId == objSave.AttendanceId && x.DeletedDate == null))
            {
                return "Already reviewed this document";
            }
            return "";
        }

        

        public IList<DropDownDto> GetPatientDropdown(int projectid)
        {
            //var econsentsetups = _econsentSetupRepository.All.Where(x => x.ProjectId == projectid).ToList();
            var data = (from econsentsetups in Context.EconsentSetup.Where(x => x.ProjectId == projectid)
                        join EconsentReviewDetails in Context.EconsentReviewDetails.Where(x => x.DeletedDate == null && x.IsApprovedByInvestigator == false) on econsentsetups.Id equals EconsentReviewDetails.EconsentDocumentId
                        join attendance in Context.Attendance.Where(x => x.DeletedDate == null) on EconsentReviewDetails.AttendanceId equals attendance.Id
                        join nonregister in Context.Randomization.Where(x => x.DeletedDate == null) on attendance.Id equals 91//nonregister.AttendanceId
                        select new DropDownDto
                        {
                            Id = attendance.Id,
                            Value = nonregister.Initial + " " + nonregister.ScreeningNumber
                        }).Distinct().ToList();

            return data;
        }

        public List<EconsentReviewDetailsDto> GetUnApprovedEconsentDocumentList(int patientid)
        {
            var EconsentReviewDetails = All.Where(x => x.DeletedDate == null && x.AttendanceId == patientid && x.IsApprovedByInvestigator == false).
                   ProjectTo<EconsentReviewDetailsDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            EconsentReviewDetails.ForEach(b =>
            {
                b.EconsentDocumentName = _econsentSetupRepository.Find((int)b.EconsentDocumentId).DocumentName;
            });
            return EconsentReviewDetails;
        }

        public List<EconsentReviewDetailsDto> GetApprovedEconsentDocumentList(int projectid)
        {
            var data = (from econsentsetups in Context.EconsentSetup.Where(x => x.ProjectId == projectid)
                        join EconsentReviewDetails in Context.EconsentReviewDetails.Where(x => x.DeletedDate == null && x.IsApprovedByInvestigator == true) on econsentsetups.Id equals EconsentReviewDetails.EconsentDocumentId
                        join attendance in Context.Attendance.Where(x => x.DeletedDate == null) on EconsentReviewDetails.AttendanceId equals attendance.Id
                        join nonregister in Context.Randomization.Where(x => x.DeletedDate == null) on attendance.Id equals 91//nonregister.AttendanceId
                        select new EconsentReviewDetailsDto
                        {
                            Id = EconsentReviewDetails.Id,
                            EconsentDocumentId = EconsentReviewDetails.EconsentDocumentId,
                            EconsentDocumentName = econsentsetups.DocumentName,
                            AttendanceName = nonregister.Initial + " " + nonregister.ScreeningNumber,
                            AttendanceId = EconsentReviewDetails.AttendanceId,
                            patientapproveddatetime = EconsentReviewDetails.patientapproveddatetime,
                            investigatorapproveddatetime = EconsentReviewDetails.investigatorapproveddatetime
                        }).ToList();

            return data;
        }
    }
}
