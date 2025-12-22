# Hướng dẫn Implement Chức năng Nâng cao

## 📊 Tổng quan Database Changes

### ✅ Đã có sẵn (không cần thay đổi DB):

| Chức năng | Điểm | Trạng thái | Ghi chú |
|-----------|------|-----------|---------|
| Khuyến mãi giảm giá | 1.0 | ✅ Hoàn chỉnh | Bảng `Discount` với 5 loại discount |
| GraphQL API | 1.0 | ✅ Đang dùng | Backend đã implement GraphQL |
| Phân quyền cơ bản | 0.5 | ✅ Có sẵn | `UserRole`: ADMIN, MANAGER, STAFF |
| Quản lý khách hàng | 0.5 | ✅ Hoàn chỉnh | Bảng `Customer` đầy đủ |
| Backup/Restore | 0.25 | 🔧 Logic only | Dùng PostgreSQL pg_dump/restore |
| Sắp xếp nhiều tiêu chí | 0.5 | 🔧 Logic only | GraphQL resolvers |
| Tìm kiếm nâng cao | 1.0 | 🔧 Logic only | GraphQL filters |
| MVVM Architecture | 0.5 | 🔧 WPF only | ViewModels pattern |
| Dependency Injection | 0.5 | 🔧 WPF only | Microsoft.Extensions.DI |
| Responsive Layout | 0.5 | 🔧 WPF only | Adaptive UI design |
| Obfuscator | 0.25 | 🔧 Build only | ConfuserEx/Dotfuscator |
| Test Cases | 0.5 | 🔧 Code only | Jest + xUnit |
| In đơn hàng | 0.5 | 🔧 Logic only | QuestPDF/iTextSharp |

### 🆕 Đã bổ sung vào DB (Zero conflict):

| Chức năng | Điểm | Bảng mới | Mô tả |
|-----------|------|----------|--------|
| Trial mode (15 ngày) | 0.5 | `AppLicense` | License management |
| Hoa hồng KPI | 0.5 | `Commission` + `SalesTarget` | Track commission & targets |

### 🎨 Frontend Only (không cần DB):

| Chức năng | Điểm | Implementation |
|-----------|------|----------------|
| Onboarding | 0.5 | WPF - localStorage/settings |
| Auto-save đơn hàng | 0.25 | WPF - local cache + timer |

---

## 🔧 Chi tiết Implementation

### 1. Trial Mode - 15 ngày (0.5đ)

**Database:** ✅ Đã thêm bảng `AppLicense`

```prisma
model AppLicense {
  id          Int      @id @default(autoincrement())
  licenseKey  String   @unique @db.VarChar(100)
  deviceId    String?  @db.VarChar(100) // Bind to device
  activatedAt DateTime @default(now())
  expiresAt   DateTime // Trial ends after 15 days
  isActive    Boolean  @default(true)
}
```

**Implementation:**
- **Backend:** 
  - GraphQL queries: `checkLicense(deviceId)`, `getLicense`
  - Mutations: `activateTrial(deviceId)`, `activateLicense(key, deviceId)`
- **WPF:**
  - Check license khi app startup
  - Hiển thị "X ngày còn lại" trong trial mode
  - 5. Responsive Layout (0.5đ)

**Database:** ❌ Không cần

**Implementation:**
- **WPF:** Sử dụng `Grid`, `StackPanel`, `WrapPanel` với `ViewBox`
- Định nghĩa `MinWidth`, `MaxWidth` cho các control
- Responsive breakpoints: 1920px, 1366px, 1024px, 768px
- Test trên nhiều độ phân giải

```xaml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" MinWidth="200" MaxWidth="300"/>
        <ColumnDefinition Width="*" MinWidth="400"/>
    </Grid.ColumnDefinitions>
    
    <!-- Adaptive layout based on window size -->
    <ContentControl Content="{Binding CurrentView}">
        <ContentControl.Style>
            <Style TargetType="ContentControl">
                <Style.Triggers>
                    <DataTrigger Binding="{Binding WindowWidth}" Value="Small">
                        <Setter Property="Template" Value="{StaticResource CompactTemplate}"/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </ContentControl.Style>
    </ContentControl>
</Grid>
```

