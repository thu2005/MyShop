# MyShop - E-commerce Management System

A modern desktop application for managing products, orders, customers, and generating business reports. Built with a **Modern Fullstack Architecture** using **WinUI 3 (Frontend)** and **Node.js (Express) + GraphQL (Backend)**.

---

## 🔧 Prerequisites

Install these before starting:

| Software | Required | Purpose |
|----------|----------|----------|
| Visual Studio 2022 | ✅ | Frontend (Load .NET Desktop & WinUI 3 workloads) |
| .NET 8.0 SDK | ✅ | Frontend development |
| Node.js (v18+) | ✅ | Backend development (TypeScript) |
| Docker Desktop | ✅ | PostgreSQL Database container |

---

## 🏗️ Architecture

- **Frontend**: WinUI 3, MVVM, GraphQL-Client (.NET).
- **Backend**: Node.js, **Express**, Apollo Server, **TypeScript**, **Prisma ORM**.
- **Database**: PostgreSQL (Dockerized).
- **Security**: JWT Authentication, BCrypt Hashing, RBAC (Role-Based Access Control).

---

## 🚀 Getting Started

### 1. Backend & Database Setup
```bash
# Navigate to backend folder
cd src/MyShop.Backend

# Start PostgreSQL database via Docker
docker-compose up -d

# Install dependencies
npm install

# Seed the database (REQUIRED for first-time setup)
npm run seed

# Start development server
npm run dev
```

### 2. Frontend Setup
1. Open `src/MyShop.sln` in **Visual Studio 2022**.
2. Restore NuGet packages.
3. Set **MyShop.App** as Startup Project.
4. Press `F5` to run.
5. **Note:** Use the **Config Screen** (on the Login page) to change the Server URL if not running on localhost.

### 🔑 Default Dev Accounts (Pre-seeded)

| Username | Password | Role |
|----------|----------|----------|
| `admin` | `Admin@123456` | **ADMIN** |
| `manager1` | `Password@123` | **MANAGER** |
| `staff1` | `Password@123` | **STAFF** |

---

## 📖 Team Resources

| Target | Guide Link |
|----------|----------|
| **Division** | [Team Work Division (Who does What?)](docs/team-work-division.md) |
| **Workflow** | [How to implement a Vertical Slice (5 Steps)](feature-development.md) |
| **Security** | [Security & RBAC Technical Guide](docs/security-and-rbac.md) |
| **Data Access** | [GraphQL Repository Base Pattern](src/MyShop.Data/Repositories/Base/GraphQLRepositoryBase.cs) |

---

## 📁 Project Structure

- `src/MyShop.App`: WinUI Views, ViewModels, and UI Logic.
- `src/MyShop.Core`: Domain Models, Interfaces, and Security Services.
- `src/MyShop.Data`: GraphQL Data Access Layer (Repositories).
- `src/MyShop.Backend`: Node.js Express & Apollo Server source.

---

## 🐛 Troubleshooting

### Port 5432 (Postgres) already in use
Check if you have a local PostgreSQL installed:
`netstat -ano | findstr :5432`
Stop the local service or change the port in `docker-compose.yml`.

### Backend "npm install" fails
Make sure you have Node.js v18+ installed. Try clearing cache:
`npm cache clean --force`

### WinUI "Windows SDK" missing
Ensure you installed the **Windows 10 SDK (10.0.19041.0)** via Visual Studio Installer.

### Database connection errors
Verify Docker is running and the container is UP:
`docker-compose ps`

---
# MyShop - Team Work Division (Fullstack)

## 👑 Person 1: Infrastructure & Authentication
**Role:** Team Lead + Architecture

### 🌐 Backend Layer (src/MyShop.Backend)
- `prisma/schema.prisma` (Shared Base)
- `src/graphql/resolvers/auth.resolver.ts` (Login/JWT Logic)
- `src/graphql/resolvers/user.resolver.ts`
- `src/index.ts` (Server Configuration)

### 🎨 UI Layer (Presentation - src/MyShop.App)
- `Views/LoginScreen.xaml`
- `Views/LoginScreen.xaml.cs`
- `Views/ConfigScreen.xaml`
- `Views/ConfigScreen.xaml.cs`
- `ViewModels/LoginViewModel.cs`
- `ViewModels/ConfigViewModel.cs`

