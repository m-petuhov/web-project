using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Homework1.Database;
using Homework1.Database.Entities;
using Homework1.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace Homework1.Services.Repositories
{
    public static class UserRepository
    {
        public static async Task<User> GetUserById(this IDbConnection db, int id)
        {
            return await db.QueryFirstOrDefaultAsync<User>("select * from users where id=@Id", new {Id = id});
        }
        
        public static void UpdateUser(this IDbConnection db, int id, UpdateUserInfoRequest request)
        {
            db.Execute("UPDATE users SET first_name=@FirstName, last_name=@LastName, patronymic=@Patronymic, " +
                       $"nick_name=@NickName, email=@Email, phone_number=@PhoneNumber, description=@Description where id=@ID", 
                new {FirstName = request.FirstName, 
                    LastName = request.LastName,
                    Patronymic = request.Patronymic,
                    NickName = request.NickName, 
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Description = request.Description,
                    ID = id,
                });
        }
        
        public static async Task<User> GetUserByEmail(this IDbConnection db, string email)
        {
            return await db.QueryFirstOrDefaultAsync<User>("SELECT * FROM users WHERE email=@Email",
                new {Email = email});
        }
        
        public static async Task<User> GetUserByNickName(this IDbConnection db, string nick)
        {
            return await db.QueryFirstOrDefaultAsync<User>("SELECT * FROM users WHERE nick_name=@Nick",
                new {Nick = nick});
        }
        
        public static async Task<string> CheckUniqueNickName(this IDbConnection db, string nick_name, int id)
        {
            return await db.QueryFirstOrDefaultAsync<string>("SELECT email FROM users WHERE nick_name=@Nick and id<>@ID",
                new {Nick = nick_name, ID = id});
        }
        
        public static async Task<string> CheckUniqueEmail(this IDbConnection db, string email, int id)
        {
            return await db.QueryFirstOrDefaultAsync<string>("SELECT nick_name FROM users WHERE email=@Email and id<>@ID",
                new {Email = email, ID = id});
        }
        
        public static async Task<IEnumerable<int>> InserUser(this IDbConnection db, User user)
        {
            return await db.QueryAsync<int>("INSERT INTO users (first_name, last_name, patronymic, nick_name, email, password, phone_number, invited_at, description, department_id, current_salary_rate_id) " +
                                            "VALUES (@FirstName, @LastName, @Patronymic, @NickName, @Email, @Password, @PhoneNumber, @InvitedAt, @Description, @DepartmentId, @CurrentSalaryRateId); " +
                                            "SELECT lastval(); ", user);
        }
    }
}