using MyShop.Core.Interfaces.Services;

namespace MyShop.Tests.Mocks
{
    public class MockConfigService : IConfigService
    {
        private string _serverUrl = "http://localhost:4000/graphql";
        private string _dbName = "MyShopDb";
        private string? _lastOpenedPage; // Backing field for the new interface member

        public string GetServerUrl() => _serverUrl;

        public void SaveServerUrl(string url) => _serverUrl = url;

        public string GetDatabaseName() => _dbName;

        public void SaveDatabaseName(string dbName) => _dbName = dbName;

        public string? GetLastOpenedPage()
        {
            return _lastOpenedPage;
        }

        public void SaveLastOpenedPage(string pageTag)
        {
            _lastOpenedPage = pageTag;
        }
    }
}