using Microsoft.UI.Xaml.Controls;
using MyShop.App.ViewModels;

namespace MyShop.App.Views
{
    public sealed partial class LoginScreen : Page
    {
        public LoginViewModel ViewModel { get; }

        public LoginScreen()
        {
            this.InitializeComponent();
            // ViewModel will be injected via DI later
            // For now, create manually (temporary)
        }
    }
}