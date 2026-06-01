using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CoreCommon.DbService
{
    public interface IDbService
    {
        // ─────────────────────────────────────────────────────────────
        // NON-TRANSACTIONAL
        // ─────────────────────────────────────────────────────────────

        /// <summary>Returns first matching row mapped to T.</summary>
        Task<ResultData<T>> GetAsync<T>(
            string command,
            object? parms = null,
            CommandType commandType = CommandType.Text,
            int commandTimeout = 30);

        /// <summary>Returns all matching rows as List of T.</summary>
        Task<ResultData<List<T>>> GetAllAsync<T>(
            string command,
            object? parms = null,
            CommandType commandType = CommandType.Text,
            int commandTimeout = 30);

        /// <summary>Executes INSERT/UPDATE/DELETE without transaction.</summary>
        Task<ResultData<int>> ExecuteAsync(
            string command,
            object? parms = null,
            CommandType commandType = CommandType.Text,
            int commandTimeout = 30);

        /// <summary>Returns a single scalar value.</summary>
        Task<ResultData<T>> GetScalarAsync<T>(
            string command,
            object? parms = null,
            CommandType commandType = CommandType.Text,
            int commandTimeout = 30);

        /// <summary>Returns GridReader for multiple result sets.</summary>
        Task<ResultData<SqlMapper.GridReader>> GetQueryMultipleAsync(
            string command,
            object? parms = null,
            CommandType commandType = CommandType.Text,
            int commandTimeout = 30);

        // ─────────────────────────────────────────────────────────────
        // SP WITH OUTPUT PARAMETER
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Executes SP that returns one row + @StatusMsg OUTPUT param.
        /// SP must set @StatusMsg = 'Success' on success.
        /// </summary>
        Task<ResultData<T>> GetWithStatusAsync<T>(
            string command,
            object? parms = null,
            int commandTimeout = 30);

        // ─────────────────────────────────────────────────────────────
        // TRANSACTIONAL
        // ─────────────────────────────────────────────────────────────

        /// <summary>Executes command with optional external transaction.</summary>
        Task<ResultData<int>> ExecuteAsync(
            string command,
            object? parms,
            CommandType commandType,
            SqlTransaction? transaction,
            int commandTimeout = 30);

        /// <summary>Executes multiple commands — all succeed or all roll back.</summary>
        Task<ResultData<int>> ExecuteTransactionAsync(
            IEnumerable<(string Command, object? Params, CommandType CommandType)> commands,
            int commandTimeout = 30);

        // ─────────────────────────────────────────────────────────────
        // PAGED QUERY
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns paged data + total count.
        /// SP must return: 1st result set = rows, 2nd = SELECT COUNT(*).
        /// </summary>
        Task<ResultData<PagedResult<T>>> GetPagedAsync<T>(
            string command,
            object? parms = null,
            CommandType commandType = CommandType.StoredProcedure,
            int commandTimeout = 30);

        Task<ResultData<string>> SignupTenantAsync(DynamicParameters parms, string spName);
    }
}
