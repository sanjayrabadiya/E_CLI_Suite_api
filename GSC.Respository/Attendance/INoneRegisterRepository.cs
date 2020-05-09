using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Entities.Attendance;

//using GSC.Data.Dto.Master;


namespace GSC.Respository.Attendance
{
    public interface INoneRegisterRepository : IGenericRepository<NoneRegister>
    {
        void SaveNonRegister(NoneRegister noneRegister, NoneRegisterDto noneRegisterDto);
        List<NoneRegisterDto> GetNonRegisterList(int projectId);

        string Duplicate(NoneRegister objSave, int projectId);
    }
}