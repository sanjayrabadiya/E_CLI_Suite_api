using System;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Respository.EmailSender;
using GSC.Shared.JWTAuth;
using GSC.Shared.Security;

namespace GSC.Respository.UserMgt
{
    public class UserOtpRepository : GenericRespository<UserOtp>, IUserOtpRepository
    {
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserPasswordRepository _userPasswordRepository;
        private readonly IUserRepository _userRepository;

        public UserOtpRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IUserRepository userRepository,
            IEmailSenderRespository emailSenderRespository,
            IUserPasswordRepository userPasswordRepository)
            : base(context)
        {
            _userRepository = userRepository;
            _emailSenderRespository = emailSenderRespository;
            _userPasswordRepository = userPasswordRepository;
        }

        public string InsertOtp(string username)
        {
            var user = _userRepository.FindBy(x => x.UserName == username && x.DeletedDate == null).FirstOrDefault();
            if (user == null) return "Invalid user name!";

            var userOtp = new UserOtp();
            var opt = RandomPassword.CreateRandomPassword(6);
            userOtp.Otp = opt;
            userOtp.UserId = user.Id;
            userOtp.CreatedDate = DateTime.Now;
            Add(userOtp);

            _emailSenderRespository.SendForgotPasswordEMail(user.Email, opt, user.UserName);
            return "";
        }

        public string VerifyOtp(UserOtpDto userOtpDto)
        {
            var user = _userRepository.FindBy(x => x.UserName == userOtpDto.UserName && x.DeletedDate == null)
                .FirstOrDefault();
            if (user == null) return "Invalid user name!";

            var userOtp = FindBy(x => x.UserId == user.Id && x.Otp == userOtpDto.Otp)
                .OrderByDescending(c => c.CreatedDate).FirstOrDefault();
            var usernewOtp = FindBy(x => x.UserId == user.Id).OrderByDescending(c => c.CreatedDate).FirstOrDefault();

            if (userOtp == null) return "OTP you have entered is not correct!";

            if (usernewOtp != null && userOtp.Otp != usernewOtp.Otp)
                if (userOtp.CreatedDate != null)
                    userOtp.CreatedDate = userOtp.CreatedDate.Value.AddHours(-1);
            var startDate = userOtp.CreatedDate;
            var endDate = DateTime.Now.ToUniversalTime();
            var totalMinutes = (Convert.ToDateTime(endDate) - Convert.ToDateTime(startDate)).TotalMinutes;
            if (totalMinutes >= 60)
                return "OTP you have entered is expire!";

            return "";
        }

        public string ChangePasswordByOtp(UserOtpDto userOtpDto)
        {
            if (string.IsNullOrEmpty(userOtpDto.Password)) return "Password can't blank!";

            var user = _userRepository.FindBy(x => x.UserName == userOtpDto.UserName && x.DeletedDate == null)
                .FirstOrDefault();
            if (user == null) return "Invalid user name!";

            var userOtp = FindBy(x => x.UserId == user.Id && x.Otp == userOtpDto.Otp).OrderBy(c => c.CreatedDate)
                .FirstOrDefault();
            var usernewOtp = FindBy(x => x.UserId == user.Id).OrderByDescending(c => c.CreatedDate).FirstOrDefault();

            if (userOtp == null) return "OTP you have entered is not correct!";

            if (usernewOtp != null && userOtp.Otp != usernewOtp.Otp)
                if (userOtp.CreatedDate != null)
                    userOtp.CreatedDate = userOtp.CreatedDate.Value.AddHours(-1);
            var startDate = userOtp.CreatedDate;
            var endDate = DateTime.Now.ToUniversalTime();
            var totalMinutes = (Convert.ToDateTime(endDate) - Convert.ToDateTime(startDate)).TotalMinutes;
            if (totalMinutes >= 60)
                return "OTP you have entered is expire!";

            _userPasswordRepository.CreatePassword(userOtpDto.Password, user.Id);

            return "";
        }
    }
}