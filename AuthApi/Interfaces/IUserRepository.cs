
using AuthAPI.Models.Entites.User;
using CoreCommon.HelperCommon;
using CoreCommon.Models.UsersModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthAPI.Interfaces
{
    public interface IUserRepository
    {
        Task<ResultData<UserViewDto?>> ValidateUserAsync(string username, string password);
        Task<ResultData<UserViewDto?>> GetUserByIdAsync(int userId);

    }
}
