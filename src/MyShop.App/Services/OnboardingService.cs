using MyShop.Core.Interfaces.Services;
using Windows.Storage;

namespace MyShop.App.Services
{
    public class OnboardingService : IOnboardingService
    {
        private const string OnboardingCompletedKey = "OnboardingCompleted";

        public bool IsOnboardingCompleted(string username)
        {
            string key = $"{OnboardingCompletedKey}_{username}";
            return ApplicationData.Current.LocalSettings.Values.ContainsKey(key) && 
                   (bool)ApplicationData.Current.LocalSettings.Values[key];
        }

        public void MarkOnboardingAsCompleted(string username)
        {
            string key = $"{OnboardingCompletedKey}_{username}";
            ApplicationData.Current.LocalSettings.Values[key] = true;
        }
    }
}