### 💼 Business Layer (Application Logic - src/MyShop.Core)
- `Services/AuthService.cs`
- `Services/ConfigService.cs`
- `Services/EncryptionService.cs`
- `Services/AuthorizationService.cs`
- `Helpers/SessionManager.cs`

### 💾 Data Layer (Database Access - src/MyShop.Data)
- `Repositories/Base/GraphQLRepositoryBase.cs` (⭐ Shared Base Class)
- `Repositories/GraphQLUserRepository.cs`
- `../../MyShop.Core/Models/User.cs`

### 🔌 Interfaces (src/MyShop.Core)
- `Interfaces/Repositories/IRepository.cs`
- `Interfaces/Repositories/IUserRepository.cs`
- `Interfaces/Services/IAuthService.cs`
- `Interfaces/Services/IConfigService.cs`
- `Interfaces/Services/IAuthorizationService.cs`

---

## 🛍️ Person 2: Products Management
**Role:** Products Feature Owner

### 🌐 Backend Layer (src/MyShop.Backend)
- `prisma/schema.prisma` (Products & Category)
- `src/graphql/resolvers/product.resolver.ts`
- `src/graphql/resolvers/category.resolver.ts`

### 🎨 UI Layer (Presentation - src/MyShop.App)
- `Views/ProductsScreen.xaml`
- `Views/ProductsScreen.xaml.cs`
- `Views/Dialogs/AddProductDialog.xaml`
- `Views/Dialogs/EditProductDialog.xaml`
- `ViewModels/ProductViewModel.cs`
- `ViewModels/ProductDetailViewModel.cs`

### 💼 Business Layer (Application Logic - src/MyShop.Core)
- `Services/ProductService.cs`
- `Services/CategoryService.cs`
- `Services/ImportService.cs`
- `Strategies/Sorting/ISortStrategy.cs`
- `Strategies/Sorting/SortByNameStrategy.cs`
- `Strategies/Sorting/SortByPriceStrategy.cs`
- `Strategies/Sorting/SortByStockStrategy.cs`
- `Strategies/Sorting/SortByPopularityStrategy.cs`
- `Helpers/ExcelImporter.cs`

### 💾 Data Layer (Database Access - src/MyShop.Data)
- `Repositories/GraphQLProductRepository.cs`
- `Repositories/GraphQLCategoryRepository.cs`
- `../../MyShop.Core/Models/Product.cs`
- `../../MyShop.Core/Models/Category.cs`

### 🔌 Interfaces (src/MyShop.Core)
- `Interfaces/Repositories/IProductRepository.cs`
- `Interfaces/Repositories/ICategoryRepository.cs`
- `Interfaces/Services/IProductService.cs`
- `Interfaces/Services/ICategoryService.cs`

---

## 📦 Person 3: Orders Management
**Role:** Orders Feature Owner

### 🌐 Backend Layer (src/MyShop.Backend)
- `prisma/schema.prisma` (Orders & OrderItem)
- `src/graphql/resolvers/order.resolver.ts`

### 🎨 UI Layer (Presentation - src/MyShop.App)
- `Views/OrdersScreen.xaml`
- `Views/OrdersScreen.xaml.cs`
- `Views/Dialogs/CreateOrderDialog.xaml`
- `Views/Dialogs/OrderDetailsDialog.xaml`
- `ViewModels/OrderViewModel.cs`
- `ViewModels/OrderDetailViewModel.cs`
- `ViewModels/CreateOrderViewModel.cs`

### 💼 Business Layer (Application Logic - src/MyShop.Core)
- `Services/OrderService.cs`
- `Services/OrderItemService.cs`
- `Services/PrintService.cs`
- `Commands/CreateOrderCommand.cs`
- `Commands/UpdateOrderStatusCommand.cs`
- `Commands/CancelOrderCommand.cs`
- `Helpers/PdfGenerator.cs`

### 💾 Data Layer (Database Access - src/MyShop.Data)
- `Repositories/GraphQLOrderRepository.cs`
- `Repositories/GraphQLOrderItemRepository.cs`
- `../../MyShop.Core/Models/Order.cs`
- `../../MyShop.Core/Models/OrderItem.cs`
- `../../MyShop.Core/Models/OrderStatus.cs` (Enum)

