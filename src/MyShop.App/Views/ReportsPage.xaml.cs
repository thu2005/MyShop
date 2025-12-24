using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using MyShop.App.ViewModels;
using System.Linq;
using MyShop.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace MyShop.App.Views
{
    public sealed partial class ReportsPage : Page
    {
        public ReportsViewModel ViewModel { get; }

        public ReportsPage()
        {
            this.InitializeComponent();
            ViewModel = ((App)Application.Current).Services.GetService<ReportsViewModel>();
        }
    }
}
