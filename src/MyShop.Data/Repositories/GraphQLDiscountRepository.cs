using GraphQL;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using MyShop.Core.Services;
using MyShop.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Data.Repositories
{
    public class GraphQLDiscountRepository : GraphQLRepositoryBase<Discount>, IDiscountRepository
    {
        public GraphQLDiscountRepository(GraphQLService graphQLService)
            : base(graphQLService, "discount")
        {
        }

        public override async Task<Discount?> GetByIdAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetDiscount($id: Int!) {
                        discount(id: $id) {
                            id
                            code
                            name
                            description
                            type
                            value
                            maxDiscount
                            minPurchase
                            startDate
                            endDate
                            isActive
                            createdAt
                            updatedAt
                        }
                    }",
                Variables = new { id }
            };

            var response = await _graphQLService.Client.SendQueryAsync<DiscountResponse>(request);
            return response.Data?.Discount;
        }

        public override async Task<List<Discount>> GetAllAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetDiscounts {
                        discounts(pagination: { pageSize: 100 }) {
                            discounts {
                                id
                                code
                                name
                                description
                                type
                                value
                                maxDiscount
                                minPurchase
                                startDate
                                endDate
                                isActive
                            }
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<DiscountsResponse>(request);

            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
            }

            return response.Data?.Discounts?.Discounts ?? new List<Discount>();
        }

        public async Task<List<Discount>> GetActiveDiscountsAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetActiveDiscounts {
                        activeDiscounts {
                            id
                            code
                            name
                            description
                            type
                            value
                            maxDiscount
                            minPurchase
                            startDate
                            endDate
                            isActive
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<ActiveDiscountsResponse>(request);
            return response.Data?.ActiveDiscounts ?? new List<Discount>();
        }

        public async Task<Discount?> GetByCodeAsync(string code)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetDiscountByCode($code: String!) {
                        discountByCode(code: $code) {
                            id
                            code
                            name
                            description
                            type
                            value
                            maxDiscount
                            minPurchase
                            startDate
                            endDate
                            isActive
                        }
                    }",
                Variables = new { code }
            };

            var response = await _graphQLService.Client.SendQueryAsync<DiscountResponse>(request);
            return response.Data?.Discount;
        }

        public override async Task<Discount> AddAsync(Discount entity)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation CreateDiscount($input: DiscountInput!) {
                        createDiscount(input: $input) {
                            id
                            code
                            name
                            description
                            type
                            value
                            maxDiscount
                            minPurchase
                            startDate
                            endDate
                            isActive
                            createdAt
                        }
                    }",
                Variables = new
                {
                    input = new
                    {
                        code = entity.Code,
                        description = entity.Description,
                        type = entity.Type.ToString(),
                        value = entity.Value,
                        maxDiscount = entity.MaxDiscount,
                        minPurchase = entity.MinPurchase,
                        startDate = entity.StartDate,
                        endDate = entity.EndDate,
                        isActive = entity.IsActive
                    }
                }
            };

            var response = await _graphQLService.Client.SendMutationAsync<CreateDiscountResponse>(request);
            return response.Data?.CreateDiscount ?? throw new Exception("Failed to create discount");
        }

        public override async Task UpdateAsync(Discount entity)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation UpdateDiscount($id: Int!, $input: DiscountInput!) {
                        updateDiscount(id: $id, input: $input) {
                            id
                        }
                    }",
                Variables = new
                {
                    id = entity.Id,
                    input = new
                    {
                        code = entity.Code,
                        description = entity.Description,
                        type = entity.Type.ToString(),
                        value = entity.Value,
                        maxDiscount = entity.MaxDiscount,
                        minPurchase = entity.MinPurchase,
                        startDate = entity.StartDate,
                        endDate = entity.EndDate,
                        isActive = entity.IsActive
                    }
                }
            };

            await _graphQLService.Client.SendMutationAsync<UpdateDiscountResponse>(request);
        }

        public override async Task DeleteAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation DeleteDiscount($id: Int!) {
                        deleteDiscount(id: $id)
                    }",
                Variables = new { id }
            };

            await _graphQLService.Client.SendMutationAsync<DeleteResponse>(request);
        }

        public override async Task<int> CountAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetDiscountsTotal {
                        discounts {
                            total
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<DiscountsResponse>(request);
            return response.Data?.Discounts?.Total ?? 0;
        }

        // Response types
        private class DiscountResponse
        {
            public Discount? Discount { get; set; }
        }

        private class DiscountsResponse
        {
            public DiscountsData? Discounts { get; set; }
        }

        private class DiscountsData
        {
            public List<Discount> Discounts { get; set; } = new();
            public int Total { get; set; }
        }

        private class ActiveDiscountsResponse
        {
            public List<Discount> ActiveDiscounts { get; set; } = new();
        }

        private class CreateDiscountResponse
        {
            public Discount? CreateDiscount { get; set; }
        }

        private class UpdateDiscountResponse
        {
            public Discount? UpdateDiscount { get; set; }
        }

        private class DeleteResponse
        {
            public bool DeleteDiscount { get; set; }
        }
    }
}
