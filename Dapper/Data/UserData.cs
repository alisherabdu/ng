using Dapper.DbAccess;
using Dapper.Models;

namespace Dapper.Data;

internal class UserData: IUserData {

    private readonly ISqlDataAccess _db;
    public UserData(ISqlDataAccess db)
    {
        _db = db;
    }

    public Task<IEnumerable<UserModel>> GetUsers()=>
        _db.LoadData<UserModel, dynamic>("select * from User", new{ });

}