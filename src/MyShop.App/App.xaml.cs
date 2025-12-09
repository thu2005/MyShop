using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyShop.Data.Context;
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
            services.AddDbContext<MyShopDbContext>(options =>
                options.UseNpgsql("Host=localhost;Port=5432;Database=myshop;Username=admin;Password=admin123!"));


            services.AddTransient<MainWindow>();
        }
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = Services.GetService<MainWindow>();

            m_window?.Activate();
        }

        private Window? m_window;
    }
}