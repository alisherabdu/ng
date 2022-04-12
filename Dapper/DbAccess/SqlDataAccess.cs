using Dapper;  
using Microsoft.Data.Sqlite;
using System.Data;

namespace Dapper.DbAccess;

internal class SqlDataAccess: ISqlDataAccess {
    private readonly IConfiguration _config;
    public SqlDataAccess(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<T>> LoadData<T, U>(string sQLQuery, U parameters, string connectionid = "Default"){
        await using var connection = new SqliteConnection(_config.GetConnectionString(connectionid));
        return await connection.QueryAsync<T>(sQLQuery,parameters,commandType: CommandType.Text);

    }
}