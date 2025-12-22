---
description: How to implement a new feature (Vertical Slice) in MyShop
---

Follow these steps to implement a new feature (e.g., Customer Management, Order History) in the MyShop project.

### 1. Backend Development (GraphQL)
First, define the data and operations in the Node.js backend:
- **Schema**: Add new types, queries, or mutations in `src/MyShop.Backend/graphql/schema.ts` (or relevant module).
- **Resolver**: Implement the logic in a resolver file (e.g., `product.resolver.ts`).
- **Prisma**: If new tables are needed, update `prisma/schema.prisma` and run `npx prisma migrate dev`.

### 2. Core Layer (Abstractions)
Update the shared logic in `MyShop.Core`:
- **Model**: update or create the entity in `Models/` (e.g., `Customer.cs`).
- **Interface**: If the interface doesn't exist, create it in `Interfaces/Repositories/` (e.g., `ICustomerRepository.cs`). 
  > [!NOTE]
  > Person 1 has already defined most interfaces. Check there first!

### 3. Data Layer (Implementation)
Implement the GraphQL communication:
- Create a new repository in `MyShop.Data/Repositories/` (e.g., `GraphQLCustomerRepository.cs`).
- Inherit from `GraphQLRepositoryBase` to reuse the GraphQL client.
- Implement the interface methods using `_client.SendQueryAsync` or `_client.SendMutationAsync`.

### 4. App Layer (Presentation)
Build the user interface:
- **ViewModel**: Create a new ViewModel in `ViewModels/` inheriting from `ViewModelBase`.
- **View**: Create a new XAML Page or UserControl in `Views/`.
- **Binding**: Use `{x:Bind}` to connect the View to the ViewModel.
- **RBAC**: Use `Visibility="{x:Bind ViewModel.UserRole, Converter={StaticResource RoleToVisibilityConverter}, ConverterParameter='ADMIN,MANAGER'}"` to protect UI elements.

### 5. Dependency Injection
Register your new classes in `src/MyShop.App/App.xaml.cs` in the `ConfigureServices` method:
```csharp
// Repository
services.AddSingleton<ICustomerRepository, GraphQLCustomerRepository>();
// ViewModel
services.AddTransient<CustomerViewModel>();
```

### 6. Verification
- Run the Backend and Frontend.
- Use the **Config Screen** in the app to ensure you are pointing to the correct GraphQL endpoint.
- Verify that your data flows from the Database -> Prisma -> GraphQL -> WinUI.
