# 🔐 MyShop Security & Authorization Guide

This guide explains how to use the security infrastructure built for the MyShop project (WinUI 3 + GraphQL).

## 1. Authentication (Login)
The `AuthService` handles user sessions.
- **Login**: Use `AuthService.LoginAsync(username, password)`.
- **Session**: Tokens are managed automatically by `SessionManager` via `LocalSettings`.

### 🔑 Default Development Accounts
After running `npm run seed` in the backend, these accounts are available:

| Role | Username | Password |
|------|----------|----------|
| **ADMIN** | `admin` | `Admin@123456` |
| **MANAGER** | `manager1` | `Password@123` |
| **STAFF** | `staff1` | `Password@123` |

## 2. Authorization (RBAC)
We use **Role-Based Access Control** with three roles: `ADMIN`, `MANAGER`, `STAFF`.

### A. UI-Level (XAML)
Use the `RoleToVisibilityConverter` to hide/show elements.

```xml
<!-- In App.xaml, the converter is registered as "RoleToVisibilityConverter" -->

<!-- Only ADMIN can see this -->
<Button Content="Manage Users"
        Visibility="{x:Bind ViewModel.CurrentUser.Role, Mode=OneWay, 
                    Converter={StaticResource RoleToVisibilityConverter}, 
                    ConverterParameter='ADMIN'}" />

<!-- ADMIN or MANAGER can see this -->
<Button Content="Edit Product"
        Visibility="{x:Bind ViewModel.CurrentUser.Role, Mode=OneWay, 
                    Converter={StaticResource RoleToVisibilityConverter}, 
                    ConverterParameter='ADMIN,MANAGER'}" />
```

### B. Logic-Level (C#)
Inject `IAuthorizationService` into your ViewModels.

```csharp
public class MyViewModel : ViewModelBase {
    private readonly IAuthorizationService _authService;

    public MyViewModel(IAuthorizationService authService) {
        _authService = authService;
        
        // Command with CanExecute check
        DeleteCommand = new RelayCommand(
            execute: _ => PerformDelete(),
            canExecute: _ => _authService.CanManageProducts() // Checks if ADMIN/MANAGER
        );
    }
}
```

### C. Backend-Level
All requests must include the JWT in the Authorization header. `GraphQLService` handles this automatically if a user is logged in. Backend resolvers are protected by `@Roles()` decorators.

## 3. Global Resources
Common converters available in `App.xaml`:
- `BoolToVisibilityConverter`: Convert `bool` to `Visibility`.
- `InvertedBoolToVisibilityConverter`: Convert `bool` to `Visibility` (inverted).
- `RoleToVisibilityConverter`: Convert `UserRole` to `Visibility` based on allowed roles.