---

### 6g `Commission`** - Track từng hoa hồng:
```prisma
model Commission {
  id               Int      @id @default(autoincrement())
  userId           Int      // Staff ID
  orderId          Int      @unique // One commission per order
  orderTotal       Decimal  @db.Decimal(10, 2)
  commissionRate   Decimal  @db.Decimal(5, 2) // % commission
  commissionAmount Decimal  @db.Decimal(10, 2)
  isPaid           Boolean  @default(false)
  paidAt           DateTime?
}
```

**Bảng `SalesTarget`** - Mục tiêu theo tháng:
```prisma
model SalesTarget {
  userId         Int
  month          Int      // 1-12
  year           Int      // 2024, 2025...
  targetAmount   Decimal  // Monthly target
  achievedAmount Decimal  // Actual sales
  commissionRate Decimal  // Default rate for this month
}
```

**Implementation:**
- **Backend:** 
  - Tự động tạo `Commission` khi order `COMPLETED`
  - Update `SalesTarget.achievedAmount` khi có order mới
  - GraphQL queries:
    - `myCommissions(month, year)` - Staff xem hoa hồng của mình
    - `staffCommissions(userId, month, year)` - Admin xem hoa hồng của staff
    - `unpaidCommissions` - Danh sách chưa thanh toán
    - `salesTargets(month, year)` - Mục tiêu KPI
  - Mutations:
    - `markCommissionPaid(id)` - Đánh dấu đã trả
    - `setSalesTarget(userId, month, year, targetAmount, commissionRate)`

**Auto-create Commission Logic:**
```typescript
// Khi order completed
async function onOrderCompleted(orderId: number) {
  const order = await prisma.order.findUnique({
    where: { id: orderId },
    include: { createdBy: true }
  });
  
  // Get commission rate từ SalesTarget của tháng này
  const now = new Date();
  const target = await prisma.salesTarget.findUnique({
    where: {
      userId_month_year: {
        userId: order.userId,
        month: now.getMonth() + 1,
        year: now.getFullYear()
      }
    }
  });
  
  if (target && target.commissionRate > 0) {
    // Tạo commission
    const commission = await prisma.commission.create({
      data: {
        userId: order.userId,
        orderId: order.id,
        orderTotal: order.total,
        commissionRate: target.commissionRate,
        commissionAmount: order.total * (target.commissionRate / 100),
        isPaid: false
      }
    });
    
    // Update achieved amount
    await prisma.salesTarget.update({
      where: { id: target.id },
      data: {
        achievedAmount: { increment: order.total }
      }
    });
  }
}
```

- **WPF:**
  - **Staff Dashboard:**
    - Hiển thị tổng sales tháng này
    - Progress bar: `achievedAmount / targetAmount`
    - Danh sách commissions (paid/unpaid)
    - Biểu đồ sales theo tháng
  - **Admin View:**
    - Set commission rate cho từng staff
    - Set monthly targets
    - Xem tổng hợp KPI team
    - Đánh dấu đã trả hoa hồng

---

### 3. Auto-save Đơn hàng (0.25đ)

**Database:** ❌ Không cần - Frontend only

**Implementation:**
- **WPF:**
  - Lưu draft vào local cache/temp file
  - Timer auto-save mỗi 30 giây
  - Show indicator "Đã lưu lúc HH:mm:ss"
  - Load draft khi mở lại màn hình tạo order
  - Clear draft sau khi order thành công

```csharp
public class OrderDraftService {
    private readonly Timer _autoSaveTimer;
    private string _draftPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MyShop", "Drafts"
    );
    
    public void StartAutoSave(OrderViewModel order) {
        _autoSaveTimer = new Timer(30000); // 30s
        _autoSaveTimer.Elapsed += (s, e) => SaveDraft(order);
        _autoSaveTimer.Start();
    }
    
    private void SaveDraft(OrderViewModel order) {
        var json = JsonSerializer.Serialize(order);
        var fileName = $"draft_{order.UserId}.json";
        File.WriteAllText(Path.Combine(_draftPath, fileName), json);
        LastSavedAt = DateTime.Now;
    }
}
```

