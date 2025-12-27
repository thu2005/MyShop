using CommunityToolkit.Mvvm.ComponentModel;
using MyShop.Core.Models;

namespace MyShop.App.ViewModels
{
    public partial class SelectableCustomer : ObservableObject
    {
        public Customer Customer { get; }

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isCheckboxVisible;

        public SelectableCustomer(Customer customer)
        {
            Customer = customer;
        }
    }
}
