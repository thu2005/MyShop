using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Services;
using MyShop.Data.Repositories;
using MyShop.App.ViewModels;
using System;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace MyShop.App
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; }
        public static Window MainWindowInstance { get; private set; }

        public App()
        {
            this.InitializeComponent();

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
        private void ConfigureServices(IServiceCollection services)
        {

            services.AddTransient<DashboardViewModel>();
            // Infrastructure
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<ISessionManager, SessionManager>();

            var configService = new ConfigService();
            var graphQLService = new GraphQLService(configService.GetServerUrl());
            services.AddSingleton(graphQLService);
            services.AddSingleton<IGraphQLService>(graphQLService);

            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IAuthorizationService, AuthorizationService>();
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddSingleton<IDashboardService, DashboardService>();

            services.AddSingleton<IUserRepository, GraphQLUserRepository>();
            services.AddSingleton<IProductRepository, GraphQLProductRepository>();
            // services.AddSingleton<ICategoryRepository, GraphQLCategoryRepository>();
            services.AddSingleton<IReportRepository, GraphQLReportRepository>();

            services.AddTransient<MainWindow>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ConfigViewModel>();
            services.AddTransient<ConfigViewModel>();
            services.AddTransient<ShellViewModel>();

            services.AddTransient<ProductViewModel>();
            services.AddTransient<ReportsViewModel>();

        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = Services.GetService<MainWindow>();
            MainWindowInstance = m_window;

            m_window?.Activate();
        }

        private Window? m_window;
    }
}