---

### 4. Onboarding (0.5đ)

**Database:** ❌ Không cần - Frontend only

**Implementation:**
- **WPF:**
  - Lưu progress vào Settings/Registry
  - Check first-time user khi app start
  - Show overlay với step-by-step guide:
    1. Giới thiệu giao diện chính
    2. Hướng dẫn thêm sản phẩm
    3. Hướng dẫn tạo đơn hàng
    4. Hướng dẫn xem báo cáo
  - Buttons: Skip / Previous / Next / Done
  - Checkbox "Không hiển thị lại"

```csharp
public class OnboardingService {
    private readonly string SettingsKey = "OnboardingCompleted";
    
    public bool IsCompleted {
        get => Properties.Settings.Default.OnboardingCompleted;
        set {
            Properties.Settings.Default.OnboardingCompleted = value;
            Properties.Settings.Default.Save();
        }
    }
    
    public int CurrentStep {
        get => Properties.Settings.Default.OnboardingStep;
        set {
            Properties.Settings.Default.OnboardingStep = value;
            Properties.Settings.Default.Save();
        }
    }
}
```

---

### 5. Responsive Layout (0.5đ)

**Database:** ❌ Không cần

**Implementation:**
- **WPF:** Sử dụng `Grid`, `StackPanel`, `WrapPanel` với `ViewBox`
- Định nghĩa `MinWidth`, `MaxWidth` cho các control
- Responsive breakpoints: 1920px, 1366px, 1024px, 768px
- Test trên nhiều độ phân giải

---

### 3. Plugin Architecture (1.0đ)

**Database:** ✅ Đã thêm bảng `Plugin`

```prisma
model Plugin {
  name        String @unique
  version     String
  status      PluginStatus // INSTALLED, ENABLED, DISABLED
  config      String? @db.Text // JSON config
  entryPoint  String? // Path to DLL
}
```

**Implementation:**
- **Backend:** 
  - GraphQL queries: `plugins`, `plugin(name)`
  - Mutations: `installPlugin`, `enablePlugin`, `disablePlugin`
- **WPF:**
  - Interface `IPlugin` với methods: `Initialize()`, `Execute()`, `Dispose()`
  - Plugin loader với MEF (Managed Extensibility Framework)
  - UI cho quản lý plugins trong Settings

**Example Plugin Structure:**
```
plugins/
  ReportPlugin/
    ReportPlugin.dll
    manifest.json  # metadata
  PrintPlugin/
    PrintPlugin.dll
```

---

### 4. Khuyến mãi giảm giá (1.0đ)

**Database:** ✅ Đã có sẵn bảng `Discount`

5 loại discount đã support:
1. `PERCENTAGE` - Giảm % (VD: 10% off)
2. `FIXED_AMOUNT` - Giảm số tiền cố định (VD: -50,000đ)
3. `BUY_X_GET_Y` - Mua X tặng Y
4. `MEMBER_DISCOUNT` - Giảm giá cho member
5. `WHOLESALE_DISCOUNT` - Giảm giá bán sỉ (theo số lượng)

**Implementation:**
- ✅ Backend đã có đầy đủ GraphQL API (xem API.md)
- **WPF:** 
  - UI quản lý discounts (CRUD)
  - Apply discount khi tạo order
  - Validate điều kiện (minPurchase, memberOnly, dateRange)
  - Hiển thị discount trong order summary

---

### 5. Obfuscator (0.25đ)

**Database:** ❌ Không cần

**Implementation:**
- Sử dụng tools: **ConfuserEx** hoặc **Dotfuscator**
- Add vào build pipeline
- Obfuscate trước khi deploy

```bash
# Example với ConfuserEx
Confuser.CLI.exe -n project.crproj
```
7
---

### 6. Trial Mode - 15 ngày (0.5đ)

**Database:** ✅ Đã thêm bảng `AppLicense`

