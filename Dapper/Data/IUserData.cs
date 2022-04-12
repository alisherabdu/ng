using Dapper.Models;

namespace Dapper.Data;

interface IUserData {
    Task<IEnumerable<UserModel>> GetUsers();
        
}