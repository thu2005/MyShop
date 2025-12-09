# MyShop - E-commerce Management System

A modern desktop application for managing products, orders, customers, and generating business reports built with WinUI 3 and .NET 8.0.

---

## 🔧 Prerequisites

Install these before starting:

| Software | Required | Download |
|----------|----------|----------|
| Visual Studio 2022 | ✅ | [Download](https://visualstudio.microsoft.com/downloads/) |
| .NET 8.0 SDK | ✅ | [Download](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Docker Desktop | ✅ | [Download](https://www.docker.com/products/docker-desktop) |
| Git | ✅ | [Download](https://git-scm.com/downloads) |

**Visual Studio Workloads Required:**
- ✅ .NET Desktop Development
- ✅ Windows App SDK (WinUI 3)

---

## Installation

### 1. Clone Repository

```bash
# Clone the project
git clone https://github.com/YOUR_USERNAME/MyShop.git

# Navigate to project folder
cd MyShop

# Switch to develop branch
git checkout develop

# Pull latest changes
git pull origin develop
```

### 2. Verify .NET 8.0

```bash
# Check .NET version
dotnet --version
# Should show: 8.0.x or higher
```

If not installed, download from [.NET 8.0 Download](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## 🗄️ Database Setup

### 1. Start Docker Desktop

- Open Docker Desktop application
- Wait until Docker icon in system tray shows green (running)

### 2. Start PostgreSQL Container

Open terminal in project root folder (where `docker-compose.yml` is):

```bash
# Start database
docker-compose up -d

# Verify container is running
docker ps
```

You should see:
```
CONTAINER ID   IMAGE                  STATUS         PORTS                    NAMES
xxxxx          postgres:15-alpine     Up 10 seconds  0.0.0.0:5432->5432/tcp   myshop_postgres
```
### 3. Test Database Connection (Optional)

```bash
# Access PostgreSQL shell
docker exec -it myshop_postgres psql -U admin -d myshop

# You should see PostgreSQL prompt:
myshop=#

# Exit
\q
```

---

## ▶️ Running the Project

### 1. Restore NuGet Packages

```bash
# In project root folder
dotnet restore
```

### 2. Apply Database Migrations

```bash
# Navigate to Data project
cd src/MyShop.Data

# Run migrations
dotnet ef database update --startup-project ../MyShop.App

# Return to root
cd ../..
```

**Note:** If migrations don't exist yet, Person 1 (Team Lead) will create them first.

### 3. Build Solution

```bash
# Build entire solution
dotnet build
```

### 4. Run Application

**Option A - Command Line:**
```bash
cd src/MyShop.App
dotnet run
```

**Option B - Visual Studio:**
1. Open `MyShop.sln`
2. Right-click `MyShop.App` → Set as Startup Project
3. Press `F5` or click ▶ Start

---

## 📁 Project Structure

```
MyShop/
├── .github/
│   ├── CONTRIBUTING.md         # Gitflow workflow guide
│   └── pull_request_template.md
├── src/
│   ├── MyShop.App/            # WinUI Application (UI Layer)
│   ├── MyShop.Core/           # Business Logic Layer
│   ├── MyShop.Data/           # Data Access Layer
│   └── MyShop.Tests/          # Unit & Integration Tests
├── docker-compose.yml          # PostgreSQL configuration
├── README.md                   # This file
└── .gitignore
```

---

## 🐛 Troubleshooting

### Issue: Port 5432 already in use

```bash
# Check what's using port 5432
netstat -ano | findstr :5432

# Stop local PostgreSQL service (if installed)
# Or change port in docker-compose.yml to 5433
```

### Issue: Docker container won't start

```bash
# Stop and restart container
docker-compose down
docker-compose up -d

# Check logs
docker-compose logs postgres
```

### Issue: Cannot connect to database

**Check:**
1. Is Docker Desktop running?
2. Is container running? `docker ps`
3. Is container healthy? Look for "healthy" status

### Issue: Migration errors

```bash
# Make sure you're in correct directory
cd src/MyShop.Data

# Try with explicit paths
dotnet ef database update --startup-project ../MyShop.App --project .
```

### Issue: NuGet restore fails

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore
```

### Issue: Build errors

```bash
# Clean and rebuild
dotnet clean
dotnet build --no-incremental
```

---

## 📚 Useful Commands

### Docker Commands

```bash
# Start database
docker-compose up -d

# Stop database
docker-compose down

# View logs
docker-compose logs -f postgres

# Restart database
docker-compose restart

# Remove database (⚠️ deletes all data)
docker-compose down -v
```

### Git Commands

```bash
# Check current branch
git branch

# View all branches
git branch -a

# Switch branch
git checkout branch-name

# Pull latest changes
git pull origin develop

# View status
git status

# View commit history
git log --oneline
```

### Entity Framework Commands

```bash
# Create new migration
dotnet ef migrations add MigrationName --startup-project ../MyShop.Data

# Update database
dotnet ef database update --startup-project ../MyShop.Data

# Remove last migration
dotnet ef migrations remove --startup-project ../MyShop.Data
```