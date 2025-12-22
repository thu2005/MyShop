using GraphQL;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using MyShop.Core.Services;
using MyShop.Data.Repositories.Base;
using System;
using System.Collections.Generic;
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
                            id
                            username
                            email
                            role
                            isActive
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<UsersResponse>(request);
            return response.Data?.Users ?? new List<User>();
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
            return response.Data?.UserByUsername;
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            // Note: Validation is typically handled by the Auth service/mutations in GraphQL
            // But if we need it here, we'd call a specific login simulation or trust the auth flow.
            // For now, mirroring the intent:
            return false; // Authentication should use IAuthService.LoginAsync
        }

        public override async Task<User> AddAsync(User entity)
        {
            // Implement based on backend schema if needed
            throw new NotImplementedException("User creation via repository not yet implemented in GraphQL.");
        }

        public override Task UpdateAsync(User entity) => throw new NotImplementedException();
        public override Task DeleteAsync(int id) => throw new NotImplementedException();
        public override Task<int> CountAsync() => throw new NotImplementedException();

        private class UserResponse { public User? User { get; set; } }
        private class UsersResponse { public List<User>? Users { get; set; } }
        private class UserByUsernameResponse { public User? UserByUsername { get; set; } }
    }
}
