using GraphQL;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using MyShop.Core.Services;
using MyShop.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Data.Repositories
{
    public class GraphQLUserRepository : GraphQLRepositoryBase<User>, IUserRepository
    {
        public GraphQLUserRepository(GraphQLService graphQLService) 
            : base(graphQLService, "user")
        {
        }

        public override async Task<User?> GetByIdAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetUser($id: Int!) {
                        user(id: $id) {
                            id
                            username
                            email
                            role
                            isActive
                            createdAt
                        }
                    }",
                Variables = new { id }
            };

            var response = await _graphQLService.Client.SendQueryAsync<UserResponse>(request);
            return response.Data?.User;
        }

        public override async Task<List<User>> GetAllAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetUsers {
                        users {
                            users {
                                id
                                username
                                email
                                role
                                isActive
                                createdAt
                            }
                            total
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<UsersResponse>(request);
            // Backend trả về object có field 'users' là mảng, nên ta lấy .Users.Users
            return response.Data?.Users?.Users ?? new List<User>();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetUserByUsername($username: String!) {
                        userByUsername(username: $username) {
                            id
                            username
                            email
                            role
                            isActive
                        }
                    }",
                Variables = new { username }
            };

            var response = await _graphQLService.Client.SendQueryAsync<UserByUsernameResponse>(request);
            
            if (response.Errors != null && response.Errors.Length > 0)
            {
                var error = string.Join(", ", response.Errors.Select(e => e.Message));
                throw new Exception($"GraphQL error: {error}");
            }

            return response.Data?.UserByUsername;
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            return false; // Authentication should use IAuthService.LoginAsync
        }

        public override async Task<User> AddAsync(User entity)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation CreateUser($input: CreateUserInput!) {
                        createUser(input: $input) {
                            id
                            username
                            email
                            role
                            isActive
                            createdAt
                        }
                    }",
                Variables = new
                {
                    input = new
                    {
                        username = entity.Username,
                        email = entity.Email,
                        password = entity.PasswordHash, // Hash should be provided by ViewModel or handled by backend
                        role = entity.Role.ToString().ToUpper()
                    }
                }
            };

            var response = await _graphQLService.Client.SendMutationAsync<CreateUserResponse>(request);
            return response.Data?.CreateUser ?? throw new Exception("Failed to create user.");
        }

        public override async Task UpdateAsync(User entity)
        {
            var updateInput = new System.Collections.Generic.Dictionary<string, object?>
            {
                { "username", entity.Username },
                { "email", entity.Email },
                { "role", entity.Role.ToString().ToUpper() },
                { "isActive", entity.IsActive }
            };

            if (!string.IsNullOrWhiteSpace(entity.PasswordHash))
            {
                updateInput.Add("password", entity.PasswordHash);
            }

            var request = new GraphQLRequest
            {
                Query = @"
                    mutation UpdateUser($id: Int!, $input: UpdateUserInput!) {
                        updateUser(id: $id, input: $input) {
                            id
                            username
                            email
                            role
                            isActive
                        }
                    }",
                Variables = new
                {
                    id = entity.Id,
                    input = updateInput
                }
            };

            await _graphQLService.Client.SendMutationAsync<UpdateUserResponse>(request);
        }

        public override async Task DeleteAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation DeleteUser($id: Int!) {
                        deleteUser(id: $id)
                    }",
                Variables = new { id }
            };

            await _graphQLService.Client.SendMutationAsync<object>(request);
        }

        public override Task<int> CountAsync() => throw new NotImplementedException();

        private class UserResponse { public User? User { get; set; } }
        private class UsersResponse { public UserListResponse? Users { get; set; } }
        private class UserListResponse { public List<User>? Users { get; set; } public int Total { get; set; } }
        private class UserByUsernameResponse { [JsonPropertyName("userByUsername")] public User? UserByUsername { get; set; } }
        private class CreateUserResponse { public User? CreateUser { get; set; } }
        private class UpdateUserResponse { public User? UpdateUser { get; set; } }
    }
}
