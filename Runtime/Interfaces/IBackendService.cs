

namespace Ncroquis.Backend
{
    /// 사용할 백엔드 서비스 식별자    
    public enum ProviderKey
    {
        NONE,
        FIREBASE,
        ADX,
        POINTPUB
    }

    public interface IBackendIdentifiable
    {
        ProviderKey providerKey { get; }
    }

    public interface IBackendService
    {
        IBackendProvider Provider(ProviderKey? key = null);
        IBackendAuth Auth(ProviderKey? key = null);
        IBackendAnalytics Analytics(ProviderKey? key = null);
        IBackendDataStore DataStore(ProviderKey? key = null);
        IBackendAds Ads(ProviderKey? key = null);
        IBackendOfferwall Offerwall(ProviderKey? key = null);
    }

}