```prisma
model AppLicense {
  licenseKey      String @unique
  licenseType     LicenseType // TRIAL, STANDARD, PRO
  activatedAt     DateTime
  expiresAt       DateTime
  isActive        Boolean
  deviceId        String? // Unique device ID
}
```8. Backup / Restore Database (0.25đ)

**Database:** ❌ Không cần thay đổi schema

**Implementation:**
- **Backend:** GraphQL mutations `backupDatabase`, `restoreDatabase`
- Sử dụng PostgreSQL commands:

```bash
# Backup
pg_dump -U admin -d myshop > backup_2024-12-22.sql

# Restore  
psql -U admin -d myshop < backup_2024-12-22.sql
```

```typescript
// Backend resolver
async backupDatabase() {
  const fileName = `backup_${Date.now()}.sql`;
  const filePath = path.join(BACKUP_DIR, fileName);
  
  await exec(`pg_dump -U ${DB_USER} -d ${DB_NAME} > ${filePath}`);
  
  return {
    success: true,
    filePath,
    fileName,
    size: fs.statSync(filePath).size
  };
}
```

- **WPF:** 
  - UI trong Settings/Tools menu
  - Button "Backup" với SaveFileDialog
  - Button "Restore" với OpenFileDialog
  - Progress bar khi backup/restore
  - Confirm dialog trước khi restore (cảnh báo mất data)

---

### 9
### 9. MVVM Architecture (0.5đ)

**Database:** ❌ Không liên quan

**Implementation:**
- ✅ Đã có structure cơ bản:
  - `ViewModels/` - Contains ViewModels
  - `Views/` - Contains XAML views
  - `ViewModels/Base/ViewModelBase.cs` - Base class
  - `ViewModels/Base/RelayCommand.cs` - Command pattern

**Cần bổ sung:**
- Messenger/EventAggregator cho communication giữa ViewModels
- Service locator hoặc DI container
- Navigation service
- Dialog service

---

### 10. Dependency Injection (0.5đ)

**Database:** ❌ Không liên quan

**Implementation:**
- Sử dụng `Microsoft.Extensions.DependencyInjection`

```csharp
// App.xaml.cs
public partial class App : Application {
    private ServiceProvider serviceProvider;
    
    10rotected override void OnStartup(StartupEventArgs e) {
        var services = new ServiceCollection();
        
        // Register services
        services.AddSingleton<IGraphQLClient, GraphQLClient>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        
        serviceProvider = services.BuildServiceProvider();
        
        var mainWindow = new MainWindow {
            DataContext = serviceProvider.GetService<MainViewModel>()
        };
        mainWindow.Show();
    }
}
```

---1

### 11. Phân quyền nâng cao (0.5đ)

**Database:** ✅ Đã có `UserRole`, đã thêm index

**Current roles:**
- `ADMIN` - Full access
- `MANAGER` - Quản lý products, orders, customers
- `STAFF` - Chỉ tạo orders, xem products

**Implementation:**
- **Backend:** 
  - Middleware check permissions
  - Filter queries theo role (VD: STAFF chỉ thấy orders của mình)
  - Hide sensitive data (costPrice) khỏi STAFF

```typescript
// Example resolver với permission
orders: async (_, { filter }, { user }) => {
  if (user.role === 'STAFF') {
    // Staff chỉ thấy orders của mình
    filter.userId = user.id;
  }
  return prisma.order.findMany({ where: filter });
}
```

- **WPF:**
  - Hide/disable UI elements theo role
  - Show/hide columns trong DataGrid
  - Customize menu items

---2

### 12. Hoa hồng bán hàng KPI (0.25đ)

**Database:** ✅ Đã thêm

- Fields trong `User`: `commissionRate`, `monthlySalesTarget`, `totalSales`, `totalCommission`
- Bảng `Commission` để track từng commission

```prisma
model Commission {
  userId          Int
  orderId         Int
  orderTotal      Decimal
  commissionRate  Decimal
  commissionAmount Decimal
  isPaid          Boolean
  paidAt          DateTime?
}
```

