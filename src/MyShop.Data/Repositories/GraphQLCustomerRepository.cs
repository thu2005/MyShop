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
    public class GraphQLCustomerRepository : GraphQLRepositoryBase<Customer>, ICustomerRepository
    {
        public GraphQLCustomerRepository(GraphQLService graphQLService)
            : base(graphQLService, "customer")
        {
        }

        public override async Task<Customer?> GetByIdAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetCustomer($id: Int!) {
                        customer(id: $id) {
                            id
                            name
                            email
                            phone
                            address
                            isMember
                            memberSince
                            totalSpent
                            notes
                            createdAt
                            updatedAt
                        }
                    }",
                Variables = new { id }
            };

            var response = await _graphQLService.Client.SendQueryAsync<CustomerResponse>(request);
            return response.Data?.Customer;
        }

        public override async Task<List<Customer>> GetAllAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetCustomers {
                        customers {
                            customers {
                                id
                                name
                                email
                                phone
                                address
                                isMember
                                memberSince
                                totalSpent
                                notes
                                createdAt
                                updatedAt
                            }
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<CustomersQueryResponse>(request);
            return response.Data?.Customers?.Customers ?? new List<Customer>();
        }

        public async Task<(List<Customer> customers, int total)> GetCustomersAsync(
            int page = 1,
            int pageSize = 20,
            string? searchText = null,
            bool? isMember = null)
        {
            var filter = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filter["name"] = searchText;
            }
            if (isMember.HasValue)
            {
                filter["isMember"] = isMember.Value;
            }

            var request = new GraphQLRequest
            {
                Query = @"
                    query GetCustomers($pagination: PaginationInput, $filter: CustomerFilterInput) {
                        customers(pagination: $pagination, filter: $filter) {
                            customers {
                                id
                                name
                                email
                                phone
                                address
                                isMember
                                memberSince
                                totalSpent
                                notes
                                createdAt
                                updatedAt
                            }
                            total
                        }
                    }",
                Variables = new
                {
                    pagination = new { page, pageSize },
                    filter = filter.Count > 0 ? filter : null
                }
            };

            var response = await _graphQLService.Client.SendQueryAsync<CustomersQueryResponse>(request);
            var data = response.Data?.Customers;
            return (data?.Customers ?? new List<Customer>(), data?.Total ?? 0);
        }

        public override async Task<Customer> AddAsync(Customer entity)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation CreateCustomer($input: CreateCustomerInput!) {
                        createCustomer(input: $input) {
                            id
                            name
                            email
                            phone
                            address
                            isMember
                            memberSince
                            totalSpent
                            notes
                            createdAt
                            updatedAt
                        }
                    }",
                Variables = new
                {
                    input = new
                    {
                        name = entity.Name,
                        email = entity.Email,
                        phone = entity.Phone,
                        address = entity.Address,
                        isMember = entity.IsMember,
                        notes = entity.Notes
                    }
                }
            };

            var response = await _graphQLService.Client.SendMutationAsync<CreateCustomerResponse>(request);
            return response.Data?.CreateCustomer ?? throw new Exception("Failed to create customer");
        }

        public override async Task UpdateAsync(Customer entity)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation UpdateCustomer($id: Int!, $input: UpdateCustomerInput!) {
                        updateCustomer(id: $id, input: $input) {
                            id
                            name
                            email
                            phone
                            address
                            isMember
                            memberSince
                            totalSpent
                            notes
                            updatedAt
                        }
                    }",
                Variables = new
                {
                    id = entity.Id,
                    input = new
                    {
                        name = entity.Name,
                        email = entity.Email,
                        phone = entity.Phone,
                        address = entity.Address,
                        isMember = entity.IsMember,
                        notes = entity.Notes
                    }
                }
            };

            await _graphQLService.Client.SendMutationAsync<UpdateCustomerResponse>(request);
        }

        public override async Task DeleteAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation DeleteCustomer($id: Int!) {
                        deleteCustomer(id: $id)
                    }",
                Variables = new { id }
            };

            await _graphQLService.Client.SendMutationAsync<DeleteCustomerResponse>(request);
        }

        public override async Task<int> CountAsync()
        {
            var (_, total) = await GetCustomersAsync(1, 1);
            return total;
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetCustomerByEmail($email: String!) {
                        customers(filter: { email: $email }) {
                            customers {
                                id
                                name
                                email
                                phone
                                address
                                isMember
                                memberSince
                                totalSpent
                                notes
                                createdAt
                                updatedAt
                            }
                        }
                    }",
                Variables = new { email }
            };

            var response = await _graphQLService.Client.SendQueryAsync<CustomersQueryResponse>(request);
            return response.Data?.Customers?.Customers?.FirstOrDefault();
        }

        public async Task<Customer?> GetByPhoneAsync(string phone)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetCustomerByPhone($phone: String!) {
                        customers(filter: { phone: $phone }) {
                            customers {
                                id
                                name
                                email
                                phone
                                address
                                isMember
                                memberSince
                                totalSpent
                                notes
                                createdAt
                                updatedAt
                            }
                        }
                    }",
                Variables = new { phone }
            };

            var response = await _graphQLService.Client.SendQueryAsync<CustomersQueryResponse>(request);
            return response.Data?.Customers?.Customers?.FirstOrDefault();
        }

        public async Task<List<Customer>> SearchAsync(string keyword)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query SearchCustomers($keyword: String!) {
                        customers(filter: { name: $keyword }) {
                            customers {
                                id
                                name
                                email
                                phone
                                address
                                isMember
                                memberSince
                                totalSpent
                                notes
                                createdAt
                                updatedAt
                            }
                        }
                    }",
                Variables = new { keyword }
            };

            var response = await _graphQLService.Client.SendQueryAsync<CustomersQueryResponse>(request);
            return response.Data?.Customers?.Customers ?? new List<Customer>();
        }

        // Response types
        private class CustomerResponse { public Customer? Customer { get; set; } }
        
        private class CustomersQueryResponse
        {
            public CustomerListData? Customers { get; set; }
        }
        
        private class CustomerListData
        {
            public List<Customer>? Customers { get; set; }
            public int Total { get; set; }
        }
        
        private class CustomersResponse
        {
            public CustomerListData? Customers { get; set; }
        }
        
        private class CreateCustomerResponse { public Customer? CreateCustomer { get; set; } }
        private class UpdateCustomerResponse { public Customer? UpdateCustomer { get; set; } }
        private class DeleteCustomerResponse { public bool DeleteCustomer { get; set; } }
    }
}
