using Azure.Core;
using CoreCommon.DbService;
using CoreCommon.Models.UsersModels;
using CoreCommon.Services.Interfaces;
using Dapper;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using UserServiceAPI.DTOs;
using UserServiceAPI.DTOs.Tenant;
using UserServiceAPI.Interfaces;
using static System.Net.WebRequestMethods;

public class TenantRepository : ITenantRepository
{
    private readonly IDbService _dbService;
    private readonly IUserRepository _userRepository ;
    private readonly IEmailService _emailService;
    public TenantRepository(IDbService dbService,IEmailService emailService, IUserRepository userRepository)
    {
        _dbService = dbService;
        _emailService = emailService;
        _userRepository = userRepository;
    }
    public async Task<ResultData<List<TenantDto>>> GetAllTenantsListAsync()
    {
        return await _dbService.GetAllAsync<TenantDto>(
            "SELECT TenantID, TenantName FROM Tenants",
            commandType: CommandType.Text);
    }

    public async Task<ResultData<List<IndustryDto>>> GetIndustriesByTenantAsync(int tenantID)
    {
        if (tenantID <= 0)
            return ResultData<List<IndustryDto>>.Fail("Invalid TenantID");
        return await _dbService.GetAllAsync<IndustryDto>(
            @"SELECT i.IndustryID, i.IndustryName 
          FROM Industries i
          INNER JOIN TenantIndustries ti ON ti.IndustryID = i.IndustryID
          WHERE ti.TenantID = @TenantID",
            new { TenantID = tenantID },
            commandType: CommandType.Text);
    }
    public async Task<ResultData<TenantOtpDto>> GetLatestOtpAsync(int userID)
    {
        var query = @"
        SELECT TOP 1 OtpID, UserId, OtpCode, CreatedAt, ExpiresAt, IsUsed
        FROM UserOtps
        WHERE UserId = @UserID
        ORDER BY CreatedAt DESC";

        return await _dbService.GetAsync<TenantOtpDto>(query, new { UserID = userID });
    }

    public async Task<ResultData<string>> ResendOtpAsync(int userID)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserID", userID);
        parameters.Add("@ResultMessage",
            dbType: DbType.String,
            size: 100,
            direction: ParameterDirection.Output);

        var result = await _dbService.ExecuteSpWithOutputAsync(parameters, "sp_ResendOtp");

        if (!result.Success)
            return ResultData<string>.Fail(result.Error);

        // ✅ fetch new OTP and user then send email
        var otp = await GetLatestOtpAsync(userID);
        var user = await _userRepository.GetUserByIdAsync(userID);

        if (!otp.Success || !user.Success)
            return ResultData<string>.Fail("Failed to retrieve OTP or user information");

        var emailResult = await _emailService.SendOtpEmailAsync(
            user.Data.Email,
            user.Data.UserName,
            otp.Data.OtpCode
        );
        if (!emailResult.Success)
            return ResultData<string>.Fail("Failed to resend OTP email");

