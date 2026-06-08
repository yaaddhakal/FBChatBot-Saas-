using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCommon.Services.Interfaces
{
   public interface IEmailService
    {
        Task<ResultData<string>> SendOtpEmailAsync(string toEmail, string userName, string otpCode);
    }
}
