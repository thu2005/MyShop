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
    public class GraphQLOrderRepository : GraphQLRepositoryBase<Order>, IOrderRepository
    {
        public GraphQLOrderRepository(GraphQLService graphQLService)
            : base(graphQLService, "order")
        {
        }

        public override async Task<Order?> GetByIdAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetOrder($id: Int!) {
                        order(id: $id) {
                            id
                            orderNumber
                            customerId
                            userId
                            status
                            subtotal
                            discountId
                            discountAmount
                            taxAmount
                            total
                            notes
                            createdAt
                            updatedAt
                            completedAt
                            customer {
                                id
                                name
                                email
                                phone
                            }
                            createdBy {
                                id
                                username
                                email
                            }
                            discount {
                                id
                                code
                                name
                                value
                                type
                                maxDiscount
                                minPurchase
                                isActive
                            }
                            orderItems {
                                id
                                productId
                                quantity
                                unitPrice
                                subtotal
                                discountAmount
                                total
                                product {
                                    id
                                    name
                                    sku
                                    price
                                }
                            }
                        }
                    }",
                Variables = new { id }
            };

            var response = await _graphQLService.Client.SendQueryAsync<OrderResponse>(request);

            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
            }

            return response.Data?.Order;
        }

        public override async Task<List<Order>> GetAllAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetOrders {
                        orders(pagination: { pageSize: 10000 }) {
                            total
                            orders {
                                id
                                orderNumber
                                customerId
                                userId
                                status
                                subtotal
                                discountAmount
                                taxAmount
                                total
                                notes
                                createdAt
                                updatedAt
                                completedAt
                                customer {
                                    id
                                    name
                                    phone
                                }
                                createdBy {
                                    id
                                    username
                                }
                            }
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<OrdersResponse>(request);

            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
            }

            return response.Data?.Orders?.Orders ?? new List<Order>();
        }

        public async Task<List<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetOrdersByDateRange($from: DateTime!, $to: DateTime!) {
                        orders(filter: { dateRange: { from: $from, to: $to } }, pagination: { pageSize: 100 }) {
                            orders {
                                id
                                orderNumber
                                customerId
                                userId
                                status
                                subtotal
                                discountAmount
                                taxAmount
                                total
                                notes
                                createdAt
                                updatedAt
                                completedAt
                                customer {
                                    id
                                    name
                                    phone
                                }
                                createdBy {
                                    id
                                    username
                                }
                            }
                        }
                    }",
                Variables = new { from = startDate, to = endDate }
            };

            var response = await _graphQLService.Client.SendQueryAsync<OrdersResponse>(request);

            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
            }

            return response.Data?.Orders?.Orders ?? new List<Order>();
        }

        public async Task<List<Order>> GetByCustomerAsync(int customerId)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetOrdersByCustomer($customerId: Int!) {
                        orders(filter: { customerId: $customerId }, pagination: { pageSize: 100 }) {
                            orders {
                                id
                                orderNumber
                                status
                                total
                                createdAt
                                completedAt
                            }
                        }
                    }",
                Variables = new { customerId }
            };

            var response = await _graphQLService.Client.SendQueryAsync<OrdersResponse>(request);
            return response.Data?.Orders?.Orders ?? new List<Order>();
        }

        public async Task<List<Order>> GetByStatusAsync(OrderStatus status)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetOrdersByStatus($status: OrderStatus!) {
                        orders(filter: { status: $status }, pagination: { pageSize: 100 }) {
                            orders {
                                id
                                orderNumber
                                customerId
                                userId
                                status
                                subtotal
                                discountAmount
                                taxAmount
                                total
                                notes
                                createdAt
                                updatedAt
                                completedAt
                                customer {
                                    id
                                    name
                                    phone
                                }
                                createdBy {
                                    id
                                    username
                                }
                            }
                        }
                    }",
                Variables = new { status = status.ToString() }
            };

            var response = await _graphQLService.Client.SendQueryAsync<OrdersResponse>(request);
            return response.Data?.Orders?.Orders ?? new List<Order>();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var orders = startDate.HasValue && endDate.HasValue
                ? await GetByDateRangeAsync(startDate.Value, endDate.Value)
                : await GetAllAsync();

            return orders.Where(o => o.Status == OrderStatus.COMPLETED).Sum(o => o.Total);
        }

        public async Task<int> GetTotalOrdersAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var orders = startDate.HasValue && endDate.HasValue
                ? await GetByDateRangeAsync(startDate.Value, endDate.Value)
                : await GetAllAsync();

            return orders.Count;
        }

        public override async Task<Order> AddAsync(Order entity)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation CreateOrder($input: CreateOrderInput!) {
                        createOrder(input: $input) {
                            id
                            orderNumber
                            customerId
                            userId
                            status
                            subtotal
                            discountId
                            discountAmount
                            taxAmount
                            total
                            notes
                            createdAt
                            customer {
                                id
                                name
                            }
                            orderItems {
                                id
                                productId
                                quantity
                                unitPrice
                                total
                                product {
                                    id
                                    name
                                }
                            }
                        }
                    }",
                Variables = new
                {
                    input = new
                    {
                        customerId = entity.CustomerId,
                        status = entity.Status.ToString(), // Add status
                        items = entity.OrderItems?.Select(item => new
                        {
                            productId = item.ProductId,
                            quantity = item.Quantity,
                            unitPrice = item.UnitPrice
                        }).ToArray() ?? Array.Empty<object>(),
                        discountId = entity.DiscountId,
                        taxAmount = entity.TaxAmount,
                        notes = entity.Notes
                    }
                }
            };

            var response = await _graphQLService.Client.SendMutationAsync<CreateOrderResponse>(request);

            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"Failed to create order: {response.Errors[0].Message}");
            }

            if (response.Data?.CreateOrder == null)
            {
                throw new Exception("Server returned success but no data.");
            }

            return response.Data.CreateOrder;
        }

        public override async Task UpdateAsync(Order entity)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation UpdateOrder($id: Int!, $input: UpdateOrderInput!) {
                        updateOrder(id: $id, input: $input) {
                            id
                            orderNumber
                            status
                            customerId
                            discountId
                            subtotal
                            discountAmount
                            total
                            notes
                            customer {
                                id
                                name
                            }
                            orderItems {
                                id
                                productId
                                quantity
                                unitPrice
                                total
                            }
                        }
                    }",
                Variables = new
                {
                    id = entity.Id,
                    input = new
                    {
                        customerId = entity.CustomerId,
                        discountId = entity.DiscountId,
                        status = entity.Status.ToString(),
                        notes = entity.Notes,
                        items = entity.OrderItems?.Select(item => new
                        {
                            productId = item.ProductId,
                            quantity = item.Quantity,
                            unitPrice = item.UnitPrice
                        }).ToArray()
                    }
                }
            };

            var response = await _graphQLService.Client.SendMutationAsync<dynamic>(request);

            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"Update failed: {response.Errors[0].Message}");
            }
        }

        public override async Task DeleteAsync(int id)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    mutation CancelOrder($id: Int!) {
                        cancelOrder(id: $id) {
                            id
                            status
                        }
                    }",
                Variables = new { id }
            };

            var response = await _graphQLService.Client.SendMutationAsync<dynamic>(request);

            if (response.Errors != null && response.Errors.Any())
            {
                throw new Exception($"Cancel order failed: {response.Errors[0].Message}");
            }
        }

        public override async Task<int> CountAsync()
        {
            var request = new GraphQLRequest
            {
                Query = @"
                    query GetOrdersCount {
                        orders {
                            total
                        }
                    }"
            };

            var response = await _graphQLService.Client.SendQueryAsync<OrdersResponse>(request);
            return response.Data?.Orders?.Total ?? 0;
        }

        private class OrderResponse { public Order? Order { get; set; } }
        private class OrdersResponse { public OrdersQueryResult? Orders { get; set; } }
        private class OrdersQueryResult
        {
            public List<Order>? Orders { get; set; }
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }
        private class CreateOrderResponse { public Order? CreateOrder { get; set; } }
    }
}
