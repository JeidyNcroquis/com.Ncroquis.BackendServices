

namespace Ncroquis.Backend
{
    public interface IBackendService
    {
        IBackendProvider Provider(string key = null);
        IBackendAuth Auth(string key = null);
        IBackendAnalytics Analytics(string key = null);
        IBackendData Data(string key = null);
        IBackendAds Ads(string key = null);
    }

    public interface IBackendIdentifiable
    {
        string ProviderName { get; }
    }
}