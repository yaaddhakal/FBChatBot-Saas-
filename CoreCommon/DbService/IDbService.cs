using CoreCommon.HelperCommon;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CoreCommon.DbService
{
    public interface IDbService
    {
        // Non-transactional operations
       Task<ResultData<T>> GetAsync<T>(string command, object? parms = null, CommandType commandType = CommandType.Text);

        Task<ResultData<List<T>>> GetAllAsync<T>(string command, object? parms = null, CommandType commandType = CommandType.Text);

        Task<ResultData<T>> GetScalarAsync<T>(string command, object? parms = null, CommandType commandType = CommandType.Text);

        Task<ResultData<SqlMapper.GridReader>> GetQueryMultipleAsync(string command, object? parms = null, CommandType commandType = CommandType.Text);

        // Transactional operations
        Task<ResultData<int>> ExecuteAsync(string command, object? parms = null, CommandType commandType = CommandType.Text, SqlTransaction? transaction = null);


        // Generic transaction wrapper
        Task<ResultData<int>> ExecuteTransactionAsync(IEnumerable<(string Command, object? Params, CommandType CommandType)> commands);
        
    }
}
