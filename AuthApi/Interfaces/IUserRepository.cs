
using AuthAPI.Models.Entites.User;
using CoreCommon.HelperCommon;
using CoreCommon.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthAPI.Interfaces
{
    public interface IUserRepository
    {
        Task<ResultData<UsersViewModel?>> ValidateUserAsync(string username, string password);
        Task<ResultData<UsersViewModel?>> GetUserByIdAsync(int userId);

    }
}
