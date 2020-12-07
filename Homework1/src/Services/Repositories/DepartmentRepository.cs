using System.Data;
using System.Threading.Tasks;
using Dapper;
using Homework1.Database.Entities;

namespace Homework1.Services.Repositories
{
    public static class DepartmentRepository
    {
        public static async Task<Department> GetDepartmentByName(this IDbConnection db, string name)
        {
            return await db.QueryFirstOrDefaultAsync<Department>("SELECT * FROM departments WHERE " +
                                                                 $"name=@DepartmentName", new {DepartmentName = name});
        }
    }
}