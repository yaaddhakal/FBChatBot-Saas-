using UserServiceAPI.DTOs.Users;

namespace UserServiceAPI.Interfaces
{
    public interface IUserRepository
    {
        // ============================================================
        // USER CRUD
        // ============================================================
        Task<ResultData<int>> CreateUserAsync(UserCreateDto dto, int createdBy);
        Task<ResultData<bool>> UpdateUserAsync(UserUpdateDto dto, int updatedBy);
        Task<ResultData<bool>> DeleteUserAsync(int userId);
        Task<ResultData<bool>> ToggleUserStatusAsync(int userId, bool isActive);

        // ============================================================
        // USER QUERIES
        // ============================================================
        Task<ResultData<UserResponseDto>> GetUserByIdAsync(int userId);
        Task<ResultData<UserDetailResponseDto>> GetUserWithDetailAsync(int userId);
        Task<ResultData<List<UserListDto>>> GetAllUsersAsync();
        //Task<ResultData<List<UserListDto>>> GetAllUsersDetailsAsync();
        Task<ResultData<List<UserDetailResponseDto>>> GetAllUsersDetailsAsync();
        Task<ResultData<List<UserListDto>>> GetAllUsersByCompanyIdAsync(int companyId);
        Task<ResultData<List<UserListDto>>> GetUsersByRoleAndCompanyIdAsync(int roleId, int companyId);
        // ============================================================
        // PASSWORD
        // ============================================================
        Task<ResultData<bool>> ChangePasswordAsync(ChangePasswordDto dto);
        Task<ResultData<bool>> ResetPasswordAsync(int userId, string newPassword, int updatedBy);

        // ============================================================
        // KYC
        // ============================================================
        Task<ResultData<bool>> UpdateKYCAsync(KYCUpdateDto dto, int updatedBy);

        // ============================================================
        // VALIDATION
        // ============================================================
        Task<ResultData<bool>> IsUsernameExistsAsync(string username);
        Task<ResultData<bool>> IsEmailExistsAsync(string email);
    }
}

