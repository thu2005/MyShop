using System;

namespace MyShop.Core.Interfaces.Services
{
    public interface IConfigService
    {
        string GetServerUrl();
        void SaveServerUrl(string url);
        string GetDatabaseName();
        void SaveDatabaseName(string dbName);
        
        // Last opened page feature
        string? GetLastOpenedPage();
        void SaveLastOpenedPage(string pageTag);
    }
}