**Implementation:**
- **Backend:** 
  - Tự động tạo `Commission` khi order `COMPLETED`
  - GraphQL queries: `myCommissions`, `staffCommissions`, `unpaidCommissions`
  - Mutation: `markCommissionPaid(id)`
- **WPF:**
  - Dashboard cho STAFF: hiển thị tổng sales, commission tháng này
  - Progress bar: X% of monthly target
  - Admin view: quản lý commission rates, pay commissions

**Calculation Logic:**
```typescript
asyncabase:** ❌ Không cần

**Implementation:**

**Backend Tests (Jest + Supertest):**
```bash
pnpm add -D jest @types/jest ts-jest supertest @types/supertest
```

```typescript
// __tests__/products.test.ts
describe('Product Queries', () => {
  test('should get all products', async () => {
    const response = await request(app)
      .post('/graphql')
      .send({ query: '{ products { products { id name } } }' });
    expect(response.status).toBe(200);
  });
});
```

**WPF Tests (xUnit + FluentAssertions):**
```csharp
public class LoginViewModelTests {
    [Fact]
    public async Task Login_WithValidCredentials_ShouldSucceed() {
        // Arrange
        var vm = new LoginViewModel(mockAuthService);
        vm.Username = "admin";
        vm.Password = "Admin@123456";
        
        // Act
        await vm.LoginCommand.ExecuteAsync(null);
        
        // Assert
        vm.IsLoggedIn.Should().BeTrue();
    }
}
```

---

### 15. In đơn hàng (0.5đ)

**Database:** ❌ Không cần

**Implementation:**
- Sử dụng library: **QuestPDF** hoặc **iTextSharp**

```csharp
public class OrderPrinter {
    public void PrintToPdf(Order order, string filePath) {
        Document.Create(container => {
            container.Page(page => {
                page.Size(PageSizes.A4);
                page.Header().Text("ĐơN HÀNG #" + order.OrderNumber);
                page.Content().Column(col => {
                    col.Item().Text($"Khách hàng: {order.Customer.Name}");
                    col.Item().Text($"Tổng tiền: {order.Total:N0} VNĐ");
                    // ... more details
                });
            });
        }).GeneratePdf(filePath);
    }
}
```

**WPF:**
- Button "In đơn hàng" trong Order details
- Save dialog để chọn file PDF/XPS
- Print preview option

---

### 16. Sắp xếp nhiều tiêu chí (0.5đ)

**Database:** ❌ Đã có indexes

**Implementation:**
- **Backend:** GraphQL sorting đã support

```graphql
query GetProducts {
  products(
    sort: [
      { field: "category", order: ASC },
      { field: "price", order: DESC }
    ]
  ) { ... }
}
```

- **WPF:**
  - DataGrid với multi-column sorting
  - Click column header để sort
  - Shift+Click để multi-sort
  - Custom sort indicator (↑↓)

---

### 17. Tìm kiếm nâng cao (1.0đ)

**Database:** ❌ Có thể thêm Full-text search index (optional)

```sql
-- Optional: PostgreSQL full-text search
CREATE INDEX products_search_idx ON products 
USING GIN (to_tsvector('english', name || ' ' || description));
```

**Implementation:**
- **Backend:** GraphQL filters đã support:

```graphql
query SearchProducts {
  products(
    filter: {
      name: "laptop"           # Contains search
      categoryId: 1
      minPrice: 100
      maxPrice: 1000
      inStock: true
      tags: ["sale", "new"]    # Advanced
    }
  ) { ... }
}
```

- **WPF:**
  - Advanced search panel (expandable)
  - Multiple criteria: name, category, price range, stock status
  - Save search presets
  - Recent searches history

---

### 18. Onboarding (0.5đ)

**Database:** ✅ Đã thêm fields vào `User`

```prisma
model User {
  hasCompletedOnboarding Boolean  @default(false)
  onboardingStep         Int      @default(0)
  lastOnboardingDate     DateTime?
}
```

**Implementation:**
- **Backend:** 
  - Query: `onboardingStatus`
  - Mutation: `updateOnboardingStep(step)`

- **WPF:**
  - Check `hasCompletedOnboarding` khi app start
  - Show step-by-step guide overlay:
    1. Giới thiệu giao diện
    2. Hướng dẫn thêm sản phẩm
    3. Hướng dẫn tạo đơn hàng
    4. Hướng dẫn xem báo cáo
  - Skip/Next/Previous buttons
  - "Không hiển thị lại" checkbox

---

## 🚀 Migration Steps

1. **Chạy migration:**
```bash
cd src/MyShop.Backend
pnpm prisma:migrate
```

2. **Generate Prisma Client:**
```bash
pnpm prisma:generate
```

3. **Seed data mới (nếu cần):**
```bash
pnpm seed
```

---

## 📝 Checklist Implementation

### Must Have (Core features):
- [x] Database schema updates
- [ ] GraphQL resolvers cho bảng mới
- [ ] WPF ViewModels cho features mới
- [ ] UI/UX design cho từng feature
- [ ] Testing

### Nice to Have:
- [ ] Performance optimization
- [ ] Caching strategies
- [ ] Logging & monitoring
- [ ] Documentation updates

---

## 🎯 Tổng điểm có thể đạt

| Loại | Điểm |
|------|------|
| **Đã có sẵn** | 4.75 |
| **Cần implement logic only** | 2.25 |
| **Cần implement + DB** | 2.5 |
| **Tổng cộng** | **9.5 điểm** |

---

Chúc bạn implement thành công! 🎉
 🚀 Migration Steps

1. **Chạy migration:**
```bash
cd src/MyShop.Backend
pnpm prisma migrate dev --name add_advanced_features
```

2. **Generate Prisma Client:**
```bash
pnpm prisma generate
```

3. **Verify migration:**
```bash
pnpm prisma studio  # Check tables created
```

---

## 📝 Checklist Implementation

### Database (Completed):
- [x] Schema design - Zero conflict với bảng cũ
- [x] Migration created
- [x] Prisma client generated
- [ ] GraphQL resolvers cho bảng mới
- [ ] Seed data cho testing

### Backend TODO:
- [ ] AppLicense resolvers (checkLicense, activateTrial)
- [ ] Commission resolvers (auto-create, queries, mutations)
- [ ] SalesTarget resolvers (CRUD, tracking)
- [ ] Backup/Restore mutations
- [ ] Unit tests

### WPF TODO:
- [ ] License check on startup
- [ ] Trial countdown UI
- [ ] KPI Dashboard (staff view)
- [ ] Commission management (admin view)
- [ ] Sales target setting
- [ ] Onboarding overlay
- [ ] Auto-save service
- [ ] Responsive layouts
- [ ] Integration tests

---

## 🎯 Tổng điểm có thể đạt

| Loại | Điểm | Chi tiết |
|------|------|----------|
| **Có sẵn trong DB** | 4.75 | Discount, GraphQL, Roles, Customer, Backup, Sort, Search |
| **Đã thêm vào DB** | 1.0 | Trial (0.5) + KPI (0.5) |
| **Frontend only** | 3.75 | MVVM, DI, Responsive, Obfuscator, Tests, Print, Onboarding, Auto-save |
| **Tổng cộng** | **9.5 điểm** | |

### Phân bổ công việc:
- **Database**: ✅ Done (3 bảng mới, zero conflict)
- **Backend**: 🔧 ~2-3 ngày (resolvers + logic)
- **WPF Frontend**: 🔧 ~5-7 ngày (UI + features)
- **Testing**: 🔧 ~1-2 ngày

---

## 💡 Best Practices

### Khi implement:
1. ✅ Test từng feature riêng biệt
2. ✅ Commit thường xuyên với message rõ ràng
3. ✅ Document code và API endpoints
4. ✅ Handle errors gracefully
5. ✅ Validate input data

### Khi PR/Merge:
1. ✅ Chạy `pnpm prisma migrate dev` trước khi push
2. ✅ Include migration files trong commit
3. ✅ Update README với setup instructions
4. ✅ Notify team về schema changes
5. ✅ Provide migration rollback steps nếu cần