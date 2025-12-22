using System;

namespace MyShop.Core.Interfaces.Services
{
    public interface IConfigService
    {
        string GetServerUrl();
        void SaveServerUrl(string url);
        string GetDatabaseName();
        void SaveDatabaseName(string dbName);
    }
}
