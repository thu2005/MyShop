import { gql } from 'graphql-tag';

export const typeDefs = gql`
  # ==================== SCALARS ====================
  scalar DateTime
  scalar Decimal

  # ==================== ENUMS ====================
  enum UserRole {
    ADMIN
    MANAGER
    STAFF
  }

  enum OrderStatus {
    PENDING
    PROCESSING
    COMPLETED
    CANCELLED
  }

  enum DiscountType {
    PERCENTAGE
    FIXED_AMOUNT
    BUY_X_GET_Y
    MEMBER_DISCOUNT
    WHOLESALE_DISCOUNT
  }

  enum SortOrder {
    ASC
    DESC
  }

  # ==================== INPUT TYPES ====================

  # Pagination
  input PaginationInput {
    page: Int = 1
    pageSize: Int = 20
  }

  # Sorting
  input SortInput {
    field: String!
    order: SortOrder = ASC
  }

  # Filter
  input DateRangeInput {
    from: DateTime
    to: DateTime
  }

  # Auth Inputs
  input LoginInput {
    username: String!
    password: String!
  }

  input RegisterInput {
    username: String!
    email: String!
    password: String!
    role: UserRole = STAFF
  }

  # User Inputs
  input CreateUserInput {
    username: String!
    email: String!
    password: String!
    role: UserRole = STAFF
  }

  input UpdateUserInput {
    username: String
    email: String
    password: String
    role: UserRole
    isActive: Boolean
  }

  # Category Inputs
  input CreateCategoryInput {
    name: String!
    description: String
  }

  input UpdateCategoryInput {
    name: String
    description: String
    isActive: Boolean
  }

  # Product Inputs
  input CreateProductInput {
    name: String!
    description: String
    sku: String!
    barcode: String
    price: Decimal!
    costPrice: Decimal
    stock: Int!
    minStock: Int = 0
    imageUrl: String
    categoryId: Int!
  }

  input UpdateProductInput {
    name: String
    description: String
    sku: String
    barcode: String
    price: Decimal
    costPrice: Decimal
    stock: Int
    minStock: Int
    imageUrl: String
    categoryId: Int
    isActive: Boolean
  }

  input ProductFilterInput {
    name: String
    categoryId: Int
    minPrice: Decimal
    maxPrice: Decimal
    inStock: Boolean
    isActive: Boolean
  }

  # Customer Inputs
  input CreateCustomerInput {
    name: String!
    email: String
    phone: String!
    address: String
    isMember: Boolean = false
    notes: String
  }

  input UpdateCustomerInput {
    name: String
    email: String
    phone: String
    address: String
    isMember: Boolean
    notes: String
  }

  input CustomerFilterInput {
    name: String
    email: String
    phone: String
    isMember: Boolean
  }

  # Discount Inputs
  input CreateDiscountInput {
    code: String!
    name: String!
    description: String
    type: DiscountType!
    value: Decimal!
    minPurchase: Decimal
    maxDiscount: Decimal
    buyQuantity: Int
    getQuantity: Int
    startDate: DateTime
    endDate: DateTime
    usageLimit: Int
    applicableToAll: Boolean = true
    memberOnly: Boolean = false
    wholesaleMinQty: Int
  }

  input UpdateDiscountInput {
    code: String
    name: String
    description: String
    type: DiscountType
    value: Decimal
    minPurchase: Decimal
    maxDiscount: Decimal
    buyQuantity: Int
    getQuantity: Int
    startDate: DateTime
    endDate: DateTime
    usageLimit: Int
    isActive: Boolean
    applicableToAll: Boolean
    memberOnly: Boolean
    wholesaleMinQty: Int
  }

  # Order Inputs
  input OrderItemInput {
    productId: Int!
    quantity: Int!
    unitPrice: Decimal!
  }

  input CreateOrderInput {
    customerId: Int
    items: [OrderItemInput!]!
    discountId: Int
    notes: String
    taxAmount: Decimal = 0
  }

  input UpdateOrderInput {
    customerId: Int
    discountId: Int
    items: [OrderItemInput!]
    status: OrderStatus
    notes: String
  }

  input OrderFilterInput {
    status: OrderStatus
    customerId: Int
    dateRange: DateRangeInput
  }

  # ==================== TYPES ====================

  # Auth Types
  type AuthPayload {
    token: String!
    refreshToken: String!
    user: User!
  }

  # User Type
  type User {
    id: Int!
    username: String!
    email: String!
    role: UserRole!
    isActive: Boolean!
    createdAt: DateTime!
    updatedAt: DateTime!
  }

  type UserList {
    users: [User!]!
    total: Int!
    page: Int!
    pageSize: Int!
    totalPages: Int!
  }

  # Category Type
  type Category {
    id: Int!
    name: String!
    description: String
    isActive: Boolean!
    createdAt: DateTime!
    updatedAt: DateTime!
    products: [Product!]
    productCount: Int!
  }

  type CategoryList {
    categories: [Category!]!
    total: Int!
    page: Int!
    pageSize: Int!
    totalPages: Int!
  }

  # Product Type
  type Product {
    id: Int!
    name: String!
    description: String
    sku: String!
    barcode: String
    price: Decimal!
    costPrice: Decimal
    stock: Int!
    minStock: Int!
    imageUrl: String
    categoryId: Int!
    category: Category!
    isActive: Boolean!
    popularity: Int!
    createdAt: DateTime!
    updatedAt: DateTime!
  }

  type ProductList {
    products: [Product!]!
    total: Int!
    page: Int!
    pageSize: Int!
    totalPages: Int!
  }

  # Customer Type
  type Customer {
    id: Int!
    name: String!
    email: String
    phone: String!
    address: String
    isMember: Boolean!
    memberSince: DateTime
    totalSpent: Decimal!
    notes: String
    createdAt: DateTime!
    updatedAt: DateTime!
    orders: [Order!]
    orderCount: Int!
  }

  type CustomerList {
    customers: [Customer!]!
    total: Int!
    page: Int!
    pageSize: Int!
    totalPages: Int!
  }

  # Discount Type
  type Discount {
    id: Int!
    code: String!
    name: String!
    description: String
    type: DiscountType!
    value: Decimal!
    minPurchase: Decimal
    maxDiscount: Decimal
    buyQuantity: Int
    getQuantity: Int
    isActive: Boolean!
    startDate: DateTime
    endDate: DateTime
    usageLimit: Int
    usageCount: Int!
    applicableToAll: Boolean!
    memberOnly: Boolean!
    wholesaleMinQty: Int
    createdAt: DateTime!
    updatedAt: DateTime!
  }

  type DiscountList {
    discounts: [Discount!]!
    total: Int!
    page: Int!
    pageSize: Int!
    totalPages: Int!
  }

  # Order Types
  type OrderItem {
    id: Int!
    orderId: Int!
    productId: Int!
    product: Product!
    quantity: Int!
    unitPrice: Decimal!
    subtotal: Decimal!
    discountAmount: Decimal!
    total: Decimal!
  }

  type Order {
    id: Int!
    orderNumber: String!
    customerId: Int
    customer: Customer
    userId: Int!
    createdBy: User!
    status: OrderStatus!
    subtotal: Decimal!
    discountId: Int
    discount: Discount
    discountAmount: Decimal!
    taxAmount: Decimal!
    total: Decimal!
    notes: String
    createdAt: DateTime!
    updatedAt: DateTime!
    completedAt: DateTime
    orderItems: [OrderItem!]!
  }

  type OrderList {
    orders: [Order!]!
    total: Int!
    page: Int!
    pageSize: Int!
    totalPages: Int!
  }

  # Dashboard Types
  type DashboardStats {
    totalProducts: Int!
    totalOrders: Int!
    totalRevenue: Decimal!
    totalCustomers: Int!
    lowStockProducts: Int!
    pendingOrders: Int!
    todayRevenue: Decimal!
    todayOrders: Int!
  }

  type TopProduct {
    product: Product!
    quantity: Int!
    revenue: Decimal!
  }

  type RevenueByDate {
    date: String!
    revenue: Decimal!
    orders: Int!
  }

  type SalesReport {
    totalRevenue: Decimal!
    totalOrders: Int!
    averageOrderValue: Decimal!
    topProducts: [TopProduct!]!
    revenueByDate: [RevenueByDate!]!
  }

  # Report Types
  enum PeriodType {
    WEEKLY
    MONTHLY
    YEARLY
    CUSTOM
  }

  enum TimelineGrouping {
    DAY
    WEEK
    MONTH
  }

  input ReportPeriodInput {
    period: PeriodType!
    startDate: DateTime
    endDate: DateTime
  }

  input TopProductsInput {
    startDate: DateTime!
    endDate: DateTime!
  }

  input TopCustomersInput {
    startDate: DateTime!
    endDate: DateTime!
    limit: Int = 10
  }

  input TimelineInput {
    startDate: DateTime!
    endDate: DateTime!
    groupBy: TimelineGrouping = DAY
  }

  type PeriodReport {
    totalProductsSold: Int!
    totalOrders: Int!
    totalRevenue: Decimal!
    totalProfit: Decimal!
    previousTotalProductsSold: Int
    previousTotalOrders: Int
    previousTotalRevenue: Decimal
    previousTotalProfit: Decimal
    productsChange: Float
    ordersChange: Float
    revenueChange: Float
    profitChange: Float
    periodStart: DateTime!
    periodEnd: DateTime!
  }

  type ProductSalesData {
    productId: Int!
    productName: String!
    quantitySold: Int!
    revenue: Decimal!
  }

  type CustomerSalesData {
    customerId: Int
    customerName: String!
    totalOrders: Int!
    totalSpent: Decimal!
  }

  type RevenueProfit {
    date: String!
    revenue: Decimal!
    profit: Decimal!
    orders: Int!
  }

  # ==================== QUERIES ====================
  type Query {
    # Auth
    me: User!

    # Users
    users(pagination: PaginationInput, sort: SortInput): UserList!
    user(id: Int!): User
    userByUsername(username: String!): User

    # Categories
    categories(pagination: PaginationInput, sort: SortInput): CategoryList!
    category(id: Int!): Category
    activeCategories: [Category!]!

    # Products
    products(
      pagination: PaginationInput
      sort: SortInput
      filter: ProductFilterInput
    ): ProductList!
    product(id: Int!): Product
    productBySku(sku: String!): Product
    lowStockProducts(threshold: Int): [Product!]!

    # Customers
    customers(
      pagination: PaginationInput
      sort: SortInput
      filter: CustomerFilterInput
    ): CustomerList!
    customer(id: Int!): Customer

    # Discounts
    discounts(pagination: PaginationInput, sort: SortInput): DiscountList!
    discount(id: Int!): Discount
    discountByCode(code: String!): Discount
    activeDiscounts: [Discount!]!

    # Orders
    orders(
      pagination: PaginationInput
      sort: SortInput
      filter: OrderFilterInput
    ): OrderList!
    order(id: Int!): Order
    orderByNumber(orderNumber: String!): Order

    # Dashboard & Reports
    dashboardStats: DashboardStats!
    salesReport(dateRange: DateRangeInput!): SalesReport!
    
    # Report Queries
    reportByPeriod(input: ReportPeriodInput!): PeriodReport!
    topProductsByQuantity(input: TopProductsInput!): [ProductSalesData!]!
    topCustomers(input: TopCustomersInput!): [CustomerSalesData!]!
    revenueAndProfitTimeline(input: TimelineInput!): [RevenueProfit!]!

    # Commissions
    getCommissions(filter: CommissionFilterInput, pagination: PaginationInput): PaginatedCommissions!
    getUserCommissions(userId: Int!): [Commission!]!
    getCommissionStats(userId: Int, dateRange: DateRangeInput): CommissionStats!

    # Sales Targets
    getSalesTargets(filter: SalesTargetFilterInput, pagination: PaginationInput): PaginatedSalesTargets!
    getMonthlyTarget(userId: Int!, month: Int!, year: Int!): SalesTarget
    getUserTargets(userId: Int!): [SalesTarget!]!

    # App License
    validateLicense(licenseKey: String!): AppLicense
    checkTrialStatus: AppLicense
  }

  # ==================== MUTATIONS ====================
  type Mutation {
    # Auth
    login(input: LoginInput!): AuthPayload!
    register(input: RegisterInput!): AuthPayload!
    refreshToken(refreshToken: String!): AuthPayload!

    # Users
    createUser(input: CreateUserInput!): User!
    updateUser(id: Int!, input: UpdateUserInput!): User!
    deleteUser(id: Int!): Boolean!

    # Categories
    createCategory(input: CreateCategoryInput!): Category!
    updateCategory(id: Int!, input: UpdateCategoryInput!): Category!
    deleteCategory(id: Int!): Boolean!

    # Products
    createProduct(input: CreateProductInput!): Product!
    updateProduct(id: Int!, input: UpdateProductInput!): Product!
    deleteProduct(id: Int!): Boolean!
    updateProductStock(id: Int!, quantity: Int!): Product!

    # Customers
    createCustomer(input: CreateCustomerInput!): Customer!
    updateCustomer(id: Int!, input: UpdateCustomerInput!): Customer!
    deleteCustomer(id: Int!): Boolean!

    # Discounts
    createDiscount(input: CreateDiscountInput!): Discount!
    updateDiscount(id: Int!, input: UpdateDiscountInput!): Discount!
    deleteDiscount(id: Int!): Boolean!

    # Orders
    createOrder(input: CreateOrderInput!): Order!
    updateOrder(id: Int!, input: UpdateOrderInput!): Order!
    cancelOrder(id: Int!): Order!

    # Commissions
    calculateCommission(orderId: Int!): Commission!
    markCommissionPaid(id: Int!): Commission!

    # Sales Targets
    createSalesTarget(input: CreateSalesTargetInput!): SalesTarget!
    updateSalesTarget(id: Int!, input: UpdateSalesTargetInput!): SalesTarget!
    deleteSalesTarget(id: Int!): Boolean!

    # App License
    activateTrial: AppLicense!
    activateLicense(input: ActivateLicenseInput!): AppLicense!
    deactivateLicense(licenseKey: String!): Boolean!
  }

  # ==================== COMMISSION ====================
  type Commission {
    id: Int!
    userId: Int!
    orderId: Int!
    orderTotal: Decimal!
    commissionRate: Decimal!
    commissionAmount: Decimal!
    isPaid: Boolean!
    createdAt: DateTime!
    user: User!
    order: Order!
  }

  type CommissionStats {
    totalCommission: Decimal!
    paidCommission: Decimal!
    unpaidCommission: Decimal!
    totalOrders: Int!
  }

  input CommissionFilterInput {
    userId: Int
    isPaid: Boolean
    dateRange: DateRangeInput
  }

  type PaginatedCommissions {
    items: [Commission!]!
    total: Int!
    page: Int!
    pageSize: Int!
    totalPages: Int!
  }

  # ==================== SALES TARGET ====================
  type SalesTarget {
    id: Int!
    userId: Int!
    month: Int!
    year: Int!
    targetAmount: Decimal!
    achievedAmount: Decimal!
    commissionRate: Decimal!
    achievementRate: Float!
    isAchieved: Boolean!
    createdAt: DateTime!
    updatedAt: DateTime!
  }

  input CreateSalesTargetInput {
    userId: Int!
    month: Int!
    year: Int!
    targetAmount: Decimal!
    commissionRate: Decimal!
  }

  input UpdateSalesTargetInput {
    targetAmount: Decimal
    achievedAmount: Decimal
    commissionRate: Decimal
  }

  input SalesTargetFilterInput {
    userId: Int
    month: Int
    year: Int
  }

  type PaginatedSalesTargets {
    items: [SalesTarget!]!
    total: Int!
    page: Int!
    pageSize: Int!
    totalPages: Int!
  }

  # ==================== APP LICENSE (TRIAL MODE) ====================
  type AppLicense {
    id: Int!
    licenseKey: String!
    activatedAt: DateTime!
    expiresAt: DateTime!
    isActive: Boolean!
    isExpired: Boolean!
    daysRemaining: Int!
    createdAt: DateTime!
  }

  input ActivateLicenseInput {
    licenseKey: String!
  }
`;
