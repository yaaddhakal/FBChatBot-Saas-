using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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

        public DBService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // ---------------- NON-TRANSACTIONAL ----------------

        public async Task<ResultData<T>> GetAsync<T>(string command, object? parms = null, CommandType commandType = CommandType.Text)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<T>(command, parms, commandType: commandType);

            if (result is null)
                return ResultData<T>.Fail("Record not found");

            return ResultData<T>.Ok(result);
        }

        public async Task<ResultData<List<T>>> GetAllAsync<T>(string command, object? parms = null, CommandType commandType = CommandType.Text)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<T>(command, parms, commandType: commandType);
            return ResultData<List<T>>.Ok(result.ToList());
        }

        public async Task<ResultData<int>> ExecuteAsync(string command, object? parms = null, CommandType commandType = CommandType.Text)
        {
            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(command, parms, commandType: commandType);
            return ResultData<int>.Ok(affectedRows);
        }

        public async Task<ResultData<T>> GetScalarAsync<T>(string command, object? parms = null, CommandType commandType = CommandType.Text)
        {
            using var connection = CreateConnection();
            var result = await connection.ExecuteScalarAsync<T>(command, parms, commandType: commandType);

            if (result is null)
                return ResultData<T>.Fail("Record not found");

            return ResultData<T>.Ok(result);
        }

        public async Task<ResultData<SqlMapper.GridReader>> GetQueryMultipleAsync(string command, object? parms = null, CommandType commandType = CommandType.Text)
        {
            var connection = CreateConnection();
            var multi = await connection.QueryMultipleAsync(command, parms, commandType: commandType);
            return ResultData<SqlMapper.GridReader>.Ok(multi);
        }

        // ---------------- TRANSACTIONAL ----------------

        public async Task<ResultData<int>> ExecuteAsync(string command, object? parms = null, CommandType commandType = CommandType.Text, SqlTransaction? transaction = null)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var trans = transaction ?? connection.BeginTransaction();
            var affectedRows = await connection.ExecuteAsync(command, parms, commandType: commandType, transaction: trans);

            if (transaction == null)
                trans.Commit();

            return ResultData<int>.Ok(affectedRows);
        }

        public async Task<ResultData<int>> ExecuteTransactionAsync(IEnumerable<(string Command, object? Params, CommandType CommandType)> commands)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var trans = connection.BeginTransaction();

            int totalAffected = 0;
            foreach (var (cmd, parms, cmdType) in commands)
            {
                totalAffected += await connection.ExecuteAsync(cmd, parms, commandType: cmdType, transaction: trans);
            }

            trans.Commit();
            return ResultData<int>.Ok(totalAffected);
        }
    }
}
