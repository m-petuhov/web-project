using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Homework1.Database;
using Homework1.Database.Entities;
using Homework1.Models.Requests;
using Homework1.Models.Responses;
using Homework1.Services.Repositories;
using Qoden.Validation;

namespace Homework1.Services
{
    public interface IChatService
    {
        Task<ChatResponse> CreateChat(int userId, string name);
        Task AddUserToChat(int chatId, int userId, int adminId);
        Task DeleteUserFromChat(int chatId, int userId, int adminId);
        Task<List<int>> GetChatsId(int userId);
        Task<MessageResponse> SendMessage(int userId, SendMessageRequest request);
        Task<List<ChatResponse>> GetChats(int userId);
        Task<List<UserInfoResponse>> GetUsersFromChat(int chatId, int adminId);
    }

    public class ChatService : IChatService
    {
        private readonly IDbConnectionFactory _dbConnFactory;

        public ChatService(IDbConnectionFactory factory)
        {
            _dbConnFactory = factory;
        }

        public async Task<ChatResponse> CreateChat(int userId, string name)
        {
            using (var conn = _dbConnFactory.CreateConnection())
            {
                var user = await conn.GetUserById(userId);
                Check.Value(user).NotNull("User doesn't exist");
                var id = await conn.InsertInChat(name, user.Id);

                var chatAdmin = AutoMapper.Mapper.Map<User, UserInfoResponse>(user);
                return new ChatResponse()
                {
                    ChatId = id.First(),
                    Name = name,
                    Messages = null,
                    Admin = chatAdmin,
                    Users = new List<UserInfoResponse>() {chatAdmin}
                };
            }
        }

        public async Task AddUserToChat(int chatId, int userId, int adminId)
        {
            using (var conn = _dbConnFactory.CreateConnection())
            {
                var admin = await conn.GetAdminId(chatId, adminId);
                Check.Value(admin, "Access failed").NotNull("You don't have rules for add this user");

                var id = await conn.CheckChat(chatId);
                Check.Value(id).NotNull("Chat doesn't exist");

                var userChat = await conn.GetChat(chatId, userId);

                if (userChat != null)
                {
                    userChat.User = null;
                    userChat.Chat = null;
                }

                Check.Value(userChat).IsNull("User already in this chat");
                await conn.InsertUserChat(id.Value, userId);
            }
        }

        public async Task DeleteUserFromChat(int chatId, int userId, int adminId)
        {
            using (var conn = _dbConnFactory.CreateConnection())
            {
                var admin = await conn.QueryFirstOrDefaultAsync<int?>
                    ($"SELECT admin_id FROM chats WHERE id='{chatId}' and admin_id='{adminId}'");
                Check.Value(admin, "Access failed").NotNull("You don't have rules for delete this user");

                var userChat = await conn.QueryFirstOrDefaultAsync<UserChat>
                    ($"SELECT * FROM user_chats WHERE chat_id='{chatId}' and user_id='{userId}'");
                Check.Value(userChat).NotNull();

                await conn.ExecuteAsync($"DELETE FROM user_chats WHERE user_id='{userId}' " +
                                        $"and chat_id='{chatId}'");
            }
        }

        public async Task<MessageResponse> SendMessage(int userId, SendMessageRequest request)
        {
            request.Validate(ImmediateValidator.Instance);

            using (var conn = _dbConnFactory.CreateConnection())
            {
                var userChat = await conn.QueryFirstOrDefaultAsync<UserChat>
                        ($"SELECT * FROM user_chats WHERE chat_id='{request.ChatId}' and user_id='{userId}'");
                Check.Value(userChat).NotNull();

                var message = new Message()
                {
                    UserId = userId,
                    ChatId = request.ChatId,
                    Text = request.Message,
                    InvitedAt = DateTime.Now
                };

                await conn.ExecuteAsync("INSERT INTO messages (user_id, chat_id, text, invited_at) " +
                                        "VALUES (@UserId, @ChatId, @Text, @InvitedAt)", message);

                return AutoMapper.Mapper.Map<Message, MessageResponse>(message);
            }
        }

        public async Task<List<int>> GetChatsId(int userId)
        {
            using (var conn = _dbConnFactory.CreateConnection())
            {
                return (await conn.GetChatsId(userId)).ToList();
            }
        }

        public async Task<List<ChatResponse>> GetChats(int userId)
        {
            using (var conn = _dbConnFactory.CreateConnection())
            {
                return (await conn.GetChats(userId)).Select(u => AutoMapper.Mapper.Map<Chat, ChatResponse>(u)).ToList();
            }
        }

        public async Task<List<UserInfoResponse>> GetUsersFromChat(int chatId, int adminId)
        {
            using (var conn = _dbConnFactory.CreateConnection())
            {
                var admin = await conn.GetAdminByIds(chatId, adminId);
                Check.Value(admin, "Access failed").NotNull("You don't have rules for viewing this chat");

                return (await conn.GetUser(chatId)).Select(u => AutoMapper.Mapper.Map<User, UserInfoResponse>(u)).ToList();
            }
        }
    }
}