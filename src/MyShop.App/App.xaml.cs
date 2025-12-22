using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Services;
using MyShop.Data.Repositories;
using MyShop.App.ViewModels;
using System;

namespace MyShop.App
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; }

        public App()
        {
            this.InitializeComponent();

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Infrastructure
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<ISessionManager, SessionManager>();

            // GraphQL Configuration (Initial load from config)
            var configService = new ConfigService();
            services.AddSingleton(new GraphQLService(configService.GetServerUrl()));

            // Services
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IAuthorizationService, AuthorizationService>();
            services.AddSingleton<IEncryptionService, EncryptionService>();

            // Repositories (GraphQL-first)
            services.AddSingleton<IUserRepository, GraphQLUserRepository>();
            services.AddSingleton<IProductRepository, GraphQLProductRepository>();

            // ViewModels
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ConfigViewModel>();
            services.AddTransient<ShellViewModel>();
        }
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = Services.GetService<MainWindow>();

            m_window?.Activate();
        }

        private Window? m_window;
    }
}