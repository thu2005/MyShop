using MyShop.App.ViewModels.Base;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyShop.App.ViewModels
{
    public class UsersViewModel : ViewModelBase
    {
        private readonly IUserRepository _userRepository;
        private bool _isLoading;

        public UsersViewModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            Users = new ObservableCollection<User>();
            LoadUsersCommand = new RelayCommand(async _ => await LoadUsersAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadUsersAsync());
        }

        public ObservableCollection<User> Users { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoadUsersCommand { get; }
        public ICommand RefreshCommand { get; }

        public async Task LoadUsersAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                var allUsers = await _userRepository.GetAllAsync();
                
                // STEP: Lọc chỉ lấy những người dùng có vai trò là STAFF
                var staffUsers = allUsers.Where(u => u.Role == UserRole.STAFF).ToList();
                
                Users.Clear();
                foreach (var user in staffUsers)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                var newUser = await _userRepository.AddAsync(user);
                Users.Insert(0, newUser);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(User user)
        {
            try
            {
                await _userRepository.DeleteAsync(user.Id);
                Users.Remove(user);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting user: {ex.Message}");
                return false;
            }
        }
    }
}
