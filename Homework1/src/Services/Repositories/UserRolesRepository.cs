using System.Data;
using System.Threading.Tasks;
using Dapper;
using Homework1.Database.Entities;

namespace Homework1.Services.Repositories
{
    public static class UserRolesRepository
    {
        public static async Task<int> InsertRole(this IDbConnection db, int userID)
        {
            return await db.ExecuteAsync($"INSERT INTO user_roles (user_id, role_id) VALUES (@UserID, @RoleID);", 
                new {UserID = userID, RoleID = 1});
        }
    }
}