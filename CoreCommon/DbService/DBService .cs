using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCommon.DbService
{
    public class DBService : IDbService
    {
        private readonly string? _connectionString;
        private readonly ILogger<DBService> _logger;
        private const int DefaultTimeout = 30;

        public DBService(IConfiguration configuration, ILogger<DBService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        private SqlConnection CreateConnection()
            => new SqlConnection(_connectionString);

        // ─────────────────────────────────────────────────────────────
        // NON-TRANSACTIONAL
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns first matching row mapped to T. Returns Fail if not found.
        /// </summary>
        public async Task<ResultData<T>> GetAsync<T>(
            string command,
            object? parms = null,
            CommandType commandType = CommandType.Text,
            int commandTimeout = DefaultTimeout)
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                var result = await connection.QueryFirstOrDefaultAsync<T>(
                    command, parms,
                    commandType: commandType,
                    commandTimeout: commandTimeout);

                if (result is null)
                    return ResultData<T>.Fail("Record not found", ResultStatusCode.NotFound);

                return ResultData<T>.Ok(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "[GetAsync] SQL error. Command: {Command}", command);
                return ResultData<T>.Fail($"Database error: {ex.Message}", ResultStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetAsync] Unexpected error. Command: {Command}", command);
                return ResultData<T>.Fail("Unexpected error occurred", ResultStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Returns all matching rows as List of T.
        /// </summary>
        public async Task<ResultData<List<T>>> GetAllAsync<T>(
            string command,
            object? parms = null,
            CommandType commandType = CommandType.Text,
            int commandTimeout = DefaultTimeout)
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                var result = await connection.QueryAsync<T>(
                    command, parms,
                    commandType: commandType,
                    commandTimeout: commandTimeout);

                var list = result?.ToList() ?? new List<T>();

                if (!list.Any())
                    return ResultData<List<T>>.Fail("No records found", ResultStatusCode.NotFound);

                return ResultData<List<T>>.Ok(list);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "[GetAllAsync] SQL error. Command: {Command}", command);
                return ResultData<List<T>>.Fail($"Database error: {ex.Message}", ResultStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetAllAsync] Unexpected error. Command: {Command}", command);
                return ResultData<List<T>>.Fail("Unexpected error occurred", ResultStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Executes a command (INSERT/UPDATE/DELETE) without transaction.
        /// Returns affected row count. -1 is treated as success (SET NOCOUNT ON).
        /// </summary>
        public async Task<ResultData<int>> ExecuteAsync(
            string command,
            object? parms = null,
            CommandType commandType = CommandType.Text,
            int commandTimeout = DefaultTimeout)
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                var affectedRows = await connection.ExecuteAsync(
                    command, parms,
                    commandType: commandType,
                    commandTimeout: commandTimeout);

                // -1 means SP used SET NOCOUNT ON — treat as success
                return ResultData<int>.Ok(affectedRows == -1 ? 1 : affectedRows);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "[ExecuteAsync] SQL error. Command: {Command}", command);
                return ResultData<int>.Fail($"Database error: {ex.Message}", ResultStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ExecuteAsync] Unexpected error. Command: {Command}", command);
                return ResultData<int>.Fail("Unexpected error occurred", ResultStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Returns a single scalar value (COUNT, MAX, a string, etc.).
        /// </summary>
        public async Task<ResultData<T>> GetScalarAsync<T>(
            string command,
            object? parms = null,
            CommandType commandType = CommandType.Text,
            int commandTimeout = DefaultTimeout)
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                var result = await connection.ExecuteScalarAsync<T>(
                    command, parms,
                    commandType: commandType,
                    commandTimeout: commandTimeout);

                if (result is null)
                    return ResultData<T>.Fail("Record not found", ResultStatusCode.NotFound);

                return ResultData<T>.Ok(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "[GetScalarAsync] SQL error. Command: {Command}", command);
                return ResultData<T>.Fail($"Database error: {ex.Message}", ResultStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetScalarAsync] Unexpected error. Command: {Command}", command);
                return ResultData<T>.Fail("Unexpected error occurred", ResultStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Returns GridReader for reading multiple result sets.
        /// NOTE: Caller is responsible for disposing the connection via using block.
        /// </summary>
        public async Task<ResultData<SqlMapper.GridReader>> GetQueryMultipleAsync(
            string command,
            object? parms = null,
            CommandType commandType = CommandType.Text,
            int commandTimeout = DefaultTimeout)
        {
            try
            {
                // NOTE: Connection is intentionally NOT disposed here —
                // GridReader needs it alive until caller finishes reading.
                var connection = CreateConnection();
                await connection.OpenAsync();

                var multi = await connection.QueryMultipleAsync(
                    command, parms,
                    commandType: commandType,
                    commandTimeout: commandTimeout);

                return ResultData<SqlMapper.GridReader>.Ok(multi);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "[GetQueryMultipleAsync] SQL error. Command: {Command}", command);
                return ResultData<SqlMapper.GridReader>.Fail($"Database error: {ex.Message}", ResultStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetQueryMultipleAsync] Unexpected error. Command: {Command}", command);
                return ResultData<SqlMapper.GridReader>.Fail("Unexpected error occurred", ResultStatusCode.InternalServerError);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // NEW: SP WITH OUTPUT PARAMETER (e.g. Login)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Executes a stored procedure that returns one row + an OUTPUT parameter (@StatusMsg).
        /// Use for login or any SP that signals success/fail via output param.
        /// SP must have: @StatusMsg NVARCHAR(100) OUTPUT
        /// </summary>
        public async Task<ResultData<T>> GetWithStatusAsync<T>(
            string command,
            object? parms = null,
            int commandTimeout = DefaultTimeout)
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                var dp = new DynamicParameters(parms);
                dp.Add("@StatusMsg",
                    dbType: DbType.String,
                    size: 100,
                    direction: ParameterDirection.Output);

                var result = await connection.QueryFirstOrDefaultAsync<T>(
                    command, dp,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: commandTimeout);

                var status = dp.Get<string>("@StatusMsg");

                if (status != "Success")
                    return ResultData<T>.Fail(
                        string.IsNullOrWhiteSpace(status) ? "Operation failed" : status,
                        ResultStatusCode.BadRequest);

                if (result is null)
                    return ResultData<T>.Fail("No data returned", ResultStatusCode.NotFound);

                return ResultData<T>.Ok(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "[GetWithStatusAsync] SQL error. Command: {Command}", command);
                return ResultData<T>.Fail($"Database error: {ex.Message}", ResultStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetWithStatusAsync] Unexpected error. Command: {Command}", command);
                return ResultData<T>.Fail("Unexpected error occurred", ResultStatusCode.InternalServerError);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // TRANSACTIONAL
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Executes a command with optional external transaction support.
        /// Pass a transaction to participate in a larger unit of work.
        /// </summary>
        public async Task<ResultData<int>> ExecuteAsync(
            string command,
            object? parms,
            CommandType commandType,
            SqlTransaction? transaction,
            int commandTimeout = DefaultTimeout)
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                using var trans = transaction ?? connection.BeginTransaction();

                var affectedRows = await connection.ExecuteAsync(
                    command, parms,
                    commandType: commandType,
                    transaction: trans,
                    commandTimeout: commandTimeout);

                if (transaction == null)
                    trans.Commit();

                return ResultData<int>.Ok(affectedRows == -1 ? 1 : affectedRows);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "[ExecuteAsync+Trans] SQL error. Command: {Command}", command);
                return ResultData<int>.Fail($"Database error: {ex.Message}", ResultStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ExecuteAsync+Trans] Unexpected error. Command: {Command}", command);
                return ResultData<int>.Fail("Unexpected error occurred", ResultStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Executes multiple commands in a single transaction.
        /// All succeed or all roll back.
        /// </summary>
        public async Task<ResultData<int>> ExecuteTransactionAsync(
            IEnumerable<(string Command, object? Params, CommandType CommandType)> commands,
            int commandTimeout = DefaultTimeout)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var trans = connection.BeginTransaction();

            try
            {
                int totalAffected = 0;

                foreach (var (cmd, parms, cmdType) in commands)
                {
                    var rows = await connection.ExecuteAsync(
                        cmd, parms,
                        commandType: cmdType,
                        transaction: trans,
                        commandTimeout: commandTimeout);

                    totalAffected += rows == -1 ? 1 : rows;
                }

                trans.Commit();
                return ResultData<int>.Ok(totalAffected);
            }
            catch (SqlException ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "[ExecuteTransactionAsync] SQL error — transaction rolled back.");
                return ResultData<int>.Fail($"Database error: {ex.Message}", ResultStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "[ExecuteTransactionAsync] Unexpected error — transaction rolled back.");
                return ResultData<int>.Fail("Unexpected error occurred", ResultStatusCode.InternalServerError);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // NEW: PAGED QUERY
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a paged result with data + total count from a stored procedure.
        /// SP must return two result sets: first = data rows, second = SELECT COUNT(*).
        /// </summary>
        public async Task<ResultData<PagedResult<T>>> GetPagedAsync<T>(
            string command,
            object? parms = null,
            CommandType commandType = CommandType.StoredProcedure,
            int commandTimeout = DefaultTimeout)
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                using var multi = await connection.QueryMultipleAsync(
                    command, parms,
                    commandType: commandType,
                    commandTimeout: commandTimeout);

                var data = (await multi.ReadAsync<T>()).ToList();
                var total = await multi.ReadFirstOrDefaultAsync<int>();

                var paged = new PagedResult<T>
                {
                    Data = data,
                    TotalCount = total
                };

                return ResultData<PagedResult<T>>.Ok(paged);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "[GetPagedAsync] SQL error. Command: {Command}", command);
                return ResultData<PagedResult<T>>.Fail($"Database error: {ex.Message}", ResultStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetPagedAsync] Unexpected error. Command: {Command}", command);
                return ResultData<PagedResult<T>>.Fail("Unexpected error occurred", ResultStatusCode.InternalServerError);
            }
        }


        public async Task<ResultData<string>> SignupTenantAsync(DynamicParameters parms,string spName)
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                await connection.ExecuteAsync(
                    spName,
                    parms,
                    commandType: CommandType.StoredProcedure);

                var message = parms.Get<string>("@ResultMessage");
                var userID = parms.Get<int>("@UserID");

                // Only return UserID on success
                return message == "Success"
                    ? ResultData<string>.Ok(userID.ToString())
                    : ResultData<string>.Fail(message, ResultStatusCode.BadRequest);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "[SignupTenantAsync] SQL error.");
                return ResultData<string>.Fail($"Database error: {ex.Message}", ResultStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SignupTenantAsync] Unexpected error.");
                return ResultData<string>.Fail("Unexpected error occurred", ResultStatusCode.InternalServerError);
            }
        }
    }
    // ─────────────────────────────────────────────────────────────
    // SUPPORTING MODEL
    // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Holds paged data + total record count for pagination.
        /// </summary>
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
