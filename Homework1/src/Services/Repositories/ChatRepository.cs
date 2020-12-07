using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Homework1.Database.Entities;

namespace Homework1.Services.Repositories
{
    public static class ChatRepository
    {
        public static async Task<int?> GetAdminByIds(this IDbConnection db, int chatId, int adminId)
        {
            return await db.QueryFirstOrDefaultAsync<int?>
                ($"SELECT admin_id FROM chats WHERE id=@ChatId and admin_id=@AdminId" , new {ChatId = chatId, AdminId = adminId});
        }
        
        public static async Task<IEnumerable<User>> GetUser(this IDbConnection db, int chatId)
        {
            return await db.QueryAsync<User>($"SELECT * FROM users WHERE user_chats.chat_id=@ChatId and " +
                                             $"user_chats.user_id=users.id", new {ChatId = chatId});
        }
        
        public static async Task<IEnumerable<Chat>> GetChats(this IDbConnection db, int userId)
        {
            return await db.QueryAsync<Chat>($"SELECT * FROM chats WHERE user_id=@UserId and flag='{true}'", new {UserId = userId});
        }
        
        public static async Task<IEnumerable<int>> GetChatsId(this IDbConnection db, int userId)
        {
            return await db.QueryAsync<int>($"SELECT chat_id FROM chats WHERE user_id=@UserId and flag='{true}'", new {UserId = userId});
        }
        
        public static async Task<IEnumerable<int>> InsertInChat(this IDbConnection db, string name, int userID)
        {
            return await db.QueryAsync<int>($"INSERT INTO chats (name, admin_id) VALUES (@Name, @UserID); " +
                                     $"SELECT lastval();", new {Name = name, UserID = userID}); 
        }
        
        public static async Task<int?> GetAdminId(this IDbConnection db, int chatId, int adminId)
        {
            return await db.QueryFirstOrDefaultAsync<int?>($"SELECT admin_id FROM chats WHERE id=@ChatId and admin_id=@AdminId", 
                new {ChatId = chatId, AdminId = adminId}); 
        }
        
        public static async Task<int?> CheckChat(this IDbConnection db, int chatId)
        {
            return await db.QueryFirstOrDefaultAsync<int?>("SELECT id FROM chats WHERE id=@Id", new {Id = chatId});
        }
        
        public static async Task<UserChat> GetChat(this IDbConnection db, int chatId, int userId)
        {
            return await db.QueryFirstOrDefaultAsync<UserChat>
                ($"SELECT * FROM user_chats WHERE chat_id=@ChatId and user_id=@UserId", 
                new {ChatId = chatId, UserId = userId});
        }
        
        public static async Task InsertUserChat(this IDbConnection db, int chatId, int userId)
        {
            await db.ExecuteAsync("INSERT INTO user_chats (chat_id, user_id, flag) VALUES (@ChatId, @UserId, @Flag)",
                new UserChat()
                {
                    ChatId = chatId,
                    UserId = userId,
                    Flag = true
                });
        }
    }
}