        return ResultData<string>.Ok("OTP resent successfully");
    }
    public async Task<ResultData<SignupResultDto>> SignupTenantAsync(SignupTenantRequestDto re)
    {
        var pass = HashPassword(re.PasswordHash);
        re.UserType="TenantUser";
        var parameters = new DynamicParameters();
        parameters.Add("@TenantName", re.TenantName);
        parameters.Add("@UserType", re.UserType);
        parameters.Add("@UserName", re.UserName);
        parameters.Add("@Email", re.Email);
        parameters.Add("@PasswordHash", pass);
        parameters.Add("@TenantID", re.TenantID, DbType.Int32);
        parameters.Add("@IndustryID", re.IndustryID, DbType.Int32);
        parameters.Add("@IndustryName", re.IndustryName, DbType.String);
        parameters.Add("@UserID",
            dbType: DbType.Int32,
            direction: ParameterDirection.Output);
        parameters.Add("@ResultMessage",
            dbType: DbType.String,
            size: 100,
            direction: ParameterDirection.Output);

       
       // int userID=0;
        var iresult = await _dbService.SignupTenantAsync(parameters, "sp_SignupTenant");
     
        if (!iresult.Success)
            return ResultData<SignupResultDto>.Fail(iresult.Error);
        var result = iresult.Data;

        if (result.UserID <= 0)
            return ResultData<SignupResultDto>.Fail("Invalid user ID");

        if (result.Message == "RedirectToTenantDashboard" || result.Message == "RedirectToOTP")
        {
            return ResultData<SignupResultDto>.Ok(result);
        }

        var user = await _userRepository.GetUserByIdAsync(result.UserID);

        if (!user.Success)
            return ResultData<SignupResultDto>.Fail("User not found");

       var otpInfo = await GetLatestOtpAsync(result.UserID);
        if (!otpInfo.Success)
            return ResultData<SignupResultDto>.Fail("OTP not found");
        
        var emailResult = await _emailService.SendOtpEmailAsync(
             user.Data.Email,
             user.Data.UserName,
             otpInfo.Data.OtpCode
                );

        result.IsEmailSent = emailResult.Success;

        return ResultData<SignupResultDto>.Ok(result);
    }
    public async Task<ResultData<UserDto>> VerifyOtpAsync(VerifyOtpRequestDto re)
    {
        if (re.UserID <= 0)
            return ResultData<UserDto>.Fail("Invalid UserID");

        if (string.IsNullOrWhiteSpace(re.OtpCode) || re.OtpCode.Length != 6)
            return ResultData<UserDto>.Fail("Invalid OTP code");
        
        var parameters = new DynamicParameters();
        parameters.Add("@UserID", re.UserID);
        parameters.Add("@OtpCode", re.OtpCode);
        parameters.Add("@ResultMessage",
            dbType: DbType.String,
            size: 100,
            direction: ParameterDirection.Output);
        string sp= "sp_VerifyOtp";

        var result = await _dbService.VerifyOtpAsync(parameters, sp);
        if (!result.Success)
            return ResultData<UserDto>.Fail("User not found");
        // fetch full user after verification
        var user = await _userRepository.GetUserByIdAsync(re.UserID);
        return ResultData<UserDto>.Ok(user.Data);

    }
    public async Task<ResultData<List<TenantDto>>> GetAllTenantsAsync()
    {
        var result = await _dbService.GetAllAsync<TenantDto>(
            "sp_Tenants_CRUD",
            new { Action = "SELECTALL" },
            CommandType.StoredProcedure);

        return result.Data != null
           ?  ResultData <List < TenantDto >>.Ok(result.Data)
           : ResultData<List<TenantDto>>.Fail("Tenant not found");
    }

    public async Task<ResultData<TenantDto>> GetTenantByIdAsync(int tenantId)
    {
        var result = await _dbService.GetAsync<TenantDto>(
            "sp_Tenants_CRUD",
            new { Action = "SELECTBYID", TenantID = tenantId },
            CommandType.StoredProcedure);

        return result.Data != null
            ? ResultData<TenantDto>.Ok(result.Data)
            : ResultData<TenantDto>.Fail("Tenant not found");
    }

    public async Task<ResultData<int>> CreateTenantAsync(TenantDto dto)
    {
        var result = await _dbService.GetScalarAsync<int>(
            "sp_Tenants_CRUD",
            new
            {
                Action = "CREATE",
                dto.TenantName,
                dto.ContactEmail,
                dto.Phno,
                dto.IsActive,
                dto.IsRecorded,
                dto.Col1,
                dto.Col2
               
            },
            CommandType.StoredProcedure);

        return result.Data > 0
            ? ResultData<int>.Ok(result.Data)
            : ResultData<int>.Fail( "Failed to create tenant");
    }

    public async Task<ResultData<bool>> UpdateTenantAsync(TenantDto dto)
    {
        var result = await _dbService.ExecuteAsync(
            "sp_Tenants_CRUD",
            new
            {
                Action = "UPDATE",
                dto.TenantID,
                dto.TenantName,
                dto.ContactEmail,
                dto.Phno,
                dto.IsActive,
                dto.IsRecorded,
                dto.Col1,
                dto.Col2
            },
            CommandType.StoredProcedure);

        return result.Data > 0
            ? ResultData<bool>.Ok(true)
            : ResultData<bool>.Fail( "Failed to update tenant");
    }

    public async Task<ResultData<bool>> DeleteTenantAsync(int tenantId)
    {
        var result = await _dbService.GetAsync<SpResult>(
            "sp_Tenants_CRUD",
            new { Action = "DELETE", TenantID = tenantId },
            CommandType.StoredProcedure);

        return result.Data?.Result > 0
            ? ResultData<bool>.Ok(true)
            : ResultData<bool>.Fail(result.Data?.Message ?? "Failed to delete tenant");
    }
    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
