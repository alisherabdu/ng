namespace Dapper.DbAccess;

interface ISqlDataAccess {    
    Task<IEnumerable<T>> LoadData<T, U>(string sQLQuery, U parameters, string connectionid = "Default");
}