### 🔌 Interfaces (src/MyShop.Core)
- `Interfaces/Repositories/IOrderRepository.cs`
- `Interfaces/Repositories/IOrderItemRepository.cs`
- `Interfaces/Services/IOrderService.cs`
- `Interfaces/Services/IPrintService.cs`

---

## 📊 Person 4: Dashboard & Reports
**Role:** Analytics & Reporting Feature Owner

### 🌐 Backend Layer (src/MyShop.Backend)
- `src/graphql/resolvers/dashboard.resolver.ts`

### 🎨 UI Layer (Presentation - src/MyShop.App)
- `Views/Dashboard.xaml`
- `Views/Dashboard.xaml.cs`
- `Views/ReportScreen.xaml`
- `Views/ReportScreen.xaml.cs`
- `ViewModels/DashboardViewModel.cs`
- `ViewModels/ReportViewModel.cs`
- `Controls/RevenueChart.xaml`
- `Controls/SalesChart.xaml`
- `Controls/StatCard.xaml`
- `Converters/ChartDataConverter.cs`

### 💼 Business Layer (Application Logic - src/MyShop.Core)
- `Services/ReportService.cs`
- `Services/StatisticsService.cs`
- `Factories/IReportFactory.cs`
- `Factories/ReportFactory.cs`
- `Factories/Reports/DailyReport.cs`
- `Factories/Reports/WeeklyReport.cs`
- `Factories/Reports/MonthlyReport.cs`
- `Factories/Reports/YearlyReport.cs`

### 💾 Data Layer (Database Access - src/MyShop.Data)
- (Uses Repositories from P2 & P3)
- `../../MyShop.Core/DTOs/ReportDto.cs`
- `../../MyShop.Core/DTOs/DashboardSummaryDto.cs`

### 🔌 Interfaces (src/MyShop.Core)
- `Interfaces/Services/IReportService.cs`
- `Interfaces/Services/IStatisticsService.cs`
- `Interfaces/Factories/IReportFactory.cs`

---

## 👥 Person 5: Customers, Discounts & Shared Features
**Role:** Customer Management & Shared UI Owner

### 🌐 Backend Layer (src/MyShop.Backend)
- `prisma/schema.prisma` (Customer & Discount)
- `src/graphql/resolvers/customer.resolver.ts`
- `src/graphql/resolvers/discount.resolver.ts`

### 🎨 UI Layer (Presentation - src/MyShop.App)
- `Views/CustomersScreen.xaml`
- `Views/CustomersScreen.xaml.cs`
- `Views/SettingsScreen.xaml`
- `Views/SettingsScreen.xaml.cs`
- `Views/Dialogs/AddCustomerDialog.xaml`
- `ViewModels/CustomerViewModel.cs`
- `ViewModels/SettingsViewModel.cs`
- `Controls/PaginationControl.xaml` (🔥 Shared)
- `Controls/SearchBox.xaml` (🔥 Shared)
- `Controls/DateRangePicker.xaml` (🔥 Shared)
- `Controls/FilterPanel.xaml` (🔥 Shared)
- `Controls/LoadingSpinner.xaml` (🔥 Shared)

### 💼 Business Layer (Application Logic - src/MyShop.Core)
- `Services/CustomerService.cs`
- `Services/DiscountService.cs`
- `Services/AutoSaveService.cs`
- `Services/OnboardingService.cs`
- `Strategies/Discounts/IDiscountStrategy.cs`
- `Helpers/ResponsiveLayoutHelper.cs`
- `Helpers/SettingsManager.cs`

### 💾 Data Layer (Database Access - src/MyShop.Data)
- `Repositories/GraphQLCustomerRepository.cs`
- `Repositories/GraphQLDiscountRepository.cs`
- `../../MyShop.Core/Models/Customer.cs`
- `../../MyShop.Core/Models/Discount.cs`
- `../../MyShop.Core/Models/DiscountType.cs` (Enum)
- `../../MyShop.Core/Models/AppSettings.cs`

### 🔌 Interfaces (src/MyShop.Core)
- `Interfaces/Repositories/ICustomerRepository.cs`
- `Interfaces/Repositories/IDiscountRepository.cs`
- `Interfaces/Services/ICustomerService.cs`
- `Interfaces/Services/IDiscountService.cs`
- `Interfaces/Strategies/IDiscountStrategy.cs`
