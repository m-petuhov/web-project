using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Homework1.Database;
using Homework1.Database.Entities;
using Homework1.Models.Requests;
using Homework1.Models.Responses;
using Homework1.Services.Repositories;
using Microsoft.AspNetCore.Identity;
using Qoden.Validation;

namespace Homework1.Services
{
    public interface IAccountService
    {
        Task<ProfileResponse> GetProfile(int id);
        Task<UserInfoResponse> ModifyUser(UpdateUserInfoRequest request, int userId);
        Task<UserInfoResponse> GetUserInfo(int id);
        Task CreateUser(CreateUserRequest request);
    }

    public class AccountService : IAccountService
    {
        private readonly IDbConnectionFactory _dbConnFactory;

        public AccountService(IDbConnectionFactory factory)
        {
            _dbConnFactory = factory;
        }

        public async Task<ProfileResponse> GetProfile(int id)
        {
            using (var conn = _dbConnFactory.CreateConnection())
            {
                var user = await conn.GetUserById(id);
                Check.Value(user, "Request").NotNull("User doesn't exist");

                return AutoMapper.Mapper.Map<User, ProfileResponse>(user);
            }
        }

        public async Task<UserInfoResponse> ModifyUser(UpdateUserInfoRequest request, int userId)
        {
            Check.Value(request, "Request").NotNull();
            request.Validate(ImmediateValidator.Instance);

            using (var conn = _dbConnFactory.CreateConnection())
            {
                var uniqueEmail = await conn.CheckUniqueEmail(request.Email, userId);
                Check.Value(uniqueEmail, "Request").IsNull("This email already exist");
                
                var uniqueNickName = await conn.CheckUniqueNickName(request.NickName, userId);
                Check.Value(uniqueNickName, "Request").IsNull("This email already exist");

                var dbUser = await conn.GetUserById(userId);
                Check.Value(dbUser).NotNull("User doesn't exist");
                
                conn.UpdateUser(userId, request); 
                return AutoMapper.Mapper.Map<UpdateUserInfoRequest, UserInfoResponse>(request);
            }
        }

        public async Task<UserInfoResponse> GetUserInfo(int id)
        {
            using (var conn = _dbConnFactory.CreateConnection())
            {
                var user = await conn.GetUserById(id);
                Check.Value(user, "Request").NotNull("User doesn't exist");

                return AutoMapper.Mapper.Map<User, UserInfoResponse>(user);
            }
        }

        public async Task CreateUser(CreateUserRequest request)
        {
            Check.Value(request, "Request").NotNull();
            request.Validate(ImmediateValidator.Instance);

            using (var conn = _dbConnFactory.CreateConnection())
            {
                var uniqueEmail = await conn.GetUserByEmail(request.Email);
                Check.Value(uniqueEmail, "Request").IsNull("This email already exist");

                var uniqueNickName = await conn.GetUserByNickName(request.NickName);
                Check.Value(uniqueNickName, "Request").IsNull("This email already exist");
                
                var department = await conn.GetDepartmentByName(request.DepartmentName);
                Check.Value(department, "Request").NotNull("Department name doesn't exist");

                var user = AutoMapper.Mapper.Map<CreateUserRequest, User>(request);

                user.InvitedAt = DateTime.Now;
                user.Password = HashPassword(user, request.Password);
                user.CurrentSalaryRateId = null;
                user.DepartmentId = department.Id;
                var id = await conn.InserUser(user);

                await conn.InsertRole(id.First());
            }
        }

        private static string HashPassword(User user, string password)
        {
            return new PasswordHasher<User>().HashPassword(user, password);
        }
    }
}