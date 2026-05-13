
using CoreCommon.HelperCommon;
using AuthAPI.Models.Entites.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthAPI.Interfaces
{
    public interface IUserRepository
    {
        Task<ResultData<UserViewModel?>> ValidateUserAsync(string username, string password);
        Task<ResultData<UserViewModel?>> GetUserByIdAsync(int userId);

    }
}
