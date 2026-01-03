# MyShop - E-commerce Management System

A modern desktop application for managing products, orders, customers, and generating business reports. Built with a **Modern Fullstack Architecture** using **WinUI 3 (Frontend)** and **Node.js (Express) + GraphQL (Backend)**.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue)
![Node.js](https://img.shields.io/badge/Node.js-18+-green)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue)

---

## 📋 Table of Contents
- [Features](#-features)
- [Architecture](#-architecture)
- [Prerequisites](#-prerequisites)
- [Getting Started](#-getting-started)
- [Project Structure](#-project-structure)
- [Advanced Features](#-advanced-features)
- [Testing](#-testing)
- [Troubleshooting](#-troubleshooting)
- [Team Resources](#-team-resources)
- [License](#-license)

---

## ✨ Features

### Core Functionality
- **Product Management**: Add, edit, delete products and categories with real-time inventory tracking
- **Order Processing**: Create, manage, and track orders with automatic commission calculation
- **Customer Management**: Comprehensive customer database with membership tiers (Member, Standard)
- **Dashboard & Analytics**: Real-time business insights with interactive charts and reports
- **User Authentication**: Secure JWT-based authentication with role-based access control
- **Print System**: Generate professional PDF invoices and reports

### Advanced Features
- **Auto-Save**: Automatic draft saving when creating orders to prevent data loss.
- **Discount System**: Flexible promotion management including member-exclusive deals.
- **Commission System**: Automatic sales commission calculation based on order completion.
- **Trial Mode**: 15-day full-feature trial period logic with hardware binding.
- **Printing System**: Generate clean, print-ready HTML invoices and reports.
- **Database Backup**: Support for database backup via PostgreSQL tools.
- **Onboarding System**: Interactive first-time user guidance.

### Role-Based Access Control (RBAC)
Three distinct user roles with different permissions:

#### **ADMIN** - Full System Access
- Complete control over products, categories, and pricing (including purchase prices)
- Add, edit, delete staff accounts
- Access all orders and customer data
- View comprehensive reports: profit margins, revenue, and staff performance
- Manage discount campaigns
- Separate onboarding experience

#### **MANAGER** - Operational Control
- Manage sales operations and view commission data
- Access store-wide revenue reports
- Monitor all orders and customer interactions
- Cannot modify products or staff accounts
- Cannot view purchase prices or profit margins

#### **STAFF** - Sales Focus
- View products and prices (excluding purchase prices)
- Create and manage own orders only
- View personal commission for each order
- Access own performance metrics only
- Cannot create discounts or access other staff's data
- Simplified onboarding experience

---

## 🏗️ Architecture

### Technology Stack
- **Frontend**: WinUI 3, MVVM Pattern, .NET 8.0, GraphQL Client
- **Backend**: Node.js, Express.js, Apollo Server, TypeScript
- **Database**: PostgreSQL 15 (Dockerized)
- **ORM**: Prisma (TypeScript-first ORM)
- **Security**: JWT Authentication, BCrypt Password Hashing, Role-Based Access Control

### Design Patterns
- **MVVM (Model-View-ViewModel)**: Clear separation between UI and business logic
- **Repository Pattern**: Centralized data access with `GraphQLRepositoryBase`
- **Dependency Injection**: Loose coupling and testability
- **Strategy Pattern**: Flexible sorting and discount calculation
- **Factory Pattern**: Dynamic report generation (Daily, Weekly, Monthly, Yearly)
- **Command Pattern**: Order operations (Create, Update, Cancel)

---

## 🔧 Prerequisites

Install these before starting:

| Software | Version | Purpose |
|----------|---------|---------|
| **Visual Studio 2022** | Latest | Frontend development (with .NET Desktop & WinUI 3 workloads) |
| **.NET SDK** | 8.0+ | .NET application development |
| **Node.js** | 18+ | Backend development |
| **npm** | 9+ | Package management (comes with Node.js) |
| **Docker Desktop** | Latest | PostgreSQL database container |
| **Git** | Latest | Version control |

### Visual Studio Workloads Required
- .NET Desktop Development
- Windows Application Development (with WinUI 3)
- Windows 10 SDK (10.0.19041.0 or higher)

---

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/d1nhnguyen/myshop.git
cd myshop
```

### 2. Backend & Database Setup

```bash
# Navigate to backend folder
cd src/MyShop.Backend

# Start PostgreSQL database via Docker
docker-compose up -d

# Install dependencies
npm install

# Generate Prisma Client
npx prisma generate

# Seed the database (REQUIRED for first-time setup)
npm run seed

# Start development server
npm run dev
```

**Backend will run on:** `http://localhost:4000`  
**GraphQL Playground:** `http://localhost:4000/graphql`

### 3. Frontend Setup

1. Open `src/MyShop.sln` in **Visual Studio 2022**
2. Right-click solution → **Restore NuGet Packages**
3. Set **MyShop.App** as Startup Project (right-click → Set as Startup Project)
4. Build the solution: **Build → Build Solution** (or `Ctrl+Shift+B`)
5. Run the application: Press `F5` or click **Debug → Start Debugging**

### 4. First Login

Use one of the pre-seeded accounts:

| Username | Password | Role | Description |
|----------|----------|------|-------------|
| `admin` | `Admin@123456` | **ADMIN** | Full system access |
| `manager1` | `Password@123` | **MANAGER** | Operations management |
| `staff1` | `Password@123` | **STAFF** | Sales representative |

**Note:** If the backend is not running on `localhost:4000`, click the **Config** button on the login screen to set a custom server URL.

---

## 📁 Project Structure
```
myshop/
├── src/
│   ├── MyShop.App/              # WinUI 3 Frontend Application
│   │   ├── Assets/              # Images, fonts, and static resources
│   │   ├── Controls/            # Reusable custom UI controls
│   │   ├── Converters/          # Data binding converters
│   │   ├── Helpers/             # UI utility classes
│   │   ├── Models/              # View-specific models
│   │   ├── Services/            # Frontend services (auth, config, etc.)
│   │   ├── ViewModels/          # MVVM ViewModels
│   │   │   └── Base/            # Base ViewModel classes
│   │   └── Views/               # XAML UI screens
│   │       └── Dialogs/         # Modal dialog windows
│   │
│   ├── MyShop.Core/             # Business Logic Layer
│   │   ├── Helpers/             # Utility classes and extensions
│   │   ├── Interfaces/          # Service and repository contracts
│   │   │   ├── Repositories/    # Repository interfaces
│   │   │   ├── Services/        # Service interfaces
│   │   │   └── Strategies/      # Strategy pattern interfaces
│   │   ├── Models/              # Domain models and entities
│   │   │   └── DTOs/            # Data Transfer Objects
│   │   ├── Services/            # Business logic services
│   │   └── Strategies/          # Strategy implementations (sorting, discounts)
│   │
│   ├── MyShop.Data/             # Data Access Layer
│   │   └── Repositories/        # GraphQL repository implementations
│   │       └── Base/            # Base repository class (GraphQLRepositoryBase)
│   │
│   ├── MyShop.Backend/          # Node.js + Express + GraphQL Backend
│   │   ├── prisma/              # Database schema and migrations
│   │   │   └── migrations/      # Database migration history
│   │   ├── src/                 # TypeScript source code
│   │   │   ├── config/          # Application configuration
│   │   │   ├── graphql/         # GraphQL schema and resolvers
│   │   │   │   ├── resolvers/   # Query and mutation resolvers
│   │   │   │   └── typeDefs/    # GraphQL type definitions
│   │   │   ├── middleware/      # Express middleware (auth, error handling)
│   │   │   ├── types/           # TypeScript type definitions
│   │   │   └── utils/           # Helper functions and utilities
│   │   ├── uploads/             # File upload storage
│   │   │   └── products/        # Product images
│   │   ├── docker-compose.yml   # PostgreSQL container configuration
│   │   └── package.json         # Node.js dependencies
│   │
│   └── MyShop.Tests/            # Unit and Integration Tests
│       ├── Mocks/               # Mock objects for testing
│       └── UnitTests/           # Test files
│           ├── Repositories/    # Repository tests
│           ├── Services/        # Service tests
│           └── Strategies/      # Strategy tests
│
├── tools/                       # Build scripts and development utilities
├── docs/                        # Additional documentation
├── RBAC.md                      # Role-Based Access Control documentation
├── TrialSystem.md               # Trial mode implementation details
├── feature-development.md       # Development workflow guide
└── README.md                    # This file
```

---

## 🎯 Advanced Features

### 1. Auto-Save System
Automatic data persistence for:
- Order creation (saves in real-time as you add items)
- Product management (auto-saves after each field change)
- Customer information updates

### 2. Advanced Search & Filtering
Multi-criteria search with support for:
- **Products**: Name, SKU, category, price range, stock level
- **Orders**: Order ID, customer name, date range, status, payment method
- **Customers**: Name, phone, email, membership tier
- **Customizable Sorting**: Any field, ascending or descending

### 3. Discount System
Flexible promotion engine:
- **Percentage-based discounts**: e.g., 20% off
- **Fixed amount discounts**: e.g., $10 off
- **Member-exclusive promotions**: Accessible only to Member tier customers
- **Time-bound campaigns**: Start and end dates
- **Automatic calculation**: Applied at checkout

### 4. Commission System
Automatic sales commission based on performance:
- **Per-order commission**: Displayed in real-time when staff creates orders
- **KPI-based bonuses**: Additional rewards for high performers
- **Staff can view**: Personal commission in orders and reports
- **Admin can view**: All staff performance metrics

### 5. Trial Mode System
15-day full-feature trial:
- First-time users get complete access to all features
- After 15 days, registration/activation required
- Activation code or license key system
- Trial period tracking and expiration notifications

### 6. Database Backup & Restore
Built-in data protection:
- **Manual backup**: Create database snapshots
- **Scheduled backups**: Automated daily/weekly backups
- **One-click restore**: Recover from any backup point
- **Export formats**: SQL dump, CSV

### 7. Onboarding System
Interactive first-use experience:
- **Role-specific tutorials**: Different guides for Admin vs Staff
- **Step-by-step walkthrough**: Key features and workflows
- **Interactive tooltips**: Contextual help throughout the app
- **Skip option**: Can be bypassed for experienced users

### 8. Printing & Reporting
Professional document generation:
- **Order invoices**: PDF generation with company branding
- **Sales reports**: Daily, weekly, monthly, yearly
- **Staff performance reports**: Commission summaries
- **Inventory reports**: Stock levels and valuation
- **Export to PDF/XPS**: For printing or digital distribution

---

## 🧪 Testing

To ensure the application logic works correctly, run the unit tests included in the solution.

### Running Tests

```bash
# From project root

# 1. Clean previous build artifacts
dotnet clean

# 2. Build the solution
dotnet build

# 3. Run all unit tests
dotnet test

```

### Test Coverage
- Unit tests for business logic (Services, Commands, Strategies)
- Integration tests for GraphQL repositories
- UI component tests for ViewModels

---

## 🐛 Troubleshooting

### Port 5432 (PostgreSQL) already in use

**Problem**: Docker container can't start because port 5432 is already occupied.

**Solution**:
```bash
# Check what's using the port
netstat -ano | findstr :5432

# Stop local PostgreSQL service (if installed)
# On Windows: Services → PostgreSQL → Stop

# Or change port in docker-compose.yml:
ports:
  - "5433:5432"  # Maps local 5433 to container 5432
```

### Backend "npm install" fails

**Problem**: Dependencies fail to install.

**Solutions**:
```bash
# Ensure Node.js v18+ is installed
node --version

# Clear npm cache
npm cache clean --force

# Delete node_modules and package-lock.json
rm -rf node_modules package-lock.json

# Reinstall
npm install
```

### WinUI "Windows SDK" missing

**Problem**: Visual Studio can't find Windows SDK.

**Solution**:
1. Open **Visual Studio Installer**
2. Click **Modify** on Visual Studio 2022
3. Go to **Individual Components** tab
4. Search for "Windows 10 SDK (10.0.19041.0)"
5. Check the box and click **Modify**

### Database connection errors

**Problem**: Backend can't connect to PostgreSQL.

**Solutions**:
```bash
# 1. Check if Docker is running
docker ps

# 2. Check if container is UP
docker-compose ps

# 3. View container logs
docker-compose logs postgres

# 4. Restart containers
docker-compose down
docker-compose up -d

# 5. Verify connection string in .env file
# Should be: postgresql://user:password@localhost:5432/myshop
```

### Frontend can't connect to backend

**Problem**: "Failed to connect to server" error.

**Solutions**:
1. Ensure backend is running (`npm run dev` in `src/MyShop.Backend`)
2. Check backend console for errors
3. Click **Config** button on login screen
4. Verify server URL (default: `http://localhost:4000`)
5. Check Windows Firewall isn't blocking port 4000

### GraphQL query errors

**Problem**: "GraphQL error: ..." messages.

**Solutions**:
1. Open GraphQL Playground: `http://localhost:4000/graphql`
2. Test queries directly in the playground
3. Check Prisma schema is in sync: `npx prisma generate`
4. Re-seed database if needed: `npm run seed`

---

## 📖 Team Resources

For detailed technical documentation:

| Document | Description |
|----------|-------------|
| [Team Work Division](docs/team-work-division.md) | Who does what? Responsibilities by person |
| [Feature Development Guide](feature-development.md) | How to implement a vertical slice (5 steps) |
| [Security & RBAC Guide](docs/security-and-rbac.md) | Authentication, authorization, and role management |
| [RBAC Implementation](RBAC.md) | Detailed role-based access control setup |
| [Trial System](TrialSystem.md) | 15-day trial mode implementation details |
| [GraphQL Repository Base](src/MyShop.Data/Repositories/Base/GraphQLRepositoryBase.cs) | Base class for data access pattern |

---

## 🤝 Contributing

### Development Workflow

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Follow the existing code style
   - Update tests if needed
   - Add documentation for new features

3. **Test your changes**
   ```bash
   dotnet test
   ```

4. **Commit with clear messages**
   ```bash
   git commit -m "feat: add customer export functionality"
   ```

5. **Push and create a pull request**
   ```bash
   git push origin feature/your-feature-name
   ```

### Code Style Guidelines
- **C#**: Follow Microsoft C# Coding Conventions
- **TypeScript**: Use ESLint configuration in `MyShop.Backend`
- **XAML**: Keep view logic minimal, use ViewModels

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built with [WinUI 3](https://github.com/microsoft/microsoft-ui-xaml)
- Powered by [Apollo GraphQL](https://www.apollographql.com/)
- Database by [PostgreSQL](https://www.postgresql.org/)
- ORM by [Prisma](https://www.prisma.io/)

---

