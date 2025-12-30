using System;
using System.Text.Json;
using Windows.Storage;

namespace MyShop.App.Services
{
    public interface IDraftService
    {
        void SaveDraft<T>(string key, T data);
        T GetDraft<T>(string key);
        void ClearDraft(string key);
        bool HasDraft(string key);
    }

    public class DraftService : IDraftService
    {
        private readonly ApplicationDataContainer _localSettings;

        public DraftService()
        {
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        public void SaveDraft<T>(string key, T data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                _localSettings.Values[key] = json;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save draft: {ex.Message}");
            }
        }

        public T GetDraft<T>(string key)
        {
            try
            {
                if (_localSettings.Values.TryGetValue(key, out var value) && value is string json)
                {
                    return JsonSerializer.Deserialize<T>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get draft: {ex.Message}");
            }

            return default;
        }

        public void ClearDraft(string key)
        {
            try
            {
                _localSettings.Values.Remove(key);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear draft: {ex.Message}");
            }
        }

        public bool HasDraft(string key)
        {
            return _localSettings.Values.ContainsKey(key);
        }
    }
}
