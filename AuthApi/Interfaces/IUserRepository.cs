
using AuthAPI.Models.Entites.User;
using CoreCommon.HelperCommon;
using CoreCommon.Models.UsersModels;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthAPI.Interfaces
{
    public interface IUserRepository
    {
        Task<ResultData<UserModelDto?>> GetSingleUserByIdAsync(int userId);
        Task<ResultData<SqlMapper.GridReader>> ValidateUserAsync(string username, string password);
        Task<ResultData<UserDto?>> GetUserByIdAsync(int userId);

    }
}
