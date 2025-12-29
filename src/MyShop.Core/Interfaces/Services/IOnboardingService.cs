namespace MyShop.Core.Interfaces.Services
{
    public interface IOnboardingService
    {
        bool IsOnboardingCompleted(string username);
        void MarkOnboardingAsCompleted(string username);
    }
}
