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
        private System.Collections.Generic.List<User> _allStaff = new System.Collections.Generic.List<User>();

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
                
                var staffUsers = allUsers.Where(u => u.Role == UserRole.STAFF).ToList();
                _allStaff = staffUsers;
                
                UpdateUsersList(staffUsers);
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

        private void UpdateUsersList(System.Collections.Generic.IEnumerable<User> users)
        {
            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        public void SearchUsers(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                UpdateUsersList(_allStaff);
                return;
            }

            var lowerKeyword = keyword.ToLower();
            var filtered = _allStaff.Where(u => 
                (u.Username?.ToLower().Contains(lowerKeyword) ?? false) ||
                (u.Email?.ToLower().Contains(lowerKeyword) ?? false)
            ).ToList();

            UpdateUsersList(filtered);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                await _userRepository.UpdateAsync(user);
                
                var index = Users.IndexOf(Users.FirstOrDefault(u => u.Id == user.Id));
                if (index != -1)
                {
                    Users[index] = user;
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                var newUser = await _userRepository.AddAsync(user);
                _allStaff.Insert(0, newUser);
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
                var toRemove = _allStaff.FirstOrDefault(u => u.Id == user.Id);
                if (toRemove != null) _allStaff.Remove(toRemove);
                Users.Remove(user);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                return user == null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking username availability: {ex.Message}");
                return false; // Default to blocked on error for safety
            }
        }
    }
}
