using MyShop.Core.Interfaces.Services;
using Windows.Storage;

namespace MyShop.Core.Services
{
    public class ConfigService : IConfigService
    {
        private const string ServerUrlKey = "ServerUrl";
        private const string DefaultServerUrl = "http://localhost:4000/graphql";
        private const string DatabaseNameKey = "DatabaseName";
        private const string DefaultDatabaseName = "myshop_db";
        private const string LastOpenedPageKey = "LastOpenedPage";

        private readonly ApplicationDataContainer _localSettings;

        public ConfigService()
        {
            _localSettings = ApplicationData.Current.LocalSettings;

            //_localSettings.Values.Remove(LastOpenedPageKey);
        }

        public string GetServerUrl()
        {
            if (_localSettings.Values.TryGetValue(ServerUrlKey, out object? value))
            {
                if (value is string url)
                {
                    return url;
                }
            }
            return DefaultServerUrl;
        }

        public void SaveServerUrl(string url)
        {
            _localSettings.Values[ServerUrlKey] = url;
        }

        public string GetDatabaseName()
        {
            if (_localSettings.Values.TryGetValue(DatabaseNameKey, out object? value))
            {
                if (value is string dbName)
                {
                    return dbName;
                }
            }
            return DefaultDatabaseName;
        }

        public void SaveDatabaseName(string dbName)
        {
            _localSettings.Values[DatabaseNameKey] = dbName;
        }

        public string? GetLastOpenedPage()
        {
            if (_localSettings.Values.TryGetValue(LastOpenedPageKey, out object? value))
            {
                if (value is string pageTag)
                {
                    return pageTag;
                }
            }
            return null;
        }

        public void SaveLastOpenedPage(string pageTag)
        {
            _localSettings.Values[LastOpenedPageKey] = pageTag;
        }
    }
}
