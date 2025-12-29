using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Services;
using MyShop.Data.Repositories;
using MyShop.App.ViewModels;
using MyShop.App.Services;
using System;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using OfficeOpenXml;

namespace MyShop.App
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; }
        public static Window MainWindowInstance { get; private set; }

        static App()
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        }

        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += App_UnhandledException;

            LiveChartsCore.LiveCharts.Configure(config =>
                config.AddSkiaSharp()
                      .AddDefaultMappers()
                      .AddLightTheme());

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();
        }

        public T GetService<T>() where T : class
        {
            return Services.GetRequiredService<T>();
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // Log the exception to debug output
            System.Diagnostics.Debug.WriteLine($"Unhandled exception: {e.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {e.Exception?.StackTrace}");
            
            // We can choose to mark it as handled to prevent the crash dialog
            // but for Win32 exceptions, this might not always work.
            e.Handled = true;
        }
        private void ConfigureServices(IServiceCollection services)
        {

            services.AddTransient<DashboardViewModel>();
            // Infrastructure
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<ISessionManager, SessionManager>();
            services.AddSingleton<IOnboardingService, OnboardingService>();

            var configService = new ConfigService();
            var graphQLService = new GraphQLService(configService.GetServerUrl());
            services.AddSingleton(graphQLService);
            services.AddSingleton<IGraphQLService>(graphQLService);

            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IAuthorizationService, AuthorizationService>();
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddSingleton<IDashboardService, DashboardService>();
            services.AddSingleton<IImageUploadService>(sp => new ImageUploadService("http://localhost:4000"));

            services.AddSingleton<IUserRepository, GraphQLUserRepository>();
            services.AddSingleton<IProductRepository, GraphQLProductRepository>();
            services.AddSingleton<ICategoryRepository, GraphQLCategoryRepository>();
            services.AddSingleton<IReportRepository, GraphQLReportRepository>();
            services.AddSingleton<IOrderRepository, GraphQLOrderRepository>();
            services.AddSingleton<ICustomerRepository, GraphQLCustomerRepository>();
            services.AddSingleton<IDiscountRepository, GraphQLDiscountRepository>();
            services.AddSingleton<ICustomerRepository, GraphQLCustomerRepository>();


            services.AddSingleton<IProductImportService, ProductImportService>();
            services.AddSingleton<IDraftService, DraftService>();

            services.AddTransient<MainWindow>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ConfigViewModel>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<UsersViewModel>();

            services.AddTransient<ProductViewModel>();
            services.AddTransient<ProductDetailViewModel>();
            services.AddTransient<AddProductViewModel>();
            services.AddTransient<ImportProductsViewModel>();
            services.AddTransient<ReportsViewModel>();
            services.AddTransient<OrderViewModel>();

            services.AddTransient<CustomersViewModel>();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = Services.GetService<MainWindow>();
            MainWindowInstance = m_window;

            if (m_window != null)
            {
                m_window.Closed += (s, e) =>
                {
                    // Force the application to exit cleanly
                    // This can help resolve some Win32 exceptions on exit in WinUI 3
                    Application.Current.Exit();
                };
                m_window.Activate();
            }
        }

        private Window? m_window;
    }
}
