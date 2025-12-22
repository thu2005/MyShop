using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.App.ViewModels;

namespace MyShop.App.Views
{
    public sealed partial class ConfigScreen : ContentDialog
    {
        public ConfigViewModel ViewModel { get; }

        public ConfigScreen()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<ConfigViewModel>();
        }
    }
}
