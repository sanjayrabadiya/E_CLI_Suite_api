﻿using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Entities.Attendance;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Attendance
{
    public class NoneRegisterRepository : GenericRespository<NoneRegister, GscContext>, INoneRegisterRepository
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;


        public NoneRegisterRepository(IUnitOfWork<GscContext> uow,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IAttendanceRepository attendanceRepository,
            IProjectDesignRepository projectDesignRepository,
            IScreeningTemplateRepository screeningTemplateRepository)
            : base(uow, jwtTokenAccesser)
        {
            _attendanceRepository = attendanceRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectDesignRepository = projectDesignRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
        }

        public void SaveNonRegister(NoneRegister noneRegister, NoneRegisterDto noneRegisterDto)
        {
            noneRegister.Id = 0;
            noneRegister.Initial = noneRegister.Initial.PadRight(3, '-');

            Add(noneRegister);
            var attendance = new Data.Entities.Attendance.Attendance();
            attendance.ProjectId = noneRegisterDto.ProjectId;
            attendance.ProjectDesignPeriodId = noneRegisterDto.ProjectDesignPeriodId;
            attendance.NoneRegister = noneRegister;
            attendance.AttendanceType = AttendanceType.NoneRegister;
            _attendanceRepository.SaveAttendance(attendance);
        }

        public List<NoneRegisterDto> GetNonRegisterList(int projectId)
        {
            //  var result = Context.Attendance.ProjectId == projectId && r.DeletedDate == null)

            var result = All.Where(r => r.Attendance.ProjectId == projectId && r.DeletedDate == null).Select(x =>
                 new NoneRegisterDto
                 {
                     Id = x.Id,
                     AttendanceId = x.AttendanceId,
                     ProjectCode = x.Attendance.Project.ProjectCode,
                     ProjectId = x.Attendance.ProjectId,
                     ProjectName = x.Attendance.Project.ProjectName,
                     Initial = x.Initial,
                     ScreeningNumber = x.ScreeningNumber,
                     DateOfScreening = x.DateOfScreening,
                     RandomizationNumber = x.RandomizationNumber,
                     DateOfRandomization = x.DateOfRandomization,
                     ProjectDesignPeriodId = x.Attendance.ProjectDesignPeriodId,
                     CreatedBy = (int)x.CreatedBy,
                     ModifiedBy = x.ModifiedBy,
                     DeletedBy = x.DeletedBy,
                     CreatedDate = x.CreatedDate,
                     ModifiedDate = x.ModifiedDate,
                     DeletedDate = x.ModifiedDate,
                     CompanyId = x.CompanyId,
                 }).OrderBy(x => x.ScreeningNumber).ToList();
            result.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });

            foreach (var item in result)
            {
                var screeningTemplate = _screeningTemplateRepository.FindByInclude(x => x.ScreeningEntry.AttendanceId == item.AttendanceId && x.DeletedDate == null).ToList();
                if (screeningTemplate.Count() <= 0 || screeningTemplate.Any(y => y.IsLocked == false))
                {
                    item.IsLocked = false;
                }
                else
                {
                    item.IsLocked = true;
                }
            }

            return result;

        }

        public string Duplicate(NoneRegister objSave, int projectId)
        {
            //if (All.Any(x =>
            //    x.Id != objSave.Id && x.ScreeningNumber == objSave.ScreeningNumber &&
            //    x.Attendance.ProjectId == projectId && x.DeletedDate == null))
            //    return "Duplicate Screening number : " + objSave.ScreeningNumber;

            if (All.Any(x =>
                x.Id != objSave.Id && x.RandomizationNumber == objSave.RandomizationNumber &&
                x.Attendance.ProjectId == projectId && !string.IsNullOrEmpty(x.RandomizationNumber) &&
                x.DeletedDate == null)) return "Duplicate Randomization Number : " + objSave.RandomizationNumber;

            return "";
        }
    }
}